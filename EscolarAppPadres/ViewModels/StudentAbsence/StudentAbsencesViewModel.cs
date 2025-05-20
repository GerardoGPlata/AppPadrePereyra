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

namespace EscolarAppPadres.ViewModels.Absences
{
    public class StudentAbsencesViewModel : INotifyPropertyChanged
    {
        private readonly StudentAbsencesService _absencesService;

        private ObservableCollection<StudentAbsence> _studentAbsences;
        private bool _isRefreshing;
        private bool _sinResultados;
        private bool _isLoading;

        public ObservableCollection<StudentAbsence> StudentAbsences
        {
            get => _studentAbsences;
            set { _studentAbsences = value; OnPropertyChanged(nameof(StudentAbsences)); }
        }

        public bool SinResultados
        {
            get => _sinResultados;
            set { _sinResultados = value; OnPropertyChanged(nameof(SinResultados)); }
        }

        public bool IsRefreshing
        {
            get => _isRefreshing;
            set { _isRefreshing = value; OnPropertyChanged(nameof(IsRefreshing)); }
        }

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

                var response = await _absencesService.GetAbsencesAsync(token);

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


        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}
