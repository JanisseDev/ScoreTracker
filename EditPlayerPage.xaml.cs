using Microsoft.Maui.Controls;
using System.Xml;

namespace ScoreTracker;

public partial class EditPlayerPage : ContentPage
{
    private int count = 0;
    private PlayerData playerData;
    private IDisposable subscription;

    public EditPlayerPage(int a_id)
	{
		InitializeComponent();
        subscription?.Dispose();
        subscription = DatabaseHandler.Instance.RealtimeCollection<PlayerData>().Id(a_id).Subscribe(x => { SetPlayerData(x); });
    }

    ~EditPlayerPage()
    {
        subscription?.Dispose();
    }

    private void SetPlayerData(PlayerData a_playerData)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            playerData = a_playerData;
            HistoryListView.ItemsSource = playerData.Points;

            if (playerData != null)
            {
                NameInput.Text = playerData.Name;
            }
            else
            {
                Navigation.PopModalAsync();
            }
        });
    }

    private void OnCounterDecrement3(object sender, EventArgs e) { IncrementCounter(-100); }
    private void OnCounterDecrement2(object sender, EventArgs e) { IncrementCounter(-10); }
    private void OnCounterDecrement1(object sender, EventArgs e) { IncrementCounter(-1); }
    private void OnCounterIncrement1(object sender, EventArgs e) { IncrementCounter(1); }
    private void OnCounterIncrement2(object sender, EventArgs e) { IncrementCounter(10); }
    private void OnCounterIncrement3(object sender, EventArgs e) { IncrementCounter(100); }

    private void IncrementCounter(int value)
    {
        count += value;
        CounterLabel.Text = (count > 0 ? "+" : "") + count.ToString();
    }

    private void OnSavePointsClicked(object sender, EventArgs e)
    {
        if (playerData != null)
        {
            playerData.Points.Add(count);
            DatabaseHandler.Instance.GetCollection<PlayerData>().Update(playerData);
        }

        Navigation.PopModalAsync();
    }

    private void OnCancelClicked(object sender, EventArgs e)
    {
        Navigation.PopModalAsync();
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

    private void NameInput_Completed(object sender, EventArgs e)
    {
        playerData.Name = NameInput.Text;
        DatabaseHandler.Instance.GetCollection<PlayerData>().Update(playerData);
    }
}