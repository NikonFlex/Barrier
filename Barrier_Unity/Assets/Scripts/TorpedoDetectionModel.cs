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
   private List<Vector3> _points = new List<Vector3>();
   [SerializeField] private float _kalmanK = 1f;

   private bool _regressionReady = false;
   private float _reg_a;
   private float _reg_b;

   private Vector3 _reg_pos;

   public bool RegressionReady => _regressionReady;
   
   [SerializeField] private float _reg_direction;
   [SerializeField] private float _reg_direction_dist;
   [SerializeField] private float _reg_velocity;
   [SerializeField] private float _reg_velocity_error;

   private void Start()
   {
      for(int i = 0; i < _kalmanPoistions.Length; i++)
         _kalmanPoistions[i] = new List<Vector3>();
   }

   public void AddTrackPoint(int idx, Vector3 trackedPoint)
   {
      if (_kalmanPoistions[idx].Count == 0)
         _kalmanPoistions[idx].Add(trackedPoint);
      else
         _kalmanPoistions[idx].Add(_kalmanK * trackedPoint + (1 - _kalmanK) * _kalmanPoistions[idx][_kalmanPoistions[idx].Count - 1]);

      // use only last 20 points
      if (_kalmanPoistions[idx].Count > 20)
         _kalmanPoistions[idx].RemoveAt(0);
   }

   public void ClearRegression()
   {
      _regressionReady = false;
      for (int i = 0; i < _kalmanPoistions.Length; i++)
         _kalmanPoistions[i].Clear();
   }

   public Vector3 CalcCourse()
   {
      if (!_regressionReady)
         return Vector3.zero;

      Vector3 p1 = new Vector3(_points.First().x, 0, _reg_a * _points.First().x + _reg_b);
      Vector3 p2 = new Vector3(_points.Last().x, 0, _reg_a * _points.Last().x + _reg_b);
                                                                      
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

   public void CalcRegression()
   {
      _regressionReady = false;
   
      if (_kalmanPoistions[0].Count < 5)
         return;

      //calculate average of points for all combinations
      _points.Clear();

      for (int i = 0; i < _kalmanPoistions[0].Count; i++)
      {
         Vector3 tp = Vector3.zero;
         int n = 0;

         for (int j = 0; j < _kalmanPoistions.Length; j++)
         {
            if (_kalmanPoistions[j][i].magnitude < 0.1f)
               continue;
            
            tp += _kalmanPoistions[j][i];
            n++;
         }

         if(n > 0)
            _points.Add(tp / n);
      }

      if (_points.Count < 2)
         return;

      // calculate regression line
      float x_mean = 0;
      float z_mean = 0;

      foreach (var p in _points)
      {
         x_mean += p.x;
         z_mean += p.z;
      }

      x_mean /= _points.Count;
      z_mean /= _points.Count;

      float xz_sum = 0;
      float x2_sum = 0;
      float z2_sum = 0;

      foreach (var p in _points)
      {
         float dx = p.x - x_mean;
         float dz = p.z - z_mean;

         xz_sum += dx * dz;
         x2_sum += dx * dx;
         z2_sum += dz * dz;
      }

      float r = xz_sum / Mathf.Sqrt(x2_sum * z2_sum);

      float sx = Mathf.Sqrt(x2_sum / (_points.Count - 1));
      float sz = Mathf.Sqrt(z2_sum / (_points.Count - 1));

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

      float[] dist = new float[_points.Count];
      float[] vel = new float[_points.Count-1];

      for(int i = 0; i < _points.Count; i++)
      {
         Vector3 p = rot * (_points[i] - p0);
         dist[i] = p.x;
         
         if (i < vel.Length)
         {
            Vector3 p_next = rot * (_points[i + 1] - p0);
            vel[i] = (p_next.z - p.z) > 0 ? (p_next.z - p.z) : 0;
         }
      }

      _reg_pos = _points.Last();

      float mean = calcMean(dist);
      float error = calcError(dist, mean);

      _reg_direction_dist = error;

      mean = calcMean(vel);
      error = calcError(vel, mean);

      _reg_velocity = mean;
      _reg_velocity_error = error;

      VarName.TargetDetectionError.Set(error * 4); //4 seconds

      _reg_direction = course;
   }

   private void OnDrawGizmos()
   {
      if (!_regressionReady)
         return;

      int count = _points.Count;
      Vector3 p1 = new Vector3(_points[0].x - 200, 5, _reg_a * (_points[0].x - 200) + _reg_b);
      Vector3 p2 = new Vector3(_points[count-1].x + 200, 5, _reg_a * (_points[count-1].x + 200) + _reg_b);

      Gizmos.color = new Color(1, 1, 0, 0.5f);
      Gizmos.DrawLine(p1, p2);

      foreach (var p in _points)
      {
         Gizmos.color = new Color(1, 1, 1, 0.75f);
         Gizmos.DrawSphere(p, 4f);
      }                     
   }
}