using System;
using System.Collections;
using System.ComponentModel;
using System.Reflection;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace EscolarAppPadres.Controls
{
    // Picker especializado para selección de hijos con ajuste automático de fuente
    public class ChildPicker : Picker
    {
        const double MinFontSize = 10;
        const double ArrowPadding = 36;
        const double Step = 0.5;

        double _baseFontSize = 13;
        bool _isUpdatingFontSize;

        public ChildPicker()
        {
            SizeChanged += (_, __) => UpdateFontSizing();
            SelectedIndexChanged += (_, __) => UpdateFontSizing();
            PropertyChanged += OnPickerPropertyChanged;
        }

        void OnPickerPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(SelectedItem) ||
                e.PropertyName == nameof(SelectedIndex) ||
                e.PropertyName == nameof(Title) ||
                e.PropertyName == nameof(Width) ||
                e.PropertyName == nameof(FontAttributes) ||
                e.PropertyName == nameof(FontFamily) ||
                e.PropertyName == nameof(ItemsSource))
            {
                UpdateFontSizing();
            }
            else if (e.PropertyName == nameof(FontSize) && !_isUpdatingFontSize)
            {
                _baseFontSize = FontSize > 0 ? FontSize : _baseFontSize;
                UpdateFontSizing();
            }
        }

        protected override void OnBindingContextChanged()
        {
            base.OnBindingContextChanged();
            UpdateFontSizing();
        }

        void UpdateFontSizing()
        {
            if (_isUpdatingFontSize)
            {
                return;
            }

            if (Dispatcher?.IsDispatchRequired ?? false)
            {
                Dispatcher.Dispatch(UpdateFontSizing);
                return;
            }

            _isUpdatingFontSize = true;

            try
            {
                var baseFont = _baseFontSize > 0 ? _baseFontSize : 13;

                if (Width <= 0)
                {
                    return;
                }

                var text = GetCurrentText();

                if (string.IsNullOrWhiteSpace(text))
                {
                    if (Math.Abs(FontSize - baseFont) > double.Epsilon)
                    {
                        FontSize = baseFont;
                    }
                    return;
                }

                var margin = Margin;
                var availableWidth = Math.Max(0, Width - margin.HorizontalThickness - ArrowPadding);
                if (availableWidth <= 0)
                {
                    availableWidth = Width;
                }

                var measurementLabel = new Label
                {
                    HorizontalOptions = LayoutOptions.Fill,
                    HorizontalTextAlignment = TextAlignment.Center,
                    VerticalTextAlignment = TextAlignment.Center,
                    FontAttributes = FontAttributes,
                    FontFamily = FontFamily,
                    LineBreakMode = LineBreakMode.TailTruncation,
                    Text = text
                };

                double targetFontSize = baseFont;
                measurementLabel.FontSize = targetFontSize;

                var request = measurementLabel.Measure(availableWidth, double.PositiveInfinity, MeasureFlags.None);
                if (request.Request.Width > availableWidth)
                {
                    for (double size = baseFont; size >= MinFontSize; size -= Step)
                    {
                        measurementLabel.FontSize = size;
                        request = measurementLabel.Measure(availableWidth, double.PositiveInfinity, MeasureFlags.None);
                        if (request.Request.Width <= availableWidth)
                        {
                            targetFontSize = size;
                            break;
                        }
                    }

                    if (request.Request.Width > availableWidth)
                    {
                        targetFontSize = MinFontSize;
                    }
                }

                if (Math.Abs(FontSize - targetFontSize) > double.Epsilon)
                {
                    FontSize = targetFontSize;
                }
            }
            finally
            {
                _isUpdatingFontSize = false;
            }
        }

        string GetCurrentText()
        {
            if (SelectedItem != null)
            {
                return GetItemText(SelectedItem);
            }

            if (SelectedIndex >= 0)
            {
                if (ItemsSource is IList list && SelectedIndex < list.Count)
                {
                    return GetItemText(list[SelectedIndex]);
                }

                if (ItemsSource is IEnumerable enumerable)
                {
                    var index = 0;
                    foreach (var item in enumerable)
                    {
                        if (index == SelectedIndex)
                        {
                            return GetItemText(item);
                        }
                        index++;
                    }
                }
            }

            return Title ?? string.Empty;
        }

        string GetItemText(object? item)
        {
            if (item == null)
            {
                return string.Empty;
            }

            if (ItemDisplayBinding is Binding binding && !string.IsNullOrWhiteSpace(binding.Path))
            {
                var value = ResolveBindingPath(item, binding.Path);
                if (value != null)
                {
                    return value.ToString() ?? string.Empty;
                }
            }

            return item.ToString() ?? string.Empty;
        }

        static object? ResolveBindingPath(object source, string path)
        {
            var current = source;
            foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                if (current == null)
                {
                    return null;
                }

                var property = current.GetType().GetRuntimeProperty(segment);
                if (property == null)
                {
                    return null;
                }

                current = property.GetValue(current);
            }

            return current;
        }
    }
}
