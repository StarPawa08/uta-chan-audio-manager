using System.ComponentModel;
using System.Windows.Media.Imaging;

namespace UtaChanManager.Models;

public class WindowInfo : INotifyPropertyChanged
{
    public int Id { get; init; } = 0;
    public IntPtr Handle { get; set; }
    public string Title { get; init; }
    public string ProcessName { get; set; }
    public BitmapSource? Icon { get; init; }
    public string Key { get; init; }
    
    private bool _isSelected;
    public bool IsSelected
    {
        get => _isSelected;
        set
        {
            if (_isSelected != value)
            {
                _isSelected = value;
                OnPropertyChanged(nameof(IsSelected));
            }
        }
    }

    private bool _isActive;
    public bool IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive == value) return;
            _isActive = value;
            OnPropertyChanged(nameof(IsActive));
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

}