using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace ScoreTracker;

public partial class EditPlayerPage : ContentPage
{
    private BehaviorSubject<int> counter = new BehaviorSubject<int>(0);
    private PlayerData playerData;
    private CompositeDisposable subscriptions;

    public EditPlayerPage(int a_id)
	{
		InitializeComponent();
        subscriptions?.Dispose();
        subscriptions = new CompositeDisposable();
        subscriptions.Add(DatabaseHandler.Instance.RealtimeCollection<PlayerData>().Id(a_id).Subscribe(x => { SetPlayerData(x); }));
        subscriptions.Add(counter.Subscribe(x => { UpdateCounterUI(x); }));
    }

    ~EditPlayerPage()
    {
        subscriptions?.Dispose();
    }

    private void SetPlayerData(PlayerData a_playerData)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            playerData = a_playerData;

            if (playerData != null)
            {
                Title = playerData.Name;
                UpdateCounterUI(counter.Value);
                HistoryListView.ItemsSource = playerData.Points;
                HistoryListView.IsVisible = playerData.Points.Count() > 0;
                EmptyView.IsVisible = playerData.Points.Count() == 0;
            }
            else
            {
                Navigation.PopAsync();
            }
        });
    }

    private void UpdateCounterUI(int value)
    {
        CounterLabel.Text = (value > 0 ? "+" : "") + value.ToString();
        int playerPoints = playerData?.TotalPoints ?? 0;
        CurrentScoreLabel.Text = $"Current score: {playerPoints}\nNew Score: {playerPoints + value}";
    }

    private void OnCounterDecrement3(object sender, EventArgs e) { IncrementCounter(-100); }
    private void OnCounterDecrement2(object sender, EventArgs e) { IncrementCounter(-10); }
    private void OnCounterDecrement1(object sender, EventArgs e) { IncrementCounter(-1); }
    private void OnCounterIncrement1(object sender, EventArgs e) { IncrementCounter(1); }
    private void OnCounterIncrement2(object sender, EventArgs e) { IncrementCounter(10); }
    private void OnCounterIncrement3(object sender, EventArgs e) { IncrementCounter(100); }

    private void IncrementCounter(int value)
    {
        counter.OnNext(counter.Value + value);
    }

    private void OnSavePointsClicked(object sender, EventArgs e)
    {
        if (playerData != null)
        {
            playerData.Points.Add(counter.Value);
            counter.OnNext(0);
            DatabaseHandler.Instance.GetCollection<PlayerData>().Update(playerData);
        }

        Navigation.PopAsync();
    }

    private void HistoryListView_ItemTapped(object sender, ItemTappedEventArgs e)
    {
        DisplayActionSheet($"Delete this entry? ({playerData.Points[e.ItemIndex]})", "No", "Yes").ContinueWith(t =>
        {
            if (t.Result == "Yes")
            {
                playerData.Points.RemoveAt(e.ItemIndex);
                DatabaseHandler.Instance.GetCollection<PlayerData>().Update(playerData);
            }
        });
    }

    private async void OnEditPlayerNameClicked(object sender, EventArgs e)
    {
        string result = await DisplayPromptAsync("Edit player name",
                                                        "Enter a new name for this player",
                                                        initialValue: playerData.Name,
                                                        keyboard: Keyboard.Text);

        playerData.Name = result;
        DatabaseHandler.Instance.GetCollection<PlayerData>().Update(playerData);
    }

    private void OnDeletePlayerClicked(object sender, EventArgs e)
    {
        DisplayActionSheet("Do you really to delete this player?", "No", "Yes").ContinueWith(t =>
        {
            if (t.Result == "Yes")
            {
                DatabaseHandler.Instance.GetCollection<PlayerData>().Delete(playerData.Id);
            }
        });
        
    }
}