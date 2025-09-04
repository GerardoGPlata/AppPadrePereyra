using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using EscolarAppPadres.Views.News;
using EscolarAppPadres.Views.FiltersPopup;
using EscolarAppPadres.Helpers;
using System.Security.AccessControl;

namespace EscolarAppPadres.ViewModels.News
{
    public class NewsViewModel : INotifyPropertyChanged
    {
        private readonly NewsService _newsService;

        private ObservableCollection<NotificacionesLeidas> _noticias;
        private ObservableCollection<NotificacionesLeidas> _filtrarNoticias;
        private ObservableCollection<RemindNotificationOptions> _remindNotificationOptions;

        private RemindNotificationOptions _selectedNotification;

        private bool _isNavigating;
        private bool _isRefreshing;

        private bool _filtroLeidos;
        private bool _filtroNoLeidos;
        private bool _sinResultados;

        private bool _isOpened;

        public NewsViewModel()
        {
            _newsService = new NewsService();
            _noticias = new ObservableCollection<NotificacionesLeidas>();
            _filtrarNoticias = new ObservableCollection<NotificacionesLeidas>();

            RemindNotificationOptions = new ObservableCollection<RemindNotificationOptions>
            {
                new RemindNotificationOptions { Icon = "partly_sunny_outline.svg", Label = "En la tarde", CustomDate = DateTime.Today, CustomTime = new TimeSpan(16,0,0), CornerRadius = new CornerRadius(30,30,0,0) },
                new RemindNotificationOptions { Icon = "sunny_outline.svg", Label = "Mañana temprano", CustomDate = DateTime.Today.AddDays(1), CustomTime = new TimeSpan(6,0,0), CornerRadius = new CornerRadius(0,0,0,0) },
                new RemindNotificationOptions { Icon = "alarm_outline.svg", Label = "Hora personalizada", CornerRadius = new CornerRadius (0,0,30,30), IsCustomTime = true }
            };

            if (RemindNotificationOptions.Any())
            {
                SelectedNotification = RemindNotificationOptions.First();
            }

            FiltroNoLeidos = Preferences.Get("FiltroNoLeidos", true);
            FiltroLeidos = Preferences.Get("FiltroLeidos", false);

            LoadNewsCommand = new Command(async () => await LoadNewsAsync());
        }

        public ObservableCollection<NotificacionesLeidas> Noticias
        {
            get => _noticias;
            set
            {
                _noticias = value;
                OnPropertyChanged(nameof(Noticias));
            }
        }

        public ObservableCollection<NotificacionesLeidas> FiltrarNoticias
        {
            get => _filtrarNoticias;
            set
            {
                _filtrarNoticias = value;
                OnPropertyChanged(nameof(FiltrarNoticias));
            }
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

        public bool FiltroLeidos
        {
            get => _filtroLeidos;
            set
            {
                if (_filtroLeidos != value)
                {
                    _filtroLeidos = value;
                    OnPropertyChanged(nameof(FiltroLeidos));
                    Preferences.Set("FiltroLeidos", _filtroLeidos);
                }
            }
        }

        public bool FiltroNoLeidos
        {
            get => _filtroNoLeidos;
            set
            {
                if (_filtroNoLeidos != value)
                {
                    _filtroNoLeidos = value;
                    OnPropertyChanged(nameof(FiltroNoLeidos));
                    Preferences.Set("FiltroNoLeidos", _filtroNoLeidos);
                }
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

        public bool IsNavigating
        {
            get => _isNavigating;
            set
            {
                _isNavigating = value;
                OnPropertyChanged(nameof(IsNavigating));
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

        public bool IsOpened
        {
            get => _isOpened;
            set
            {
                if (_isNavigating != value)
                {
                    _isNavigating = value;
                    OnPropertyChanged(nameof(IsNavigating));
                }
            }
        }

        public ICommand LoadNewsCommand { get; }
        public ICommand SendReadNewsCommand => new Command<NotificacionesLeidas>(async (notificacion) => await NewsReadAsync(notificacion));
        public ICommand OpenBondNewsCommand => new Command<NotificacionesLeidas>(async (selectedNoticia) => await NavigateToOpenBondTappedAsync(selectedNoticia));
        public ICommand ApplyFiltersNewsCommand => new Command(ApplyFiltersNews);
        public ICommand OpenFilterPopupNewsCommand => new Command(async () => await OpenFilterPopupNewsAsync());
        public ICommand OpenFilterCalendarReminderCommand => new Command<NotificacionesLeidas>(async (noticia) => await OpenFilterCalendarReminderNewsAsync(noticia));
        public ICommand OpenFilterCalendarReminderVinculoCommand => new Command<NotificacionesLeidas>(async (noticia) => await OpenFilterCalendarReminderVinculoNewsAsync(noticia));

        public async Task LoadNewsAsync()
        {
            IsRefreshing = true;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var Alumno_Id = 2058.ToString();

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(Alumno_Id) || !int.TryParse(Alumno_Id, out var AlumnoId))
                {
                    await DialogsHelper2.ShowErrorMessage("No se encontró la información de usuario o token.");
                    return;
                }

                var GetNewsApiResponse = await _newsService.GetNewsAsync(AlumnoId, token);

                if (GetNewsApiResponse != null && !GetNewsApiResponse.IsClientError)
                {
                    if (GetNewsApiResponse.Result && GetNewsApiResponse.Valoration && GetNewsApiResponse.Data.Any())
                    {
                        Noticias.Clear();
                        FiltrarNoticias.Clear();
                        foreach (var noticia in GetNewsApiResponse.Data)
                        {
                            if (noticia.Notificacion?.Formato != null)
                            {
                                var textoPlano = System.Text.Encoding.UTF8.GetString(noticia.Notificacion.Formato);

                                noticia.Notificacion.NotificacionImageFormato = textoPlano;
                            }

                            noticia.Leido = noticia.Leido ?? false;

                            Noticias.Add(noticia);
                            FiltrarNoticias.Add(noticia);
                        }

                        ApplyFiltersNews();

                        //await DialogsHelper.ShowSuccessMessage("Éxito", GetNewsApiResponse.Message);
                    }
                    else if (GetNewsApiResponse.Result && !GetNewsApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(GetNewsApiResponse.Message);
                    }
                    else if (!GetNewsApiResponse.Result && !GetNewsApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(GetNewsApiResponse.Log!);
                    }
                }
                else
                {
                    await DialogsHelper2.ShowErrorMessage(GetNewsApiResponse?.Message ?? "Error desconocido");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                //await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema al mostrar la notificaciones. Por favor, intenta nuevamente más tarde.");
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private void ApplyFiltersNews()
        {
            var filteredNoticias = FiltrarNoticias.AsEnumerable();

            if (FiltroLeidos && !FiltroNoLeidos)
            {
                filteredNoticias = filteredNoticias.Where(n => n.Leido == true);
            }
            else if (!FiltroLeidos && FiltroNoLeidos)
            {
                filteredNoticias = filteredNoticias.Where(n => n.Leido == false || n.Leido == null);
            }
            else if (!FiltroLeidos && !FiltroNoLeidos)
            {
                filteredNoticias = Enumerable.Empty<NotificacionesLeidas>();
            }

            MainThread.BeginInvokeOnMainThread(() =>
            {
                Noticias.Clear();
                foreach (var noticia in filteredNoticias)
                {
                    Noticias.Add(noticia);
                }

                SinResultados = !Noticias.Any();
            });
        }

        private async Task NewsReadAsync(NotificacionesLeidas notificacion)
        {
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var Alumno_Id = await SecureStorage.GetAsync("Alumno_Id");

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(Alumno_Id) || !int.TryParse(Alumno_Id, out var AlumnoId))
                {
                    await DialogsHelper2.ShowErrorMessage("No se encontró la información de usuario o token.");
                    return;
                }

                DialogsHelper.ShowLoadingMessage("Actualizando noticia, por favor espere...");

                notificacion.Leido = !(notificacion.Leido ?? false);

                var UpdateNewsApiResponse = await _newsService.UpdateNewsReadAsync(AlumnoId, notificacion, token);

                if (UpdateNewsApiResponse != null && !UpdateNewsApiResponse.IsClientError)
                {
                    if (UpdateNewsApiResponse.Result && UpdateNewsApiResponse.Valoration)
                    {
                        await LoadNewsAsync();
                        await DialogsHelper2.ShowSuccessMessage(UpdateNewsApiResponse.Message);
                    }
                    else if (UpdateNewsApiResponse.Result && !UpdateNewsApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(UpdateNewsApiResponse.Message);
                    }
                    else if (!UpdateNewsApiResponse.Result && !UpdateNewsApiResponse.Valoration)
                    {
                        await DialogsHelper2.ShowErrorMessage(UpdateNewsApiResponse.Log!);
                    }
                }
                else
                {
                    await DialogsHelper2.ShowErrorMessage(UpdateNewsApiResponse?.Message ?? "Error desconocido");
                }
            }
            catch (Exception ex)
            {
                var errorMessage = $"Error al procesar la solicitud Message: {ex.Message}";
                var errorStackTrace = $"Error al procesar la solicitud StackTrace: {ex.StackTrace}";
                Console.WriteLine("=== ERROR DETECTADO ===");
                Console.WriteLine(errorMessage);
                Console.WriteLine(errorStackTrace);
                Console.WriteLine("=======================");
                await DialogsHelper2.ShowErrorMessage("Ha ocurrido un problema al actualizar la notificacion. Por favor, intenta nuevamente más tarde.");
            }
            finally
            {
                DialogsHelper.HideLoadingMessage();
            }
        }

        private async Task NavigateToOpenBondTappedAsync(NotificacionesLeidas selectedNoticia)
        {
            if (selectedNoticia?.Notificacion != null)
            {
                var vinculoView = new VinculoView(this, selectedNoticia);

                await App.Current!.MainPage!.Navigation.PushAsync(vinculoView);
            }
        }

        private async Task OpenFilterPopupNewsAsync()
        {
            await PopupFilterNewsView.ShowPopupFilterNewsIfNotOpen(this);
        }

        public async Task OpenFilterCalendarReminderNewsAsync(NotificacionesLeidas notificacionesLeidas)
        {
            if (notificacionesLeidas?.Notificacion != null)
            {
                await PopupFilterReadCalendarView.ShowPopupFilterCalendarReminderNewsIfNotOpen(this, notificacionesLeidas.Notificacion!.Vinculo!,
                    notificacionesLeidas.Notificacion!.Titulo!, notificacionesLeidas.Notificacion.Mensaje!);
            }
        }

        public async Task OpenFilterCalendarReminderVinculoNewsAsync(NotificacionesLeidas notificacionesLeidas)
        {
            if (notificacionesLeidas?.Notificacion != null)
            {
                await PopupFilterReadCalendarView.ShowPopupFilterCalendarReminderNewsIfNotOpen(this, notificacionesLeidas.Notificacion!.Vinculo!,
                    notificacionesLeidas.Notificacion!.Titulo!, notificacionesLeidas.Notificacion.Mensaje!);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public Task toastTest()
        {
            return DialogsHelper2.ShowErrorMessage("Error de prueba");
        }
    }
}
