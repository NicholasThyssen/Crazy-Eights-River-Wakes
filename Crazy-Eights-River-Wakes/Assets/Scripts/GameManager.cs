using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class GameManager : MonoBehaviour
{
    public GameState gameState;
    [HideInInspector] public static GameManager instance;
    [HideInInspector] public List<BaseCharacter> characters;
    [HideInInspector] public InteractionLayerMask grabbableLayer;
    [HideInInspector] public InteractionLayerMask notGrabbableLayer;

    private void Awake()
    {
        instance = this;
        gameState = GameState.Default;
        characters = BuildCharactersArray();
        grabbableLayer = InteractionLayerMask.GetMask("Grabbable");
        notGrabbableLayer = InteractionLayerMask.GetMask("Not Grabbable");
    }

    public List<BaseCharacter> BuildCharactersArray()
    {
        var charactersArray = FindObjectsByType<BaseCharacter>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var centerPos = GetCenter(charactersArray.Select(c => c.transform).ToList());
        // attempt to make order of array match the circular order by sorting by angle from center
        List<BaseCharacter> charactersList = charactersArray.OrderBy(c =>
        {
            Vector3 centerToCharacter = c.transform.position - centerPos;
            float angle = Mathf.Atan2(centerToCharacter.z, centerToCharacter.x);
            return angle;
        }).ToList();
        return charactersList;
    }

    // Get center of all the characters
    private static Vector3 GetCenter(List<Transform> transforms)
    {
        if (transforms.Count == 0)
            return Vector3.zero;

        Vector3 sum = Vector3.zero;

        foreach (Transform t in transforms)
        {
            sum += t.position;
        }

        return sum / transforms.Count;
    }

    private void Start()
    {

    }

    private void Update()
    {

    }
}

public enum GameState
{
    Default,
}

public class Utils
{
    public static IEnumerator AnimateTransform(
    Transform t,
    Vector3 targetPos,
    Quaternion targetRot,
    bool posIsLocal,
    bool rotIsLocal,
    float duration,
    float delayMS = 0f
)
    {
        if (delayMS > 0f)
        {
            yield return new WaitForSeconds(delayMS / 1000);
        }
        if (duration <= 0f)
        {
            if (posIsLocal)
                t.localPosition = targetPos;
            else
                t.position = targetPos;

            if (rotIsLocal)
                t.localRotation = targetRot;
            else
                t.rotation = targetRot;

            yield break;
        }
        Vector3 startPos = posIsLocal ? t.localPosition : t.position;
        Quaternion startRot = rotIsLocal ? t.localRotation : t.rotation;

        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float tNorm = elapsed / duration;

            float eased = Mathf.SmoothStep(0f, 1f, tNorm);

            if (posIsLocal)
            {
                t.localPosition = Vector3.Lerp(startPos, targetPos, eased);
            }
            else
            {
                t.position = Vector3.Lerp(startPos, targetPos, eased);
            }

            if (rotIsLocal)
            {
                t.localRotation = Quaternion.Slerp(startRot, targetRot, eased);
            }
            else
            {
                t.rotation = Quaternion.Slerp(startRot, targetRot, eased);
            }


            yield return null;
        }

        if (posIsLocal)
        {
            t.localPosition = targetPos;
        }
        else
        {
            t.position = targetPos;
        }

        if (rotIsLocal)
        {
            t.localRotation = targetRot;
        }
        else
        {
            t.rotation = targetRot;
        }

    }
}

