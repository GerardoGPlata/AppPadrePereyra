using Syncfusion.Maui.DataGrid;
using EscolarAppPadres.ViewModels.StudentGrade;
using EscolarAppPadres.Models;
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

            // Suscribirse al evento para actualizar las columnas
            _studentGradeViewModel.ColumnsChanged += OnColumnsChanged;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _studentGradeViewModel.InitializeAsync();
        }

        private void OnColumnsChanged(object sender, EventArgs e)
        {
            GenerateTableColumns();
        }

        private void GenerateTableColumns()
        {
            if (_studentGradeViewModel.NombresPeriodos == null || !_studentGradeViewModel.NombresPeriodos.Any())
                return;

            try
            {
                var screenWidth = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density;
                var totalPeriods = _studentGradeViewModel.NombresPeriodos.Count;
                var availableWidth = screenWidth - 40; // Aumentar un poco el margen

                // Reducir el porcentaje de la columna de materia a 15%
                var subjectColumnWidth = Math.Max(70, availableWidth * 0.15);
                var periodColumnWidth = Math.Max(50, (availableWidth - subjectColumnWidth) / (totalPeriods + 1));

                // Configurar encabezados
                SetupHeaders(subjectColumnWidth, periodColumnWidth);

                // Configurar template del CollectionView
                SetupCollectionViewTemplate(subjectColumnWidth, periodColumnWidth);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error al generar columnas: {ex.Message}");
            }
        }

        private void SetupHeaders(double subjectColumnWidth, double periodColumnWidth)
        {
            // Limpiar encabezados existentes
            headerGrid.ColumnDefinitions.Clear();
            headerGrid.Children.Clear();

            // Agregar definiciones de columnas
            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(subjectColumnWidth, GridUnitType.Absolute) });

            foreach (var periodo in _studentGradeViewModel.NombresPeriodos)
            {
                headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(periodColumnWidth, GridUnitType.Absolute) });
            }

            headerGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(periodColumnWidth, GridUnitType.Absolute) });

            // Agregar encabezados
            var materiaHeader = new Label
            {
                Text = "Materia",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(materiaHeader, 0);
            headerGrid.Children.Add(materiaHeader);

            for (int i = 0; i < _studentGradeViewModel.NombresPeriodos.Count; i++)
            {
                var periodoHeader = new Label
                {
                    Text = _studentGradeViewModel.NombresPeriodos[i].Length > 8 ?
                           _studentGradeViewModel.NombresPeriodos[i].Substring(0, 6) + ".." :
                           _studentGradeViewModel.NombresPeriodos[i],
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.White,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                Grid.SetColumn(periodoHeader, i + 1);
                headerGrid.Children.Add(periodoHeader);
            }

            var promedioHeader = new Label
            {
                Text = "Prom.",
                FontSize = 12,
                FontAttributes = FontAttributes.Bold,
                TextColor = Colors.White,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };
            Grid.SetColumn(promedioHeader, _studentGradeViewModel.NombresPeriodos.Count + 1);
            headerGrid.Children.Add(promedioHeader);
        }

        private void SetupCollectionViewTemplate(double subjectColumnWidth, double periodColumnWidth)
        {
            var dataTemplate = new DataTemplate(() =>
            {
                var grid = new Grid
                {
                    Padding = new Thickness(5),
                    HeightRequest = 45,
                    BackgroundColor = Colors.White
                };

                // Agregar línea separadora
                var separator = new BoxView
                {
                    HeightRequest = 1,
                    BackgroundColor = Colors.LightGray,
                    VerticalOptions = LayoutOptions.End
                };
                Grid.SetColumnSpan(separator, _studentGradeViewModel.NombresPeriodos.Count + 2);
                grid.Children.Add(separator);

                // Agregar definiciones de columnas
                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(subjectColumnWidth, GridUnitType.Absolute) });

                foreach (var periodo in _studentGradeViewModel.NombresPeriodos)
                {
                    grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(periodColumnWidth, GridUnitType.Absolute) });
                }

                grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(periodColumnWidth, GridUnitType.Absolute) });

                // Columna de materia
                var materiaLabel = new Label
                {
                    FontSize = 11,
                    TextColor = Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Start,
                    VerticalTextAlignment = TextAlignment.Center,
                    Padding = new Thickness(3, 0),
                    LineBreakMode = LineBreakMode.TailTruncation,
                    MaxLines = 2
                };
                materiaLabel.SetBinding(Label.TextProperty, "NombreCorto");
                Grid.SetColumn(materiaLabel, 0);
                grid.Children.Add(materiaLabel);

                // Columnas de períodos
                for (int i = 0; i < _studentGradeViewModel.NombresPeriodos.Count; i++)
                {
                    var periodoLabel = new Label
                    {
                        FontSize = 12,
                        TextColor = Colors.Black,
                        HorizontalTextAlignment = TextAlignment.Center,
                        VerticalTextAlignment = TextAlignment.Center,
                        BackgroundColor = Colors.Transparent
                    };

                    var periodo = _studentGradeViewModel.NombresPeriodos[i];
                    periodoLabel.SetBinding(Label.TextProperty, new Binding($"CalificacionesPorPeriodo[{periodo}]", stringFormat: "{0:0.0}"));

                    // Agregar gesto de tap
                    var tapGesture = new TapGestureRecognizer();
                    tapGesture.Tapped += (s, e) => OnCellTapped(s, periodo);
                    periodoLabel.GestureRecognizers.Add(tapGesture);

                    Grid.SetColumn(periodoLabel, i + 1);
                    grid.Children.Add(periodoLabel);
                }

                // Columna de promedio
                var promedioLabel = new Label
                {
                    FontSize = 12,
                    FontAttributes = FontAttributes.Bold,
                    TextColor = Colors.Black,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center
                };
                promedioLabel.SetBinding(Label.TextProperty, new Binding("Promedio", stringFormat: "{0:0.0}"));
                Grid.SetColumn(promedioLabel, _studentGradeViewModel.NombresPeriodos.Count + 1);
                grid.Children.Add(promedioLabel);

                return grid;
            });

            gradesCollectionView.ItemTemplate = dataTemplate;
        }

        private async void OnCellTapped(object sender, string periodoDescripcionCorta)
        {
            if (sender is Label label && label.BindingContext is EscolarAppPadres.ViewModels.StudentGrade.SubjectGrades subjectGrades)
            {
                var nombreMateria = subjectGrades.NombreCorto;

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

                _studentGradeViewModel.PopupHeader = nombreMateria.ToUpperInvariant();
                _studentGradeViewModel.PopupPeriod = periodo.DescripcionCorta.ToUpperInvariant();
                _studentGradeViewModel.PopupCriteria = new ObservableCollection<StudentCriteriaGrade>(criterios);

                var total = criterios.Sum(c => c.CalificacionCriterio);
                _studentGradeViewModel.PopupTotal = total.ToString("0.##");

                criteriaPopup.BindingContext = _studentGradeViewModel;
                criteriaPopup.IsOpen = true;
            }
        }
    }
}