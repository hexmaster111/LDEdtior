using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using LdLib;
using LdLib.Types;
using Microsoft.VisualBasic;

namespace LDEditor.Uc;

public partial class UcLdElement : UserControl
{
    public UcLdElement()
    {
        InitializeComponent();
        MouseMove += OnMouseMove;
    }


    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            if (sender is Control control && control.DataContext is LdElement elem)
            {
                DragDrop.DoDragDrop(control, elem, DragDropEffects.Move);
            }
        }
    }

    LdElement? DataCtx => DataContext as LdElement;

    private void ChangeLabel_OnClick(object sender, RoutedEventArgs e)
    {
        var newLabel = Interaction.InputBox("Label", "", DataCtx?.Label ?? "");
        if (string.IsNullOrEmpty(newLabel)) return;
        if (DataCtx != null) DataCtx.Label = newLabel;
    }


}