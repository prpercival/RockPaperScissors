using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
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
    public partial class TwoPlayer : Page, INotifyPropertyChanged
    {
        public static Choice YourChoice;

        private User _user;

        public User User
        {
            get { return _user; }
            set { this.SetProperty(ref this._user, value); }
        }

        private User _opponent;

        public User Opponent 
        {
            get { return _opponent; }
            set { this.SetProperty(ref this._opponent, value); }
        }

        private int _myScore;

        public int MyScore 
        {
            get { return _myScore; }
            set {
                Score = $"{MyScore} - {OpponentScore}";
                _myScore = value;
            }
        }

        private int _opponentScore
        {
            get { return _opponentScore; }
            set 
            {
                Score = $"{MyScore} - {OpponentScore}";
                _opponentScore = value;
            }
        }

        public int OpponentScore { get; set; }

        private string _score;

        public string Score
        {
            get { return _score; }
            set {
                this.SetProperty(ref this._score, value);
            }
        }

        HttpClient client;

        public TwoPlayer()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            client = new HttpClient(httpClientHandler);

            Title = "Waiting for opponent to join...";
            Opponent = OnlineSearch.Lobby.Users.Where(x => x.Key != MainPage.UserKey).FirstOrDefault().Value;
            User = MainPage.User;

            MyScore = 0;
            OpponentScore = 0;           

            this.InitializeComponent();
            DataContext = this;

            var t = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => WaitForOpponent());
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
            this.OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged(string propertyName)
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

                Enum.TryParse<Choice>(choice, out YourChoice);

                switch (YourChoice)
                {
                    case Choice.Rock:
                        ImagePath = @"ms-appx:///Assets/rock.png";
                        break;
                    case Choice.Paper:
                        ImagePath = @"ms-appx:///Assets/paper.png";
                        break;
                    case Choice.Scissors:
                        ImagePath = @"ms-appx:///Assets/scissors.png";
                        break;
                    case Choice.Moo:
                        ImagePath = @"ms-appx:///Assets/moo.png";
                        break;
                }
            }
        }

        private Dictionary<int, string> _choices = new Dictionary<int, string>() { { 1, "rock" }, { 2, "paper" }, { 3, "scissors" } };

        private async void WaitForOpponent()
        {
            while(OnlineSearch.Lobby != null && OnlineSearch.Lobby.Users.Count < 2)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(OnlineSearch.Lobby);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{MainPage.endpoint}/api/search/getlobby", data);

                data.Dispose();

                var results = await response.Content.ReadAsStringAsync();

                var lobby = Newtonsoft.Json.JsonConvert.DeserializeObject<Lobby>(results);

                OnlineSearch.Lobby = lobby;

                if (lobby == null)
                    return;

                Opponent = OnlineSearch.Lobby.Users.Where(x => x.Key != MainPage.UserKey).FirstOrDefault().Value;

                await Task.Delay(TimeSpan.FromMilliseconds(500));
            }

            Title = "Choose your move!";
        }

        private async void Shoot_Button_Click(object sender, RoutedEventArgs e)
        {
            var user = MainPage.User;

            user.Choice = YourChoice;

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{MainPage.endpoint}/api/search/shoot", data);

            data.Dispose();

            var httpData = await response.Content.ReadAsStringAsync();

            var lobby = Newtonsoft.Json.JsonConvert.DeserializeObject<Lobby>(httpData);

            var isReady = lobby.Users.All(x => x.Value.Choice != null) && lobby.Users.Count > 1;

            Choice? choice = null;

            Title = "Waiting on other player...";
            OpponentImagePath = "";

            while (!isReady)
            {
                json = Newtonsoft.Json.JsonConvert.SerializeObject(user);

                data = new StringContent(json, Encoding.UTF8, "application/json");

                response = await client.PostAsync($"{MainPage.endpoint}/api/search/shoot", data);

                data.Dispose();

                httpData = await response.Content.ReadAsStringAsync();

                lobby = Newtonsoft.Json.JsonConvert.DeserializeObject<Lobby>(httpData);

                if (lobby == null)
                    return;
             
                isReady = lobby.Users.All(x => x.Value.Choice != null) && lobby.Users.Count > 1;

                await Task.Delay(TimeSpan.FromMilliseconds(100));
            }

            for (int i = 0; i < 25; i++)
            {
                await Task.Delay(TimeSpan.FromMilliseconds(25));

                if (i % 3 == 0)
                    OpponentImagePath = @"ms-appx:///Assets/rock.png";
                else if (i % 3 == 1)
                    OpponentImagePath = @"ms-appx:///Assets/paper.png";
                else if (i % 3 == 2)
                    OpponentImagePath = @"ms-appx:///Assets/scissors.png";
            }

            var opponent = lobby.Users.Where(x => x.Key != MainPage.UserKey).FirstOrDefault();
            Opponent = opponent.Value;

            choice = opponent.Value?.Choice;

            json = Newtonsoft.Json.JsonConvert.SerializeObject(OnlineSearch.Lobby);

            data = new StringContent(json, Encoding.UTF8, "application/json");

            response = await client.PostAsync($"{MainPage.endpoint}/api/search/reset", data);

            httpData = await response.Content.ReadAsStringAsync();

            lobby = Newtonsoft.Json.JsonConvert.DeserializeObject<Lobby>(httpData);

            OnlineSearch.Lobby = lobby;

            var mainPageUser = MainPage.User;
            mainPageUser.Choice = null;
            user.Choice = null;

            data.Dispose();

            switch (choice)
            {
                case Choice.Rock:
                    OpponentImagePath = @"ms-appx:///Assets/rock.png";
                    break;
                case Choice.Paper:
                    OpponentImagePath = @"ms-appx:///Assets/paper.png";
                    break;
                case Choice.Scissors:
                    OpponentImagePath = @"ms-appx:///Assets/scissors.png";
                    break;
                case Choice.Moo:
                    OpponentImagePath = @"ms-appx:///Assets/moo.png";
                    break;
            }

            if ((YourChoice == Choice.Rock && choice == Choice.Scissors) || (YourChoice == Choice.Scissors && choice == Choice.Paper) || (YourChoice == Choice.Paper && choice == Choice.Rock))
            {
                MyScore++;
                Score = $"{MyScore} - {OpponentScore}";

                Title = "You Win!";

                MediaElement mysong = new MediaElement();
                Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                Windows.Storage.StorageFile file = await folder.GetFileAsync("tada.mp3");
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
            }
            else if (YourChoice == Choice.Moo || choice == Choice.Moo)
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
            else if (YourChoice == choice)
            {
                Title = "You tied, try again!";
            }           
            else
            {
                OpponentScore++;
                Score = $"{MyScore} - {OpponentScore}";

                Title = "You Lose! :(";

                MediaElement mysong = new MediaElement();
                Windows.Storage.StorageFolder folder = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFolderAsync("Assets");
                Windows.Storage.StorageFile file = await folder.GetFileAsync("error.mp3");
                var stream = await file.OpenAsync(Windows.Storage.FileAccessMode.Read);
                mysong.SetSource(stream, file.ContentType);
                mysong.Play();
            }
        }

        private async void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(MainPage.User);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{MainPage.endpoint}/api/search/leavelobby?lobbyId={OnlineSearch.Lobby.Id}", data);

            OnlineSearch.Lobby = null;

            ElementSoundPlayer.Play(ElementSoundKind.GoBack);
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
