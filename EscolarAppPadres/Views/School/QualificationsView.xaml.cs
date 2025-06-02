using EscolarAppPadres.ViewModels.StudentGrade;

namespace EscolarAppPadres.Views.School
{
    public partial class QualificationsView : ContentPage
    {
        private readonly StudentGradeViewModel _studentGradeViewModel;

        public QualificationsView()
        {
            InitializeComponent();
            _studentGradeViewModel = new StudentGradeViewModel();
            BindingContext = _studentGradeViewModel;

            // Pasar la referencia del DataGrid al ViewModel para columnas dinámicas
            _studentGradeViewModel.SetDataGridReference(dataGrid);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            // Inicializar datos después de que la página sea visible
            await _studentGradeViewModel.InitializeAsync();
        }
    }
}