using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;
using EscolarAppPadres.Services;

namespace EscolarAppPadres.Views.Service
{
    public partial class PaymentWebViewPage : ContentPage
    {
        public PaymentWebViewPage(string url)
        {
            InitializeComponent();
            PaymentWebView.Source = url;
        }
        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            // Recupera el ID de la transacción
            var transactionId = Preferences.Get("LastTransactionId", null);

            if (!string.IsNullOrEmpty(transactionId))
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var paymentsService = new PaymentsService();
                var response = await paymentsService.GetOpenpayChargeStatusAsync(transactionId, token);

                if (response?.Result == true && response.Data != null && response.Data.Any())
                {
                    var status = response.Data.First().Status;
                    if (status == "completed")
                    {
                        // Lógica para pago completado
                    }
                    else
                    {
                        // Lógica para pago pendiente u otro estado
                    }
                }
                else
                {
                    // Manejo de error o estatus no encontrado
                }
            }
        }
    }
}