using UnityEngine;

public enum GameDifficulty { Easy, Normal, Hard }

public static class DifficultySettings
{
    public static GameDifficulty Selected = GameDifficulty.Normal;

    public struct Tuning
    {
        public float bot1CorrectChance; // 0..1
        public float bot2CorrectChance; // 0..1
    }

    public static Tuning GetTuning()
    {
        switch (Selected)
        {
            case GameDifficulty.Easy:
                return new Tuning { bot1CorrectChance = 0.50f, bot2CorrectChance = 0.55f };
            case GameDifficulty.Hard:
                return new Tuning { bot1CorrectChance = 0.90f, bot2CorrectChance = 0.95f };
            default: // Normal
                return new Tuning { bot1CorrectChance = 0.70f, bot2CorrectChance = 0.75f };
        }
    }
}
