using EscolarAppPadres.ViewModels.Payments;

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
        await _viewModel.LoadPendingPaymentsAsync();
    }
}