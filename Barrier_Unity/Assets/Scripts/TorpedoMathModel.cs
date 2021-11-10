using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

struct TrackedPoint
{
   private float time;
   private Vector3 pos;

   public TrackedPoint(float time_, Vector3 pos_)
   {
      time = time_;
      pos = pos_;
   }

   public float Time => time;
   public Vector3 Pos => pos;
}

public class TorpedoMathModel
{
   private List<TrackedPoint> _trackedPositions = new List<TrackedPoint>();

   private List<Vector3> _kalmanPoistions = new List<Vector3>();
   [SerializeField] private float _kalmanK = 0.5f;

   public void AddTrackPoint(Vector3 trackedPoint, float time)
   {
      _trackedPositions.Add(new TrackedPoint(time, trackedPoint));
   }

   public Vector3 CalcCourse()
   {
      Vector3 pointsSum = Vector3.zero;
      for (int i = 0; i < _trackedPositions.Count; i++)
         pointsSum -= _trackedPositions[i].Pos;

      return (pointsSum / _trackedPositions.Count).normalized;
   }

   public float CalcSpeed()
   {
      float magnitudesSum = 0;
      for (int i = 0; i < _trackedPositions.Count - 1; i++)
         magnitudesSum += (_trackedPositions[i + 1].Pos - _trackedPositions[i].Pos).magnitude;

      return magnitudesSum / _trackedPositions.Count;
   }

   public Vector3 CalcPos()
   {
      if (_kalmanPoistions.Count == 0)
         _kalmanPoistions.Add(_trackedPositions.Last().Pos);

      _kalmanPoistions.Add(_kalmanK * _trackedPositions.Last().Pos + (1 - _kalmanK) * _kalmanPoistions[_kalmanPoistions.Count - 1]);

      return _kalmanPoistions.Last();
   }

   public Vector3 CalcPrognozePos(float deltaTime)
   {
      return _kalmanPoistions.Last() + CalcCourse() * CalcSpeed() * deltaTime;
   }
}
