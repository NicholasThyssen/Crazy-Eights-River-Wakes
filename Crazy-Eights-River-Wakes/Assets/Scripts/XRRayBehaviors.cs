using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class XRRayBehaviors : MonoBehaviour
{
    [Header("Drag your hand/controller GameObject here")]
    public GameObject handObj;

    public Color normalColor = Color.white;
    public Color hoverColor = Color.green;

    private XRBaseInteractor interactor;
    private Renderer[] handRenderers;
    private Material[] materialInstances;
    private int grabbableHoverCount = 0;

    private static readonly int BaseColor = Shader.PropertyToID("_BaseColor");
    private static readonly int ColorId = Shader.PropertyToID("_Color");

    void Awake()
    {
        interactor = GetComponent<XRBaseInteractor>();

        if (handObj == null)
        {
            Debug.LogError("Drag your hand/controller GameObject into handObj.", this);
            enabled = false;
            return;
        }

        handRenderers = handObj.GetComponentsInChildren<Renderer>(true);
        materialInstances = new Material[handRenderers.Length];

        for (int i = 0; i < handRenderers.Length; i++)
        {
            materialInstances[i] = new Material(handRenderers[i].sharedMaterial);
            handRenderers[i].material = materialInstances[i];
        }

        SetHandColor(normalColor);
    }

    void OnEnable()
    {
        interactor.hoverEntered.AddListener(OnHoverEnter);
        interactor.hoverExited.AddListener(OnHoverExit);
    }

    void OnDisable()
    {
        interactor.hoverEntered.RemoveListener(OnHoverEnter);
        interactor.hoverExited.RemoveListener(OnHoverExit);
    }

    void OnHoverEnter(HoverEnterEventArgs args)
    {
        if (args.interactableObject.transform.GetComponentInParent<XRGrabInteractable>() == null)
            return;

        grabbableHoverCount++;
        SetHandColor(hoverColor);
    }

    void OnHoverExit(HoverExitEventArgs args)
    {
        if (args.interactableObject.transform.GetComponentInParent<XRGrabInteractable>() == null)
            return;

        grabbableHoverCount--;

        if (grabbableHoverCount <= 0)
        {
            grabbableHoverCount = 0;
            SetHandColor(normalColor);
        }
    }

    void SetHandColor(Color color)
    {
        foreach (Material mat in materialInstances)
        {
            if (mat == null) continue;

            if (mat.HasProperty(BaseColor))
                mat.SetColor(BaseColor, color);

            if (mat.HasProperty(ColorId))
                mat.SetColor(ColorId, color);

            mat.color = color;
        }
    }
}