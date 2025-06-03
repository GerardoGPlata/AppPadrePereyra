using Syncfusion.Maui.DataGrid;
using EscolarAppPadres.ViewModels.StudentGrade;
using EscolarAppPadres.Models; // <-- Agrega este using
using System.Collections.ObjectModel;

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
            _studentGradeViewModel.SetDataGridReference(dataGrid);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _studentGradeViewModel.InitializeAsync();
        }

        private async void dataGrid_CellTapped(object sender, DataGridCellTappedEventArgs e)
        {
            if (e.RowData is EscolarAppPadres.ViewModels.StudentGrade.SubjectGrades subjectGrades)
            {
                var nombreMateria = subjectGrades.NombreCorto;
                var column = e.Column;
                var periodoDescripcionCorta = column?.MappingName?.Replace("CalificacionesPorPeriodo[", "").Replace("]", "");

                if (string.IsNullOrEmpty(periodoDescripcionCorta))
                    return;

                var periodo = _studentGradeViewModel.EvaluationPeriods.FirstOrDefault(p => p.DescripcionCorta == periodoDescripcionCorta);
                if (periodo == null)
                    return;

                var studentGrade = periodo.Calificaciones.FirstOrDefault(c => c.NombreCorto == nombreMateria);
                if (studentGrade == null)
                    return;

                int materiaId = studentGrade.MateriaId;
                int periodoEvaluacionId = periodo.PeriodoEvaluacionId;

                var criterios = await _studentGradeViewModel.GetCriteriaGradesForSubjectAsync(materiaId, periodoEvaluacionId);

                // Setea los datos para el popup
                _studentGradeViewModel.PopupHeader = nombreMateria.ToUpperInvariant();
                _studentGradeViewModel.PopupPeriod = periodo.DescripcionCorta.ToUpperInvariant();
                _studentGradeViewModel.PopupCriteria = new ObservableCollection<EscolarAppPadres.Models.StudentCriteriaGrade>(criterios);

                // Calcula el total sumando CalificacionCriterio
                var total = criterios.Sum(c => c.CalificacionCriterio);
                _studentGradeViewModel.PopupTotal = total.ToString("0.##");

                // Muestra el popup
                criteriaPopup.BindingContext = _studentGradeViewModel;
                criteriaPopup.IsOpen = true;
            }
        }

        // Cierra el popup al presionar OK
        private void OnPopupOkClicked(object sender, EventArgs e)
        {
            criteriaPopup.IsOpen = false;
        }
    }
}