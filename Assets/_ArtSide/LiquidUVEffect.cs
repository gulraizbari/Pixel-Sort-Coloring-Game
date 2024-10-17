using UnityEngine;

public class LiquidUVEffect : MonoBehaviour
{
    public float intensity = 0.05f; // Controls the magnitude of the UV offset
    public float speed = 1f;        // Controls the speed of the UV movement

    private Renderer objectRenderer;
    private Vector2 initialOffset;

    void Start()
    {
        objectRenderer = GetComponent<Renderer>();
        initialOffset = objectRenderer.material.GetTextureOffset("_BaseMap");
    }

    void Update()
    {
        // Calculate the new UV offset
        Vector2 offset = new Vector2(
            Mathf.Sin(Time.time * speed) * intensity,
            Mathf.Cos(Time.time * speed) * intensity
        );

        // Apply the offset to the texture's UV coordinates
        objectRenderer.material.SetTextureOffset("_BaseMap", initialOffset + offset);
    }
}
