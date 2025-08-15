using EscolarAppPadres.ViewModels.Payments;
using Syncfusion.Maui.Buttons;

namespace EscolarAppPadres.Views.Service;

public partial class PendingPaymentsView : ContentPage
{
    private readonly PendingPaymentsViewModel _viewModel;

    public PendingPaymentsView()
    {
        InitializeComponent();
        _viewModel = new PendingPaymentsViewModel();
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        // Siempre cargar al aparecer para mantener datos actualizados
        await _viewModel.LoadPendingPaymentsAsync();
    }
}