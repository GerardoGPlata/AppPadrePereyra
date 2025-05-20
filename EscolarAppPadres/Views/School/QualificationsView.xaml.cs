using EscolarAppPadres.ViewModels.StudentGrade;
using Syncfusion.Maui.DataGrid;

namespace EscolarAppPadres.Views.School;

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

        await _studentGradeViewModel.LoadGradesAsync();

        //// Limpiar columnas existentes de periodos
        //var columnasExistentes = GradesGrid.Columns
        //    .Where(c => c.MappingName != "MateriaNombre")
        //    .ToList();
        //foreach (var col in columnasExistentes)
        //{
        //    GradesGrid.Columns.Remove(col);
        //}

        //// Agregar columnas para cada periodo
        //foreach (var periodo in _studentGradeViewModel.NombresPeriodos)
        //{
        //    GradesGrid.Columns.Add(new DataGridTextColumn
        //    {
        //        MappingName = $"CalificacionesPorPeriodo[{periodo}]",
        //        HeaderText = periodo
        //    });
        //}
    }

}