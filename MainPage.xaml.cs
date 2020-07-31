using Microsoft.Xbox.Services.System;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Graphics.Display;
using Windows.System;
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
        public static User User { get; set; }

        public static string UserKey = Guid.NewGuid().ToString();

        public static string endpoint = "https://75.7.110.224:45146";

        public MainPage()
        {
            if (Debugger.IsAttached)
                endpoint = "https://75.7.110.224:45146";

            ElementSoundPlayer.State = ElementSoundPlayerState.On;
            this.InitializeComponent();
            DataContext = this;

            User = new User() { Id = UserKey };

            Window.Current.CoreWindow.KeyDown += Window_KeyDown;
        }

        //public async void GetUser()
        //{
        //    XboxLiveUser user = new XboxLiveUser();
        //    SignInResult result = await user.SignInSilentlyAsync(Window.Current.Dispatcher);
        //    if (result.Status == SignInStatus.UserInteractionRequired)
        //    {
        //        result = await user.SignInAsync(Window.Current.Dispatcher);
        //    }
        //}

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

        private void One_Player(object sender, RoutedEventArgs e)
        {
            this.Frame.Navigate(typeof(OnePlayer));
        }

        private async void Two_Player(object sender, RoutedEventArgs e)
        {
            string name = await InputTextDialogAsync("Please Enter Your Name");

            if (string.IsNullOrWhiteSpace(name))
            {
                NotificationModal("No name entered", "Please enter a name");
                return;
            }
            else if(name == "returning")
            {
                return;
            }

            this.Frame.Navigate(typeof(OnlineSearch), name);
        }

        private async void NotificationModal(string title, string content)
        {
            ContentDialog dialog = new ContentDialog();
            dialog.Content = content;
            dialog.Title = title;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            await dialog.ShowAsync();
            return;
        }

        private async Task<string> InputTextDialogAsync(string title)
        {
            TextBox inputTextBox = new TextBox();
            inputTextBox.AcceptsReturn = false;
            inputTextBox.Height = 32;
            ContentDialog dialog = new ContentDialog();
            dialog.Content = inputTextBox;
            dialog.Title = title;
            dialog.IsSecondaryButtonEnabled = true;
            dialog.PrimaryButtonText = "Ok";
            dialog.SecondaryButtonText = "Cancel";
            var result = await dialog.ShowAsync();

            if (result == ContentDialogResult.Secondary)
                return "returning";
            else if (result == ContentDialogResult.Primary)
                return inputTextBox.Text;
            else
                return null;
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
