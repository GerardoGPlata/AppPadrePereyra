using EscolarAppPadres.ViewModels.SchoolDirectories;
using Microsoft.Maui.Controls;
using Microsoft.Maui.ApplicationModel.Communication;
using System;

namespace EscolarAppPadres.Views.Service
{
    public partial class DirectoryView : ContentPage
    {
        private readonly SchoolDirectoryViewModel _viewModel;

        public DirectoryView()
        {
            InitializeComponent();
            _viewModel = new SchoolDirectoryViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadDirectoryAsync(); // o InitializeAsync si agregas m�s l�gica
        }

        private async void OnCallClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is string extension && !string.IsNullOrWhiteSpace(extension))
            {
                try
                {
                    if (DeviceInfo.Platform == DevicePlatform.Android)
                    {
                        // En Android usamos Launcher con extensión
                        var phoneNumber = $"tel:8717526090,{extension}";
                        await Launcher.Default.OpenAsync(phoneNumber);
                    }
                    else if (DeviceInfo.Platform == DevicePlatform.iOS)
                    {
                        // En iOS verificamos si es simulador
                        if (DeviceInfo.DeviceType == DeviceType.Virtual)
                        {
                            await DisplayAlert("Simulador", "Las llamadas no están disponibles en el simulador de iOS.", "OK");
                            return;
                        }

                        // Usamos PhoneDialer que es más específico para llamadas
                        if (PhoneDialer.Default.IsSupported)
                        {
                            PhoneDialer.Default.Open("8717526090");
                            await DisplayAlert("Extensión", $"Cuando se abra la app de teléfono, marca la extensión: {extension}", "OK");
                        }
                        else
                        {
                            await DisplayAlert("Error", "Las llamadas telefónicas no están disponibles en este dispositivo.", "OK");
                        }
                    }
                    else
                    {
                        // Para otras plataformas
                        var phoneNumber = "tel:8717526090";
                        await Launcher.Default.OpenAsync(phoneNumber);
                        await DisplayAlert("Extensión", $"Cuando se abra la app de teléfono, marca la extensión: {extension}", "OK");
                    }
                }
                catch (Exception ex)
                {
                    await DisplayAlert("Error", $"No se pudo abrir la app de teléfono: {ex.Message}", "OK");
                }
            }
            else
            {
                await DisplayAlert("Aviso", "Extensión no válida o vacía.", "OK");
            }
        }
    }
}
