namespace MusicTheoryTrainerBlazor.Services;

public class SessionStats
{
    public int Correct { get; set; }
    public int Total { get; set; }
    public int BestStreak { get; set; }
    public int ArcadeRunScore { get; set; }
    public int ArcadeLevel { get; set; }

    public void Reset()
    {
        Correct = 0;
        Total = 0;
        BestStreak = 0;
        ArcadeRunScore = 0;
        ArcadeLevel = 0;
    }
}