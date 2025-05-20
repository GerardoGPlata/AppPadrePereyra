using EscolarAppPadres.Models;
using EscolarAppPadres.ViewModels.Calendar;

namespace EscolarAppPadres.Views.Calendar;

public partial class EventCalendar : ContentPage
{
    private readonly CalendarViewModel _calendarViewModel;
    private readonly EventModel _eventModel;

    public EventCalendar(CalendarViewModel calendarViewModel, EventModel eventModel)
	{
		InitializeComponent();
        _calendarViewModel = calendarViewModel;
        _eventModel = eventModel;
        BindingContext = _eventModel;

        Title = _eventModel.TipoEvento;
    }

    public void OpenFilterEventCalendarReminderCommand(object sender, EventArgs e)
    {
        _calendarViewModel.OpenFilterEventCalendarReminderCommand.Execute(_eventModel);
    }
}