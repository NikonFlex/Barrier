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
   private List<Vector3>[] _kalmanPoistions = new List<Vector3>[15]; // 15 - number of combinations for 6 buoys
   [SerializeField] private float _kalmanK = 1f;

   private bool _regressionReady = false;
   private float _reg_a;
   private float _reg_b;

   private Vector3 _reg_pos;
   
   [SerializeField] private float _reg_direction;
   [SerializeField] private float _reg_direction_dist;
   [SerializeField] private float _reg_velocity;
   [SerializeField] private float _reg_velocity_error;

   public void AddTrackPoint(int idx, Vector3 trackedPoint)
   {
      if (_kalmanPoistions[idx] == null)
         _kalmanPoistions[idx] = new List<Vector3>();

      if (_kalmanPoistions[idx].Count == 0)
         _kalmanPoistions[idx].Add(trackedPoint);
      else
         _kalmanPoistions[idx].Add(_kalmanK * trackedPoint + (1 - _kalmanK) * _kalmanPoistions[idx][_kalmanPoistions[idx].Count - 1]);

      // use only last 20 points
      if (_kalmanPoistions[idx].Count > 20)
         _kalmanPoistions[idx].RemoveAt(0);

      if(_kalmanPoistions[0].Count > 4)
         calcRegression();
   }

   public void ClearRegression()
   {
      _regressionReady = false;
      for (int i = 0; i < _kalmanPoistions.Length; i++)
         _kalmanPoistions[i] = null;
   }

   public Vector3 CalcCourse()
   {
      if (!_regressionReady)
         return Vector3.zero;

      int count = _kalmanPoistions[0].Count;
      Vector3 p1 = new Vector3(_kalmanPoistions[0][0].x, 0, _reg_a * _kalmanPoistions[0][0].x + _reg_b);
      Vector3 p2 = new Vector3(_kalmanPoistions[0][count-1].x, 0, _reg_a * _kalmanPoistions[0][count-1].x + _reg_b);

      return (p2 - p1).normalized;
   }

   public float CalcSpeed()
   {
      if (!_regressionReady)
         return 0;

      return _reg_velocity;
   }

   public Vector3 CalcPos()
   {
      Vector3 dir = CalcCourse();

      float a = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

      Quaternion rot_org = Quaternion.Euler(0, a, 0);
      Quaternion rot_inv = Quaternion.Euler(0, -a, 0);

      Vector3 p0 = new Vector3(0, 0, _reg_b);

      Vector3 p = rot_inv * (_reg_pos - p0);
      p = new Vector3(0, 0, p.z);
      p = rot_org * p + p0;

      return p;
   }

   public Vector3 CalcPrognosisPos(float deltaTime)
   {
      return CalcPos() + CalcCourse() * CalcSpeed() * deltaTime;
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
      
      return Mathf.Sqrt(sum / arr.Length) * 3;  // error = 3 * sigma
   }

   private void calcRegression()
   {
      //calculate average of points for all combinations
      Vector3[] points = new Vector3[_kalmanPoistions[0].Count];

      for (int i = 0; i < _kalmanPoistions[0].Count; i++)
      {
         Vector3 tp = Vector3.zero;
         int num = 0;

         for (int j = 0; j < _kalmanPoistions.Length; j++)
         {
            if (_kalmanPoistions[j] == null)
               break;

            if (i > _kalmanPoistions[j].Count - 1)
               break;

            tp += _kalmanPoistions[j][i];
            num++;
         }

         points[i] = tp / num;
      }

      // calculate regression line
      float x_mean = 0;
      float z_mean = 0;

      for (int i = 0; i < points.Length; i++)
      {
         x_mean += points[i].x;
         z_mean += points[i].z;
      }

      x_mean /= points.Length;
      z_mean /= points.Length;

      float xz_sum = 0;
      float x2_sum = 0;
      float z2_sum = 0;

      for (int i = 0; i < points.Length; i++)
      {
         float dx = points[i].x - x_mean;
         float dz = points[i].z - z_mean;

         xz_sum += dx * dz;
         x2_sum += dx * dx;
         z2_sum += dz * dz;
      }

      float r = xz_sum / Mathf.Sqrt(x2_sum * z2_sum);

      float sx = Mathf.Sqrt(x2_sum / (points.Length - 1)) ;
      float sz = Mathf.Sqrt(z2_sum / (points.Length - 1));

      float a = r * sz / sx;
      float b = z_mean - a * x_mean;

      // z = ax + b
      _reg_a = a;
      _reg_b = b;

      _regressionReady = true;

      // calculate statistics
      Vector3 dir = CalcCourse();
      float course = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;

      Quaternion rot = Quaternion.Euler(0, -course, 0);

      Vector3 p0 = new Vector3(0, 0, _reg_b);

      float[] dist = new float[points.Length];
      float[] vel = new float[points.Length - 1];

      for(int i = 0; i < points.Length; i++)
      {
         Vector3 p = rot * (points[i] - p0);
         dist[i] = p.x;
         
         if (i < vel.Length)
         {
            Vector3 p_next = rot * (points[i + 1] - p0);
            vel[i] = p_next.z - p.z;
         }
      }

      _reg_pos = points[points.Length - 1];

      float mean = calcMean(dist);
      float error = calcError(dist, mean);

      _reg_direction_dist = error;

      mean = calcMean(vel);
      error = calcError(vel, mean);

      _reg_velocity = mean;
      _reg_velocity_error = error;

      VarName.TargetDetectionError.Set(string.Format("{0:0}", error * 4));

      _reg_direction = course;
   }

   private void OnDrawGizmos()
   {
      if (!_regressionReady)
         return;

      int count = _kalmanPoistions[0].Count;
      Vector3 p1 = new Vector3(_kalmanPoistions[0][0].x - 200, 5, _reg_a * (_kalmanPoistions[0][0].x - 200) + _reg_b);
      Vector3 p2 = new Vector3(_kalmanPoistions[0][count-1].x + 200, 5, _reg_a * (_kalmanPoistions[0][count-1].x + 200) + _reg_b);

      Gizmos.color = new Color(1, 1, 0, 0.5f);
      Gizmos.DrawLine(p1, p2);

      for (int i = 0; i < _kalmanPoistions[0].Count; i++)
      {
         Vector3 tp = Vector3.zero;
         count = 0;

         for (int j = 0; j < _kalmanPoistions.Length; j++)
         {
            if (_kalmanPoistions[j] == null)
               break;

            if (i > _kalmanPoistions[j].Count -1)
               break;

            tp += _kalmanPoistions[j][i];
            count++;
         }

         Gizmos.color = new Color(1, 1, 1, 0.75f);
         Gizmos.DrawSphere(tp/count, 4f);
      }
   }
}