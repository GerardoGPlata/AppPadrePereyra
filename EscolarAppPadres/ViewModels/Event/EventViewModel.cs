using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models.Response;
using System.Threading.Tasks;
using System;

namespace EscolarAppPadres.ViewModels
{
    public class EventViewModel : INotifyPropertyChanged
    {
        private readonly EventService _eventService;
        //private readonly CalendarService _calendarService;

        private ObservableCollection<Models.Event> _eventos;
        private ObservableCollection<Models.Event> _filtrarEventos;

        private bool _isRefreshing;
        private bool _sinResultados;

        public EventViewModel()
        {
            _eventService = new EventService();
            //_calendarService = new CalendarService();

            Eventos = new ObservableCollection<Models.Event>();
            FiltrarEventos = new ObservableCollection<Models.Event>();

            LoadEventsCommand = new Command(async () => await LoadEventsAsync());
        }

        public ObservableCollection<Models.Event> Eventos
        {
            get => _eventos;
            set
            {
                _eventos = value;
                OnPropertyChanged(nameof(Eventos));
            }
        }

        public ObservableCollection<Models.Event> FiltrarEventos
        {
            get => _filtrarEventos;
            set
            {
                _filtrarEventos = value;
                OnPropertyChanged(nameof(FiltrarEventos));
            }
        }

        public bool SinResultados
        {
            get => _sinResultados;
            set
            {
                if (_sinResultados != value)
                {
                    _sinResultados = value;
                    OnPropertyChanged(nameof(SinResultados));
                }
            }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        public ICommand LoadEventsCommand { get; }

        /// <summary>
        /// Carga los eventos asociados al usuario actual utilizando los datos almacenados en SecureStorage.
        /// Realiza validaciones, obtiene los eventos desde el servicio remoto y actualiza la interfaz de usuario.
        /// </summary>
        /// <returns>
        /// Una tarea asincrónica que representa la operación de carga de eventos.
        /// Muestra mensajes de error o advertencia según corresponda.
        /// </returns>
        public async Task LoadEventsAsync()
        {
            IsRefreshing = true;
            SinResultados = false; // Resetear estado inicial

            try
            {
                // Obtener token almacenado
                var token = await SecureStorage.GetAsync("auth_token");

                // Obtener profileId, puede ser opcional
                var profileId = await SecureStorage.GetAsync("Tipo_Usuario_Id");

                Console.WriteLine($"Datos del usuario - Token: {!string.IsNullOrEmpty(token)}, ProfileId: {profileId}");

                // Validar token obligatorio
                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Sesión expirada. Por favor inicie sesión nuevamente.");
                    return;
                }

                // Usar profileId genérico si no es válido
                if (!long.TryParse(profileId, out var idPerfil))
                {
                    Console.WriteLine("[LoadEventsAsync] ProfileId inválido, usando valor por defecto 1.");
                    idPerfil = 1;
                }

                // Usar studentIds dummy si se omite Alumno_Id
                var studentIds = new long[] { 1000 };

                // Obtener eventos
                var response = await _eventService.GetEventsAsync(idPerfil, studentIds, token);

                if (response?.Data != null && response.Data.Any())
                {
                    Eventos.Clear();
                    FiltrarEventos.Clear();

                    var nombresEventos = new List<string>();
                    foreach (var evento in response.Data.OrderBy(e => e.DateInicio))
                    {
                        Eventos.Add(evento);
                        FiltrarEventos.Add(evento);
                        nombresEventos.Add($"{evento.DateInicio:dd/MM}: {evento.Nombre}");
                    }

                    Console.WriteLine("Eventos obtenidos:");
                    Console.WriteLine(string.Join("\n", nombresEventos));

                    SinResultados = false;

                    await DialogsHelper2.ShowSuccessMessage("Eventos cargados correctamente.");
                }
                else
                {
                    SinResultados = true;
                    var mensaje = response?.IsClientError == true
                        ? response.Message
                        : "No se encontraron eventos programados.";

                    await DialogsHelper2.ShowWarningMessage(mensaje);
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Error de conexión: {httpEx.Message}");
                await DialogsHelper2.ShowErrorMessage("Error de conexión. Verifique su acceso a Internet.");
                SinResultados = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex}");
                await DialogsHelper2.ShowErrorMessage("Error al cargar eventos. Intente nuevamente.");
                SinResultados = true;
            }
            finally
            {
                IsRefreshing = false;
                OnPropertyChanged(nameof(Eventos));
                OnPropertyChanged(nameof(FiltrarEventos));
            }
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
