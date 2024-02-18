using System.Net;
using System.Text;
using System.Windows;


namespace WpfApp1;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public class MainWindow
{


    private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
    {
        var ip = "192.168.1.1";


        if (!IPAddress.TryParse(ip, out var ipAddress))
        {
            MessageBox.Show("invalid IP address");
            return;
        }
        
        var bytes = ipAddress.GetAddressBytes();
        
        var sb = new StringBuilder();
        foreach (var b in bytes)
        {
            sb.Append(b.ToString("X2"));
        }
        
        MessageBox.Show(sb.ToString());
    }
}