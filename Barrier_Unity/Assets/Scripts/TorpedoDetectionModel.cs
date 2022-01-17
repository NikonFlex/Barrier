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

   private bool _regressionReady = false;
   private float _a;
   private float _b;

   public void AddTrackPoint(Vector3 trackedPoint)
   {
      if (_kalmanPoistions.Count == 0)
         _kalmanPoistions.Add(trackedPoint);
      else
         _kalmanPoistions.Add(_kalmanK * trackedPoint + (1 - _kalmanK) * _kalmanPoistions[_kalmanPoistions.Count - 1]);

      // use only last 10 points
      if (_kalmanPoistions.Count > 10)
      {
         _kalmanPoistions.RemoveAt(0);
         calcRegression();
      }
   }

   public Vector3 CalcCourse()
   {
      if (!_regressionReady)
         return Vector3.zero;

      Vector3 p1 = new Vector3(_kalmanPoistions[0].x, 5, _a * _kalmanPoistions[0].x + _b);
      Vector3 p2 = new Vector3(_kalmanPoistions[9].x, 5, _a * _kalmanPoistions[9].x + _b);

      return (p2 - p1).normalized;
   }

   public float CalcSpeed()
   {
      float magnitudesSum = 0;

      if (_kalmanPoistions.Count < 2)
         return magnitudesSum;

      // dt == 1
      for (int i = 0; i < _kalmanPoistions.Count - 1; i++)
         magnitudesSum += (_kalmanPoistions[i + 1] - _kalmanPoistions[i]).magnitude;

      return magnitudesSum / (_kalmanPoistions.Count - 1);
   }

   public Vector3 CalcPos()
   {
      Vector3 dir = CalcCourse();

      float a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

      Quaternion rot = Quaternion.Euler(0, a, 0);
      Quaternion rot2 = Quaternion.Euler(0, -a, 0);

      Vector3 p0 = new Vector3(0, 0, _b);

      Vector3 p = rot2 * (_kalmanPoistions.Last() - p0);
      p = new Vector3(0, 0, p.z);
      p = rot * p + p0;

      return p;
   }

   public Vector3 CalcPrognosisPos(float deltaTime)
   {
      return _kalmanPoistions.Last() + CalcCourse() * CalcSpeed() * deltaTime;
   }

   private float calcMean(float[] arr)
   {
      float sum = 0;
      for (int i = 0; i < arr.Length; i++)
         sum += arr[i];
      
      return sum / arr.Length;
   }
   private float calcError(float[] arr, float m)
   {
      float sum = 0;
      for (int i = 0; i < arr.Length; i++)
         sum += Mathf.Pow(arr[i] - m, 2);
      
      return Mathf.Sqrt(sum / arr.Length) * 6;  // error = 3 * sigma * 2
   }

   private void calcRegression()
   {
      float x_mean = 0;
      float z_mean = 0;

      for (int i = 0; i < _kalmanPoistions.Count; i++)
      {
         x_mean += _kalmanPoistions[i].x;
         z_mean += _kalmanPoistions[i].z;
      }

      x_mean /= _kalmanPoistions.Count;
      z_mean /= _kalmanPoistions.Count;

      float xz_sum = 0;
      float x2_sum = 0;
      float z2_sum = 0;

      for (int i = 0; i < _kalmanPoistions.Count; i++)
      {
         float dx = _kalmanPoistions[i].x - x_mean;
         float dz = _kalmanPoistions[i].z - z_mean;

         xz_sum += dx * dz;
         x2_sum += dx * dx;
         z2_sum += dz * dz;
      }

      float r = xz_sum / Mathf.Sqrt(x2_sum * z2_sum);

      float sx = Mathf.Sqrt(x2_sum / (_kalmanPoistions.Count - 1));
      float sz = Mathf.Sqrt(z2_sum / (_kalmanPoistions.Count - 1));

      float a = r * sz / sx;
      float b = z_mean - a * x_mean;

      // z = ax + b
      _a = a;
      _b = b;

      _regressionReady = true;
   }

   private void OnDrawGizmos()
   {
      if (!_regressionReady)
         return;

      Vector3 p1 = new Vector3(_kalmanPoistions[0].x - 100, 5, _a * (_kalmanPoistions[0].x - 100) + _b);
      Vector3 p2 = new Vector3(_kalmanPoistions[9].x + 100, 5, _a * (_kalmanPoistions[9].x + 100) + _b);

      Gizmos.color = new Color(1, 1, 0, 0.5f);
      Gizmos.DrawLine(p1, p2);

      Vector3 dir = (p2 - p1).normalized;

      float a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

      direction = a;

      Quaternion rot  = Quaternion.Euler(0, a, 0);
      Quaternion rot2 = Quaternion.Euler(0, -a, 0);

      Vector3 p0 = new Vector3(0, 0, _b);

      float[] dist = new float[_kalmanPoistions.Count];
      float[] vel = new float[_kalmanPoistions.Count-1];

      for (int i = 0; i < _kalmanPoistions.Count; i++)
      {
         Gizmos.color = new Color(1, 0, 0, 0.5f);
         Gizmos.DrawSphere(_kalmanPoistions[i], 4f);

         Vector3 p = rot2 * (_kalmanPoistions[i] - p0);

         dist[i] = p.x;

         if (i < vel.Length)
         {
            Vector3 p_next = rot2 * (_kalmanPoistions[i+1] - p0);
            vel[i] = p_next.z - p.z;
         }

         p = new Vector3(0, 0, p.z);
         p = rot * p + p0;

         Gizmos.color = new Color(1, 1, 1, 0.5f);
         Gizmos.DrawLine(_kalmanPoistions[i], p);
      }

      float mean = calcMean(dist);
      float error = calcError(dist, mean);

      direction_error = error;

      mean = calcMean(vel);
      error = calcError(vel, mean);

      velocity = mean;
      velocity_error = error;

   }

   public float direction;
   public float velocity;
   public float direction_error;
   public float velocity_error;
}