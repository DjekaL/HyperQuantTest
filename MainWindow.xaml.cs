using System.Diagnostics;
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

namespace Connector_Test;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        

        //_ = Run();
    }
    
    public async Task Run() {
        

        var con = new SocketConnector();
        /*con.NewBuyTrade += trade => {
            Trace.WriteLine($"New {trade.Side} trade");
        };*/
        /*con.NewSellTrade += trade => {
            Trace.WriteLine($"New {trade.Side} trade");
        };*/
        await con.ConnectAsync();
        con.SubscribeTrades("tBTCUSD");
        con.SubscribeCandles("tBTCUSD", 60, DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow, 5);
    } 
}