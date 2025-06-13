using Microsoft.Maui.Controls;

namespace EscolarAppPadres.Views.Service
{
    public partial class PaymentWebViewPage : ContentPage
    {
        public PaymentWebViewPage(string url)
        {
            InitializeComponent();
            PaymentWebView.Source = url;
        }
    }
}