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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace RockPaperScissors
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public partial class OnlineSearch : Page, INotifyPropertyChanged
    {
        public static Lobby Lobby { get; set; }

        private string _accountName;

        public string AccountName
        {
            get { return _accountName; }

            set { this.SetProperty(ref this._accountName, value); }
        }

        private IEnumerable<Lobby> _lobbies;

        public IEnumerable<Lobby> Lobbies
        {
            get { return _lobbies; }

            set { this.SetProperty(ref this._lobbies, value); }
        }

        private bool _hadTwoPlayers = false;

        HttpClient client;

        public OnlineSearch()
        {
            var httpClientHandler = new HttpClientHandler();
            httpClientHandler.ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => { return true; };
            client = new HttpClient(httpClientHandler);

            this.InitializeComponent();
            DataContext = this;

            PopulateLobbies();        
        }

        //private async Task<string> InputTextDialogAsync(string title)
        //{
        //    TextBox inputTextBox = new TextBox();
        //    inputTextBox.AcceptsReturn = false;
        //    inputTextBox.Height = 32;
        //    ContentDialog dialog = new ContentDialog();
        //    dialog.Content = inputTextBox;
        //    dialog.Title = title;
        //    dialog.IsSecondaryButtonEnabled = true;
        //    dialog.PrimaryButtonText = "Ok";
        //    dialog.SecondaryButtonText = "Cancel";
        //    if (await dialog.ShowAsync() == ContentDialogResult.Primary)
        //    {
        //        return inputTextBox.Text;
        //    }
        //    else
        //    {
        //        this.Frame.Navigate(typeof(MainPage));
        //        return "";
        //    }
        //}

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);

            AccountName = e.Parameter.ToString();

            MainPage.User.Name = AccountName;

            Console.WriteLine(e.Parameter.ToString());
        }

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

        public async void PopulateLobbies()
        {
            try
            {
                var response = await client.GetAsync($"{MainPage.endpoint}/api/search/getlobbies");
                var data = await response.Content.ReadAsStringAsync();

                var lobbies = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Lobby>>(data);

                Lobbies = lobbies;
            }
            catch(Exception ex)
            {
                NotificationModal("Error connecting to server, returning to main menu", ex.ToString());
                Lobbies = null;
                Lobby = null;
                AccountName = null;
                _hadTwoPlayers = false;
                this.Frame.Navigate(typeof(MainPage));
            }
        }

        private async void Join_Click(object sender, RoutedEventArgs e)
        {
            var choice = (Lobby)this.listView.SelectedItem;

            if(choice == null)
            {
                NotificationModal("No lobby selected", "Please choose a lobby");
                return;
            }

            if (choice.Size >= 2)
            {
                NotificationModal("Lobby is full", "Please select another lobby");
                return;
            }

            choice.Users.Add(MainPage.UserKey, MainPage.User);
            if (choice.Size < 2)
            {
                var json = Newtonsoft.Json.JsonConvert.SerializeObject(choice);

                var data = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{MainPage.endpoint}/api/search/joinlobby?userId={MainPage.UserKey}", data);

                data.Dispose();

                var results = await response.Content.ReadAsStringAsync();

                var lobby = Newtonsoft.Json.JsonConvert.DeserializeObject<Lobby>(results);

                if(lobby == null)
                {
                    NotificationModal("Lobby is full or no longer exists", "Please select another lobby");
                    PopulateLobbies();
                    return;
                }

                Lobby = lobby;

                var t = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => HeartBeat());

                this.Frame.Navigate(typeof(TwoPlayer));
            }
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

        private async void Create_Click(object sender, RoutedEventArgs e)
        {
            var user = MainPage.User;

            var json = Newtonsoft.Json.JsonConvert.SerializeObject(user);

            var data = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync($"{MainPage.endpoint}/api/search/addlobby", data);

            data.Dispose();

            var results = await response.Content.ReadAsStringAsync();

            var lobbies = Newtonsoft.Json.JsonConvert.DeserializeObject<IEnumerable<Lobby>>(results);

            if (lobbies == null)
                return;

            Lobbies = lobbies;

            Lobby = lobbies.Where(x => x.Users.ContainsKey(user.Id)).FirstOrDefault();

            var t = Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () => HeartBeat());

            this.Frame.Navigate(typeof(TwoPlayer));
        }

        public async void HeartBeat()
        {
            while (Lobby != null)
            {
                var response = await client.PostAsync($"{MainPage.endpoint}/api/search/heartbeat?userId={MainPage.User.Id}", null);

                var results = await response.Content.ReadAsStringAsync();

                var lobby = Newtonsoft.Json.JsonConvert.DeserializeObject<Lobby>(results);
                lobby.Size = lobby.Users.Count;

                if(lobby.Size > 1)
                {
                    _hadTwoPlayers = true;
                }

                if(_hadTwoPlayers == true && lobby.Size < 2)
                {
                    _hadTwoPlayers = false;
                    NotificationModal("Opponent has left...", "Returning to main menu");
                    this.Frame.Navigate(typeof(MainPage));
                    return;
                }

                await Task.Delay(TimeSpan.FromMilliseconds(1000));
            }
        }

        private void Refresh_Click(object sender, RoutedEventArgs e)
        {
            PopulateLobbies();
        }

        private void Exit_Button_Click(object sender, RoutedEventArgs e)
        {
            ElementSoundPlayer.Play(ElementSoundKind.GoBack);
            this.Frame.Navigate(typeof(MainPage));
        }
    }
}
