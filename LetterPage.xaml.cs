namespace ScoreTracker;

public partial class LetterPage : ContentPage
{
    private readonly List<char> letters = new List<char>() { 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z' };
    private List<char> lettersLeft = new List<char>();

    private IDisposable subscription;
    private ERandomLetterMode currentMode = ERandomLetterMode.Normal;

    enum ERandomLetterMode
    {
        Normal,
        Elimination
    }

    public LetterPage()
	{
		InitializeComponent();
        lettersLeft = letters.ToList();

        subscription?.Dispose();
        subscription = DatabaseHandler.Instance.RealtimeCollection<DbModels.Setting>().Id(DbModels.Setting.RANDOM_LETTER_MODE).Subscribe(x => UpdateRandomMode(x));
    }

    ~LetterPage()
    {
        subscription?.Dispose();
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

    private void RandomLetterClicked(object sender, EventArgs e)
    {
        if(lettersLeft.Count == 0 && currentMode == ERandomLetterMode.Elimination)
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

        List<char> letterPool;
        switch (currentMode)
        {
            case ERandomLetterMode.Elimination:
                letterPool = lettersLeft;
                break;
            default:
                letterPool = letters;
                break;
        }

        int randomIndex = Random.Shared.Next(letterPool.Count);
        char letter = letterPool[randomIndex];
        LetterResultLabel.Text = letter.ToString();

        if(currentMode == ERandomLetterMode.Elimination)
        {
            lettersLeft.RemoveAt(randomIndex);
            if (FindByName($"Label_{letter}") is Label label && label != null)
            {
                label.IsEnabled = false;
                label.Scale = 0.5;
            }
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

        lettersLeft = letters.ToList();
        for (int i = 0; i < letters.Count; i++)
        {
            if(FindByName($"Label_{letters[i]}") is Label label && label != null) {
                label.IsEnabled = true;
                label.Scale = 1;
            }
        }
    }

    private void EliminationModeSwitchToggled(object sender, ToggledEventArgs e)
    {
        DbModels.Setting setting = new DbModels.Setting(DbModels.Setting.RANDOM_LETTER_MODE);
        setting.Value = Enum.GetName(typeof(ERandomLetterMode), e.Value ? ERandomLetterMode.Elimination : ERandomLetterMode.Normal);
        DatabaseHandler.Instance.GetCollection<DbModels.Setting>().Upsert(setting);
    }
}