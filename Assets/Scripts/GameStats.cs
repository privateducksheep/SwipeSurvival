using UnityEngine;

public static class GameStats
{
    private const string HighScoreKey = "HighScore";

    public static int daysSurvived = 0;
    public static int highScore = 0;
    public static Card prevCard = null;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void LoadSavedStats()
    {
        highScore = PlayerPrefs.GetInt(HighScoreKey, 0);
    }

    public static void RecordHighScore(int score)
    {
        if (score <= highScore)
            return;

        highScore = score;
        PlayerPrefs.SetInt(HighScoreKey, highScore);
        PlayerPrefs.Save();
    }

}

