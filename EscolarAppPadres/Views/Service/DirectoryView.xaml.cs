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
            await _viewModel.LoadDirectoryAsync(); // o InitializeAsync si agregas más lógica
        }

        private async void OnCallClicked(object sender, EventArgs e)
        {
            if (sender is ImageButton button && button.CommandParameter is string extension && !string.IsNullOrWhiteSpace(extension))
            {
                var phoneNumber = $"tel:8717526090,{extension}";

                try
                {
                    await Launcher.Default.OpenAsync(phoneNumber);
                }
                catch (Exception)
                {
                    await DisplayAlert("Error", "No se pudo abrir la app de teléfono.", "OK");
                }
            }
            else
            {
                await DisplayAlert("Aviso", "Extensión no válida o vacía.", "OK");
            }
        }
    }
}
