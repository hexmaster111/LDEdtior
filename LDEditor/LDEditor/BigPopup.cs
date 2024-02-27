using System.Windows;
using System.Windows.Controls;

namespace LDEditor;

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