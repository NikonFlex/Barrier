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
   private float _targetRadius = -1;
   private float _currentRadius = -1;
   private float _smoothVelocity = 0.0F;

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
      if (_currentRadius == -1)
         _currentRadius = _targetRadius = radius;
      else
         _targetRadius = radius;
   }

   public void SetColor(Color newColor)
   {
      ZoneColor = newColor;
      createZone();
   }

   private void Update()
   {
      var a = Mathf.Clamp(Mathf.PingPong(Time.time * BlinkingRate, ZoneColor.a), 0.2f, 1f);
      _fillMesh.material.color = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b, a);
      Mathf.SmoothDamp(_currentRadius, _targetRadius, ref _smoothVelocity, 1);
      _fillMesh.transform.localScale = new Vector3(_currentRadius, _currentRadius, 1);
      if (_strokeCircle != null)
         _strokeCircle.SetRadius(_currentRadius);
   }

   private void createZone()
   {
      _stroke = Stroke.GetComponent<LineRenderer>();
      //_strokeCircle = Stroke.GetComponent<DrawCircle>();
      _fillMesh = Fill.GetComponent<MeshRenderer>();
      _fillMesh.material.color = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b, ZoneColor.a);
      _stroke.endColor = ZoneColor;
      _stroke.endColor = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b);
      _stroke.startColor = new Color(ZoneColor.r, ZoneColor.g, ZoneColor.b);
   }

}
