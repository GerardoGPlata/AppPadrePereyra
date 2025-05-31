using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.SchoolDirectories
{
    public class SchoolDirectoryViewModel : INotifyPropertyChanged
    {
        #region Services
        private readonly SchoolDirectoryService _directoryService;
        #endregion

        #region Properties
        private ObservableCollection<SchoolDirectory> _directoryEntries;
        private bool _isRefreshing;
        private bool _sinResultados;

        public ObservableCollection<SchoolDirectory> DirectoryEntries
        {
            get => _directoryEntries;
            set
            {
                _directoryEntries = value;
                OnPropertyChanged(nameof(DirectoryEntries));
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

        public ICommand LoadDirectoryCommand { get; }
        #endregion

        #region Constructor
        public SchoolDirectoryViewModel()
        {
            _directoryService = new SchoolDirectoryService();
            DirectoryEntries = new ObservableCollection<SchoolDirectory>();
            LoadDirectoryCommand = new Command(async () => await LoadDirectoryAsync());
        }
        #endregion

        #region Methods
        public async Task LoadDirectoryAsync()
        {
            IsRefreshing = true;
            SinResultados = false;

            try
            {
                var token = await SecureStorage.GetAsync("auth_token");

                if (string.IsNullOrEmpty(token))
                {
                    await DialogsHelper2.ShowErrorMessage("Sesión expirada. Por favor inicie sesión nuevamente.");
                    return;
                }

                var response = await _directoryService.GetDirectoryAsync(token);

                if (response?.Data != null && response.Data.Count > 0)
                {
                    DirectoryEntries.Clear();

                    foreach (var entry in response.Data)
                    {
                        DirectoryEntries.Add(entry);
                    }
                }
                else
                {
                    SinResultados = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando directorio: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage("Ocurrió un error al cargar el directorio.");
            }
            finally
            {
                IsRefreshing = false;
                OnPropertyChanged(nameof(DirectoryEntries));
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
