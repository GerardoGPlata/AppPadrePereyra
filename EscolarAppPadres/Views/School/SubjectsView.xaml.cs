using EscolarAppPadres.Models;
using EscolarAppPadres.Services;
using EscolarAppPadres.ViewModels.Subjects;
using Microsoft.Maui.Controls;
using System.Linq;
using System.Threading.Tasks;

namespace EscolarAppPadres.Views.School
{
    public partial class SubjectsView : ContentPage
    {
        private readonly SubjectsViewModel _viewModel;

        public SubjectsView()
        {
            InitializeComponent();
            _viewModel = new SubjectsViewModel();
            _viewModel.OnSubjectPopupRequested += ShowPopup;
            BindingContext = _viewModel;
        }

        private void ShowPopup(object sender, string materia)
        {
            sfPopup.HeaderTitle = $"Tareas de {materia}";
            //sfPopup.IsFullScreen = true;
            sfPopup.IsOpen = true;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadSubjectsAsync();
        }
    }

}
