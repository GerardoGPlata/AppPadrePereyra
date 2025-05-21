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
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _studentGradeViewModel.InitializeAsync();
        }
    }

}
