using System.Diagnostics;

namespace ScoreTracker
{
    public partial class MainPage : ContentPage
    {
        IDisposable subscription;

        public MainPage()
        {
            InitializeComponent();

            subscription?.Dispose();
            subscription = DatabaseHandler.Instance.RealtimeCollection<PlayerData>().Subscribe(_ => UpdatePlayersList());
        }

        ~MainPage()
        {
            subscription?.Dispose();
        }

        private void UpdatePlayersList()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var players = DatabaseHandler.Instance.GetCollection<PlayerData>().FindAll();
                PlayerListView.ItemsSource = players;
            });
        }

        private async void OnPlayerCountClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Player count",
                                                        "How many players do you want?",
                                                        initialValue: DatabaseHandler.Instance.GetCollection<PlayerData>().Count().ToString(),
                                                        maxLength: 2,
                                                        keyboard: Keyboard.Numeric);
            int desiredPlayerCount;
            if(int.TryParse(result, out desiredPlayerCount))
            {
                int delta = DatabaseHandler.Instance.GetCollection<PlayerData>().Count() - desiredPlayerCount;

                if(delta < 0)
                {
                    for (int i = delta; i < 0; i++)
                    {
                        var np = new PlayerData { Name = "Player" };
                        DatabaseHandler.Instance.GetCollection<PlayerData>().Insert(np);
                    }
                }
                else if(delta > 0)
                {
                    try
                    {
                        for(int i = 0;  i < delta; i++)
                        {
                            var lastPlayer = DatabaseHandler.Instance.GetCollection<PlayerData>().FindAll().Last();
                            DatabaseHandler.Instance.GetCollection<PlayerData>().Delete(lastPlayer.Id);
                        }
                    }
                    catch { }
                }
            }
        }

        private void OnResetScoresClicked(object sender, EventArgs e)
        {
            DisplayActionSheet("Reset scores?", "No", "Yes").ContinueWith(t =>
            {
                if(t.Result == "Yes")
                {
                    DatabaseHandler.Instance.GetCollection<PlayerData>().UpdateMany("{Points: []}", "Points != []");
                }
            });
        }

        private void PlayerListView_ItemTapped(object sender, ItemTappedEventArgs e)
        {
            PlayerData player = (PlayerData)e.Item;
            Navigation.PushModalAsync(new EditPlayerPage(player.Id));
        }
    }
}
