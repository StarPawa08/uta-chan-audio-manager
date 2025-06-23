using System.Collections.ObjectModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using UtaChanManager.Services;
using UtaChanManager.Utils;
using UtaChanManager.ViewModels;

namespace UtaChanManager;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly WindowSelectionVm _vm = new();
    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;
        _vm.Refresh();
        LoadUserConfig();
    }
    
    private void Refresh_Click(object sender, RoutedEventArgs e)
    {
        ConfigManager.Save(_vm.SelectedPriorities.ToList());
        _vm.Refresh();
    }

    protected override void OnClosed(EventArgs e)
    {
        _ = AudioManager.UnMuteAllAsync();
        base.OnClosed(e);
        ConfigManager.Save(_vm.SelectedPriorities.ToList());
    }

    private void LoadUserConfig()
    {
        var saved = ConfigManager.Load();
        _vm.SelectedPriorities = new ObservableCollection<string>(saved);
    }
}