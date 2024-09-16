using System.Reactive.Disposables;

namespace ScoreTracker;

public partial class LetterPage : ContentPage
{
    private readonly string allowedLetterPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private string eliminatedMode_LetterPool = "";

    private CompositeDisposable subscriptions;
    private ERandomLetterMode currentMode = ERandomLetterMode.Normal;

    enum ERandomLetterMode
    {
        Normal,
        Elimination
    }

    public LetterPage()
	{
		InitializeComponent();

        subscriptions?.Dispose();
        subscriptions = new CompositeDisposable();
        subscriptions.Add(DatabaseHandler.Instance.RealtimeCollection<DbModels.Setting>().Id(DbModels.Setting.RANDOM_LETTER_MODE).Subscribe(x => UpdateRandomMode(x)));
        subscriptions.Add(DatabaseHandler.Instance.RealtimeCollection<DbModels.Setting>().Id(DbModels.Setting.ELIMINATED_MODE_LETTER_POOL).Subscribe(x => UpdateEleminatedModeLetterPool(x)));
    }

    ~LetterPage()
    {
        subscriptions?.Dispose();
    }

    private void UpdateRandomMode(DbModels.Setting setting)
    {
        if(setting == null) return;

        try
        {
            currentMode = (ERandomLetterMode)Enum.Parse(typeof(ERandomLetterMode), (string)setting.Value);
            EliminationModeSwitch.IsToggled = currentMode == ERandomLetterMode.Elimination;
            LettersLayout.IsEnabled = currentMode == ERandomLetterMode.Elimination;
        }
        catch
        {
            currentMode = ERandomLetterMode.Elimination;
            EliminationModeSwitch.IsToggled = true;
            LettersLayout.IsEnabled = true;
        }
    }

    private void UpdateEleminatedModeLetterPool(DbModels.Setting setting)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            eliminatedMode_LetterPool = (setting != null) 
                                            ? (string)setting.Value ?? ""
                                            : allowedLetterPool.ToString();

            for (int i = 0; i < allowedLetterPool.Length; i++)
            {
                UpdateLetterUI(allowedLetterPool[i], false);
            }

            for (int i = 0; i < eliminatedMode_LetterPool.Length; i++)
            {
                UpdateLetterUI(eliminatedMode_LetterPool[i], true);
            }
        });
    }

    private void RandomLetterClicked(object sender, EventArgs e)
    {
        if(eliminatedMode_LetterPool.Length == 0 && currentMode == ERandomLetterMode.Elimination)
        {
            DisplayActionSheet("No available letter left!\nReset available letters list?", "No", "Yes").ContinueWith(t =>
            {
                if (t.Result == "Yes")
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        ResetLetters();
                    });
                }
            });
            return;
        }

        string letterPool;
        switch (currentMode)
        {
            case ERandomLetterMode.Elimination:
                letterPool = eliminatedMode_LetterPool;
                break;
            default:
                letterPool = allowedLetterPool;
                break;
        }

        int randomIndex = Random.Shared.Next(letterPool.Length);
        char letter = letterPool[randomIndex];
        LetterResultLabel.Text = letter.ToString();

        if(currentMode == ERandomLetterMode.Elimination)
        {
            string newPool = eliminatedMode_LetterPool.Remove(randomIndex, 1);
            DatabaseHandler.Instance.GetCollection<DbModels.Setting>().Upsert(new DbModels.Setting(DbModels.Setting.ELIMINATED_MODE_LETTER_POOL, newPool));
        }

        Vibration.Default.Vibrate(50);
    }

    private void OnResetLettersClicked(object sender, EventArgs e)
    {
        DisplayActionSheet("Reset available letters list?", "No", "Yes").ContinueWith(t =>
        {
            if (t.Result == "Yes")
            {
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    ResetLetters();
                });
            }
        });
    }

    private void ResetLetters()
    {
        LetterResultLabel.Text = "-";

        eliminatedMode_LetterPool = allowedLetterPool.ToString();
        DatabaseHandler.Instance.GetCollection<DbModels.Setting>().Delete(DbModels.Setting.ELIMINATED_MODE_LETTER_POOL);
    }

    private void UpdateLetterUI(char letter, bool isEnabled)
    {
        if (FindByName($"Label_{letter}") is Label label && label != null)
        {
            label.IsEnabled = isEnabled;
            label.Scale = isEnabled ? 1 : 0.5;
        }
    }

    private void EliminationModeSwitchToggled(object sender, ToggledEventArgs e)
    {
        DbModels.Setting setting = new DbModels.Setting(DbModels.Setting.RANDOM_LETTER_MODE);
        setting.Value = Enum.GetName(typeof(ERandomLetterMode), e.Value ? ERandomLetterMode.Elimination : ERandomLetterMode.Normal);
        DatabaseHandler.Instance.GetCollection<DbModels.Setting>().Upsert(setting);
    }
}