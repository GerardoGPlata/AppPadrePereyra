using Microsoft.Maui.Handlers;

namespace EscolarAppPadres.Handlers
{
    public static class ControlStylingMapper
    {
        // Estilo para Entry
        public static void ApplyEntryStyle()
        {
            EntryHandler.Mapper.AppendToMapping(nameof(BorderEntry), (handler, view) =>
            {
                if (view is BorderEntry)
                {
#if __ANDROID__
                    handler.PlatformView.SetBackgroundColor(Android.Graphics.Color.Transparent);
#elif __IOS__
                    handler.PlatformView.BorderStyle = UIKit.UITextBorderStyle.None;
#endif
                }
            });
        }

        // Estilo para RadioButton
        public static void ApplyRadioButtonStyle()
        {
            RadioButtonHandler.Mapper.AppendToMapping(nameof(RadioButton), (handler, view) =>
            {
#if __ANDROID__
                var radioButtonNative = handler.PlatformView as Android.Widget.RadioButton;
                if (radioButtonNative != null)
                {
                    var states = new int[][]
                    {
                        new int[] { Android.Resource.Attribute.StateChecked },
                        new int[] { -Android.Resource.Attribute.StateChecked }
                    };
                    var colors = new int[]
                    {
                        Android.Graphics.Color.ParseColor("#8A2BE2"),
                        Android.Graphics.Color.ParseColor("#000000")
                    };

                    var colorStateList = new Android.Content.Res.ColorStateList(states, colors);
                    radioButtonNative.ButtonTintList = colorStateList;
                    radioButtonNative.SetPadding(10, 10, 10, 10);
                }
#elif __IOS__
                if (handler.PlatformView is Microsoft.Maui.Platform.ContentView contentView)
                {
                    var borderColor = UIKit.UIColor.Black.CGColor;
                    var selectedColor = UIKit.UIColor.FromRGB(138, 43, 226);
                    var unselectedColor = UIKit.UIColor.Clear;

                    contentView.Layer.BorderColor = borderColor;
                    contentView.Layer.BorderWidth = 2;
                    contentView.BackgroundColor = unselectedColor;

                    if (view is RadioButton radioButton)
                    {
                        radioButton.CheckedChanged += (s, e) =>
                        {
                            contentView.BackgroundColor = radioButton.IsChecked ? selectedColor : unselectedColor;
                        };
                        contentView.BackgroundColor = radioButton.IsChecked ? selectedColor : unselectedColor;
                    }
                }
#endif
            });
        }

        // Estilo para CheckBox
        public static void ApplyCheckBoxStyle()
        {
            CheckBoxHandler.Mapper.AppendToMapping(nameof(CheckBox), (handler, view) =>
            {
#if __ANDROID__
                var checkBoxNative = handler.PlatformView as Android.Widget.CheckBox;
                if (checkBoxNative != null)
                {
                    var states = new int[][]
                    {
                        new int[] { Android.Resource.Attribute.StateChecked },
                        new int[] { -Android.Resource.Attribute.StateChecked }
                    };

                    var colors = new int[]
                    {
                        Android.Graphics.Color.ParseColor("#8A2BE2"),
                        Android.Graphics.Color.ParseColor("#000000")
                    };

                    var colorStateList = new Android.Content.Res.ColorStateList(states, colors);
                    checkBoxNative.ButtonTintList = colorStateList;
                    checkBoxNative.SetPadding(10, 10, 10, 10);
                }
#elif __IOS__
                if (handler.PlatformView is UIKit.UIButton checkBoxNative)
                {
                    var borderColor = UIKit.UIColor.Black.CGColor;
                    var selectedColor = UIKit.UIColor.FromRGB(138, 43, 226);
                    var unselectedColor = UIKit.UIColor.Clear;

                    checkBoxNative.Layer.BorderColor = borderColor;
                    checkBoxNative.Layer.BorderWidth = 2;
                    checkBoxNative.BackgroundColor = unselectedColor;

                    if (view is CheckBox checkBox)
                    {
                        checkBox.CheckedChanged += (s, e) =>
                        {
                            checkBoxNative.BackgroundColor = checkBox.IsChecked ? selectedColor : unselectedColor;
                        };
                    }
                }
#endif
            });
        }

        // Método global para aplicar todos los estilos
        public static void ApplyAllControlStyles()
        {
            ApplyEntryStyle();
            ApplyRadioButtonStyle();
            ApplyCheckBoxStyle();
        }
    }
}
