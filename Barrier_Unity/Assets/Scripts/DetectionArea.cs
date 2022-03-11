using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetectionArea : MonoBehaviour
{
    [SerializeField]
    private GameObject Stroke;
    [SerializeField]
    private GameObject Fill;
    [SerializeField]
    private Color ZoneColor;
    [SerializeField]
    private float BlinkingRate;

    private LineRenderer _stroke;
    private DrawCircle _strokeCircle;
    private MeshRenderer _fillMesh;

    private void Start()
    {
        createZone();
        //_stroke = Stroke.GetComponent<LineRenderer>();
        //_strokeCircle = Stroke.GetComponent<DrawCircle>();
        //_fillMesh = Fill.GetComponent<MeshRenderer>();
        //_fillMesh.material.color = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b, ZoneColor.a);
        //_stroke.endColor = ZoneColor;
        //_stroke.endColor = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b);
        //_stroke.startColor = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b);
    }

    public void SetRadius(float radius)
    {
        _strokeCircle.SetRadius(radius);
        _fillMesh.transform.localScale = new Vector3(radius, radius, 1);
    }

    public void SetColor(Color newColor)
    {
       ZoneColor = newColor;
       createZone();
    }

    private void Update()
    {
        _fillMesh.material.color = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b, Mathf.PingPong(Time.time*BlinkingRate, ZoneColor.a));
    }

    private void createZone()
    {
        _stroke = Stroke.GetComponent<LineRenderer>();
        _strokeCircle = Stroke.GetComponent<DrawCircle>();
        _fillMesh = Fill.GetComponent<MeshRenderer>();
        _fillMesh.material.color = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b, ZoneColor.a);
        _stroke.endColor = ZoneColor;
        _stroke.endColor = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b);
        _stroke.startColor = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b);
   }

}
