using UnityEngine;

public class TurnIndicator : MonoBehaviour
{
    private BaseCharacter parent;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame

    void Update()
    {
        transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        float bounce = Mathf.Sin(Time.time * 1.2f) * 0.05f;

        Vector3 basePos =
            this.parent.GetTransform().position
            + (Vector3.up * 2.2f)
            - (Vector3.up * parent.GetCameraHeight());

        // Add fun sin bounce
        transform.position = basePos + Vector3.up * bounce;
    }

    public void SetParent(BaseCharacter parent)
    {
        this.parent = parent;
    }
}
