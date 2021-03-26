using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;

namespace BtcIO_Avalonia
{
    public class PasswordWindow : Window
    {
        public PasswordWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private TextBox pTb;
        private Button OkBtn;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);

            pTb = this.FindControl<TextBox>("pTb");
            OkBtn = this.FindControl<Button>("OkBtn");
        }

        private void Button_Ok_Click(object sender, RoutedEventArgs e)
        {
            Close(pTb.Text);
        }
    }
}
