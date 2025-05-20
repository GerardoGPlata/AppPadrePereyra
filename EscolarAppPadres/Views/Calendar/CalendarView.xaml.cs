using EscolarAppPadres.ViewModels;
using EscolarAppPadres.ViewModels.Calendar;

namespace EscolarAppPadres.Views.Calendar;

public partial class CalendarView : ContentPage
{
    private readonly CalendarViewModel _calendarViewModel;
    private readonly EventViewModel _eventViewModel;

    public CalendarView()
	{
		InitializeComponent();
        _calendarViewModel = new CalendarViewModel();
        _eventViewModel = new EventViewModel();

        BindingContext = _calendarViewModel;

        Calendar.Culture = _calendarViewModel.Culture;
        
    }

    //protected override async void OnAppearing()
    //{
    //    base.OnAppearing();

    //    if (BindingContext is CalendarViewModel viewModel)
    //    {
    //        await viewModel.InitializeAsync();
    //    }
    //}

}