using EscolarAppPadres.ViewModels;
using EscolarAppPadres.ViewModels.News;

namespace EscolarAppPadres.Views.News;

public partial class NewsView : ContentPage
{
    private readonly NewsViewModel _newsViewModel;
    //private readonly EventViewModel _eventViewModel;
    private bool _isNewsLoaded = false;

    public NewsView()
	{
		InitializeComponent();
        _newsViewModel = new NewsViewModel();
        //_eventViewModel = new EventViewModel();
        BindingContext = _newsViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (!_isNewsLoaded)
        {
            await _newsViewModel.LoadNewsAsync();
            _isNewsLoaded = true;
        }
    }
}