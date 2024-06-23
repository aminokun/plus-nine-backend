using Microsoft.Extensions.Options;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Logic.Models;
using Stripe;
using Stripe.Checkout;
using System.Runtime.InteropServices;

namespace PlusNine.Logic
{
    public class StripeService
    {
        private readonly StripeModel _model;
        private readonly CustomerService _customerService;
        private readonly ProductService _productService;
        private readonly IUnitOfWork _unitOfWork;

        public StripeService(
            IOptions<StripeModel> model,
            CustomerService customerService,
            ProductService productService,
            IUnitOfWork unitOfWork)
        {
            _model = model.Value;
            _customerService = customerService;
            _productService = productService;
            _unitOfWork = unitOfWork;
            StripeConfiguration.ApiKey = _model.SecretKey;
        }

        public Session CreateCheckoutSession(string priceId, string customerId)
        {
            var options = new SessionCreateOptions
            {
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions
                    {
                        Price = priceId,
                        Quantity = 1,
                    }
                },
                Mode = "payment",
                SuccessUrl = "http://localhost:3000/success",
                CancelUrl = "http://localhost:3000/"
            };

            options.Customer = customerId;

            var service = new SessionService();
            Session session = service.Create(options);
            return session;
        }

        public async Task<Customer> CreateCustomer(string username, string email)
        {
            var customerOptions = new CustomerCreateOptions
            {
                Email = email,
                Name = username
            };

            var options = new CustomerSearchOptions
            {
                Query = $"name:'{username}'",
            };

            var customerService = new CustomerService();
            var existingCustomer = customerService.Search(options);

            if (!existingCustomer.Data.Any())
            {
                var customer = await _customerService.CreateAsync(customerOptions);
                var user = await _unitOfWork.User.SingleOrDefaultAsync(u => u.UserName == username && u.Email == email);

                user.CustomerId = customer.Id;

                await _unitOfWork.User.Update(user);
                await _unitOfWork.CompleteAsync();
                return customer;
            }

            return customerService.Get(existingCustomer.Data.First().Id);

        }

        public StripeList<Product> GetAllProducts()
        {
            var options = new ProductListOptions { Active = true, Expand = new List<string>() { "data.default_price" } };
            var products = _productService.List(options);
            return products;
        }


        public async Task UpdateUserRoleOnPaymentSuccess(string customerId)
        {
            await UpdateUserRoleAsync(customerId, "Premium");
        }

        private async Task UpdateUserRoleAsync(string customerId, string role)
        {
            var user = await _unitOfWork.User.GetByCustomerIdAsync(customerId);
            if (user != null)
            {
                user.Role = role;
                await _unitOfWork.User.Update(user);
                await _unitOfWork.CompleteAsync();
            }
        }
    }
}