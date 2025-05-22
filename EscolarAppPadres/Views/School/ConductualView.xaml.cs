using EscolarAppPadres.ViewModels.StudentConductualReports;
using Syncfusion.Maui.Popup;

namespace EscolarAppPadres.Views.School;

public partial class ConductualView : ContentPage
{
    private readonly StudentConductualReportViewModel _viewModel;
    public ConductualView()
	{
        InitializeComponent();
        _viewModel = new StudentConductualReportViewModel();
        _viewModel.OnReportPopupRequested += ShowPopup;
        BindingContext = _viewModel;
    }
    private void ShowPopup(object sender, string fecha)
    {
        sfPopup.HeaderTitle = $"Reporte de {fecha}";
        sfPopup.IsOpen = true;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitializeAsync();
    }
}