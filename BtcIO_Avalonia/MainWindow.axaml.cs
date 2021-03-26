using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using BtcWalletTools.Your.Namespace.Security;
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
        private CheckBox entropyChB, useTorCh;

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            newAddrTypeCombo = this.FindControl<ComboBox>("newAddrTypeCombo");
            netTypeCombo = this.FindControl<ComboBox>("netTypeCombo");
            indexTb = this.FindControl<TextBox>("indexTb");
            seedTb = this.FindControl<TextBox>("seedTb");
            newWifTb = this.FindControl<TextBox>("newWifTb");
            newAddrTb = this.FindControl<TextBox>("newAddrTb");
            entropyChB = this.FindControl<CheckBox>("entropyChB");


            ballLabel = this.FindControl<Label>("ballLabel");
            addrTb = this.FindControl<TextBox>("addrTb");


            txLabel = this.FindControl<Label>("txLabel");
            valueTb = this.FindControl<TextBox>("valueTb");
            feeTb = this.FindControl<TextBox>("feeTb");
            wifTb = this.FindControl<TextBox>("wifTb");
            addrFromTb = this.FindControl<TextBox>("addrFromTb");
            addrToTb = this.FindControl<TextBox>("addrToTb");
            TxTb = this.FindControl<TextBox>("TxTb");
            useTorCh = this.FindControl<CheckBox>("useTorCh");



        }



        private TextBox valueTb, feeTb, wifTb, addrFromTb, addrToTb, TxTb, indexTb, seedTb, newWifTb, newAddrTb;
        private Label txLabel;
        private ComboBox newAddrTypeCombo, netTypeCombo;


        private void GetBallance_Click(object sender, RoutedEventArgs e)
        {
            string net = "test3";
            var a = addrTb.Text;
            if (a == null) return;

            var c0 = a[0];
            if (c0 == '3' || c0 == '1' || c0 == 'b') net = "main";

            var b = WalletTools.GetBallance(a, net);

            ballLabel.Content = b;
        }



        private async void SendBtc_Click(object sender, RoutedEventArgs e)
        {

            decimal value = 0, fee = 0.0001m;
            var pv = decimal.TryParse(valueTb.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
            var pf = decimal.TryParse(feeTb.Text, NumberStyles.Any, CultureInfo.InvariantCulture, out fee);

            if (pv)
            {
                var t = WalletTools.Sendbtc2One(wifTb.Text, addrFromTb.Text, addrToTb.Text, value, fee);
                if (t.tx == null) return;

                var m = $"you send to:\n";
                foreach (var p in t.toList) m += $"{p.Key} : {p.Value}\n";

                var trw = new NewTransactionWindow(m);

                var sendAprove = await trw.ShowDialog<bool>(this);

                // var prove = MessageBox.Show(m, "Send Transaction!", MessageBoxButton.YesNo);

                if (sendAprove)
                {
                    var res = WalletTools.PushTx2Bc(t.note, t.tx, (bool)useTorCh.IsChecked);
                    //  var res = PushTx2Ninja(netStr, transaction,client);

                    if (res.suc && res.hash.Length > 5)
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
            if (netTypeCombo != null && seedTb != null && newAddrTypeCombo != null && addrTb != null && newAddrTb != null && newWifTb != null)
            {
                if (string.IsNullOrEmpty(seedTb.Text))
                {
                    newAddrTb.Text = "";
                    newWifTb.Text = "";
                    return;
                }
                var seed = seedTb.Text + indexTb.Text;
                var net = netTypeCombo.SelectedIndex == 0 ? "main" : "test3";
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
            seedTb.Text = Tech.RandomSeed(userEntropy);
            NewAddrWif();
        }



        private async void Save_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var seed = seedTb.Text;

                var sfd = new SaveFileDialog();
                var p = await sfd.ShowAsync(this);

                if (p!=null && seed != null && seed!="")
                {
                    var pw = new PasswordWindow();
                    var pass = await pw.ShowDialog<string>(this);

                    string walletText = "";
                    if (pass != null) walletText = AesCrypto.Encrypt(seed, pass);
                    else walletText = seed;

                    File.WriteAllText(p, walletText);
                }
            }
            catch (Exception exception)
            {

            }
        }

        private async void Open_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                OpenFileDialog ofd = new OpenFileDialog();
                var p = await ofd.ShowAsync(this);


                if (p.Length > 0)
                {
                    var pw = new PasswordWindow();
                    var pass = await pw.ShowDialog<string>(this);

                    var txt = File.ReadAllText(p[0]);

                    string seed;
                    if (pass != null) seed = AesCrypto.Decrypt(txt, pass);
                    else seed = txt;

                    seedTb.Text = seed;

                    NewAddrWif();
                }
            }
            catch (Exception exception)
            {

            }
        }


        private void seedTb_TextChanged(object sender, KeyEventArgs e)
        {
            entropyChB.IsChecked = false;
            NewAddrWif();
        }

        private void seedTb_TextInput(object sender, TextInputEventArgs e)
        {
            entropyChB.IsChecked = false;
            NewAddrWif();
        }

        private byte[] userEntropy = null;
        private async void Entropy_OnChecked(object sender, RoutedEventArgs e)
        {
            var dew = new DrawEntropyWindow();

            userEntropy = await dew.ShowDialog<byte[]>(this);
            seedTb.Text = Tech.RandomSeed(userEntropy);
            NewAddrWif();
        }

        private async void Entropy_UnChecked(object sender, RoutedEventArgs e)
        {

            userEntropy = null;
            seedTb.Text = Tech.RandomSeed(userEntropy);
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
        private void indexTb_TextInput(object sender, TextInputEventArgs e)
        {
            NewAddrWif();
        }

    }
}
