using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace BtcIO_Avalonia
{
    public class NewTransactionWindow : Window
    {
        private string message;

        public NewTransactionWindow()
        {

            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        public NewTransactionWindow(string message)
        {
            this.message = message;
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private TextBlock tb;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            tb = this.FindControl<TextBlock>("tb");
            tb.Text = message;
        }


        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {

            Close(true);
        }

        private void Button_No_Click(object sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
