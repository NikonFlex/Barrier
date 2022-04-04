using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TorpedoDetectionModel : MonoBehaviour
{
   private List<Vector3>[] _rawPoints = new List<Vector3>[15]; // 15 - number of combinations for 6 buoys
   private List<Vector3> _points = new List<Vector3>();
   [SerializeField] private float _kalmanK = 1f;

   private bool _regressionReady = false;
   private float _reg_a;
   private float _reg_b;

   private Vector3 _reg_pos;

   public bool RegressionReady => _regressionReady;
   
   [SerializeField] private float _reg_direction;
   [SerializeField] private float _reg_velocity;
   [SerializeField] private float _reg_velocity_error;

   private void Start()
   {
      for(int i = 0; i < _rawPoints.Length; i++)
         _rawPoints[i] = new List<Vector3>();
   }

   public void AddTrackPoint(int idx, Vector3 trackedPoint)
   {
      if (_rawPoints[idx].Count == 0)
         _rawPoints[idx].Add(trackedPoint);
      else
         _rawPoints[idx].Add(_kalmanK * trackedPoint + (1 - _kalmanK) * _rawPoints[idx][_rawPoints[idx].Count - 1]);

      // use only last 10 points
      if (_rawPoints[idx].Count > 10)
         _rawPoints[idx].RemoveAt(0);
   }

   public void ClearRegression()
   {
      _regressionReady = false;
      for (int i = 0; i < _rawPoints.Length; i++)
         _rawPoints[i].Clear();
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
      return _reg_pos;
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
   
      if (_rawPoints[0].Count < 5)
         return;

      //calculate average of points for all combinations
      _points.Clear();

      for (int i = 0; i < _rawPoints[0].Count; i++)
      {
         Vector3 tp = Vector3.zero;
         int n = 0;

         for (int j = 0; j < _rawPoints.Length; j++)
         {
            if (_rawPoints[j][i].magnitude < 0.1f)
               continue;
            
            tp += _rawPoints[j][i];
            n++;
         }

         if (n > 0)
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

      Quaternion rot_org = Quaternion.Euler(0, course, 0);
      Quaternion rot_inv = Quaternion.Euler(0, -course, 0);

      Vector3 p0 = new Vector3(0, 0, _reg_b);

      List<float> velList = new();

      for(int i = 0; i < _points.Count - 1; i++)
      {
         Vector3 p1 = rot_inv * (_points[i] - p0);
         Vector3 p2 = rot_inv * (_points[i + 1] - p0);

         if ((p2.z - p1.z) > 0)
         {
            velList.Add(p2.z - p1.z);
            _reg_pos = rot_org * new Vector3(0, 0, p2.z) + p0;
         }
      }

      float[] vel = velList.ToArray();
      float mean = calcMean(vel);
      float error = calcError(vel, mean);

      VarName.TargetDetectionError.Set(error * 4); //4 seconds

      _reg_direction = course;
      _reg_velocity = mean;
      _reg_velocity_error = error;
   }

   private void OnDrawGizmos()
   {
      if (_regressionReady)
      {
         Vector3 p1 = new Vector3(_points.First().x - 200, 5, _reg_a * (_points.First().x - 200) + _reg_b);
         Vector3 p2 = new Vector3(_points.Last().x + 200, 5, _reg_a * (_points.Last().x + 200) + _reg_b);

         Gizmos.color = new Color(1, 1, 0, 0.5f);
         Gizmos.DrawLine(p1, p2);
      }

      if (_points.Count > 1)
      {
         Gizmos.color = Color.white * new Color(1, 1, 1, 0.5f);
         for (int i = 0; i < _points.Count - 1; i++)
         {
            Gizmos.DrawSphere(_points[i], 5f);
            Gizmos.DrawLine(_points[i], _points[i + 1]);
         }
         Gizmos.color = Color.blue * new Color(1, 1, 1, 0.5f);
         Gizmos.DrawSphere(_reg_pos, 5f);
      }
   }
}