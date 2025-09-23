using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Syncfusion.Maui.Popup;
using Microsoft.Maui.ApplicationModel;

namespace EscolarAppPadres.Views.Account
{
    public partial class ColorPickerPopup : SfPopup
    {
        private bool _completed;


        public ObservableCollection<string> Colores { get; } = new(new[]
        {
            "#000000","#F87171","#FB923C","#FBBF24","#A3E635","#34D399","#22D3EE",
            "#60A5FA","#818CF8","#C084FC","#F472B6","#94A3B8","#64748B","#475569","#0EA5E9",
            "#EF4444","#F59E0B","#10B981","#059669","#2563EB","#7C3AED","#D946EF","#EA580C"
        });

        public static readonly BindableProperty SelectedColorProperty = BindableProperty.Create(
            nameof(SelectedColor), typeof(string), typeof(ColorPickerPopup), default(string),
            propertyChanged: (bindable, oldVal, newVal) =>
            {
                var popup = (ColorPickerPopup)bindable;
                var hex = newVal as string;
                if (!string.IsNullOrWhiteSpace(hex))
                {
                    popup.ColorChanged?.Invoke(popup, hex);
                }
            }
        );

        public string? SelectedColor
        {
            get => (string?)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public TaskCompletionSource<string?> ResultSource { get; } =
            new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ICommand ColorTappedCommand { get; }

        public event EventHandler<string>? ColorChanged;


        public ColorPickerPopup()
        {
            InitializeComponent();
            BindingContext = this;
            ColorTappedCommand = new Command<string?>(c =>
            {
                if (string.IsNullOrWhiteSpace(c)) return;
                // Solo actualizar selección; no cerrar ni completar resultado aquí
                SelectedColor = c;
            });

            // SelectedColor es TwoWay desde el CollectionView. Si deseas auto-aceptar, puedes hacerlo en el callback de SelectedColorProperty.

            // Si el usuario cierra con la “X”, devolvemos el color seleccionado si no se había completado.
            this.Closed += (_, __) =>
            {
                if (!_completed)
                {
                    _completed = true;
                    ResultSource.TrySetResult(SelectedColor);
                }
            };
        }

        // Sin selección nativa: el tap del item establece SelectedColor

        void OnCancelClicked(object sender, EventArgs e)
        {
            if (!_completed)
            {
                _completed = true;
                ResultSource.TrySetResult(null);
            }
            this.Dismiss();
        }

        void OnAcceptClicked(object sender, EventArgs e)
        {
            if (!_completed)
            {
                _completed = true;
                ResultSource.TrySetResult(SelectedColor);
            }
            this.Dismiss();
        }

    }
}
