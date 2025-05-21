using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using Microsoft.Maui.Controls.PlatformConfiguration;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.Subjects
{
    public class SubjectsViewModel : INotifyPropertyChanged
    {
        #region Services
        private readonly SubjectsService _subjectsService;
        #endregion

        #region Properties
        private ObservableCollection<StudentSubject> _studentSubjects;
        public ObservableCollection<Hijo> Hijos { get; set; } = new ObservableCollection<Hijo>();
        private bool _isRefreshing;
        private bool _sinResultados;

        private string subjectName;
        private string subjectProfessor;

        private Hijo _selectedHijo;

        public string SubjectName
        {
            get => subjectName;
            set
            {
                subjectName = value;
                OnPropertyChanged(nameof(SubjectName));
            }
        }

        public string SubjectProfessor
        {
            get => subjectProfessor;
            set
            {
                subjectProfessor = value;
                OnPropertyChanged(nameof(SubjectProfessor));
            }
        }

        public ObservableCollection<StudentSubject> StudentSubjects
        {
            get => _studentSubjects;
            set
            {
                _studentSubjects = value;
                OnPropertyChanged(nameof(StudentSubjects));
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

        private string _popupContent;
        public string PopupContent
        {
            get => _popupContent;
            set
            {
                _popupContent = value;
                OnPropertyChanged(nameof(PopupContent));
            }
        }

        public ICommand SubjectTappedCommand { get; }

        public event EventHandler<string> OnSubjectPopupRequested;



        #endregion

        #region Commands
        public ICommand LoadSubjectsCommand { get; }
        #endregion

        #region Constructor
        /// <summary>
        /// Inicializa una nueva instancia del ViewModel de materias.
        /// Configura el servicio de materias, la colección observable y el comando para cargar materias.
        /// </summary>
        public SubjectsViewModel()
        {
            _subjectsService = new SubjectsService(); // Servicio para obtener datos de materias
            StudentSubjects = new ObservableCollection<StudentSubject>(); // Colección enlazable a la UI
            LoadSubjectsCommand = new Command(async () => await LoadSubjectsAsync()); // Comando para refrescar
            SubjectTappedCommand = new Command<StudentSubject>(async (subject) => await OnSubjectTapped(subject));

        }
        #endregion

        #region Methods
        /// <summary>
        /// Carga asincrónicamente las materias del alumno desde el servidor.
        /// Maneja estados de carga, errores y actualiza la interfaz de usuario.
        /// </summary>
        /// <exception cref="HttpRequestException">Error de conexión con el servidor</exception>
        /// <exception cref="Exception">Errores inesperados</exception>
        public async Task LoadSubjectsAsync()
        {
            IsRefreshing = true; // Activa el indicador de carga
            SinResultados = false; // Reinicia el estado de "sin resultados"

            try
            {
                // 1. Obtener credenciales del almacen seguro
                var token = await SecureStorage.GetAsync("auth_token");
                var profileId = await SecureStorage.GetAsync("Tipo_Usuario_Id");
                string alumnoId = SelectedHijo?.AlumnoId.ToString();
                if (string.IsNullOrEmpty(alumnoId))
                {
                    await DialogsHelper2.ShowErrorMessage("Seleccione un hijo válido.");
                    return;
                }


                // 2. Validaciones críticas
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

                // 3. Obtener materias del servidor
                var response = await _subjectsService.GetStudentSubjectsAsync(token, alumnoId);


                if (response == null)
                {
                    await DialogsHelper2.ShowErrorMessage("No se recibió respuesta del servidor.");
                    return;
                }

                // 4. Procesar respuesta
                if (response?.Data != null && response.Data.Any())
                {
                    StudentSubjects.Clear(); // Limpiar colección existente

                    foreach (var subject in response.Data)
                    {
                        StudentSubjects.Add(subject); // Poblar con nuevos datos
                    }

                    // Verificar si llegaron vacíos (caso edge)
                    if (StudentSubjects.Count == 0)
                    {
                        SinResultados = true;
                    }

                    //await DialogsHelper2.ShowSuccessMessage("Materias cargadas correctamente.");
                }
                else // Caso: respuesta sin datos
                {
                    SinResultados = true;
                    //await DialogsHelper2.ShowWarningMessage("No se encontraron materias registradas.");
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"Error de red: {httpEx.Message}");
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
                IsRefreshing = false; // Desactiva el indicador de carga
                OnPropertyChanged(nameof(StudentSubjects)); // Notifica cambios a la UI
            }
        }

        private async Task OnSubjectTapped(StudentSubject selectedSubject)
        {
            if (selectedSubject == null)
                return;

            var homeworkService = new HomeworkService();
            var token = await SecureStorage.GetAsync("auth_token");
            var response = await homeworkService.GetStudentHomeworkAsync(token, selectedSubject.ProfesorPorMateriaId);

            if (response?.Data != null && response.Data.Any())
            {
                string tareas = string.Join("\n• ", response.Data.Select(t => t.TareaNombre));
                PopupContent = $"Tareas para {selectedSubject.Materia}:\n\n• {tareas}";
            }
            else
            {
                PopupContent = $"No se encontraron tareas.";
            }

            // Dispara el evento para que el code-behind abra el popup
            OnSubjectPopupRequested?.Invoke(this, selectedSubject.Materia);
        }

        public async Task LoadChildrenData()
        {
            try
            {
                var childrenJson = await SecureStorage.GetAsync("Hijos");

                if (!string.IsNullOrEmpty(childrenJson))
                {
                    var hijos = JsonSerializer.Deserialize<List<Hijo>>(childrenJson);

                    if (hijos != null)
                    {
                        Hijos.Clear();
                        foreach (var hijo in hijos)
                        {
                            Hijos.Add(hijo);
                            Console.WriteLine($"AlumnoID: {hijo.AlumnoId}, NombreCompleto: {hijo.NombreCompleto}, Matricula: {hijo.Matricula}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando hijos: {ex.Message}");
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
                    _ = LoadSubjectsAsync(); // Cargar materias al seleccionar hijo
                }
            }
        }

        public async Task InitializeAsync()
        {
            await LoadChildrenData(); // ahora sí espera a que se cargue

            if (Hijos.Any())
            {
                SelectedHijo = Hijos.First();
                // Esto activará LoadSubjectsAsync()
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