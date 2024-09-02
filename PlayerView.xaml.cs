using System.Xml;

namespace ScoreTracker;

public partial class PlayerView : ContentView
{
    private int playerId;
    private IDisposable subscription;

    public PlayerView()
	{
		InitializeComponent();
	}

    ~PlayerView()
    {
        subscription?.Dispose();
    }

    public void SetPlayerId(int a_id)
    {
        playerId = a_id;
        subscription?.Dispose();
        subscription = DatabaseHandler.Instance.RealtimeCollection<PlayerData>().Id(playerId).Subscribe(data => UpdateView(data));
    }

    private void UpdateView(PlayerData a_data)
    {
        if(a_data == null) { return; }

        MainThread.BeginInvokeOnMainThread(() =>
        {
            NameLabel.Text = a_data.Name;
            ScoreLabel.Text = a_data.Points.Sum().ToString();
        });
    }

    private void OnViewClicked(object sender, EventArgs e)
    {
        Navigation.PushModalAsync(new EditPlayerPage(playerId));
    }
}