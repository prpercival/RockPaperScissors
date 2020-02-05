using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=402352&clcid=0x409

namespace RockPaperScissors
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class MainPage : Page
    {
        public MainPage()
        {
            ElementSoundPlayer.State = ElementSoundPlayerState.On;
            this.InitializeComponent();
            DataContext = this;

            Window.Current.CoreWindow.KeyDown += Window_KeyDown;
        }

        // Property Change Logic
        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (object.Equals(storage, value)) return false;
            storage = value;
            this.OnPropertyChaned(propertyName);
            return true;
        }

        private void OnPropertyChaned(string propertyName)
        {
            var eventHandler = this.PropertyChanged;
            if (eventHandler != null)
                eventHandler(this, new PropertyChangedEventArgs(propertyName));
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(RockPaperScissors));
        }

        private double Mod(double k, double n) { return ((k %= n) < 0) ? k + n : k; }

        private void Window_KeyDown(Windows.UI.Core.CoreWindow sender, Windows.UI.Core.KeyEventArgs e)
        {
            //var bounds = ApplicationView.GetForCurrentView().VisibleBounds;
            //var scaleFactor = DisplayInformation.GetForCurrentView().RawPixelsPerViewPixel;
            //var size = new Size(bounds.Width * scaleFactor, bounds.Height * scaleFactor);

            //var offset = arrow.TransformToVisual(grid).TransformPoint(new Point(0, 0));

            //if (e.VirtualKey == Windows.System.VirtualKey.Up)
            //{
            //    myTranslateTransform.Y = Mod((offset.Y - 50.0), size.Height);     
            //    //myRotateTransform.Angle = 0;
            //}
            //if (e.VirtualKey == Windows.System.VirtualKey.Down)
            //{
            //    myTranslateTransform.Y = Mod((offset.Y + 50.0), size.Height);
            //    //myRotateTransform.Angle = 180;
            //}
            //if (e.VirtualKey == Windows.System.VirtualKey.Right)
            //{
            //    myTranslateTransform.X = Mod((offset.X + 50.0), size.Width);
            //    //myRotateTransform.Angle = 90;
            //}
            //if (e.VirtualKey == Windows.System.VirtualKey.Left)
            //{
            //    myTranslateTransform.X = Mod((offset.X - 50.0), size.Width);
            //    //myRotateTransform.Angle = 270;
            //}

            ////myRotateTransform.CenterX = arrow.CenterPoint.X;
            ////myRotateTransform.CenterY = arrow.CenterPoint.Y;

            //offset = arrow.TransformToVisual(grid).TransformPoint(new Point(0, 0));

            //if (e.VirtualKey == Windows.System.VirtualKey.Up)
            //{
            //    //myRotateTransform.Angle = 0;
            //}
            //if (e.VirtualKey == Windows.System.VirtualKey.Down)
            //{
            //    //myRotateTransform.Angle = 180;
            //}
            //if (e.VirtualKey == Windows.System.VirtualKey.Right)
            //{
            //    //myRotateTransform.Angle = 90;
            //}
            //if (e.VirtualKey == Windows.System.VirtualKey.Left)
            //{
            //    //myRotateTransform.Angle = 270;
            //}

            //Debug.WriteLine($"({offset.X}, {offset.Y})");
        }
    }
}
