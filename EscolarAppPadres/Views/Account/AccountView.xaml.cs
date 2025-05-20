using EscolarAppPadres.ViewModels.Login;

namespace EscolarAppPadres.Views.Account;

public partial class AccountView : ContentPage
{
    private readonly LogoutViewModel _logoutViewModel;

    public AccountView()
	{
		InitializeComponent();
        _logoutViewModel = new LogoutViewModel();
        BindingContext = _logoutViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _logoutViewModel.LoadChildrenData();
    }
}