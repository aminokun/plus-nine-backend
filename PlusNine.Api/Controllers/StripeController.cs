﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PlusNine.DataService.Repositories.Interfaces;
using PlusNine.Logic;
using PlusNine.Logic.Models;
using Stripe;

namespace PlusNine.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : BaseController
    {
        private readonly StripeModel model;
        private readonly StripeService _stripeService;

        public StripeController(
            IOptions<StripeModel> _model,
            StripeService stripeService,
            IUnitOfWork unitOfWork,
            IMapper mapper
            ) : base(unitOfWork, mapper)
        {
            model = _model.Value;
            _stripeService = stripeService;
        }

        [Authorize]
        [HttpPost("Pay")]
        public async Task<IActionResult> Pay([FromBody] string priceId)
        {
            var session = _stripeService.CreateCheckoutSession(priceId);
            return Ok(session.Url);
        }

        [Authorize]
        [HttpPost("CreateCustomer")]
        public async Task<IActionResult> CreateCustomer()
        {
            string username = GetUsernameFromClaims();
            string email = GetEmailFromClaims();
            var customer = await _stripeService.CreateCustomer(username, email);
            return Ok(new { customer });
        }

        [HttpGet("GetAllProducts")]
        public IActionResult GetAllProducts()
        {
            var products = _stripeService.GetAllProducts();
            return Ok(products);
        }

        [HttpPost("WebhookEndpoint")]
        public async Task<IActionResult> WebhookEndpoint()
        {
            string WebhookSecret = model.WebhookSecret;
            //const string endpointSecret = "whsec_a6d1b71156c345d90c385004473a54c8492e7efe527980a899106700cbae1b56";

            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], WebhookSecret);

            if (stripeEvent.Type == Events.CheckoutSessionCompleted)
            {
                var session = stripeEvent.Data.Object as Stripe.Checkout.Session;
                await _stripeService.UpdateUserRoleOnPaymentSuccess(session.CustomerId);
            }

            return Ok();
        }

        private string GetUsernameFromClaims()
        {
            var usernameClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "username");
            return usernameClaim?.Value;
        }

        private Guid GetIdFromClaims()
        {
            var idClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "Id");
            Guid id = Guid.Parse(idClaim?.Value);
            return id;
        }

        private string GetEmailFromClaims()
        {
            var emailClaim = HttpContext.User.Claims.FirstOrDefault(c => c.Type == "email");
            return emailClaim?.Value;
        }
    }
}