using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace LDEditor.Uc;

public partial class UcLdLine : UserControl
{
    public UcLdLine()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
    }


    private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (e.OldValue != null)
        {
            if (e.OldValue is not LdLine oldLine) return;

            oldLine.Elements.CollectionChanged -= OnElementsChanged;
        }

        if (e.NewValue is not LdLine line) throw new Exception("Wrong type for the uc");
        line.Elements.CollectionChanged += OnElementsChanged;
        OnElementsChanged(this.DataContext,
            new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
    }

    private void OnElementsChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        CtrlGrid.RowDefinitions.Clear();
        CtrlGrid.Children.Clear();

        if (sender is not LdLine line) return;

        var rows = line.GetMaxRows() + 1;

        for (int i = 0; i < rows; i++)
        {
            CtrlGrid.RowDefinitions.Add(new RowDefinition());
        }

        Height = 64 * rows;


        foreach (var elem in line.Elements)
        {
            var uc = new UcLdElement()
            {
                DataContext = elem,
            };
            CtrlGrid.Children.Add(uc);
            Grid.SetRow(uc, elem.LinePos.Row);
            Grid.SetColumn(uc, elem.LinePos.Col);
        }
    }

    const string LdEditorElementFormatString = nameof(LDEditor) + "." + nameof(LdElement);

    private void UcLdLine_OnDrop(object sender, DragEventArgs e)
    {
        e.Effects = DragDropEffects.None;
        if (e.Data.GetDataPresent(LdEditorElementFormatString))
        {
            if (e.Data.GetData(LdEditorElementFormatString) is not LdElement elemDropped) return;

            var hit = VisualTreeHelper.HitTest(this, e.GetPosition(this));
            if (hit == null) return;

            var grid = hit.VisualHit.Parent<Grid>();
            if (grid == null) return;

            var gridPosition = grid.GetColumnRow(e.GetPosition(grid));

            elemDropped.LinePos = new RowCol() { Row = (int)gridPosition.Y, Col = (int)gridPosition.X };
            var line = DataContext as LdLine;
            if (line == null) throw new Exception("Wrong type");
            if (!line.Elements.Contains(elemDropped)) line.Elements.Add(elemDropped);
            OnElementsChanged(line, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }

    private void UcLdLine_OnPreviewDrop(object sender, DragEventArgs e)
    {
    }

    private void UcLdLine_OnPreviewDragEnter(object sender, DragEventArgs e)
    {
    }

    private void UcLdLine_OnPreviewDragLeave(object sender, DragEventArgs e)
    {
    }


    private void UcLdLine_OnPreviewDragOver(object sender, DragEventArgs e)
    {
    }
}