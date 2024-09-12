using System.ComponentModel;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Windows.Input;

namespace ScoreTracker;

public partial class EditPlayerPage : ContentPage, INotifyPropertyChanged
{
    public ICommand EditCounterCommand { get; private set; }

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

        EditCounterCommand = new Command<string>(x =>
        {
            int value = int.Parse(x);
            counter.OnNext(counter.Value + value);
            Vibration.Default.Vibrate(50);
        });

        BindingContext = this;
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

    private void OnSavePointsClicked(object sender, EventArgs e)
    {
        if (playerData != null)
        {
            playerData.Points.Add(counter.Value);
            counter.OnNext(0);
            DatabaseHandler.Instance.GetCollection<PlayerData>().Update(playerData);
            Vibration.Default.Vibrate(50);
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

        if(result != null)
        {
            playerData.Name = result;
            DatabaseHandler.Instance.GetCollection<PlayerData>().Update(playerData);
        }
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