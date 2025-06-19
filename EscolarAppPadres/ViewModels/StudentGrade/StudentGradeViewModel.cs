using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.StudentGrade
{
    public class SubjectGrades : INotifyPropertyChanged
    {
        public string NombreCorto { get; set; }

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
                    .Select(c => c.Value)
                    .ToList();

                if (!calificacionesValidas.Any())
                    return null;

                return Math.Round(calificacionesValidas.Average(), 1);
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
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

        private Hijo _selectedHijo;
        public Hijo SelectedHijo
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

        private string _popupHeader;
        public string PopupHeader
        {
            get => _popupHeader;
            set { _popupHeader = value; OnPropertyChanged(nameof(PopupHeader)); }
        }

        private string _popupPeriod;
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

        private string _popupTotal;
        public string PopupTotal
        {
            get => _popupTotal;
            set { _popupTotal = value; OnPropertyChanged(nameof(PopupTotal)); }
        }

        // Evento para notificar cuando se deben actualizar las columnas
        public event EventHandler ColumnsChanged;
        #endregion

        #region Commands
        public ICommand LoadGradesCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand DescargarBoletaCommand { get; }
        #endregion

        #region Constructor
        public StudentGradeViewModel()
        {
            _gradeService = new GradeService();
            _criteriaGradesService = new StudentCriteriaGradesService();
            LoadGradesCommand = new Command(async () => await LoadGradesAsync());
            RefreshCommand = new Command(async () => await RefreshDataAsync());
            DescargarBoletaCommand = new Command(DescargarBoleta);
        }
        #endregion

        #region Methods
        private void DescargarBoleta()
        {
            if (SelectedHijo?.AlumnoId > 0)
            {
                var alumnoId = SelectedHijo.AlumnoId;
                var url = $"https://webpereyra.com.mx/Jesuitas_webServices/web/api/Controlescolar/Boletaimpresion/alumno/{alumnoId}?publicacion=1";
                Launcher.OpenAsync(new Uri(url));
            }
            else
            {
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DialogsHelper2.ShowErrorMessage("Seleccione un hijo válido para descargar la boleta.");
                });
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
                    await DialogsHelper2.ShowErrorMessage("Seleccione un hijo válido.");
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

                await Task.Run(() =>
                {
                    foreach (var period in response.Data)
                        EvaluationPeriods.Add(period);

                    var periodos = response.Data.Select(p => p.DescripcionCorta).ToList();
                    var materias = response.Data
                        .SelectMany(p => p.Calificaciones)
                        .GroupBy(c => c.NombreCorto)
                        .Select(g => g.Key)
                        .ToList();

                    var nuevasMaterias = new ObservableCollection<SubjectGrades>();

                    foreach (var materia in materias)
                    {
                        var calificaciones = new Dictionary<string, double?>();

                        foreach (var periodo in EvaluationPeriods)
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

                    NombresPeriodos = periodos;

                    MainThread.BeginInvokeOnMainThread(() =>
                    {
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
                await DialogsHelper2.ShowErrorMessage("Error inesperado: " + ex.Message);
            }
            finally
            {
                IsRefreshing = false;
            }
        }

        public async Task<List<StudentCriteriaGrade>> GetCriteriaGradesForSubjectAsync(int materiaId, int periodoEvaluacionId)
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
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}