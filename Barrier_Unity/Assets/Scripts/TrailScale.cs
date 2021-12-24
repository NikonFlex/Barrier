using UnityEngine;

public class TrailScale : MonoBehaviour
{
    // Scroll main texture based on time

    Renderer rend;
    TrailRenderer trail;
    public float _kx = 1;
    public float _ky = 1;

    void Start()
    {
        trail = GetComponent<TrailRenderer>();
        //trail.textureMode = LineTextureMode.Tile;
        
    }

    void Update()
    {
        trail.material.SetTextureScale("_MainTex", new Vector2(_kx, _ky));
    }
}