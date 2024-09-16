namespace ScoreTracker;

public partial class RootPage : TabbedPage
{
    private bool initialized = false;

	public RootPage()
	{
		InitializeComponent();

        DbModels.Setting storedCurrentTabSetting = DatabaseHandler.Instance.GetCollection<DbModels.Setting>().FindById(DbModels.Setting.CURRENT_TAB_INDEX);
        if(storedCurrentTabSetting != null)
        {
            CurrentPage = Children[(int)storedCurrentTabSetting.Value];
        }

        initialized = true;
    }

    private void TabbedPage_CurrentPageChanged(object sender, EventArgs e)
    {
        if (initialized)
        {
		    int currentPageIndex = this.Children.IndexOf(this.CurrentPage);
            DatabaseHandler.Instance.GetCollection<DbModels.Setting>().Upsert(new DbModels.Setting(DbModels.Setting.CURRENT_TAB_INDEX, currentPageIndex));
        }
    }
}