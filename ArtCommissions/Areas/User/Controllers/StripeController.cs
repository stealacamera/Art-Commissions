using ArtCommissions.BLL;
using ArtCommissions.Common.Constants;
using ArtCommissions.Common.Enums;
using ArtCommissions.Common.Exceptions;
using ArtCommissions.Controllers;
using Microsoft.AspNetCore.Mvc;
using Stripe;

namespace ArtCommissions.Areas.User.Controllers
{
    public class StripeController : BaseController
    {
        private readonly string _webhookKey;

        public StripeController(IConfiguration configuration, IServicesManager servicesManager) : base(servicesManager, configuration)
        {
            _webhookKey = configuration["StripeSettings:WebhookKey"] ?? throw new ArtCommissionsException("\"StripeSettings:WebhookKey\" key is missing");
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Index()
        {
            var json = await new StreamReader(Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webhookKey);

                switch (stripeEvent.Type)
                {
                    case Events.PaymentIntentProcessing:
                        {
                            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                            await DeletePaymentIntentIfInvoiceUpdated(paymentIntent);
                        }
                        break;
                    case Events.PaymentIntentSucceeded:
                        {
                            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                            await PayInvoice(paymentIntent);
                        }
                        break;
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }

        [HttpPost("webhook/create-payment")]
        public async Task<IActionResult> CreatePaymentIntent(int orderId, int invoiceId)
        {
            var paymentIntent = await _servicesManager.StripeService.CreatePaymentIntent(GetCurrentUserId(), orderId, invoiceId);
            return Ok(new { clientSecret = paymentIntent.ClientSecret });
        }

        private async Task PayInvoice(PaymentIntent paymentIntent)
        {
            int appOrderId = int.Parse(paymentIntent.Metadata[StripeConstants.ORDER_ID_METADATA]),
                appClientId = int.Parse(paymentIntent.Metadata[StripeConstants.CLIENT_ID_METADATA]),
                appInvoiceId = int.Parse(paymentIntent.Metadata[StripeConstants.INVOICE_ID_METADATA]);

            await _servicesManager.InvoicesService.PayAsync(appInvoiceId, appClientId);
            await _servicesManager.OrdersService.SetInProgressAsync(appOrderId, appClientId);
        }

        private async Task DeletePaymentIntentIfInvoiceUpdated(PaymentIntent paymentIntent)
        {
            int appOrderId = int.Parse(paymentIntent.Metadata[StripeConstants.ORDER_ID_METADATA]),
                appClientId = int.Parse(paymentIntent.Metadata[StripeConstants.CLIENT_ID_METADATA]),
                appInvoiceId = int.Parse(paymentIntent.Metadata[StripeConstants.INVOICE_ID_METADATA]);

            decimal appInvoicePrice = decimal.Parse(paymentIntent.Metadata[StripeConstants.INVOICE_PRICE_METADATA]);

            var appOrder = await _servicesManager.OrdersService.GetByIdAsync(appOrderId);
            var appInvoice = await _servicesManager.InvoicesService.GetByIdAsync(appInvoiceId);

            // Cancel payment if the user who submitted it wasn't the order's client
            // or invoice has been updated
            if (appClientId != appOrder.Client.Id
                || appInvoice.Status != InvoiceStatus.WAITING_PAYMENT
                || appInvoice.Price != appInvoicePrice)
            {
                await _servicesManager.StripeService.CancelPaymentIntent(paymentIntent.Id);
            }
        }
    }
}
