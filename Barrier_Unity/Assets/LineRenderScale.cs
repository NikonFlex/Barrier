using UnityEngine;

public class LineRenderScale : MonoBehaviour
{

    Renderer rend;
    LineRenderer line;
    public float _kx = 1;
    public float _ky = 1;

    void Start()
    {
        line = GetComponent<LineRenderer>();
        line.material.SetTextureScale("_MainTex", new Vector2(_kx, _ky));
    }

}