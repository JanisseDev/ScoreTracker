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
            //DatabaseHandler.Instance.GetCollection<PlayerData>().DeleteAll();
        }

        ~MainPage()
        {
            subscription?.Dispose();
        }

        private void UpdatePlayersList()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var players = DatabaseHandler.Instance.GetCollection<PlayerData>().FindAll().ToList();

                int delta = PlayersLayout.Count - players.Count();
                if(delta < 0)
                {
                    for(int i = delta; i < 0; i++)
                    {
                        PlayersLayout.Add(new PlayerView());
                    }
                } 
                else if(delta > 0)
                {
                    for (int i = 0; i < delta; i++)
                    {
                        PlayersLayout.RemoveAt(0);
                    }
                }

                for (int i = 0;i < players.Count;i++)
                {
                    ((PlayerView)PlayersLayout[i]).SetPlayerId(players[i].Id);
                }
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
    }
}
