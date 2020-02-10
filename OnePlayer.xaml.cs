using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Core;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockPaperScissors
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class OnePlayer : Page, INotifyPropertyChanged
    {
        public static string YourChoice;

        public OnePlayer()
        {
            Title = "Choose your move!";

            this.InitializeComponent();
            DataContext = this;
        }

        private string _title;

        public string Title
        {
            get { return _title; }

            set { this.SetProperty(ref this._title, value); }
        }

        private string _imagePath;

        public string ImagePath
        {
            get { return _imagePath; }

            set { this.SetProperty(ref this._imagePath, value); }
        }

        private string _opponentImagePath;

        public string OpponentImagePath
        {
            get { return _opponentImagePath; }

            set { this.SetProperty(ref this._opponentImagePath, value); }
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

        private void BGRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton rb = sender as RadioButton;

            ElementSoundPlayer.Play(ElementSoundKind.MoveNext);

            if (rb != null)
            {
                string choice = rb.Tag.ToString();

                YourChoice = choice.ToLower();

                switch (choice)
                {
                    case "Rock":
                        ImagePath = @"ms-appx:///Assets/rock.png";
                        break;
                    case "Paper":
                        ImagePath = @"ms-appx:///Assets/paper.png";
                        break;
                    case "Scissors":
                        ImagePath = @"ms-appx:///Assets/scissors.png";
                        break;
                    case "Moo":
                        ImagePath = @"ms-appx:///Assets/moo.png";
                        break;
                }
            }
        }

        private Dictionary<int, string> _choices = new Dictionary<int, string>() { { 1, "rock" }, { 2, "paper" }, { 3, "scissors" } };

        private async void Shoot_Button_Click(object sender, RoutedEventArgs e)
        {
            for(int i = 0; i < 25; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25));

                if (i % 3 == 0)
                    OpponentImagePath = @"ms-appx:///Assets/rock.png";
                else if(i % 3 == 1)
                    OpponentImagePath = @"ms-appx:///Assets/paper.png";
                else if (i % 3 == 2)
                    OpponentImagePath = @"ms-appx:///Assets/scissors.png";
            }

            var randNumber = new Random().Next(1, 4);

            _choices.TryGetValue(randNumber, out var choice);

            switch (choice)
            {
                case "rock":
                    OpponentImagePath = @"ms-appx:///Assets/rock.png";
                    break;
                case "paper":
                    OpponentImagePath = @"ms-appx:///Assets/paper.png";
                    break;
                case "scissors":
                    OpponentImagePath = @"ms-appx:///Assets/scissors.png";
                    break;
                case "moo":
                    OpponentImagePath = @"ms-appx:///Assets/moo.png";
                    break;
            }

            if ((YourChoice == "rock" && choice == "scissors") || (YourChoice == "scissors" && choice == "paper") || (YourChoice == "paper" && choice == "rock"))
            {
                Title = "You Win!";

                MediaElement mysong = new MediaElement();
                Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                Windows.Storage.StorageFile file = await folder.GetFileAsync("tada.mp3");
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
            }
            else if (YourChoice == choice)
            {
                Title = "You tied, try again!";
            }
            else if(YourChoice == "moo")
            {
                OpponentImagePath = @"ms-appx:///Assets/moo.png";
                Title = "MOOOOOOOOOOOOO!";

                MediaElement mysong = new MediaElement();
                Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                Windows.Storage.StorageFile file = await folder.GetFileAsync("cow.mp3");
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
            }
            else
            {
                Title = "You Lose! :(";

                MediaElement mysong = new MediaElement();
                Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                Windows.Storage.StorageFile file = await folder.GetFileAsync("error.mp3");
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
            }
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            ElementSoundPlayer.Play(ElementSoundKind.GoBack);
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
