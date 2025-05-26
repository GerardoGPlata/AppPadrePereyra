using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.Communication;

namespace EscolarAppPadres.Views.Service
{
    public partial class DirectoryView : ContentPage
    {
        public DirectoryView()
        {
            InitializeComponent();
        }

        private async void OnCallClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is string extension)
            {
                // Puedes personalizar el número base si se necesita
                var phoneNumber = $"tel:8717526090,{extension}";

                try
                {
                    await Launcher.Default.OpenAsync(phoneNumber);
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", "No se pudo abrir la app de teléfono.", "OK");
                }
            }
        }
    }
}
