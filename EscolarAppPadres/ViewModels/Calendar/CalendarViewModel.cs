using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Windows.Input;
using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using EscolarAppPadres.Views.Calendar;
using EscolarAppPadres.Views.FiltersPopup;
using Plugin.Maui.Calendar.Enums;
using Plugin.Maui.Calendar.Models;
using Syncfusion.Maui.Scheduler;

namespace EscolarAppPadres.ViewModels.Calendar
{
    public class CalendarViewModel : INotifyPropertyChanged
    {
        private WeekLayout _selectedCalendarLayout;
        public CultureInfo Culture => new CultureInfo("es-MX");

        private ObservableCollection<RemindNotificationOptions> _remindNotificationOptions;

        private RemindNotificationOptions _selectedNotification;

        public EventCollection _Events { get; set; }

        // NUEVO: Colección de citas para Syncfusion Scheduler
        private ObservableCollection<SchedulerAppointment> _schedulerEvents = new();
        private ObservableCollection<EventModel> _schedulerEventModels = new();
        private DateTime _displayDate = DateTime.Today;

        private List<EventModel> _agendaEvents;

        private bool _isEventoChecked;
        private bool _isDiaFestivoChecked;
        private bool _isExamenesChecked;
        private bool _isTareasChecked;
        private bool _isBecasChecked;
        private bool _isAdmisionesChecked;

        private DateTime? _selectedDate;
        private bool _isAgendaSelected;
        private bool _isSemanaSelected;
        private bool _isMesSelected;

        private bool _isAgendaResult;
        private bool _selectedDateResult;

        private bool _isAgendaVisible;
        private bool _isCalendarVisible = true;
        private bool _footerArrowVisible;
        private bool _isFilterPopupOpen;
        private SchedulerView _schedulerViewMode = SchedulerView.Month;

        private readonly EventService _eventService;
        private ObservableCollection<Event> _eventos;

        public ObservableCollection<Event> Eventos
        {
            get => _eventos;
            set
            {
                _eventos = value;
                OnPropertyChanged(nameof(Eventos));
            }
        }

        public CalendarViewModel()
        {
            _eventService = new EventService();
            _selectedCalendarLayout = WeekLayout.Month;
            _schedulerViewMode = SchedulerView.Month;
            // Establecer selección inicial a Mes usando el setter para notificar bindings
            IsMesSelected = true;

            // Inicialización rápida de propiedades
            RemindNotificationOptions = new ObservableCollection<RemindNotificationOptions>
            {
                new RemindNotificationOptions { Icon = "sunny_outline.svg", Label = "En la tarde", CustomDate = DateTime.Today, CustomTime = new TimeSpan(16,0,0), CornerRadius = new CornerRadius(30,30,0,0) },
                new RemindNotificationOptions { Icon = "moon_outline.svg", Label = "En la noche", CustomDate = DateTime.Today, CustomTime = new TimeSpan(19,0,0), CornerRadius = new CornerRadius(0,0,0,0) },
                new RemindNotificationOptions { Icon = "alarm_outline.svg", Label = "Hora personalizada", CornerRadius = new CornerRadius (0,0,30,30), IsCustomTime = true }
            };

            SelectedNotification = RemindNotificationOptions.FirstOrDefault();

            _Events = new EventCollection();

            // Iniciar carga asíncrona sin esperar
            Task.Run(async () => await LoadEventsAsync());
        }

        private CancellationTokenSource _loadEventsCts;

        public async Task LoadEventsAsync()
        {
            _loadEventsCts?.Cancel();
            _loadEventsCts = new CancellationTokenSource();
            var token = _loadEventsCts.Token;

            try
            {
                var authToken = await SecureStorage.GetAsync("auth_token");
                var profileId = await SecureStorage.GetAsync("Tipo_Usuario_Id");

                if (string.IsNullOrEmpty(authToken) || !long.TryParse(profileId, out var idPerfil))
                {
                    await DialogsHelper2.ShowErrorMessage("Información de sesión inválida.");
                    return;
                }

                // Usar un ID de alumno genérico (dummy) para cumplir con la firma actual del servicio
                var studentIds = new long[] { 1000 };

                // Obtener datos
                var response = await _eventService.GetEventsAsync(idPerfil, studentIds, authToken);

                if (token.IsCancellationRequested) return;

                if (response?.Data == null || !response.Data.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                        await DialogsHelper2.ShowErrorMessage("No se encontraron eventos."));
                    return;
                }

                var eventosOrdenados = response.Data.OrderBy(e => e.DateInicio).ToList();
                var tempEventCollection = new EventCollection();

                foreach (var evento in eventosOrdenados)
                {
                    if (token.IsCancellationRequested) return;

                    var eventModel = ConvertToEventModel(evento);
                    var eventDate = evento.DateInicio.Date;

                    if (tempEventCollection.ContainsKey(eventDate))
                    {
                        ((List<EventModel>)tempEventCollection[eventDate]).Add(eventModel);
                    }
                    else
                    {
                        tempEventCollection[eventDate] = new List<EventModel> { eventModel };
                    }
                }

                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Eventos = new ObservableCollection<Event>(eventosOrdenados);
                    _Events = tempEventCollection;
                    Events = _Events; // Forzar la actualización del binding
                    // Mostrar siempre el Scheduler por defecto
                    IsCalendarVisible = true;
                    IsAgendaVisible = false;

                    // Poblar las citas del Scheduler (limpio y vuelvo a crear)
                    _schedulerEvents.Clear();
                    _schedulerEventModels.Clear();
                    foreach (var ev in Eventos)
                    {
                        var colorHex = ev.Color ?? "#32CD32";
                        Color bg;
                        try { bg = Color.FromArgb(colorHex); } catch { bg = Colors.LightGreen; }

                        bool hasEnd = ev.DateFin.HasValue;
                        bool isMultiDay = hasEnd && ev.DateFin!.Value.Date > ev.DateInicio.Date;
                        bool hasExplicitStartTime = ev.DateInicio.TimeOfDay != TimeSpan.Zero;
                        bool hasExplicitEndTime = hasEnd && ev.DateFin!.Value.TimeOfDay != TimeSpan.Zero;

                        if (isMultiDay)
                        {
                            // Crear una cita por día del rango, 8:00–9:00 por defecto salvo donde haya hora explícita
                            var fromDate = ev.DateInicio.Date;
                            var toDate = ev.DateFin!.Value.Date;
                            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
                            {
                                DateTime dStart = d.AddHours(8);
                                DateTime dEnd = dStart.AddHours(1);

                                if (d == ev.DateInicio.Date && hasExplicitStartTime)
                                {
                                    dStart = ev.DateInicio;
                                    dEnd = hasExplicitEndTime && ev.DateFin!.Value.Date == d
                                        ? ev.DateFin!.Value
                                        : dStart.AddHours(1);
                                }
                                else if (d == ev.DateFin!.Value.Date && hasExplicitEndTime)
                                {
                                    // Último día con hora fin explícita
                                    dEnd = ev.DateFin!.Value;
                                    // Si no hay hora inicio explícita para el último día, 8:00 por defecto
                                    if (dStart >= dEnd)
                                        dStart = dEnd.AddHours(-1);
                                }

                                if (dEnd <= dStart)
                                {
                                    dEnd = dStart.AddHours(1);
                                }

                                _schedulerEvents.Add(new SchedulerAppointment
                                {
                                    StartTime = dStart,
                                    EndTime = dEnd,
                                    Subject = ev.Nombre ?? ev.TipoEvento ?? "Evento",
                                    Background = new SolidColorBrush(bg),
                                    IsAllDay = false
                                });
                            }
                        }
                        else
                        {
                            // Evento de un día: usar horas explícitas si existen, si no 08:00–09:00
                            DateTime start = hasExplicitStartTime ? ev.DateInicio : ev.DateInicio.Date.AddHours(8);
                            DateTime end = hasEnd
                                ? (hasExplicitEndTime ? ev.DateFin!.Value : start.AddHours(1))
                                : start.AddHours(1);

                            if (end <= start)
                                end = start.AddHours(1);

                            _schedulerEvents.Add(new SchedulerAppointment
                            {
                                StartTime = start,
                                EndTime = end,
                                Subject = ev.Nombre ?? ev.TipoEvento ?? "Evento",
                                Background = new SolidColorBrush(bg),
                                IsAllDay = false
                            });
                        }
                        // Construir también la fuente para AppointmentMapping como lista plana por día

                        if (isMultiDay)
                        {
                            var fromDate = ev.DateInicio.Date;
                            var toDate = ev.DateFin!.Value.Date;
                            for (var d = fromDate; d <= toDate; d = d.AddDays(1))
                            {
                                DateTime dStart = d.AddHours(8);
                                DateTime dEnd = dStart.AddHours(1);

                                if (d == ev.DateInicio.Date && hasExplicitStartTime)
                                {
                                    dStart = ev.DateInicio;
                                    dEnd = hasExplicitEndTime && ev.DateFin!.Value.Date == d
                                        ? ev.DateFin!.Value
                                        : dStart.AddHours(1);
                                }
                                else if (d == ev.DateFin!.Value.Date && hasExplicitEndTime)
                                {
                                    dEnd = ev.DateFin!.Value;
                                    if (dStart >= dEnd)
                                        dStart = dEnd.AddHours(-1);
                                }

                                if (dEnd <= dStart)
                                    dEnd = dStart.AddHours(1);

                                _schedulerEventModels.Add(new EventModel
                                {
                                    TipoEvento = ev.TipoEvento,
                                    Nivel = ev.Nivel,
                                    Nombre = ev.Nombre,
                                    Descripcion = ev.Descripcion,
                                    FechaInicio = dStart.Date,
                                    HoraInicio = dStart.TimeOfDay,
                                    FechaFin = dEnd.Date,
                                    HoraFin = dEnd.TimeOfDay,
                                    Imagen = ev.Imagen ?? "icono_logo.png",
                                    ColorIndicador = ev.Color ?? "#32CD32"
                                });
                            }
                        }
                        else
                        {
                            DateTime start = hasExplicitStartTime ? ev.DateInicio : ev.DateInicio.Date.AddHours(8);
                            DateTime end = hasEnd
                                ? (hasExplicitEndTime ? ev.DateFin!.Value : start.AddHours(1))
                                : start.AddHours(1);

                            if (end <= start)
                                end = start.AddHours(1);

                            _schedulerEventModels.Add(new EventModel
                            {
                                TipoEvento = ev.TipoEvento,
                                Nivel = ev.Nivel,
                                Nombre = ev.Nombre,
                                Descripcion = ev.Descripcion,
                                FechaInicio = start.Date,
                                HoraInicio = start.TimeOfDay,
                                FechaFin = end.Date,
                                HoraFin = end.TimeOfDay,
                                Imagen = ev.Imagen ?? "icono_logo.png",
                                ColorIndicador = ev.Color ?? "#32CD32"
                            });
                        }
                    }
                    // Establecer fecha de visualización a hoy o primer evento
                    _displayDate = Eventos.FirstOrDefault()?.DateInicio.Date ?? DateTime.Today;

                    OnPropertyChanged(nameof(Eventos));
                    OnPropertyChanged(nameof(Events));
                    OnPropertyChanged(nameof(SchedulerEvents));
                    OnPropertyChanged(nameof(DisplayDate));
                    OnPropertyChanged(nameof(SchedulerEventModels));

                    // Asegurarse de actualizar la vista apropiadamente
                    ApplyFiltersCalendar();
                });
            }
            catch (OperationCanceledException)
            {
                // Carga cancelada
            }
            catch (Exception ex)
            {
                if (!token.IsCancellationRequested)
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        Console.WriteLine($"Error al cargar eventos: {ex.Message}");
                        await DialogsHelper2.ShowErrorMessage($"Error al cargar eventos: {ex.Message}");
                    });
                }
            }
        }


        // Método para cancelar la carga si es necesario (por ejemplo, al navegar fuera de la página)
        public void CancelLoading()
        {
            _loadEventsCts?.Cancel();
        }
        private EventModel ConvertToEventModel(Event evento)
        {
            // Determinar hora de inicio: si no viene hora (00:00), usar 08:00
            var start = evento.DateInicio;
            var hasStartTime = start.TimeOfDay != TimeSpan.Zero;
            if (!hasStartTime)
            {
                start = start.Date.AddHours(8);
            }

            // Determinar hora de fin: respetar DateFin si viene con hora; si no, 1 hora después del inicio
            DateTime end;
            if (evento.DateFin.HasValue)
            {
                end = evento.DateFin.Value;

                // Si no trae hora y es el mismo día, asumir 1h después del inicio
                if (end.TimeOfDay == TimeSpan.Zero && end.Date == start.Date)
                {
                    end = start.AddHours(1);
                }

                // Asegurar que fin sea posterior al inicio
                if (end <= start)
                {
                    end = start.AddHours(1);
                }
            }
            else
            {
                end = start.AddHours(1);
            }

            return new EventModel
            {
                TipoEvento = evento.TipoEvento,
                Nivel = evento.Nivel,
                Nombre = evento.Nombre,
                Descripcion = evento.Descripcion,
                // Guardamos la fecha (sin hora) en las propiedades de fecha y la hora en las de hora
                FechaInicio = start.Date,
                FechaFin = end.Date,
                HoraInicio = start.TimeOfDay,
                HoraFin = end.TimeOfDay,
                Imagen = evento.Imagen ?? "icono_logo.png",
                ColorIndicador = evento.Color ?? "#32CD32"
            };
        }
        public ObservableCollection<RemindNotificationOptions> RemindNotificationOptions
        {
            get => _remindNotificationOptions;
            set
            {
                _remindNotificationOptions = value;
                OnPropertyChanged(nameof(RemindNotificationOptions));
            }
        }

        public RemindNotificationOptions SelectedNotification
        {
            get => _selectedNotification;
            set
            {
                if (_selectedNotification != value)
                {
                    _selectedNotification = value;

                    if (_selectedNotification != null)
                    {
                        _selectedNotification.SelectedDate = DateTime.Now;

                        if (_selectedNotification.IsCustomTime)
                        {
                            _selectedNotification.SelectedTime = new TimeSpan(0, 0, 0);
                        }
                        else
                        {
                            _selectedNotification.SelectedDate = _selectedNotification.CustomDate;
                            _selectedNotification.SelectedTime = _selectedNotification.CustomTime;
                        }
                    }

                    OnPropertyChanged(nameof(SelectedNotification));
                }
            }
        }

        public EventCollection Events
        {
            get => _Events;
            set
            {
                if (_Events != value)
                {
                    _Events = value;
                    OnPropertyChanged(nameof(Events));
                }
            }
        }

        // Exponer colección para Scheduler
        public ObservableCollection<SchedulerAppointment> SchedulerEvents
        {
            get => _schedulerEvents;
            set
            {
                if (_schedulerEvents != value)
                {
                    _schedulerEvents = value;
                    OnPropertyChanged(nameof(SchedulerEvents));
                }
            }
        }

        // Fuente alternativa con AppointmentMapping
        public ObservableCollection<EventModel> SchedulerEventModels
        {
            get => _schedulerEventModels;
            set
            {
                if (_schedulerEventModels != value)
                {
                    _schedulerEventModels = value;
                    OnPropertyChanged(nameof(SchedulerEventModels));
                }
            }
        }

        public DateTime DisplayDate
        {
            get => _displayDate;
            set
            {
                if (_displayDate != value)
                {
                    _displayDate = value;
                    OnPropertyChanged(nameof(DisplayDate));
                }
            }
        }

        public DateTime? SelectedDate
        {
            get => _selectedDate;
            set
            {
                if (_selectedDate != value)
                {
                    _selectedDate = value;
                    OnPropertyChanged(nameof(SelectedDate));
                    UpdateFooterArrowVisibility();
                }
            }
        }

        public List<EventModel> AgendaEvents
        {
            get => _agendaEvents;
            set
            {
                if (_agendaEvents != value)
                {
                    _agendaEvents = value;
                    OnPropertyChanged(nameof(AgendaEvents));
                }
            }
        }

        public WeekLayout SelectedCalendarLayout
        {
            get => _selectedCalendarLayout;
            set
            {
                if (_selectedCalendarLayout != value)
                {
                    _selectedCalendarLayout = value;
                    OnPropertyChanged(nameof(SelectedCalendarLayout));
                }
            }
        }

        public bool IsAgendaSelected
        {
            get => _isAgendaSelected;
            set
            {
                if (_isAgendaSelected != value)
                {
                    _isAgendaSelected = value;
                    OnPropertyChanged(nameof(IsAgendaSelected));
                    if (value)
                    {
                        // Asegurar exclusividad
                        if (_isSemanaSelected) { _isSemanaSelected = false; OnPropertyChanged(nameof(IsSemanaSelected)); }
                        if (_isMesSelected) { _isMesSelected = false; OnPropertyChanged(nameof(IsMesSelected)); }
                        // Cambiar vista del Scheduler
                        SchedulerViewMode = SchedulerView.Agenda;
                        IsCalendarVisible = true;
                        IsAgendaVisible = false;
                        // Recargar Scheduler al cambiar tipo
                        ApplyFiltersCalendar();
                        OnPropertyChanged(nameof(SchedulerEventModels));
                    }
                }
            }
        }

        public bool IsSemanaSelected
        {
            get => _isSemanaSelected;
            set
            {
                if (_isSemanaSelected != value)
                {
                    _isSemanaSelected = value;
                    OnPropertyChanged(nameof(IsSemanaSelected));
                    if (value)
                    {
                        if (_isAgendaSelected) { _isAgendaSelected = false; OnPropertyChanged(nameof(IsAgendaSelected)); }
                        if (_isMesSelected) { _isMesSelected = false; OnPropertyChanged(nameof(IsMesSelected)); }
                        SelectedCalendarLayout = WeekLayout.Week;
                        SchedulerViewMode = SchedulerView.Week;
                        IsCalendarVisible = true;
                        IsAgendaVisible = false;
                        // Recargar Scheduler al cambiar tipo
                        ApplyFiltersCalendar();
                        OnPropertyChanged(nameof(SchedulerEventModels));
                    }
                }
            }
        }

        public bool IsMesSelected
        {
            get => _isMesSelected;
            set
            {
                if (_isMesSelected != value)
                {
                    _isMesSelected = value;
                    OnPropertyChanged(nameof(IsMesSelected));
                    if (value)
                    {
                        if (_isAgendaSelected) { _isAgendaSelected = false; OnPropertyChanged(nameof(IsAgendaSelected)); }
                        if (_isSemanaSelected) { _isSemanaSelected = false; OnPropertyChanged(nameof(IsSemanaSelected)); }
                        SelectedCalendarLayout = WeekLayout.Month;
                        SchedulerViewMode = SchedulerView.Month;
                        IsCalendarVisible = true;
                        IsAgendaVisible = false;
                        // Recargar Scheduler al cambiar tipo
                        ApplyFiltersCalendar();
                        OnPropertyChanged(nameof(SchedulerEventModels));
                    }
                }
            }
        }

        public bool IsEventoChecked
        {
            get => _isEventoChecked;
            set
            {
                if (_isEventoChecked != value)
                {
                    _isEventoChecked = value;
                    OnPropertyChanged(nameof(IsEventoChecked));
                }
            }
        }

        public bool IsDiaFestivoChecked
        {
            get => _isDiaFestivoChecked;
            set
            {
                if (_isDiaFestivoChecked != value)
                {
                    _isDiaFestivoChecked = value;
                    OnPropertyChanged(nameof(IsDiaFestivoChecked));
                }
            }
        }

        public bool IsExamenesChecked
        {
            get => _isExamenesChecked;
            set
            {
                if (_isExamenesChecked != value)
                {
                    _isExamenesChecked = value;
                    OnPropertyChanged(nameof(IsExamenesChecked));
                }
            }
        }

        public bool IsTareasChecked
        {
            get => _isTareasChecked;
            set
            {
                if (_isTareasChecked != value)
                {
                    _isTareasChecked = value;
                    OnPropertyChanged(nameof(IsTareasChecked));
                }
            }
        }

        public bool IsBecasChecked
        {
            get => _isBecasChecked;
            set
            {
                if (_isBecasChecked != value)
                {
                    _isBecasChecked = value;
                    OnPropertyChanged(nameof(IsBecasChecked));
                }
            }
        }

        public bool IsAdmisionesChecked
        {
            get => _isAdmisionesChecked;
            set
            {
                if (_isAdmisionesChecked != value)
                {
                    _isAdmisionesChecked = value;
                    OnPropertyChanged(nameof(IsAdmisionesChecked));
                }
            }
        }

        public bool IsAgendaVisible
        {
            get => _isAgendaVisible;
            set
            {
                if (_isAgendaVisible != value)
                {
                    _isAgendaVisible = value;
                    OnPropertyChanged(nameof(IsAgendaVisible));
                }
            }
        }

        public bool IsCalendarVisible
        {
            get => _isCalendarVisible;
            set
            {
                if (_isCalendarVisible != value)
                {
                    _isCalendarVisible = value;
                    OnPropertyChanged(nameof(IsCalendarVisible));
                }
            }
        }

        // Popup de filtro (Syncfusion)
        public bool IsFilterPopupOpen
        {
            get => _isFilterPopupOpen;
            set
            {
                if (_isFilterPopupOpen != value)
                {
                    _isFilterPopupOpen = value;
                    OnPropertyChanged(nameof(IsFilterPopupOpen));
                }
            }
        }

        public bool FooterArrowVisible
        {
            get => _footerArrowVisible;
            set
            {
                if (_footerArrowVisible != value)
                {
                    _footerArrowVisible = value;
                    OnPropertyChanged(nameof(FooterArrowVisible));
                }
            }
        }

        public bool SelectedAgendaResult
        {
            get => _isAgendaResult;
            set
            {
                if (_isAgendaResult != value)
                {
                    _isAgendaResult = value;
                    OnPropertyChanged(nameof(SelectedAgendaResult));
                }
            }
        }

        public bool SelectedDateResult
        {
            get => _selectedDateResult;
            set
            {
                if (_selectedDateResult != value)
                {
                    _selectedDateResult = value;
                    OnPropertyChanged(nameof(SelectedDateResult));
                }
            }
        }

        public ICommand ApplyFiltersCalendarCommand => new Command(ApplyFiltersCalendar);
        public ICommand OpenFilterPopupCalendarCommand => new Command(async () => await OpenFilterPopupCalendarAsync());
        public ICommand CloseFilterPopupCalendarCommand => new Command(() => IsFilterPopupOpen = false);
        public ICommand ApplyFilterCalendarAndCloseCommand => new Command(() => { ApplyFiltersCalendar(); IsFilterPopupOpen = false; });
        public ICommand EventCalendarCommand => new Command<EventModel>(async (selectedEvent) => await NavigateToEventCalendarAsync(selectedEvent));
        public ICommand OpenFilterEventCalendarReminderCommand => new Command<EventModel>(async (Event) => await OpenFilterEventCalendarReminderAsync(Event));

        private void ApplyFiltersCalendar()
        {
            // Siempre usar el Scheduler; decidir vista según selección
            IsCalendarVisible = true;
            IsAgendaVisible = false;

            if (IsAgendaSelected)
            {
                SchedulerViewMode = SchedulerView.Agenda;
            }
            else if (IsSemanaSelected)
            {
                SelectedCalendarLayout = WeekLayout.Week;
                SchedulerViewMode = SchedulerView.Week;
            }
            else // por defecto Mes
            {
                SelectedCalendarLayout = WeekLayout.Month;
                SchedulerViewMode = SchedulerView.Month;
            }

            SelectedDateResult = false;
        }

        public SchedulerView SchedulerViewMode
        {
            get => _schedulerViewMode;
            set
            {
                if (_schedulerViewMode != value)
                {
                    _schedulerViewMode = value;
                    OnPropertyChanged(nameof(SchedulerViewMode));
                }
            }
        }

        private void UpdateFooterArrowVisibility()
        {
            if (SelectedDate.HasValue)
            {
                if (Events.ContainsKey(SelectedDate.Value))
                {
                    FooterArrowVisible = true;
                    SelectedDateResult = false;
                }
                else
                {
                    FooterArrowVisible = false;
                    SelectedDateResult = true;
                }
            }
            else
            {
                FooterArrowVisible = false;
                SelectedDateResult = false;
            }
        }

        public async Task OpenFilterPopupCalendarAsync()
        {
            IsFilterPopupOpen = true;
            await Task.CompletedTask;
        }

        public async Task NavigateToEventCalendarAsync(EventModel selectedEvent)
        {
            if (selectedEvent != null)
            {
                var eventDetailPage = new EventCalendar(this, selectedEvent);
                await Application.Current!.MainPage!.Navigation.PushAsync(eventDetailPage);
            }
        }

        public async Task OpenFilterEventCalendarReminderAsync(EventModel Event)
        {
            if (Event != null)
            {
                DateTime FechaHoraInicio = (Event.FechaInicio.HasValue && Event.HoraInicio.HasValue)
                    ? Event.FechaInicio.Value.Date + Event.HoraInicio.Value
                    : DateTime.MinValue;

                DateTime FechaHoraFin = (Event.FechaFin.HasValue && Event.HoraFin.HasValue)
                    ? Event.FechaFin.Value.Date + Event.HoraFin.Value
                    : DateTime.MinValue;

                await PopupFilterReadEventCalendarView.ShowPopupFilterEventCalendarReminderIfNotOpen(this, Event.Nombre!, Event.Descripcion!, FechaHoraInicio, FechaHoraFin);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
