using EscolarAppPadres.ViewModels.News;
using EscolarAppPadres.Interface;
using Microsoft.Maui.Devices;
using EscolarAppPadres.Helpers;
using EscolarAppPadres.Services;

namespace EscolarAppPadres.Views.FiltersPopup;

public partial class PopupFilterReadCalendarView
{
    private readonly NewsViewModel _newsViewModel;

    private string _url;
    private string _noticiaTitulo;
    private string _noticiaMensaje;

    private static bool isPopupFilterCalendarReminderOpen = false;
    private static bool isPopupFilterReadCalendarClosed = false;

    public PopupFilterReadCalendarView(NewsViewModel newsViewModel, string Url, string noticiaTitulo, string NoticiaMensaje)
	{
		InitializeComponent();
        _newsViewModel = newsViewModel;

        _url = Url;
        _noticiaTitulo = noticiaTitulo;
        _noticiaMensaje = NoticiaMensaje;

        BindingContext = _newsViewModel;

        if (_newsViewModel.RemindNotificationOptions.Any())
        {
            _newsViewModel.SelectedNotification = _newsViewModel.RemindNotificationOptions.First();
        }
    }

    protected override async void OnDisappearing()
    {
        base.OnDisappearing();

        if (!isPopupFilterReadCalendarClosed)
        {
            if (_newsViewModel.SelectedNotification != null)
            {
                _newsViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
                _newsViewModel.SelectedNotification = null!;
            }
        }
        isPopupFilterCalendarReminderOpen = false;
        isPopupFilterReadCalendarClosed = false;
        PopupManager.IsPopupOpen = false;
        await PopupService.Instance.ClosePopupAsync();
    }

    private async void OnAddToCalendarClicked(object sender, EventArgs e)
    {
        try
        {
            string Url = _url;
            string Title = _noticiaTitulo;
            string Description = _noticiaMensaje;
            DateTime Today = DateTime.Today;
            DateTime FechaInicio = Today.AddHours(17);
            DateTime FechaFin = Today.AddHours(22);

            var calendarService = DependencyService.Get<ICalendarService>();

            if (DeviceInfo.Platform == DevicePlatform.Android)
            {
                await calendarService.AddEventAsync(Url, Title, Description, FechaInicio, FechaFin);
            }
            else if (DeviceInfo.Platform == DevicePlatform.iOS)
            {
                await calendarService.AddEventAsync(Url, Title, Description, FechaInicio, FechaFin);
            }

            if (_newsViewModel.SelectedNotification != null)
            {
                _newsViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
                _newsViewModel.SelectedNotification = null!;
            }

            isPopupFilterCalendarReminderOpen = false;
            isPopupFilterReadCalendarClosed = true;
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

    private async void OnClosePopupFilterCalendarReminderNewsButtonClicked(object sender, EventArgs e)
    {
        isPopupFilterCalendarReminderOpen = false;
        isPopupFilterReadCalendarClosed = true;
        PopupManager.IsPopupOpen = false;

        if (_newsViewModel.SelectedNotification != null)
        {
            _newsViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
            _newsViewModel.SelectedNotification = null!;
        }

        await PopupService.Instance.ClosePopupAsync();
    }

    public static async Task ShowPopupFilterCalendarReminderNewsIfNotOpen(NewsViewModel newsViewModel, string Url, string noticiaTitulo, string NoticiaMensaje)
    {
        if (!isPopupFilterCalendarReminderOpen)
        {
            isPopupFilterCalendarReminderOpen = true;
            PopupManager.IsPopupOpen = true;
            var PopupFilterReadCalendar = new PopupFilterReadCalendarView(newsViewModel, Url, noticiaTitulo, NoticiaMensaje);
            await PopupService.Instance.ShowPopupAsync(PopupFilterReadCalendar);
        }
    }

    private async void OnSendPopupFilterCalendarReminderNewsButtonClicked(object sender, EventArgs e)
    {
        string Url = _url;
        string Title = _noticiaTitulo;
        string Description = _noticiaMensaje;

        if (_newsViewModel.SelectedNotification != null)
        {
            DateTime fechaSeleccionada = _newsViewModel.SelectedNotification.SelectedDate;
            TimeSpan horaSeleccionada = _newsViewModel.SelectedNotification.SelectedTime ?? TimeSpan.Zero;

            DateTime fechaYHora = fechaSeleccionada.Date + horaSeleccionada;

            try
            {
                var calendarService = DependencyService.Get<ICalendarService>();

                if (DeviceInfo.Platform == DevicePlatform.Android)
                {
                    await calendarService.AddEventSilentlyAsync(Url, Title, Description, fechaYHora, fechaYHora.AddHours(1));
                }
                else if (DeviceInfo.Platform == DevicePlatform.iOS)
                {
                    await calendarService.AddEventSilentlyAsync(Url, Title, Description, fechaYHora, fechaYHora.AddHours(1));
                }

                if (_newsViewModel.SelectedNotification != null)
                {
                    _newsViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
                    _newsViewModel.SelectedNotification = null!;
                }

                isPopupFilterCalendarReminderOpen = false;
                isPopupFilterReadCalendarClosed = true;
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

    private async void OnCancelFilterCalendarReminderNewsButtonClicked(object sender, EventArgs e)
    {
        isPopupFilterCalendarReminderOpen = false;
        PopupManager.IsPopupOpen = false;

        if (_newsViewModel.SelectedNotification != null)
        {
            _newsViewModel.SelectedNotification.SelectedTime = TimeSpan.Zero;
            _newsViewModel.SelectedNotification = null!;
        }

        await PopupService.Instance.ClosePopupAsync();
    }
}