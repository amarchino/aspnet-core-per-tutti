using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyCourse.Models.Enums;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using Stripe;
using Stripe.Checkout;

namespace MyCourse.Models.Services.Infrastructure
{
    public class StripePaymentGateway : IPaymentGateway
    {
        private readonly IOptionsMonitor<StripeOptions> options;

        public StripePaymentGateway(IOptionsMonitor<StripeOptions> options)
        {
            this.options = options;
        }

        public async Task<CourseSubscribeInputModel> CapturePaymentAsync(string token)
        {
            RequestOptions requestOptions = new()
            {
                ApiKey = options.CurrentValue.PrivateKey
            };
            SessionService sessionService = new();
            Session session = await sessionService.GetAsync(token, requestOptions: requestOptions);

            PaymentIntentService paymentIntentService = new();
            PaymentIntent paymentIntent = await paymentIntentService.CaptureAsync(session.PaymentIntentId, requestOptions: requestOptions);
            string[] customIdParts = session.ClientReferenceId.Split('/');

            return new()
            {
                CourseId = int.Parse(customIdParts[0]),
                UserId = customIdParts[1],
                Paid = new(Enum.Parse<Currency>(paymentIntent.Currency, ignoreCase: true), paymentIntent.Amount / 100m),
                TransactionId = paymentIntent.Id,
                PaymentDate = paymentIntent.Created,
                PaymentType = "Stripe"
            };
        }

        public async Task<string> GetPaymentUrlAsync(CoursePayInputModel inputModel)
        {
            SessionCreateOptions sessionCreateOptions = new()
            {
                ClientReferenceId = $"{inputModel.CourseId}/{inputModel.UserId}",
                LineItems = new List<SessionLineItemOptions>
                {
                    new SessionLineItemOptions()
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions()
                        {
                            Currency = inputModel.Price.Currency.ToString(),
                            UnitAmount = Convert.ToInt64(inputModel.Price.Amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions()
                            {
                                Name = inputModel.Title,
                                Description = inputModel.Description
                            }
                        }
                    }
                },
                Mode = "payment",
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    CaptureMethod = "manual"
                },
                PaymentMethodTypes = new List<string>
                {
                    "card"
                },
                SuccessUrl = inputModel.ReturnUrl + "?token={CHECKOUT_SESSION_ID}",
                CancelUrl = inputModel.CancelUrl
            };

            RequestOptions requestOptions = new()
            {
                ApiKey = options.CurrentValue.PrivateKey
            };
            SessionService sessionService = new();
            Session session = await sessionService.CreateAsync(sessionCreateOptions, requestOptions);
            return session.Url;
        }
    }
}
