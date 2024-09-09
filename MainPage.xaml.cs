﻿using System.Diagnostics;

namespace ScoreTracker
{
    public partial class MainPage : ContentPage
    {
        IDisposable subscription;

        public MainPage()
        {
            InitializeComponent();

            PlayerListView.IsVisible = false;
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

        private void TapGestureRecognizer_Tapped(object sender, TappedEventArgs e)
        {
            _ = Navigation.PushAsync(new EditPlayerPage((int)e.Parameter));
        }
    }
}
