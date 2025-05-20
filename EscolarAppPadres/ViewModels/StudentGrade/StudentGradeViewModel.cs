using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.StudentGrade
{
    public class SubjectGrades
    {
        public string MateriaNombre { get; set; }
        public Dictionary<string, double?> CalificacionesPorPeriodo { get; set; } = new();
    }


    class StudentGradeViewModel : INotifyPropertyChanged
    {
        #region Services
        private readonly GradeService _gradeService;
        #endregion

        #region Properties
        private ObservableCollection<EvaluationPeriod> _evaluationPeriods;
        private ObservableCollection<SubjectGrades> _materiasConCalificaciones;
        private bool _isRefreshing;
        private bool _sinResultados;

        public ObservableCollection<EvaluationPeriod> EvaluationPeriods
        {
            get => _evaluationPeriods;
            set
            {
                _evaluationPeriods = value;
                OnPropertyChanged(nameof(EvaluationPeriods));
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
        #endregion

        #region Commands
        public ICommand LoadGradesCommand { get; }
        #endregion

        #region Constructor
        public StudentGradeViewModel()
        {
            _gradeService = new GradeService();
            EvaluationPeriods = new ObservableCollection<EvaluationPeriod>();
            MateriasConCalificaciones = new ObservableCollection<SubjectGrades>();
            LoadGradesCommand = new Command(async () => await LoadGradesAsync());
        }
        #endregion

        #region Methods
        public ObservableCollection<SubjectGrades> MateriasConCalificaciones { get; set; } = new();
        public List<string> NombresPeriodos { get; set; } = new();

        public async Task LoadGradesAsync()
        {
            IsRefreshing = true;
            SinResultados = false;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                var profileId = await SecureStorage.GetAsync("Tipo_Usuario_Id");
                var alumnoId = await SecureStorage.GetAsync("Alumno_Id");

                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Sesión expirada. Por favor inicie sesión nuevamente.");
                    return;
                }

                if (!long.TryParse(profileId, out _) || !long.TryParse(alumnoId, out _))
                {
                    await DialogsHelper2.ShowErrorMessage("Información del usuario inválida.");
                    return;
                }

                var response = await _gradeService.GetGradesAsync(token);

                if (response == null)
                {
                    await DialogsHelper2.ShowErrorMessage("No se recibió respuesta del servidor.");
                    return;
                }

                if (response.Data != null && response.Data.Any())
                {
                    EvaluationPeriods.Clear();
                    foreach (var period in response.Data)
                        EvaluationPeriods.Add(period);

                    // Obtener nombres únicos de materias
                    var materias = response.Data
                        .SelectMany(p => p.Calificaciones)
                        .GroupBy(c => c.MateriaNombre)
                        .Select(g => g.Key)
                        .ToList();

                    // Obtener nombres de periodos
                    NombresPeriodos = response.Data
                        .Select(p => p.DescripcionCorta)
                        .ToList();

                    // Construir la colección de MateriasConCalificaciones
                    MateriasConCalificaciones.Clear();
                    foreach (var materia in materias)
                    {
                        var calificacionesPorPeriodoDecimal = new Dictionary<string, decimal?>();

                        foreach (var periodo in EvaluationPeriods)
                        {
                            var calificacion = periodo.Calificaciones
                                .FirstOrDefault(c => c.MateriaNombre == materia)?.Calificacion;

                            calificacionesPorPeriodoDecimal[periodo.DescripcionCorta] = calificacion;
                        }

                        var calificacionesPorPeriodoDouble = calificacionesPorPeriodoDecimal
                            .ToDictionary(
                                kvp => kvp.Key,
                                kvp => kvp.Value.HasValue ? (double?)kvp.Value.Value : null
                            );

                        MateriasConCalificaciones.Add(new SubjectGrades
                        {
                            MateriaNombre = materia,
                            CalificacionesPorPeriodo = calificacionesPorPeriodoDouble
                        });
                    }


                    if (MateriasConCalificaciones.Count == 0)
                        SinResultados = true;
                }
                else
                {
                    SinResultados = true;
                }
            }
            catch (HttpRequestException)
            {
                await DialogsHelper2.ShowErrorMessage("No se pudo conectar al servidor. Verifique su conexión a Internet e intente de nuevo.");
            }
            catch (TaskCanceledException)
            {
                await DialogsHelper2.ShowErrorMessage("La solicitud ha expirado. Intente nuevamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error inesperado: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage("Ocurrió un error inesperado: " + ex.Message);
            }
            finally
            {
                IsRefreshing = false;
                OnPropertyChanged(nameof(MateriasConCalificaciones));
                OnPropertyChanged(nameof(NombresPeriodos));
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
