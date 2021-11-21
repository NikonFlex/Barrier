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

public class TorpedoDetectionModel : MonoBehaviour
{
   private List<Vector3> _kalmanPoistions = new List<Vector3>();
   [SerializeField] private float _kalmanK = 0.5f;

   public void AddTrackPoint(Vector3 trackedPoint)
   {
      if (_kalmanPoistions.Count == 0)
         _kalmanPoistions.Add(trackedPoint);
      else
         _kalmanPoistions.Add(_kalmanK * trackedPoint + (1 - _kalmanK) * _kalmanPoistions[_kalmanPoistions.Count - 1]);

      // use only last 10 points
      if (_kalmanPoistions.Count > 10)
         _kalmanPoistions.RemoveAt(0);
   }

   public Vector3 CalcCourse()
   {
      Vector3 pointsSum = Vector3.zero;

      if (_kalmanPoistions.Count < 2)
         return pointsSum;

      for (int i = 0; i < _kalmanPoistions.Count; i++)
         pointsSum -= _kalmanPoistions[i];

      return (pointsSum / (_kalmanPoistions.Count)).normalized;
   }

   public float CalcSpeed()
   {
      float magnitudesSum = 0;

      if (_kalmanPoistions.Count < 2)
         return magnitudesSum;

      // dt == 1
      for (int i = 0; i < _kalmanPoistions.Count - 1; i++)
         magnitudesSum += (_kalmanPoistions[i + 1] - _kalmanPoistions[i]).magnitude;

      return magnitudesSum / (_kalmanPoistions.Count-1);
   }

   public Vector3 CalcPos()
   {
      return _kalmanPoistions.Last();
   }

   public Vector3 CalcPrognosisPos(float deltaTime)
   {
      return _kalmanPoistions.Last() + CalcCourse() * CalcSpeed() * deltaTime;
   }
}
