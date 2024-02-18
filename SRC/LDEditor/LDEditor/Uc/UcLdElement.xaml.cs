using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

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
}