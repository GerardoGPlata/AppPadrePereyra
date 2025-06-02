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

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Forzar la recarga de eventos cuando la página aparece
        await _calendarViewModel.LoadEventsAsync();

        // Aplicar los filtros para actualizar correctamente la vista
        _calendarViewModel.ApplyFiltersCalendarCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Cancelar cualquier carga en progreso cuando la página desaparece
        _calendarViewModel.CancelLoading();
    }
}