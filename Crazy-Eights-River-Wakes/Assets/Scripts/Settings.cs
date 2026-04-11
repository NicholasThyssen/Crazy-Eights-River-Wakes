using UnityEngine;

// Don't put the SettingsData on a component. Static classes shouldn't be instanced.

public static class SettingsData
{
    private static float cardSfxVolume = 100.0f;
    private static float envSfxVolume = 100.0f;

    private static int startingCards = 5;
    private static bool selectWithXRController = false;

    public static float GetCardSfxVolume() => cardSfxVolume;
    public static void SetCardSfxVolume(float _cardSfxVolume) => cardSfxVolume = _cardSfxVolume;
    public static float GetEnvSfxVolume() => envSfxVolume;
    public static void SetEnvSfxVolume(float _envSfxVolume) => envSfxVolume = _envSfxVolume;
    public static int GetStartingCards() => startingCards;
    public static float SetStartingcards(int _startingCards) => startingCards = _startingCards;
}
