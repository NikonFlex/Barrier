using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TorpedoMathModel
{
   private List<Vector3> _trackedPositions = new List<Vector3>();

   private List<Vector3> _kalmanPoistions = new List<Vector3>();
   [SerializeField] private float _kalmanK = 0.5f;

   public void AddTrackPoint(Vector3 trackedPoint)
   {
      _trackedPositions.Add(trackedPoint);
   }

   public Vector3 CalcCourse()
   {
      Vector3 pointsSum = Vector3.zero;
      for (int i = 0; i < _trackedPositions.Count; i++)
         pointsSum -= _trackedPositions[i].normalized;

      return (pointsSum / _trackedPositions.Count).normalized;
   }

   public float CalcSpeed()
   {
      float magnitudesSum = 0;
      for (int i = 0; i < _trackedPositions.Count - 1; i++)
         magnitudesSum += (_trackedPositions[i + 1] - _trackedPositions[i]).magnitude;

      return magnitudesSum / _trackedPositions.Count;
   }

   public Vector3 CalcPos()
   {
      if (_kalmanPoistions.Count == 0)
         _kalmanPoistions.Add(_trackedPositions.Last());

      _kalmanPoistions.Add(_kalmanK * _trackedPositions.Last() + (1 - _kalmanK) * _kalmanPoistions[_kalmanPoistions.Count - 1]);

      return _kalmanPoistions.Last();
   }

   public Vector3 CalcNextPos()
   {
      return _kalmanPoistions.Last() + CalcCourse() * CalcSpeed();
   }
}
