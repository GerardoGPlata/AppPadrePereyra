using Mopups.Services;
using EscolarAppPadres.ViewModels.Calendar;
using EscolarAppPadres.Helpers;
using EscolarAppPadres.Services;

namespace EscolarAppPadres.Views.FiltersPopup;

public partial class PopupFilterCalendarView
{
    private readonly CalendarViewModel _calendarViewModel;
    private static bool isPopupFilterCalendarOpen = false;
    private static bool isPopupFilterCalendarClosed = false;

    private bool lastCheckedValue = false;

    public PopupFilterCalendarView(CalendarViewModel calendarViewModel)
	{
		InitializeComponent();
        _calendarViewModel = calendarViewModel;
        BindingContext = _calendarViewModel;
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (!isPopupFilterCalendarClosed)
        {

        }
        isPopupFilterCalendarOpen = false;
        isPopupFilterCalendarClosed = false;
        PopupManager.IsPopupOpen = false;
        await PopupService.Instance.ClosePopupAsync();
    }

    private async void OnClosePopupFilterCalendarButtonClicked(object sender, EventArgs e)
    {
        isPopupFilterCalendarOpen = false;
        isPopupFilterCalendarClosed = true;
        PopupManager.IsPopupOpen = false;
        await PopupService.Instance.ClosePopupAsync();
    }

    public static async Task ShowPopupFilterCalendarIfNotOpen(CalendarViewModel calendarViewModel)
    {
        if (!isPopupFilterCalendarOpen)
        {
            PopupManager.IsPopupOpen = true;
            isPopupFilterCalendarOpen = true;
            var PopupFilterCalendar = new PopupFilterCalendarView(calendarViewModel);
            await PopupService.Instance.ShowPopupAsync(PopupFilterCalendar);
        }
    }

    private async void OnSendPopupFilterCalendarButtonClicked(object sender, EventArgs e)
    {
        isPopupFilterCalendarOpen = false;
        isPopupFilterCalendarClosed = true;
        PopupManager.IsPopupOpen = false;
        _calendarViewModel.ApplyFiltersCalendarCommand.Execute(null);
        await PopupService.Instance.ClosePopupAsync();
    }

    private void OnRestorePopupFilterCalendarButtonClicked(object sender, EventArgs e)
    {
        _calendarViewModel.IsAgendaSelected = true;
        _calendarViewModel.IsEventoChecked = true;
        _calendarViewModel.IsDiaFestivoChecked = true;
        _calendarViewModel.IsExamenesChecked = true;
        _calendarViewModel.IsTareasChecked = true;
        _calendarViewModel.IsBecasChecked = false;
        _calendarViewModel.IsAdmisionesChecked = false;
    }
}