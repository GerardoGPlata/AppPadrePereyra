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
    public class SubjectGrades
    {
        public string MateriaNombre { get; set; }
        public Dictionary<string, double?> CalificacionesPorPeriodo { get; set; } = new();
    }

    public class StudentGradeViewModel : INotifyPropertyChanged
    {
        #region Services
        private readonly GradeService _gradeService;
        #endregion

        #region Properties
        public ObservableCollection<EvaluationPeriod> EvaluationPeriods { get; set; } = new();
        public ObservableCollection<SubjectGrades> MateriasConCalificaciones { get; set; } = new();
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
                    _ = LoadGradesAsync(); // Al cambiar hijo, cargar calificaciones
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
        #endregion

        #region Commands
        public ICommand LoadGradesCommand { get; }
        #endregion

        #region Constructor
        public StudentGradeViewModel()
        {
            _gradeService = new GradeService();
            LoadGradesCommand = new Command(async () => await LoadGradesAsync());
        }
        #endregion

        #region Methods
        public async Task InitializeAsync()
        {
            await LoadChildrenData();

            if (Hijos.Any())
                SelectedHijo = Hijos.First(); // Esto dispara LoadGradesAsync
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

        public async Task LoadGradesAsync()
        {
            IsRefreshing = true;
            SinResultados = false;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var profileId = "3";
                //var alumnoId = await SecureStorage.GetAsync("Alumno_Id");
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

                EvaluationPeriods.Clear();
                MateriasConCalificaciones.Clear();

                foreach (var period in response.Data)
                    EvaluationPeriods.Add(period);

                var materias = response.Data
                    .SelectMany(p => p.Calificaciones)
                    .GroupBy(c => c.MateriaNombre)
                    .Select(g => g.Key)
                    .ToList();

                NombresPeriodos = response.Data.Select(p => p.DescripcionCorta).ToList();

                foreach (var materia in materias)
                {
                    var calificaciones = new Dictionary<string, double?>();

                    foreach (var periodo in EvaluationPeriods)
                    {
                        var calificacion = periodo.Calificaciones
                            .FirstOrDefault(c => c.MateriaNombre == materia)?.Calificacion;

                        calificaciones[periodo.DescripcionCorta] = calificacion.HasValue ? (double?)calificacion.Value : null;
                    }

                    MateriasConCalificaciones.Add(new SubjectGrades
                    {
                        MateriaNombre = materia,
                        CalificacionesPorPeriodo = calificaciones
                    });
                }

                if (MateriasConCalificaciones.Count == 0)
                    SinResultados = true;
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
                OnPropertyChanged(nameof(MateriasConCalificaciones));
                OnPropertyChanged(nameof(NombresPeriodos));
            }
        }
        #endregion

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        #endregion
    }
}
