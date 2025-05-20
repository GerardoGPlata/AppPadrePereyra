using CommunityToolkit.Maui.Views;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace EscolarAppPadres.Views.Account;

public partial class ColorPickerPopup : Popup, INotifyPropertyChanged
{
    public ObservableCollection<string> Colores { get; } = new([
        "#FF5733", "#33B5FF", "#FF33A8", "#33FF57", "#FFC300",
        "#8E44AD", "#1ABC9C", "#E67E22", "#2ECC71", "#3498DB",
        "#F39C12", "#C0392B"
    ]);

    private string? _selectedColor;
    public string? SelectedColor
    {
        get => _selectedColor;
        set
        {
            if (_selectedColor != value)
            {
                _selectedColor = value;
                OnPropertyChanged();
            }
        }
    }

    public TaskCompletionSource<string?> ResultSource { get; set; } = new();
    public ICommand ColorTappedCommand => new Command<string>((color) =>
    {
        SelectedColor = color;
    });


    public ColorPickerPopup()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        ResultSource.TrySetResult(null);
        Close();
    }

    private void OnAcceptClicked(object sender, EventArgs e)
    {
        ResultSource.TrySetResult(SelectedColor);
        Close();
    }

    private void ColorsCollection_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (e.CurrentSelection.FirstOrDefault() is string color)
        {
            SelectedColor = color;
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
