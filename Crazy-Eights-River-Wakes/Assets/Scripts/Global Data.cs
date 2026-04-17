using UnityEngine;

[CreateAssetMenu(menuName = "Game/Global Data")]
public class GlobalData : ScriptableObject
{
    public static GlobalData Instance;

    public GameObject turnIndicatorPrefab;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void Init()
    {
        Instance = Resources.Load<GlobalData>("GlobalData");
    }
}