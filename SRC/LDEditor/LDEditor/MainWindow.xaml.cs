using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LDEditor.Uc;
using LdLib.Types;
using Newtonsoft.Json;

namespace LDEditor;

public class UiOptions : NotifyObject
{
    private bool _ShowRowColOnUiElements = false;

    public bool ShowRowColOnUiElements
    {
        get => _ShowRowColOnUiElements;
        set => SetFieldAndTell(ref _ShowRowColOnUiElements, value,
            nameof(ShowRowColOnUiElements),
            nameof(ShowRowColOnUiElementsVisibility)
        );
    }

    public Visibility ShowRowColOnUiElementsVisibility =>
        ShowRowColOnUiElements ? Visibility.Visible : Visibility.Collapsed;
}

public class MainWindowViewModel : NotifyObject
{
    public UiOptions UiOptions { get; } = MainWindow.UiOptions;

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

    public static UiOptions UiOptions { get; set; } = new();

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

    private void CompOut_OnClick(object sender, RoutedEventArgs e)
    {
        BigPopup.ShowDialog(_vm.ActiveDocument.Lines.First().GetLogicalStatement());
    }

    private void Save_OnClick(object sender, RoutedEventArgs e)
    {
        var data = JsonConvert.SerializeObject(_vm.ActiveDocument);
        File.WriteAllText("Save.json", data);
    }

    private void Load_OnClick(object sender, RoutedEventArgs e)
    {
        if (File.Exists("Save.json"))
        {
            var text = File.ReadAllText("Save.json");
            var obj = JsonConvert.DeserializeObject<LdDocument>(text);
            if (obj != null) _vm.ActiveDocument = obj;
        }
    }
}

internal static class BigPopup
{
    public static void ShowDialog(string text)
    {
        var wind = new Window()
        {
            Height = 266,
            Content = new Viewbox()
            {
                Child = new TextBlock()
                {
                    Text = text
                }
            }
        };
        wind.ShowDialog();
    }
}