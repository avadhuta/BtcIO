using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using BtcIO;
using BtcWalletTools;
using Application = Avalonia.Application;
using Window = Avalonia.Controls.Window;

namespace BtcIO_Avalonia
{
    public class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private Label ballLabel;
        private TextBox addrTb;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            netCheck1 = this.FindControl<RadioButton>("netCheck1");
            netCheck2 = this.FindControl<RadioButton>("netCheck2");
            newAddrTypeCombo = this.FindControl<ComboBox>("newAddrTypeCombo");
            indexTb = this.FindControl<TextBox>("indexTb");
            seedTb = this.FindControl<TextBox>("seedTb");
            newWifTb = this.FindControl<TextBox>("newWifTb");
            newAddrTb = this.FindControl<TextBox>("newAddrTb");


            ballLabel = this.FindControl<Label>("ballLabel");
            addrTb = this.FindControl<TextBox>("addrTb");


            txLabel = this.FindControl<Label>("txLabel");
            valueTb = this.FindControl<TextBox>("valueTb");
            feeTb = this.FindControl<TextBox>("feeTb");
            wifTb = this.FindControl<TextBox>("wifTb");
            addrFromTb = this.FindControl<TextBox>("addrFromTb");
            addrToTb = this.FindControl<TextBox>("addrToTb");
            TxTb = this.FindControl<TextBox>("TxTb");
        }



        private TextBox valueTb, feeTb, wifTb, addrFromTb, addrToTb, TxTb, indexTb, seedTb, newWifTb, newAddrTb;
        private Label txLabel;
        private ComboBox newAddrTypeCombo;
        private RadioButton netCheck1, netCheck2;


        private void GetBallance_Click(object sender, RoutedEventArgs e)
        {
            string net = "test3";
            var a = addrTb.Text;

            var c0 = a[0];
            if (c0 == '3' || c0 == '1' || c0 == 'b') net = "main";

            var b = WalletTools.GetBallance(a, net);

            ballLabel.Content = b;
        }


        private async void SendBtc_Click(object sender, RoutedEventArgs e)
        {

            decimal value = 0, fee = 0.0001m;
            var pv = decimal.TryParse(valueTb.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            var pf = decimal.TryParse(feeTb.Text,NumberStyles.Any, CultureInfo.InvariantCulture, out fee);

            if (pv)
            {
                var t = WalletTools.Sendbtc2One(wifTb.Text, addrFromTb.Text, addrToTb.Text, value, fee);
                if(t.tx==null) return;

                var m = $"you send to:\n";
                foreach (var p in t.toList) m += $"{p.Key} : {p.Value}\n";

                var trw = new NewTransactionWindow(m);

                var sendAprove = await trw.ShowDialog<bool>(this);

                // var prove = MessageBox.Show(m, "Send Transaction!", MessageBoxButton.YesNo);

                if (sendAprove)
                {
                    var res = WalletTools.PushTx2Bc(t.note, t.tx);
                    //  var res = PushTx2Ninja(netStr, transaction,client);

                    if (res.hash != null)
                    {
                        TxTb.Text = res.hash;
                        txLabel.IsVisible = true;
                        TxTb.IsVisible = true;

                        await Task.Run(() =>
                         {
                             Thread.Sleep(1000);
                             var h = t.tx.GetHash();
                             Tech.OpenBrowser($"https://live.blockcypher.com/btc-testnet/tx/{h}");
                         });
                    }
                }

            }

        }


        private void NewAddrWif()
        {
            if (netCheck1 != null && seedTb != null && newAddrTypeCombo != null && addrTb != null && newAddrTb != null && newWifTb != null)
            {

                var seed = seedTb.Text + indexTb.Text;
                var net = (bool)netCheck1.IsChecked ? "main" : "test3";
                var addrtype = newAddrTypeCombo.SelectedIndex;

                var aw = WalletTools.NewWifAddr(seed, net, addrtype);

                newAddrTb.Text = aw.address;
                newWifTb.Text = aw.wif;

                addrTb.Text = aw.address;
                addrFromTb.Text = aw.address;

                wifTb.Text = aw.wif;
            }
        }

        private void NewAddress_Click(object sender, RoutedEventArgs e)
        {
            seedTb.Text = Tech.RandomSeed();
            NewAddrWif();
        }


        private void seedTb_TextChanged(object sender, KeyEventArgs e)
        {
            NewAddrWif();
        }

        private void seedTb_TextInput(object sender, TextInputEventArgs e)
        {
            NewAddrWif();
        }

        private void NetCheck1_OnChecked(object sender, RoutedEventArgs e)
        {
            NewAddrWif();
        }

        private void NewAddrTypeCombo_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            NewAddrWif();
        }

        private void indexTb_TextChanged(object sender, KeyEventArgs e)
        {
            NewAddrWif();
        }


    }
}
