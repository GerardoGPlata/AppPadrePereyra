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

namespace EscolarAppPadres.ViewModels.Homework
{
    public class StudentHomeworkViewModel : INotifyPropertyChanged
    {
        private readonly HomeworkService _homeworkService;

        private ObservableCollection<StudentHomework> _studentHomework;
        private bool _isRefreshing;
        private bool _sinResultados;
        private bool _isLoading;

        public ObservableCollection<StudentHomework> StudentHomework
        {
            get => _studentHomework;
            set { _studentHomework = value; OnPropertyChanged(nameof(StudentHomework)); }
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

        public ICommand LoadHomeworkCommand { get; }

        public StudentHomeworkViewModel()
        {
            _homeworkService = new HomeworkService();
            StudentHomework = new ObservableCollection<StudentHomework>();
            LoadHomeworkCommand = new Command<int>(async (id) => await LoadHomeworkAsync(id));
        }

        public async Task LoadHomeworkAsync(int profesorMateriaPlanEstudiosId)
        {
            if (!IsRefreshing)
                IsLoading = true;

            SinResultados = false;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Sesión expirada. Por favor inicie sesión nuevamente.");
                    return;
                }

                var response = await _homeworkService.GetStudentHomeworkAsync(token, profesorMateriaPlanEstudiosId);

                StudentHomework.Clear();

                if (response?.Data != null && response.Data.Any())
                {
                    foreach (var homework in response.Data)
                        StudentHomework.Add(homework);

                    SinResultados = StudentHomework.Count == 0;
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
                IsLoading = false;
                OnPropertyChanged(nameof(StudentHomework));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
