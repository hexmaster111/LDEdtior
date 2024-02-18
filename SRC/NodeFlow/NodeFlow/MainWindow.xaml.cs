using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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
using NodeGraph;
using NodeGraph.Model;

namespace NodeFlow;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    readonly MainWindowViewModule _vm = new();

    public MainWindow()
    {
        InitializeComponent();
        DataContext = _vm;

        Loaded += (o, s) =>
        {
            NodeGraphManager.OutputDebugInfo = true;
            NodeGraphManager.SelectionMode = NodeGraph.SelectionMode.Include;

            FlowChart flowChart = NodeGraphManager.CreateFlowChart(false, Guid.NewGuid(), typeof(FlowChart));
            _vm.FlowChartViewModel = flowChart.ViewModel;
        };
    }
}

public class MainWindowViewModule : NotifyObj
{
    private NodeGraph.ViewModel.FlowChartViewModel _flowChartViewModel = null!;

    public NodeGraph.ViewModel.FlowChartViewModel FlowChartViewModel
    {
        get => _flowChartViewModel;
        set => SetField(ref _flowChartViewModel, value);
    }

  

}

public class NotifyObj : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}