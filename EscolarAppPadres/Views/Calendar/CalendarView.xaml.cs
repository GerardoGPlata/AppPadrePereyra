using EscolarAppPadres.ViewModels;
using EscolarAppPadres.ViewModels.Calendar;
using Syncfusion.Maui.Scheduler;
using EscolarAppPadres.Models;

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

        // (Opcional) Configuración adicional de Scheduler se puede hacer aquí.
        // Ejemplo de manejar selección más adelante:
        // Scheduler.SelectionChanged += (s,e) => { /* mapear SelectedDate si se desea */ };
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // Forzar la recarga de eventos cuando la p�gina aparece
        await _calendarViewModel.LoadEventsAsync();

        // Aplicar los filtros para actualizar correctamente la vista
        _calendarViewModel.ApplyFiltersCalendarCommand.Execute(null);
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();

        // Cancelar cualquier carga en progreso cuando la p�gina desaparece
        _calendarViewModel.CancelLoading();
    }

    private async void OnSchedulerTapped(object sender, SchedulerTappedEventArgs e)
    {
        try
        {
            // Si se tocó una cita, tomar la primera
            if (e?.Appointments != null && e.Appointments.Count > 0)
            {
                var apt = e.Appointments[0];

                // Con AppointmentMapping, los appointments son EventModel
                if (apt is EventModel em)
                {
                    await _calendarViewModel.NavigateToEventCalendarAsync(em);
                    return;
                }

                // Si no, intentar mapear desde SchedulerAppointment a EventModel mínimo
                if (apt is SchedulerAppointment sa)
                {
                    var mapped = new EventModel
                    {
                        Nombre = sa.Subject,
                        FechaInicio = sa.StartTime.Date,
                        HoraInicio = sa.StartTime.TimeOfDay,
                        FechaFin = sa.EndTime.Date,
                        HoraFin = sa.EndTime.TimeOfDay,
                        ColorIndicador = (sa.Background is SolidColorBrush sb && sb.Color != null) ? sb.Color.ToHex() : "#32CD32"
                    };

                    await _calendarViewModel.NavigateToEventCalendarAsync(mapped);
                    return;
                }
            }

            // Si no hay appointment, actualizar la fecha seleccionada (opcional)
            if (e != null)
            {
                _calendarViewModel.SelectedDate = e.Date;
            }
        }
        catch
        {
            // Swallow: tap no crítico
        }
    }
}