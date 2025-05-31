using EscolarAppPadres.Helpers;
using EscolarAppPadres.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EscolarAppPadres.ViewModels.Academy
{
    public class AcademyViewModel : INotifyPropertyChanged
    {
        #region Properties

        private ObservableCollection<Hijo> _hijos;
        public ObservableCollection<Hijo> Hijos
        {
            get => _hijos;
            set
            {
                _hijos = value;
                OnPropertyChanged(nameof(Hijos));
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
                    _ = OnHijoSelectedAsync(); // Ejecutar lógica al seleccionar hijo
                }
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        #endregion

        #region Commands

        public ICommand LoadChildrenCommand { get; }

        #endregion

        #region Constructor

        public AcademyViewModel()
        {
            Hijos = new ObservableCollection<Hijo>();
            LoadChildrenCommand = new Command(async () => await LoadChildrenData());
        }

        #endregion

        #region Methods

        public async Task LoadChildrenData()
        {
            try
            {
                IsLoading = true;

                var childrenJson = await SecureStorage.GetAsync("Hijos");

                if (!string.IsNullOrEmpty(childrenJson))
                {
                    var hijosList = JsonSerializer.Deserialize<ObservableCollection<Hijo>>(childrenJson);

                    if (hijosList != null)
                    {
                        Hijos.Clear();

                        foreach (var hijo in hijosList)
                        {
                            Hijos.Add(hijo);
                        }

                        if (Hijos.Count > 0)
                            SelectedHijo = Hijos[0]; // Selecciona el primero por defecto
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al cargar hijos: {ex.Message}");
                await DialogsHelper2.ShowErrorMessage("Ocurrió un error al cargar los hijos.");
            }
            finally
            {
                IsLoading = false;
            }
        }



        private async Task OnHijoSelectedAsync()
        {
            if (SelectedHijo == null)
                return;

            // Aquí puedes cargar datos adicionales relacionados al hijo seleccionado
            Console.WriteLine($"Hijo seleccionado: {SelectedHijo.NombreCompleto}");

            // Por ejemplo, si luego necesitas cargar academias del hijo:
            // await LoadAcademiesForStudent(SelectedHijo.AlumnoId);
        }

        public async Task InitializeAsync()
        {
            await LoadChildrenData();
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        #endregion
    }

}
