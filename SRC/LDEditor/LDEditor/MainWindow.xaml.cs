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

namespace LDEditor;

public class MainWindowViewModel : NotifyObject
{
    private LdDocument _ActiveDocument = new LdDocument();

    public LdDocument ActiveDocument
    {
        get => _ActiveDocument;
        set => SetField(ref _ActiveDocument, value);
    }
}

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly MainWindowViewModel _vm;

    public MainWindow()
    {
        InitializeComponent();


        this.DataContext = _vm = new MainWindowViewModel()
        {
        };
    }

    private void NewElem_MouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (sender is MenuItem mi && mi.Tag is string str)
            {
                if (!Enum.TryParse(str, out ElementType elementType)) return;

                DragDrop.DoDragDrop(mi, new LdElement()
                {
                    ElementType = elementType,
                    Label = elementType switch
                    {
                        ElementType.Wire => string.Empty,
                        ElementType.OrWire => string.Empty,
                        ElementType.OrBranchEnd => string.Empty,
                        ElementType.OrBranchStart => string.Empty,
                        _ => "???"
                    }
                }, DragDropEffects.Move);
            }
        }
    }

    private void NewElem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem mi && mi.Tag is string str)
        {
            if (!Enum.TryParse(str, out ElementType elementType)) return;
        }
    }

    private void AddRung_OnClick(object sender, RoutedEventArgs e)
    {
        _vm.ActiveDocument.Lines.Add(new LdLine()
        {
            Elements = new ObservableCollection<LdElement>(new[]
            {
                new LdElement()
                {
                    ElementType = ElementType.Wire, LinePos = new() { Row = 0, Col = 0 }
                }
            })
        });
    }
}