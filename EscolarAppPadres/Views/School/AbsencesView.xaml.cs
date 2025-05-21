using EscolarAppPadres.ViewModels.Absences;

namespace EscolarAppPadres.Views.School
{
    public partial class AbsencesView : ContentPage
    {
        private readonly StudentAbsencesViewModel _studentAbsenceViewModel;

        public AbsencesView()
        {
            InitializeComponent();
            _studentAbsenceViewModel = new StudentAbsencesViewModel();
            BindingContext = _studentAbsenceViewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _studentAbsenceViewModel.InitializeAsync();
        }
    }
}
