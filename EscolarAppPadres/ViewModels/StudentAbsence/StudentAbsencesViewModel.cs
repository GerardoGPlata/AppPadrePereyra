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

namespace EscolarAppPadres.ViewModels.Absences
{
    public class StudentAbsencesViewModel : INotifyPropertyChanged
    {
        private readonly StudentAbsencesService _absencesService;

        public ObservableCollection<StudentAbsence> StudentAbsences { get; set; }
        public ObservableCollection<Hijo> Hijos { get; set; } = new ObservableCollection<Hijo>();

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
                    _ = LoadAbsencesAsync(); // Cargar al cambiar hijo
                }
            }
        }

        private bool _isRefreshing;
        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(nameof(IsRefreshing)); }
        }

        private bool _sinResultados;
        public bool SinResultados
        {
            get => _sinResultados;
            set { _sinResultados = value; OnPropertyChanged(nameof(SinResultados)); }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged(nameof(IsLoading));
                    OnPropertyChanged(nameof(IsNotLoading));
                }
            }
        }

        public bool IsNotLoading => !IsLoading;

        public ICommand LoadAbsencesCommand { get; }

        public StudentAbsencesViewModel()
        {
            _absencesService = new StudentAbsencesService();
            StudentAbsences = new ObservableCollection<StudentAbsence>();
            LoadAbsencesCommand = new Command(async () => await LoadAbsencesAsync());
        }

        public async Task LoadAbsencesAsync()
        {
            IsRefreshing = true;
            SinResultados = false;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");
                string alumnoId = SelectedHijo?.AlumnoId.ToString();

                if (string.IsNullOrEmpty(alumnoId))
                {
                    await DialogsHelper2.ShowErrorMessage("Seleccione un hijo válido.");
                    return;
                }

                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Sesión expirada. Por favor inicie sesión nuevamente.");
                    return;
                }

                var response = await _absencesService.GetAbsencesAsync(token, alumnoId);

                StudentAbsences.Clear();

                if (response?.Data != null && response.Data.Any())
                {
                    foreach (var absence in response.Data)
                        StudentAbsences.Add(absence);

                    SinResultados = StudentAbsences.Count == 0;
                }
                else
                {
                    SinResultados = true;
                }
            }
            catch (Exception ex)
            {
                await DialogsHelper2.ShowErrorMessage("Ocurrió un error inesperado: " + ex.Message);
            }
            finally
            {
                IsRefreshing = false;
                OnPropertyChanged(nameof(StudentAbsences));
            }
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
                            Console.WriteLine($"AlumnoID: {hijo.AlumnoId}, NombreCompleto: {hijo.NombreCompleto}");
                        }
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
                SelectedHijo = Hijos.First(); // Triggea LoadAbsencesAsync()
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
