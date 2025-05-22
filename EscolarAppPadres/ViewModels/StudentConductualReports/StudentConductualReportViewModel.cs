using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Models.Response;
using EscolarAppPadres.Services;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.StudentConductualReports
{
    public class StudentConductualReportViewModel : INotifyPropertyChanged
    {
        #region Services
        private readonly StudentConductualReportService _conductualReportService;
        #endregion

        #region Properties
        private ObservableCollection<StudentConductualReport> _reports;
        private bool _isRefreshing;
        private bool _sinResultados;
        private string _popupContent;
        private Hijo _selectedHijo;

        public ObservableCollection<Hijo> Hijos { get; set; } = new ObservableCollection<Hijo>();

        public ObservableCollection<StudentConductualReport> Reports
        {
            get => _reports;
            set
            {
                _reports = value;
                OnPropertyChanged(nameof(Reports));
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

        public string PopupContent
        {
            get => _popupContent;
            set
            {
                _popupContent = value;
                OnPropertyChanged(nameof(PopupContent));
            }
        }

        public Hijo SelectedHijo
        {
            get => _selectedHijo;
            set
            {
                if (_selectedHijo != value)
                {
                    _selectedHijo = value;
                    OnPropertyChanged(nameof(SelectedHijo));
                    _ = LoadConductualReportsAsync(); // Cargar reportes al seleccionar hijo
                }
            }
        }

        public ICommand LoadReportsCommand { get; }
        public ICommand ReportTappedCommand { get; }
        public event EventHandler<string> OnReportPopupRequested;
        #endregion

        #region Constructor
        public StudentConductualReportViewModel()
        {
            _conductualReportService = new StudentConductualReportService(new HttpClient());
            Reports = new ObservableCollection<StudentConductualReport>();
            LoadReportsCommand = new Command(async () => await LoadConductualReportsAsync());
            ReportTappedCommand = new Command<StudentConductualReport>(async (report) => await OnReportTapped(report));
        }
        #endregion

        #region Methods
        public async Task LoadConductualReportsAsync()
        {
            IsRefreshing = true;
            SinResultados = false;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var alumnoId = SelectedHijo?.AlumnoId.ToString();

                if (string.IsNullOrEmpty(token) || string.IsNullOrEmpty(alumnoId))
                {
                    await DialogsHelper2.ShowErrorMessage("Seleccione un hijo válido o inicie sesión.");
                    return;
                }

                var response = await _conductualReportService.GetStudentConductualReportAsync(token, alumnoId);

                if (response?.Data != null && response.Data.Any())
                {
                    Reports.Clear();
                    foreach (var item in response.Data)
                    {
                        Reports.Add(item);
                    }
                }
                else
                {
                    SinResultados = true;
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Error de red: {httpEx.Message}");
                await DialogsHelper2.ShowErrorMessage("No se pudo conectar al servidor.");
            }
            catch (TaskCanceledException)
            {
                await DialogsHelper2.ShowErrorMessage("La solicitud ha expirado.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage("Ocurrió un error inesperado: " + ex.Message);
            }
            finally
            {
                IsRefreshing = false;
                OnPropertyChanged(nameof(Reports));
            }
        }

        private async Task OnReportTapped(StudentConductualReport report)
        {
            if (report == null) return;

            PopupContent = $"Reporte de conducta:\n\nFecha: {report.FechaRegistro:dd/MM/yyyy}\nTipo ID: {report.TipoReporteId}\nObservaciones: {report.Observaciones}";
            OnReportPopupRequested?.Invoke(this, report.FechaRegistro.ToString("dd/MM/yyyy"));

        }

        public async Task LoadChildrenData()
        {
            try
            {
                var childrenJson = await SecureStorage.GetAsync("Hijos");
                if (!string.IsNullOrEmpty(childrenJson))
                {
                    var hijos = JsonSerializer.Deserialize<List<Hijo>>(childrenJson);
                    Hijos.Clear();

                    foreach (var hijo in hijos)
                    {
                        Hijos.Add(hijo);
                        Console.WriteLine($"AlumnoID: {hijo.AlumnoId}, NombreCompleto: {hijo.NombreCompleto}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando hijos: {ex.Message}");
            }
        }

        public async Task InitializeAsync()
        {
            await LoadChildrenData();
            if (Hijos.Any())
            {
                SelectedHijo = Hijos.First();
                // Disparará LoadConductualReportsAsync()
            }
        }
        #endregion

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
