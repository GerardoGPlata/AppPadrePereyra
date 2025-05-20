using EscolarAppPadres.ViewModels.News;
using EscolarAppPadres.Helpers;
using EscolarAppPadres.Services;

namespace EscolarAppPadres.Views.FiltersPopup;

public partial class PopupFilterNewsView
{
    private readonly NewsViewModel _newsViewModel;
    private static bool isPopupFilterNewsOpen = false;
    private static bool isPopupFilterNewsClosed = false;

    private bool _originalFiltroLeidos;
    private bool _originalFiltroNoLeidos;

    public PopupFilterNewsView(NewsViewModel newsViewModel)
	{
		InitializeComponent();
        _newsViewModel = newsViewModel;

        _originalFiltroLeidos = _newsViewModel.FiltroLeidos;
        _originalFiltroNoLeidos = _newsViewModel.FiltroNoLeidos;

        BindingContext = _newsViewModel;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (!isPopupFilterNewsClosed)
        {
            _newsViewModel.FiltroLeidos = _originalFiltroLeidos;
            _newsViewModel.FiltroNoLeidos = _originalFiltroNoLeidos;
        }

        isPopupFilterNewsOpen = false;
        isPopupFilterNewsClosed = false;
        PopupManager.IsPopupOpen = false;
        await PopupService.Instance.ClosePopupAsync();
    }

    private async void OnClosePopupFilterNewsButtonClicked(object sender, EventArgs e)
    {
        isPopupFilterNewsOpen = false;
        isPopupFilterNewsClosed = true;
        PopupManager.IsPopupOpen = false;
        _newsViewModel.FiltroLeidos = _originalFiltroLeidos;
        _newsViewModel.FiltroNoLeidos = _originalFiltroNoLeidos;
        await PopupService.Instance.ClosePopupAsync();
    }

    public static async Task ShowPopupFilterNewsIfNotOpen(NewsViewModel newsViewModel)
    {
        if (!isPopupFilterNewsOpen)
        {
            isPopupFilterNewsOpen = true;
            PopupManager.IsPopupOpen = true;
            var PopupFilterNews = new PopupFilterNewsView(newsViewModel);
            await PopupService.Instance.ShowPopupAsync(PopupFilterNews);
        }
    }

    private async void OnSendReadPopupFilterNewsButtonClicked(object sender, EventArgs e)
    {
        isPopupFilterNewsOpen = false;
        isPopupFilterNewsClosed = true;
        PopupManager.IsPopupOpen = false;
        _newsViewModel.ApplyFiltersNewsCommand.Execute(null);
        await PopupService.Instance.ClosePopupAsync();
    }

    private void OnRestorePopupFilterNewsButtonClicked(object sender, EventArgs e)
    {
        _newsViewModel.FiltroNoLeidos = true;
        _newsViewModel.FiltroLeidos = false;
    }
}