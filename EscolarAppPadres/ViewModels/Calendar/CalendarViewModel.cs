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

namespace EscolarAppPadres.ViewModels.Calendar
{
    public class CalendarViewModel : INotifyPropertyChanged
    {
        private WeekLayout _selectedCalendarLayout;
        public CultureInfo Culture => new CultureInfo("es-MX");

        private ObservableCollection<RemindNotificationOptions> _remindNotificationOptions;

        private RemindNotificationOptions _selectedNotification;

        public EventCollection _Events { get; set; }

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
            _isMesSelected = true;

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
            // Cancelar cualquier carga previa
            _loadEventsCts?.Cancel();
            _loadEventsCts = new CancellationTokenSource();

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var profileId = await SecureStorage.GetAsync("Tipo_Usuario_Id");
                var alumnoId = await SecureStorage.GetAsync("Alumno_Id");

                if (string.IsNullOrEmpty(token) ||
                    !long.TryParse(profileId, out var idPerfil) ||
                    !long.TryParse(alumnoId, out var idAlumno))
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                        await DialogsHelper2.ShowErrorMessage("Información de sesión inválida."));
                    return;
                }

                // Obtener datos en segundo plano
                var response = await _eventService.GetEventsAsync(idPerfil, new[] { idAlumno }, token)
                    .ConfigureAwait(false);

                if (response?.Data == null || !response.Data.Any())
                {
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                        await DialogsHelper2.ShowErrorMessage("No se encontraron eventos."));
                    return;
                }

                // Procesamiento en segundo plano
                var eventosOrdenados = response.Data.OrderBy(e => e.DateInicio).ToList();
                var tempEventCollection = new EventCollection();

                foreach (var evento in eventosOrdenados)
                {
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

                // Actualización en el hilo UI
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    Eventos = new ObservableCollection<Event>(eventosOrdenados);
                    _Events = tempEventCollection;
                    IsAgendaVisible = true;

                    OnPropertyChanged(nameof(Eventos));
                    OnPropertyChanged(nameof(Events)); // Asegúrate de tener esta propiedad pública
                });
            }
            catch (OperationCanceledException)
            {
                // Carga cancelada, no hacer nada
            }
            catch (Exception ex)
            {
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    Console.WriteLine($"Error al cargar eventos: {ex.Message}");
                    await DialogsHelper2.ShowErrorMessage($"Error al cargar eventos: {ex.Message}");
                });
            }
        }

        // Método para cancelar la carga si es necesario (por ejemplo, al navegar fuera de la página)
        public void CancelLoading()
        {
            _loadEventsCts?.Cancel();
        }
        private EventModel ConvertToEventModel(Event evento)
        {
            return new EventModel
            {
                TipoEvento = evento.TipoEvento,
                Nivel = evento.Nivel,
                Nombre = evento.Nombre,
                Descripcion = evento.Descripcion,
                FechaInicio = evento.DateInicio,
                FechaFin = evento.DateFin ?? evento.DateInicio,
                HoraInicio = evento.DateInicio.TimeOfDay,
                HoraFin = (evento.DateFin ?? evento.DateInicio.AddHours(1)).TimeOfDay,
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
        public ICommand EventCalendarCommand => new Command<EventModel>(async (selectedEvent) => await NavigateToEventCalendarAsync(selectedEvent));
        public ICommand OpenFilterEventCalendarReminderCommand => new Command<EventModel>(async (Event) => await OpenFilterEventCalendarReminderAsync(Event));

        private void ApplyFiltersCalendar()
        {
            if (IsAgendaSelected)
            {
                IsAgendaVisible = true;
                IsCalendarVisible = false;
                SelectedDateResult = false;

                AgendaEvents = _Events.Values
                    .Cast<List<EventModel>>()
                    .SelectMany(v => v)
                    .ToList();

                SelectedAgendaResult = AgendaEvents == null || !AgendaEvents.Any();
            }
            else
            {
                IsCalendarVisible = true;
                IsAgendaVisible = false;

                if (IsSemanaSelected)
                {
                    SelectedCalendarLayout = WeekLayout.Week;
                }
                else if (IsMesSelected)
                {
                    SelectedCalendarLayout = WeekLayout.Month;
                }

                SelectedDateResult = false;

                if (SelectedDate.HasValue && !Events.ContainsKey(SelectedDate.Value))
                {
                    SelectedDateResult = true;
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
            await PopupFilterCalendarView.ShowPopupFilterCalendarIfNotOpen(this);
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
