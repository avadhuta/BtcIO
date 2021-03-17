using System;
using System.Collections.Generic;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using BtcWalletTools;

namespace BtcIO_Avalonia
{
    public class DrawEntropyWindow : Window
    {
        public DrawEntropyWindow()
        {
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private Canvas canvas;
        private Label EntropyPrctLb;
        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
            canvas = this.FindControl<Canvas>("canvas");
            EntropyPrctLb = this.FindControl<Label>("EntropyPrctLb");

        }

        Point currentPoint;


        private void Canvas_OnMouseDown(object sender, PointerPressedEventArgs e)
        {
            currentPoint = e.GetPosition(this);
        }

        List<byte> points = new List<byte>();
        private void Canvas_OnMouseMove(object sender, PointerEventArgs e)
        {
            if (true)
            {
                Line line = new Line();

                line.Stroke = new SolidColorBrush(Color.FromRgb(0, 0, 0));
                line.StartPoint = new Point(currentPoint.X, currentPoint.Y);
                line.EndPoint = new Point(e.GetPosition(this).X, e.GetPosition(this).Y);

                points.Add((byte)line.StartPoint.X);
                points.Add((byte)line.StartPoint.Y);
                points.Add((byte)line.EndPoint.X);
                points.Add((byte)line.EndPoint.Y);

                if (points.Count > 3000) Exit();

                EntropyPrctLb.Content = Math.Round(((double) points.Count / 3000) * 100) + "%";

                currentPoint = e.GetPosition(this);

                canvas.Children.Add(line);
            }

        }


        private void Exit()
        {

            var h1 = Tech.Sha256(points.ToArray());
            var h2 = Tech.Sha256(points.ToArray(),2);
            var l = h1.ToList();
            l.AddRange(h2);

            Close(l.ToArray());
        }
    }
}
