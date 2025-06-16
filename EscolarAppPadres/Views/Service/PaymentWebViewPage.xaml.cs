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
            
            Console.WriteLine($"[DEBUG] Navegando a URL de pago: {url}");
        }

        private void OnWebViewNavigated(object sender, WebNavigatedEventArgs e)
        {
            // Ocultar el indicador de carga cuando termine de cargar
            LoadingIndicator.IsVisible = false;
            LoadingIndicator.IsRunning = false;
            
            Console.WriteLine($"[DEBUG] WebView navegó a: {e.Url}");
        }

        protected override async void OnDisappearing()
        {
            base.OnDisappearing();

            // Recupera el ID de la transacción
            var transactionId = Preferences.Get("LastTransactionId", null);

            if (!string.IsNullOrEmpty(transactionId))
            {
                try
                {
                    var token = await SecureStorage.GetAsync("auth_token");
                    var paymentsService = new PaymentsService();
                    var response = await paymentsService.GetOpenpayChargeStatusAsync(transactionId, token);

                    if (response?.Result == true && response.Data != null && response.Data.Any())
                    {
                        var status = response.Data.First().Status;
                        Console.WriteLine($"[DEBUG] Estado del pago: {status}");
                        
                        if (status == "completed")
                        {
                            // Mostrar mensaje de éxito
                            await DisplayAlert("Pago Exitoso", "Su pago ha sido procesado correctamente.", "OK");
                        }
                        else if (status == "cancelled")
                        {
                            // Mostrar mensaje de cancelación
                            await DisplayAlert("Pago Cancelado", "El pago fue cancelado.", "OK");
                        }
                        else
                        {
                            // Mostrar estado actual
                            await DisplayAlert("Estado del Pago", $"Estado actual: {status}", "OK");
                        }
                    }
                    else
                    {
                        Console.WriteLine("[DEBUG] No se pudo obtener el estado del pago");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERROR] Error verificando estado del pago: {ex.Message}");
                }
            }
        }
    }
}