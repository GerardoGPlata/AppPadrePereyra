using EscolarAppPadres.ViewModels.Calendar;
using EscolarAppPadres.Interface;
using Microsoft.Maui.Devices;
using EscolarAppPadres.Helpers;
using EscolarAppPadres.Services;

namespace EscolarAppPadres.Views.FiltersPopup;

public partial class PopupFilterReadEventCalendarView
{
	private readonly CalendarViewModel _calendarViewModel;

    private string _eventNombre;
    private string _eventDescripcion;
    private DateTime _eventFechaHoraInico;
    private DateTime _eventFechaHoraFin;

    private static bool isPopupFilterEventCalendarReminderOpen = false;
    private static bool isPopupFilterEventReadCalendarClosed = false;

    public PopupFilterReadEventCalendarView(CalendarViewModel calendarViewModel, string EventNombre, string EventDescripcion, DateTime FechaHoraIncio, DateTime FechaHoraFin)
	{
		InitializeComponent();
		_calendarViewModel = calendarViewModel;

        _eventNombre = EventNombre;
        _eventDescripcion = EventDescripcion;
        _eventFechaHoraInico = FechaHoraIncio;
        _eventFechaHoraFin = FechaHoraFin;

		BindingContext = _calendarViewModel;

        LeerNoticiaLabel.Text = _eventNombre;

        if (_calendarViewModel.RemindNotificationOptions.Any() == true)
        {
            _calendarViewModel.SelectedNotification = _calendarViewModel.RemindNotificationOptions.First();
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (!isPopupFilterEventReadCalendarClosed)
        {
            if (_calendarViewModel.SelectedNotification != null)
            {
                _calendarViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
                _calendarViewModel.SelectedNotification = null!;
            }
        }
        isPopupFilterEventCalendarReminderOpen = false;
        isPopupFilterEventReadCalendarClosed = false;
        PopupManager.IsPopupOpen = false;
        await PopupService.Instance.ClosePopupAsync();
    }

    private async void OnAddToEventCalendarClicked(object sender, EventArgs e)
    {
        try
        {
            string Title = _eventNombre;
            string Description = _eventDescripcion;
            DateTime FechaHoraIncio = _eventFechaHoraInico;
            DateTime FechaHoraFin = _eventFechaHoraFin;

            var calendarService = DependencyService.Get<ICalendarService>();

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                await calendarService.AddEventAsync(null!, Title, Description, FechaHoraIncio, FechaHoraFin);
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                await calendarService.AddEventAsync(null!, Title, Description, FechaHoraIncio, FechaHoraFin);
            }

            if (_calendarViewModel.SelectedNotification != null)
            {
                _calendarViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
                _calendarViewModel.SelectedNotification = null!;
            }

            isPopupFilterEventCalendarReminderOpen = false;
            isPopupFilterEventReadCalendarClosed = true;
            PopupManager.IsPopupOpen = false;
            await PopupService.Instance.ClosePopupAsync();
            await DialogsHelper2.ShowSuccessMessage("Recordatorio programado en el calendario.");
        }
        catch (Exception ex)
        {
            var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
            var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";

            Console.WriteLine("=== ERROR DETECTADO ===");
            Console.WriteLine(errorMessage);
            Console.WriteLine(errorStackTrace);
            Console.WriteLine("=======================");

            await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema. Por favor, intenta nuevamente más tarde.");
        }
    }

    private async void OnClosePopupFilterEventCalendarReminderButtonClicked(object sender, EventArgs e)
    {
        isPopupFilterEventCalendarReminderOpen = false;
        isPopupFilterEventReadCalendarClosed = true;
        PopupManager.IsPopupOpen = false;

        if (_calendarViewModel.SelectedNotification != null)
        {
            _calendarViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
            _calendarViewModel.SelectedNotification = null!;
        }

        await PopupService.Instance.ClosePopupAsync();
    }

    public static async Task ShowPopupFilterEventCalendarReminderIfNotOpen(CalendarViewModel calendarViewModel, string EventNombre, string EventDescripcion, DateTime FechaHoraIncio, DateTime FechaHoraFin)
    {
        if (!isPopupFilterEventCalendarReminderOpen)
        {
            isPopupFilterEventCalendarReminderOpen = true;
            PopupManager.IsPopupOpen = true;
            var PopupFilterReadEventCalendar = new PopupFilterReadEventCalendarView(calendarViewModel, EventNombre, EventDescripcion, FechaHoraIncio, FechaHoraFin);
            await PopupService.Instance.ShowPopupAsync(PopupFilterReadEventCalendar);
        }
    }

    private async void OnSendPopupEventFilterCalendarReminderButtonClicked(object sender, EventArgs e)
    {
        string Title = _eventNombre;
        string Description = _eventDescripcion;

        if (_calendarViewModel.SelectedNotification != null)
        {
            DateTime fechaSeleccionada = _calendarViewModel.SelectedNotification.SelectedDate;
            TimeSpan horaSeleccionada = _calendarViewModel.SelectedNotification.SelectedTime ?? TimeSpan.Zero;

            DateTime fechaYHora = fechaSeleccionada.Date + horaSeleccionada;

            try
            {
                var calendarService = DependencyService.Get<ICalendarService>();

                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    await calendarService.AddEventSilentlyAsync(null!, Title, Description, fechaYHora, fechaYHora.AddHours(1));
                }
                else if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await calendarService.AddEventSilentlyAsync(null!, Title, Description, fechaYHora, fechaYHora.AddHours(1));
                }

                if (_calendarViewModel.SelectedNotification != null)
                {
                    _calendarViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
                    _calendarViewModel.SelectedNotification = null!;
                }

                isPopupFilterEventCalendarReminderOpen = false;
                isPopupFilterEventReadCalendarClosed = true;
                PopupManager.IsPopupOpen = false;
                await PopupService.Instance.ClosePopupAsync();
                await DialogsHelper2.ShowSuccessMessage("Recordatorio programado en el calendario.");
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";

                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");

                await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema. Por favor, intenta nuevamente más tarde.");
            }
        }
        else
        {
            await DialogsHelper2.ShowWarningMessage("Por favor seleccione una notificación");
        }
    }

    private async void OnCancelFilterEventCalendarReminderButtonClicked(object sender, EventArgs e)
    {
        isPopupFilterEventCalendarReminderOpen = false;
        PopupManager.IsPopupOpen = false;

        if (_calendarViewModel.SelectedNotification != null)
        {
            _calendarViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
            _calendarViewModel.SelectedNotification = null!;
        }

        await PopupService.Instance.ClosePopupAsync();
    }
}