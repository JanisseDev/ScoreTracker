﻿using System.Diagnostics;

namespace ScoreTracker
{
    public partial class ScorePage : ContentPage
    {
        private IDisposable subscription;

        public ScorePage()
        {
            InitializeComponent();

            PlayerListView.IsVisible = false;
            subscription?.Dispose();
            subscription = DatabaseHandler.Instance.RealtimeCollection<DbModels.PlayerData>().Subscribe(_ => UpdatePlayersList());
        }

        ~ScorePage()
        {
            subscription?.Dispose();
        }

        private void UpdatePlayersList()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var players = DatabaseHandler.Instance.GetCollection<DbModels.PlayerData>().FindAll();
                PlayerListView.ItemsSource = players;
                PlayerListView.IsVisible = players.Count() > 0;
                EmptyView.IsVisible = players.Count() == 0;
            });
        }

        private async void OnPlayerCountClicked(object sender, EventArgs e)
        {
            string result = await DisplayPromptAsync("Player count",
                                                        "How many players do you want?",
                                                        maxLength: 2,
                                                        keyboard: Keyboard.Numeric);
            int desiredPlayerCount;
            if(int.TryParse(result, out desiredPlayerCount))
            {
                int delta = DatabaseHandler.Instance.GetCollection<DbModels.PlayerData>().Count() - desiredPlayerCount;

                if(delta < 0)
                {
                    for (int i = delta; i < 0; i++)
                    {
                        var np = new DbModels.PlayerData { Name = "Player" };
                        DatabaseHandler.Instance.GetCollection<DbModels.PlayerData>().Insert(np);
                    }
                }
                else if(delta > 0)
                {
                    try
                    {
                        for(int i = 0;  i < delta; i++)
                        {
                            var lastPlayer = DatabaseHandler.Instance.GetCollection<DbModels.PlayerData>().FindAll().Last();
                            DatabaseHandler.Instance.GetCollection<DbModels.PlayerData>().Delete(lastPlayer.Id);
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
                    DatabaseHandler.Instance.GetCollection<DbModels.PlayerData>().UpdateMany("{Points: []}", "Points != []");
                }
            });
        }

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            _ = Navigation.PushAsync(new EditPlayerPage((int)e.Parameter));
        }
    }
}
