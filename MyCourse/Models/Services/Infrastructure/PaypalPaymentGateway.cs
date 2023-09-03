using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using MyCourse.Models.Enums;
using MyCourse.Models.Exceptions.Infrastructure;
using MyCourse.Models.InputModels.Courses;
using MyCourse.Models.Options;
using PayPalCheckoutSdk.Core;
using PayPalCheckoutSdk.Orders;
using PayPalHttp;

namespace MyCourse.Models.Services.Infrastructure
{
    public class PaypalPaymentGateway : IPaymentGateway
    {
        private readonly IOptionsMonitor<PaypalOptions> options;

        public PaypalPaymentGateway(IOptionsMonitor<PaypalOptions> options)
        {
            this.options = options;
        }

        public async Task<CourseSubscribeInputModel> CapturePaymentAsync(string token)
        {
            try
            {
                PayPalEnvironment environment = GetPayPalEnvironment(options.CurrentValue);
                PayPalHttpClient client = new PayPalHttpClient(environment);

                OrdersCaptureRequest request = new(token);
                request.RequestBody(new OrderActionRequest());
                request.Prefer("return=representation");

                HttpResponse response = await client.Execute(request);
                Order result = response.Result<Order>();

                PurchaseUnit purchaseUnit = result.PurchaseUnits.First();
                Capture capture = purchaseUnit.Payments.Captures.First();
                string[] customIdParts = purchaseUnit.CustomId.Split('/');

                return new CourseSubscribeInputModel()
                {
                    CourseId = int.Parse(customIdParts[0]),
                    UserId = customIdParts[1],
                    Paid = new(Enum.Parse<Currency>(capture.Amount.CurrencyCode), decimal.Parse(capture.Amount.Value, CultureInfo.InvariantCulture)),
                    TransactionId = capture.Id,
                    PaymentDate = DateTime.Parse(capture.CreateTime, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal),
                    PaymentType = "Paypal"
                };
            }
            catch (Exception exc)
            {
                throw new PaymentGatewayException(exc);
            }
        }

        public async Task<string> GetPaymentUrlAsync(CoursePayInputModel inputModel)
        {
            try
            {
                OrderRequest order = new()
                {
                    CheckoutPaymentIntent = "CAPTURE",
                    ApplicationContext = new ApplicationContext()
                    {
                        ReturnUrl = inputModel.ReturnUrl,
                        CancelUrl = inputModel.CancelUrl,
                        BrandName = options.CurrentValue.BrandName,
                        ShippingPreference = "NO_SHIPPING"
                    },
                    PurchaseUnits = new List<PurchaseUnitRequest>()
                    {
                        new PurchaseUnitRequest()
                        {
                            CustomId = $"{inputModel.CourseId}/{inputModel.UserId}",
                            Description = inputModel.Description.Substring(0, 100),
                            AmountWithBreakdown = new AmountWithBreakdown()
                            {
                                CurrencyCode = inputModel.Price.Currency.ToString(),
                                Value = inputModel.Price.Amount.ToString(CultureInfo.InvariantCulture)
                            }
                        }
                    }
                };
                PayPalEnvironment environment = GetPayPalEnvironment(options.CurrentValue);
                PayPalHttpClient client = new PayPalHttpClient(environment);

                OrdersCreateRequest request = new();
                request.RequestBody(order);
                request.Prefer("return=representation");

                HttpResponse response = await client.Execute(request);
                Order result = response.Result<Order>();
                LinkDescription link = result.Links.Single(link => link.Rel == "approve");
                return link.Href;
            }
            catch (Exception exc)
            {
                throw new PaymentGatewayException(exc);
            }
        }

        private PayPalEnvironment GetPayPalEnvironment(PaypalOptions options)
        {
            return options.IsSandbox ? new SandboxEnvironment(options.ClientId, options.ClientSecret) : new LiveEnvironment(options.ClientId, options.ClientSecret);
        }
    }
}
