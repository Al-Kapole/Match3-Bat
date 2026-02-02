public class GeneralGameInfo
{
    private static GeneralGameInfo instance;
    public static GeneralGameInfo Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                instance = new GeneralGameInfo();
                return instance;
            }
        }
    }

    public int SelectedGridSize = 7;
    public int FinalScore;
}
