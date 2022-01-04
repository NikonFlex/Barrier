using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MSC : MonoBehaviour
{
   [SerializeField] private GameObject _fastener;

   private float _distToShip;
   private float _offsetFromWater = 0.25f; //can't see under water
   private LineRenderer _lineRenderer;

   void Start()
   {
      _lineRenderer = gameObject.GetComponent<LineRenderer>();
   }

   private void drawFastener()
   {
      if (!gameObject.activeSelf)
         return;

      List<Vector3> pos = new List<Vector3>();
      pos.Add(gameObject.transform.position);
      pos.Add(_fastener.transform.position);
      _lineRenderer.startWidth = 0.1f;
      _lineRenderer.endWidth = 0.1f;
      _lineRenderer.SetPositions(pos.ToArray());
      _lineRenderer.useWorldSpace = true;
   }

   public void SetUpSettings(bool isActive, float distToShip)
   {
      gameObject.SetActive(isActive);
      _distToShip = distToShip;
      LabelHelper.ShowLabel(gameObject);
   }

   public void SetPosition(Vector3 newShipPos, Vector3 shipForward)
   {
      if (!gameObject.activeSelf)
         return;

      newShipPos.y += _offsetFromWater;
      gameObject.transform.position = newShipPos + -shipForward * _distToShip;
      drawFastener();
   }
}
