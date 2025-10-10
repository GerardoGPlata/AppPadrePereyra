using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using EscolarAppPadres.Views.School;
using Microsoft.Maui.ApplicationModel;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.StudentGrade
{
    public class SubjectGrades : INotifyPropertyChanged
    {
        public string NombreCorto { get; set; } = string.Empty;

        private Dictionary<string, double?> _calificacionesPorPeriodo = new();
        public Dictionary<string, double?> CalificacionesPorPeriodo
        {
            get => _calificacionesPorPeriodo;
            set
            {
                _calificacionesPorPeriodo = value;
                OnPropertyChanged(nameof(CalificacionesPorPeriodo));
                OnPropertyChanged(nameof(Promedio));
            }
        }

        public double? Promedio
        {
            get
            {
                var calificacionesValidas = CalificacionesPorPeriodo.Values
                    .Where(c => c.HasValue)
                    .Select(c => c!.Value)
                    .ToList();

                if (!calificacionesValidas.Any())
                    return null;

                return Math.Round(calificacionesValidas.Average(), 1);
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public class StudentGradeViewModel : INotifyPropertyChanged
    {
        #region Services
        private readonly GradeService _gradeService;
        private readonly StudentCriteriaGradesService _criteriaGradesService;
        #endregion

        #region Properties
        public ObservableCollection<EvaluationPeriod> EvaluationPeriods { get; set; } = new();

        private ObservableCollection<SubjectGrades> _materiasConCalificaciones = new();
        public ObservableCollection<SubjectGrades> MateriasConCalificaciones
        {
            get => _materiasConCalificaciones;
            set
            {
                _materiasConCalificaciones = value;
                OnPropertyChanged(nameof(MateriasConCalificaciones));
            }
        }

        public ObservableCollection<Hijo> Hijos { get; set; } = new();

        private List<string> _nombresPeriodos = new();
        public List<string> NombresPeriodos
        {
            get => _nombresPeriodos;
            set
            {
                _nombresPeriodos = value;
                OnPropertyChanged(nameof(NombresPeriodos));
            }
        }

        private Hijo? _selectedHijo;
        public Hijo? SelectedHijo
        {
            get => _selectedHijo;
            set
            {
                if (_selectedHijo != value)
                {
                    _selectedHijo = value;
                    OnPropertyChanged(nameof(SelectedHijo));
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        MateriasConCalificaciones = new ObservableCollection<SubjectGrades>();
                        EvaluationPeriods.Clear();
                    });
                    _ = LoadGradesAsync();
                }
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set
            {
                _isRefreshing = value;
                OnPropertyChanged(nameof(IsRefreshing));
            }
        }

        private bool _sinResultados;
        public bool SinResultados
        {
            get => _sinResultados;
            set
            {
                _sinResultados = value;
                OnPropertyChanged(nameof(SinResultados));
            }
        }

        private string _popupHeader = string.Empty;
        public string PopupHeader
        {
            get => _popupHeader;
            set { _popupHeader = value; OnPropertyChanged(nameof(PopupHeader)); }
        }

        private string _popupPeriod = string.Empty;
        public string PopupPeriod
        {
            get => _popupPeriod;
            set { _popupPeriod = value; OnPropertyChanged(nameof(PopupPeriod)); }
        }

        private ObservableCollection<StudentCriteriaGrade> _popupCriteria = new();
        public ObservableCollection<StudentCriteriaGrade> PopupCriteria
        {
            get => _popupCriteria;
            set { _popupCriteria = value; OnPropertyChanged(nameof(PopupCriteria)); }
        }

        private string _popupTotal = string.Empty;
        public string PopupTotal
        {
            get => _popupTotal;
            set { _popupTotal = value; OnPropertyChanged(nameof(PopupTotal)); }
        }

        private bool _isBoletaPopupOpen;
        public bool IsBoletaPopupOpen
        {
            get => _isBoletaPopupOpen;
            set
            {
                if (_isBoletaPopupOpen != value)
                {
                    _isBoletaPopupOpen = value;
                    OnPropertyChanged(nameof(IsBoletaPopupOpen));
                }
            }
        }

        private string _boletaPopupMessage = string.Empty;
        public string BoletaPopupMessage
        {
            get => _boletaPopupMessage;
            set
            {
                if (_boletaPopupMessage != value)
                {
                    _boletaPopupMessage = value;
                    OnPropertyChanged(nameof(BoletaPopupMessage));
                }
            }
        }

        private string _boletaPopupTitle = "Aviso";
        public string BoletaPopupTitle
        {
            get => _boletaPopupTitle;
            set
            {
                if (_boletaPopupTitle != value)
                {
                    _boletaPopupTitle = value;
                    OnPropertyChanged(nameof(BoletaPopupTitle));
                }
            }
        }

        // Evento para notificar cuando se deben actualizar las columnas
        public event EventHandler? ColumnsChanged;
        #endregion

        #region Commands
        public ICommand LoadGradesCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DescargarBoletaCommand { get; }
        #endregion

        private CancellationTokenSource? _boletaDownloadCts;

        #region Constructor
        public StudentGradeViewModel()
        {
            _gradeService = new GradeService();
            _criteriaGradesService = new StudentCriteriaGradesService();
            LoadGradesCommand = new Command(async () => await LoadGradesAsync());
            RefreshCommand = new Command(async () => await RefreshDataAsync());
            DescargarBoletaCommand = new Command(async () => await DescargarBoletaAsync());
        }
        #endregion

        #region Methods
        private async Task DescargarBoletaAsync()
        {
            DialogsHelper.ShowLoadingMessage("Cargando boleta...");
            CancellationToken cancellationToken = CancellationToken.None;
            try
            {
                if (SelectedHijo?.AlumnoId is null or <= 0)
                {
                    await ShowBoletaMessageAsync("Seleccione un hijo válido para descargar la boleta.");
                    return;
                }

                var url = $"https://webpereyra.com.mx/Jesuitas_webServices/web/api/Controlescolar/Boletaimpresion/alumno/{SelectedHijo.AlumnoId}?publicacion=1";

                _boletaDownloadCts?.Cancel();
                _boletaDownloadCts?.Dispose();
                _boletaDownloadCts = new CancellationTokenSource();
                cancellationToken = _boletaDownloadCts.Token;

                using var client = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(45)
                };

                using var response = await client.GetAsync(url, cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    var errorBody = await response.Content.ReadAsStringAsync(cancellationToken);
                    var message = TryExtractMessage(errorBody) ?? $"No se pudo descargar la boleta (Código {(int)response.StatusCode}).";
                    await ShowBoletaMessageAsync(message);
                    return;
                }

                var mediaType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

                if (mediaType.Contains("pdf", StringComparison.OrdinalIgnoreCase))
                {
                    var pdfBytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
                    if (pdfBytes == null || pdfBytes.Length == 0)
                    {
                        await ShowBoletaMessageAsync("El archivo de la boleta está vacío.");
                        return;
                    }

                    await OpenPdfAsync(pdfBytes);
                }
                else
                {
                    var rawContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var message = TryExtractMessage(rawContent) ?? "No se pudo descargar la boleta en este momento.";
                    await ShowBoletaMessageAsync(message);
                }
            }
            catch (HttpRequestException)
            {
                await ShowBoletaMessageAsync("No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo.", "Red");
            }
            catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                await ShowBoletaMessageAsync("La solicitud ha expirado. Intente nuevamente.", "Tiempo de espera");
            }
            catch (OperationCanceledException)
            {
                // Descarga cancelada manualmente o reemplazada por una nueva solicitud.
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado al descargar boleta: {ex.Message}");
                await ShowBoletaMessageAsync("Ocurrió un error inesperado al descargar la boleta.", "Error");
            }
            finally
            {
                DialogsHelper.HideLoadingMessage();
            }
        }

        public async Task InitializeAsync()
        {
            await LoadChildrenData();
            if (Hijos.Any())
                SelectedHijo = Hijos.First();
        }

        private async Task LoadChildrenData()
        {
            try
            {
                var json = await SecureStorage.GetAsync("Hijos");
                if (!string.IsNullOrEmpty(json))
                {
                    var hijos = JsonSerializer.Deserialize<List<Hijo>>(json);
                    if (hijos != null)
                    {
                        Hijos.Clear();
                        foreach (var hijo in hijos)
                            Hijos.Add(hijo);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando hijos: {ex.Message}");
            }
        }

        private async Task RefreshDataAsync()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                MateriasConCalificaciones = new ObservableCollection<SubjectGrades>();
                EvaluationPeriods.Clear();
            });
            await LoadGradesAsync();
        }

        public async Task LoadGradesAsync()
        {
            IsRefreshing = true;
            SinResultados = false;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var profileId = "3";
                var alumnoId = SelectedHijo?.AlumnoId.ToString();

                if (string.IsNullOrEmpty(alumnoId))
                {
                    //await DialogsHelper2.ShowErrorMessage("Seleccione un hijo válido.");
                    return;
                }

                if (string.IsNullOrEmpty(token) || SelectedHijo == null)
                {
                    await DialogsHelper2.ShowErrorMessage("Sesión expirada o hijo no seleccionado.");
                    return;
                }

                if (!long.TryParse(profileId, out _) || SelectedHijo.AlumnoId <= 0)
                {
                    await DialogsHelper2.ShowErrorMessage("Información del usuario inválida.");
                    return;
                }

                var response = await _gradeService.GetGradesAsync(token, SelectedHijo.AlumnoId.ToString());

                if (response == null || response.Data == null || !response.Data.Any())
                {
                    SinResultados = true;
                    return;
                }

                // Preparar datos en segundo plano sin tocar colecciones de UI
                List<EvaluationPeriod> periods = new();
                List<string> periodos = new();
                ObservableCollection<SubjectGrades> nuevasMaterias = new();

                await Task.Run(() =>
                {
                    periods = response.Data.ToList();

                    periodos = periods.Select(p => p.DescripcionCorta).ToList();
                    var materias = periods
                        .SelectMany(p => p.Calificaciones)
                        .GroupBy(c => c.NombreCorto)
                        .Select(g => g.Key)
                        .ToList();

                    foreach (var materia in materias)
                    {
                        var calificaciones = new Dictionary<string, double?>();

                        foreach (var periodo in periods)
                        {
                            var calificacion = periodo.Calificaciones
                                .FirstOrDefault(c => c.NombreCorto == materia)?.Calificacion;

                            calificaciones[periodo.DescripcionCorta] = calificacion.HasValue ? (double?)calificacion.Value : null;
                        }

                        nuevasMaterias.Add(new SubjectGrades
                        {
                            NombreCorto = materia,
                            CalificacionesPorPeriodo = calificaciones
                        });
                    }
                });

                // Actualizar UI en el hilo principal
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    EvaluationPeriods.Clear();
                    foreach (var period in periods)
                        EvaluationPeriods.Add(period);

                    NombresPeriodos = periodos;

                    if (nuevasMaterias.Count == 0)
                    {
                        SinResultados = true;
                    }
                    else
                    {
                        MateriasConCalificaciones = nuevasMaterias;
                        ColumnsChanged?.Invoke(this, EventArgs.Empty);
                    }
                });
            }
            catch (HttpRequestException)
            {
                await DialogsHelper2.ShowErrorMessage("No se pudo conectar al servidor.");
            }
            catch (TaskCanceledException)
            {
                await DialogsHelper2.ShowErrorMessage("La solicitud ha expirado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                //await DialogsHelper2.ShowErrorMessage("Error inesperado: " + ex.Message);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        private async Task OpenPdfAsync(byte[] pdfBytes)
        {
            if (pdfBytes == null || pdfBytes.Length == 0)
            {
                await ShowBoletaMessageAsync("El archivo de la boleta está vacío.");
                return;
            }

            var pdfStream = new MemoryStream(pdfBytes);

            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                var navigation = Application.Current?.MainPage?.Navigation;
                if (navigation != null)
                {
                    await navigation.PushAsync(new BoletaPdfView(pdfStream));
                }
                else
                {
                    pdfStream.Dispose();
                    BoletaPopupTitle = "Error";
                    BoletaPopupMessage = "No fue posible mostrar la boleta.";
                    IsBoletaPopupOpen = true;
                }
            });
        }

        private async Task ShowBoletaMessageAsync(string message, string title = "Aviso")
        {
            if (MainThread.IsMainThread)
            {
                BoletaPopupTitle = title;
                BoletaPopupMessage = message;
                IsBoletaPopupOpen = true;
            }
            else
            {
                await MainThread.InvokeOnMainThreadAsync(() =>
                {
                    BoletaPopupTitle = title;
                    BoletaPopupMessage = message;
                    IsBoletaPopupOpen = true;
                });
            }
        }

        public void HideBoletaPopup()
        {
            IsBoletaPopupOpen = false;
        }

        private static string? TryExtractMessage(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                return null;
            }

            try
            {
                using var document = JsonDocument.Parse(content);
                var message = ExtractMessageFromElement(document.RootElement);
                if (!string.IsNullOrWhiteSpace(message))
                {
                    return message;
                }
            }
            catch (JsonException)
            {
                // Contenido no es JSON, se usará el texto sin procesar.
            }

            var trimmed = content.Trim();
            if (trimmed.Length > 500)
            {
                trimmed = trimmed.Substring(0, 497) + "...";
            }

            return trimmed;
        }

        private static string? ExtractMessageFromElement(JsonElement element)
            => ExtractMessageFromElement(element, false, true);

        private static string? ExtractMessageFromElement(JsonElement element, bool treatStringAsMessage, bool isRoot)
        {
            switch (element.ValueKind)
            {
                case JsonValueKind.Object:
                    {
                        foreach (var property in element.EnumerateObject())
                        {
                            if (IsMessageKey(property.Name))
                            {
                                var direct = ExtractMessageFromElement(property.Value, true, false);
                                if (!string.IsNullOrWhiteSpace(direct))
                                {
                                    return direct;
                                }
                            }
                        }

                        foreach (var property in element.EnumerateObject())
                        {
                            var result = ExtractMessageFromElement(property.Value, false, false);
                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                return result;
                            }
                        }

                        break;
                    }
                case JsonValueKind.Array:
                    {
                        foreach (var item in element.EnumerateArray())
                        {
                            var result = ExtractMessageFromElement(item, treatStringAsMessage, false);
                            if (!string.IsNullOrWhiteSpace(result))
                            {
                                return result;
                            }
                        }

                        break;
                    }
                case JsonValueKind.String:
                    {
                        var text = element.GetString();
                        if (!string.IsNullOrWhiteSpace(text) && (treatStringAsMessage || isRoot))
                        {
                            return text;
                        }

                        break;
                    }
                case JsonValueKind.Number:
                case JsonValueKind.True:
                case JsonValueKind.False:
                    {
                        if (treatStringAsMessage)
                        {
                            return element.ToString();
                        }

                        break;
                    }
            }

            return null;
        }

        private static bool IsMessageKey(string propertyName)
        {
            return propertyName.Equals("message", StringComparison.OrdinalIgnoreCase) ||
                   propertyName.Equals("descripcion", StringComparison.OrdinalIgnoreCase) ||
                   propertyName.Equals("description", StringComparison.OrdinalIgnoreCase) ||
                   propertyName.Equals("detail", StringComparison.OrdinalIgnoreCase);
        }

        public async Task<List<StudentCriteriaGrade>> GetCriteriaGradesForSubjectAsync(int materiaId, int periodoEvaluacionId)
        {
            DialogsHelper.ShowLoadingMessage("Cargando...");
            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var alumnoId = SelectedHijo?.AlumnoId.ToString();

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(alumnoId))
                    return new List<StudentCriteriaGrade>();

                var response = await _criteriaGradesService.GetStudentCriteriaGradesAsync(token, alumnoId, materiaId, periodoEvaluacionId);

                if (response != null && response.Data != null && response.Data.Any())
                    return response.Data;

                return new List<StudentCriteriaGrade>();
            }
            catch (HttpRequestException)
            {
                await DialogsHelper.ShowErrorMessage("Red", "No se pudo conectar al servidor. Verifique su conexión e intente de nuevo.");
                return new List<StudentCriteriaGrade>();
            }
            catch (TaskCanceledException)
            {
                await DialogsHelper.ShowErrorMessage("Tiempo de espera", "La solicitud ha expirado. Intente nuevamente.");
                return new List<StudentCriteriaGrade>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado al cargar criterios: {ex.Message}");
                await DialogsHelper.ShowErrorMessage("Error", "Ocurrió un error inesperado al cargar los criterios.");
                return new List<StudentCriteriaGrade>();
            }
            finally
            {
                DialogsHelper.HideLoadingMessage();
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}