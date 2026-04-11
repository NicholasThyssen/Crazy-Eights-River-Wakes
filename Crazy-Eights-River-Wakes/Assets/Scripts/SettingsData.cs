using UnityEngine;
using UnityEngine.Audio;

// Don't put the SettingsData on a component. Static classes shouldn't be instanced.

public static class SettingsData
{
    /* VOLUME */
    private static float cardSfxVolume = 100.0f;
    private static float envSfxVolume = 100.0f;
    public static float GetCardSfxVolume() => cardSfxVolume;
    public static void SetCardSfxVolume(float _cardSfxVolume) => cardSfxVolume = _cardSfxVolume;
    public static float GetEnvSfxVolume() => envSfxVolume;
    public static void SetEnvSfxVolume(float _envSfxVolume) => envSfxVolume = _envSfxVolume;

    /* XR MODE CONTROL OPTIONS */
    private static bool selectWithXRController = false;

    public static bool GetSelectWithXRController() => selectWithXRController;
    public static void SetSelectWithXRController(bool _selectWithXRController) => selectWithXRController = _selectWithXRController;

    /* GAME SETUP */
    private static int startingCards = 5;
    private static int botCount = 3;
    public static int GetStartingCards() => startingCards;
    public static void SetStartingCards(int _startingCards) => startingCards = _startingCards;
    public static int GetBotCount() => botCount;
    public static void SetBotCount(int _botCount) => botCount = _botCount;
}