using System.ComponentModel;
using System.Windows.Input;

namespace ScoreTracker;

public partial class DicePage : ContentPage, INotifyPropertyChanged
{
    public ICommand RollDiceCommand { get; private set; }

    public DicePage()
	{
		InitializeComponent();

        RollDiceCommand = new Command<string>(x =>
        {
            int param = int.Parse(x);
            int result = Random.Shared.Next(param) + 1;
            DiceHeaderLabel.Text = $"The result of your D{param} is:";
            DiceResultLabel.Text = result.ToString();
            DiceResultLabel.IsVisible = true;
            Vibration.Default.Vibrate(50);
        });

        BindingContext = this;
    }
}