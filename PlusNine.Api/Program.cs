using Microsoft.EntityFrameworkCore;
using PlusNine.DataService.Data;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.DataService.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using PlusNine.Logic.Interfaces;
using PlusNine.Logic;
using PlusNine.Logic.Models;
using PlusNine.Logic.Hubs;
using Stripe;

var builder = WebApplication.CreateBuilder(args);
ConfigurationManager configuration = builder.Configuration;

var allowedOrigins = builder.Configuration
    .GetValue<string>("AllowedOrigins")?
    .Split(',', StringSplitOptions.RemoveEmptyEntries)
    .Select(origin => origin.Trim())
    .ToArray() ?? Array.Empty<string>();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseMySQL(builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.")));

builder.Services.AddCors(options => {

    // React
    options.AddPolicy("reactapp", policyBuilder =>
    {
        policyBuilder.WithOrigins(allowedOrigins);
        policyBuilder.AllowAnyHeader();
        policyBuilder.AllowAnyMethod();
        policyBuilder.AllowCredentials();
    });
});

builder.Services.AddMvc().AddNewtonsoftJson();
// Add services to the container.
builder.Services.AddSignalR();
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//stripe
builder.Services.Configure<StripeModel>(builder.Configuration.GetSection("Stripe"));
builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<ProductService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<ChargeService>();

builder.Services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<StripeService>();
builder.Services.AddScoped<IObjectiveService, ObjectiveService>();
builder.Services.AddScoped<IFriendService, FriendService>();

builder.Services.Configure<AppSettings>(
    builder.Configuration.GetSection("ApplicationSettings"));

builder.Services.AddAuthentication(authOpt =>
{
    authOpt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    authOpt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddCookie(cookieOpt =>
{
    cookieOpt.Cookie.Name = "token";

}).AddJwtBearer(jwtOpt =>
{
    jwtOpt.RequireHttpsMetadata = false;
    jwtOpt.SaveToken = true;
    jwtOpt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["ApplicationSettings:Secret"])),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
    jwtOpt.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            context.Token = context.Request.Cookies["X-Access-Token"];
            return Task.CompletedTask;
        }
    };
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors("reactapp");

app.UseRouting();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHub<FriendHub>("/friendHub");

app.Run();