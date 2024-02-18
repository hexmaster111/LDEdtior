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

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        UcDoc.DataContext =
            // new LdDocument()
            // {
            // Lines = new ObservableCollection<LdLine>(new[]
            // {
            new LdLine()
            {
                Elements = new ObservableCollection<LdElement>(new[]
                {
                    new LdElement()
                    {
                        ElementType = ElementType.NormallyOpenContact,
                        Label = "START",
                        LinePos = new RowCol { Col = 0, Row = 0 }
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.NormallyOpenContact,
                        Label = "!STOP",
                        LinePos = new RowCol { Col = 1, Row = 0 }
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.NormallyOpenContact,
                        Label = "MEM_0",
                        LinePos = new RowCol() { Col = 0, Row = 1 },
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.Wire,
                        Label = "",
                        LinePos = new RowCol() { Col = 1, Row = 1 },
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.OrWire,
                        Label = "",
                        LinePos = new RowCol() { Col = 2, Row = 0 },
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.OrBranchEnd,
                        Label = "",
                        LinePos = new RowCol() { Col = 2, Row = 1 },
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.Wire,
                        Label = "",
                        LinePos = new RowCol() { Col = 3, Row = 0 },
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.Wire,
                        Label = "",
                        LinePos = new RowCol() { Col = 3, Row = 0 },
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.Wire,
                        Label = "",
                        LinePos = new RowCol() { Col = 4, Row = 0 },
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.Wire,
                        Label = "",
                        LinePos = new RowCol() { Col = 5, Row = 0 },
                    },
                    new LdElement()
                    {
                        ElementType = ElementType.Coil,
                        Label = "MEM_0",
                        LinePos = new RowCol() { Col = 5, Row = 0 },
                    },
                })
                // }
                // })
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
                    Label = "???"
                }, DragDropEffects.Move);
            }
        }
    }

    private void NewElem_OnClick(object sender, RoutedEventArgs e)
    {
        if (sender is MenuItem mi && mi.Tag is string str)
        {
            if (!Enum.TryParse(str, out ElementType elementType)) return;

            DragDrop.DoDragDrop(mi, new LdElement()
            {
                ElementType = elementType,
                Label = "???"
            }, DragDropEffects.Move);
        }
    }
}