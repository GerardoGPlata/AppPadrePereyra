using EscolarAppPadres.ViewModels.Academy;

namespace EscolarAppPadres.Views.Service;

public partial class AcademyView : ContentPage
{
    private AcademyViewModel ViewModel => BindingContext as AcademyViewModel;

    public AcademyView()
    {
        InitializeComponent();
        BindingContext = new AcademyViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.InitializeAsync();
    }

    private void OnAgregarClicked(object sender, EventArgs e)
    {
        sfPopup.IsOpen = true;
    }

    private void OnCancelarClicked(object sender, EventArgs e)
    {
        sfPopup.IsOpen = false;
    }

    private async void OnPagarClicked(object sender, EventArgs e)
    {
        // Aquí puedes llamar un método del ViewModel para procesar la inscripción/pago
        //await ViewModel.ProcesarPagoAsync();

        // Cerrar popup después del pago
        sfPopup.IsOpen = false;
    }
}