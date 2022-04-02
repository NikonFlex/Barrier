using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public enum ZoneColor
{
   Green,
   Yellow,
   Red,
   None
}

public class BuoyGuard : MonoBehaviour
{
   [SerializeField] private Transform _torpedo;
   [SerializeField] private Material _material;
   [SerializeField] private Material _targetErrorZoneMaterial;
   [SerializeField] private TorpedoDetectionModel _torpedoDetectionModel;

   private List<Buoy> _buoys = new List<Buoy>();
   public bool IsTorpedoFinallyDetected { get; private set; }

   private GameObject _rombZone;
   private GameObject _elipseZone;
   private GameObject _splineZone;
   private DetectionArea _detectionZone;
   private ZoneColor _zoneColor = ZoneColor.None;

   private float _startScanTime = -1;
   private float _scanningError = 1f;

   private float _rocketSpeed;

   public Vector3[] RealZone { get; private set; }
   public Transform DetectZone => _splineZone.transform;
   public float ScanningError => _scanningError;
   private float _detectRange => VarSync.GetFloat(VarName.BuoysDetectRange);

   void Start()
   {
      _rocketSpeed = FindObjectOfType<RocketLauncher>().RocketSpeed;
      createZoneObject();
      StartCoroutine(trackTargetPosition());
   }

   void Update()
   {
      if (!Scenario.IsTargetAlive)
      {
         deactivateZone();
         return;
      }

      if (_buoys.Count > 1)
         activateZone();
      else
         return;

      if (_startScanTime < 0)
         _startScanTime = Scenario.Instance.ScenarioTime;
      else
      {
         _scanningError = Mathf.Clamp01(1 - (Scenario.Instance.ScenarioTime - _startScanTime) / 5);
      }

      Vector3[] zonePoints = updateZonePoints();
      if (zonePoints.Length == 0)
         return;

      RealZone = zonePoints;
      Vector3 c1 = RealZone[0];
      Vector3 c2 = RealZone[1];
      Vector3 c3 = RealZone[2];
      Vector3 c4 = RealZone[3];

      Vector3[] vertices = { c1 - c1, c2 - c1, c3 - c1, c4 - c1 };
      updateZoneMesh(c1, c2, c3, c4, vertices);
   }

   private void createZoneObject()
   {
      _splineZone = new GameObject("spline_zone");
      _splineZone.AddComponent<MeshFilter>().mesh = Utils.CreateSplineMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
      _splineZone.AddComponent<MeshRenderer>().material = _material;

      _detectionZone = Instantiate(Resources.Load<DetectionArea>("DetectionArea"));
   }

   private void activateZone()
   {
      _splineZone.SetActive(true);
      _detectionZone.gameObject.SetActive(true);
   }

   private void deactivateZone()
   {
      _splineZone.SetActive(false);
      _detectionZone.gameObject.SetActive(false);
   }

   private Vector3[] updateZonePoints()
   {
      for (int i = 0; i < _buoys.Count - 1; i++)
      {
         if (_buoys[i].State != BuoyState.Working)
            continue;

         for (int j = i + 1; j < _buoys.Count; j++)
         {
            if (_buoys[j].State != BuoyState.Working)
               continue;

            float b1Error = _buoys[i].Error;
            float b2Error = _buoys[j].Error;

            Vector3 vb = (_torpedo.position - _buoys[i].transform.position).normalized;
            vb = new Vector3(vb.x, 0, vb.z).normalized;
            Vector3 vr = getDir(vb, b1Error + Buoy.GetBearingError() / 2f);
            Vector3 vl = getDir(vb, b1Error - Buoy.GetBearingError() / 2f);

            Vector3 p1 = new Vector3(_buoys[i].transform.position.x, 0, _buoys[i].transform.position.z);
            Vector3 p1r = p1 + vr * _detectRange;
            Vector3 p1l = p1 + vl * _detectRange;

            vb = (_torpedo.position - _buoys[j].transform.position).normalized;
            vb = new Vector3(vb.x, 0, vb.z).normalized;
            vr = getDir(vb, b2Error + Buoy.GetBearingError() / 2f);
            vl = getDir(vb, b2Error - Buoy.GetBearingError() / 2f);

            Vector3 p2 = new Vector3(_buoys[j].transform.position.x, 0, _buoys[j].transform.position.z);
            Vector3 p2r = p2 + vr * _detectRange;
            Vector3 p2l = p2 + vl * _detectRange;

            Vector3 c1 = Vector3.zero;
            bool f1 = getCross(p1, p1l, p2, p2l, out c1);
            Vector3 c2 = Vector3.zero;
            bool f2 = getCross(p1, p1r, p2, p2l, out c2);
            Vector3 c3 = Vector3.zero;
            bool f3 = getCross(p1, p1r, p2, p2r, out c3);
            Vector3 c4 = Vector3.zero;
            bool f4 = getCross(p1, p1l, p2, p2r, out c4);

            if (f1 && f2 && f3 && f4)
            {
               return new Vector3[] { c1, c2, c3, c4 };
            }
         }
      }

      return new Vector3[0];
   }

   private void updateZoneMesh(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4, Vector3[] vertices)
   {
      //draw spline
      _splineZone.GetComponent<MeshFilter>().mesh = Utils.CreateSplineMesh(vertices[0], vertices[1], vertices[2], vertices[3]);
      _splineZone.transform.position = new Vector3(c1.x, 3f, c1.z);
      _splineZone.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, Mathf.PingPong(Time.time, 0.5f));

      //draw detection zone
      if (_torpedoDetectionModel.RegressionReady)
      {
         float r = VarName.TargetDetectionError.GetFloat();
         _detectionZone.transform.position += _torpedoDetectionModel.CalcCourse() * _torpedoDetectionModel.CalcSpeed() * Time.deltaTime;
         _detectionZone.SetRadius(r);
      }
   }

   private IEnumerator trackTargetPosition()
   {
      while (true)
      {
         if (_buoys.Count < 2)
         {
            yield return null;
            continue;
         }

         if (Scenario.IsTargetAlive)
         {
            int idx = 0;

            for (int i = 0; i < _buoys.Count - 1; i++)
            {
               for (int j = i + 1; j < _buoys.Count; j++)
               {
                  if (_buoys[i].State != BuoyState.Working || _buoys[j].State != BuoyState.Working)
                  {
                     _torpedoDetectionModel.AddTrackPoint(idx, Vector3.zero);
                     idx++;
                     continue;
                  }

                  //calculate real tracked point
                  Vector3 b1Bearing = (_torpedo.position - _buoys[i].transform.position).normalized;
                  Vector3 b2Bearing = (_torpedo.position - _buoys[j].transform.position).normalized;

                  Vector3 b1BearingWithError = getDir(b1Bearing, _buoys[i].Error);
                  Vector3 b2BearingWithError = getDir(b2Bearing, _buoys[j].Error);

                  Vector3 p11 = _buoys[i].transform.position;
                  Vector3 p12 = p11 + b1BearingWithError * _detectRange;
                  Vector3 p21 = _buoys[j].transform.position;
                  Vector3 p22 = p21 + b2BearingWithError * _detectRange;

                  Vector3 bouysBearingIntersection = Vector3.zero;
                  bool crossed = getCross(p11, p12, p21, p22, out bouysBearingIntersection);
                  if (!crossed)
                     bouysBearingIntersection = Vector3.zero;

                  Vector3 trackedPosition = new Vector3(bouysBearingIntersection.x, 0, bouysBearingIntersection.z);

                  _torpedoDetectionModel.AddTrackPoint(idx, trackedPosition);

                  idx++;
               }
            }

            for (int i = idx; i < 15; i++)
               _torpedoDetectionModel.AddTrackPoint(i, Vector3.zero);

            _torpedoDetectionModel.CalcRegression();

            if (_torpedoDetectionModel.RegressionReady)
            {
               refreshDetectionZoneColor(VarName.TargetDetectionError.GetFloat());
               float timeToShoot = Scenario.Instance.TargetInfo.Distance / (_rocketSpeed + _torpedoDetectionModel.CalcSpeed());
               Vector3 p = _torpedoDetectionModel.CalcPrognosisPos(timeToShoot);
               _detectionZone.transform.position = new Vector3(p.x, 1.5f, p.z);
            }

            yield return new WaitForSeconds(1f);
         }

         yield return null;
      }
   }

   private void OnDrawGizmos()
   {
      float h = _torpedo.position.y;
      Vector3 t0 = new Vector3(_torpedo.position.x, h, _torpedo.position.z);
      Vector3 t1 = _torpedo.forward * 100 + Vector3.up * h;
      Vector3 t2 = Quaternion.Euler(new Vector3(0, 180, 0)) * _torpedo.forward * 500 + Vector3.up * h;
      Vector3 t3 = Quaternion.Euler(new Vector3(0, 90, 0)) * _torpedo.forward * 250 + Vector3.up * h;
      Vector3 t4 = Quaternion.Euler(new Vector3(0, -90, 0)) * _torpedo.forward * 250 + Vector3.up * h;

      Gizmos.color = new Color(1, 0, 1, 0.5f);
      Gizmos.DrawLine(t0, t0 + t1);
      Gizmos.DrawLine(t0, t0 + t2);
      Gizmos.DrawLine(t0, t0 + t3);
      Gizmos.DrawLine(t0, t0 + t4);

      foreach (var b in _buoys)
      {
         if (b.State != BuoyState.Working)
            continue;

         Vector3 bearing = (_torpedo.position - b.transform.position).normalized;
         bearing = new Vector3(bearing.x, 0, bearing.z).normalized;
         Vector3 bCenter = getDir(bearing, b.Error);
         Vector3 bLeft = getDir(bearing, 0/*b.Error*/ - Buoy.GetBearingError() / 2f);
         Vector3 bRight = getDir(bearing, 0/*b.Error*/ + Buoy.GetBearingError() / 2f);
         Vector3 p = new Vector3(b.transform.position.x, 2, b.transform.position.z);
         Vector3 pCenter = p + bCenter * _detectRange;
         Vector3 pLeft = p + bLeft * _detectRange;
         Vector3 pRight = p + bRight * _detectRange;

         Gizmos.color = Color.blue + new Color(0, 0, 0, 0.5f);
         Gizmos.DrawLine(p, pLeft);
         Gizmos.DrawLine(p, pRight);

         Gizmos.color = Color.yellow + new Color(0, 0, 0, 0.5f); 
         Gizmos.DrawLine(p, pCenter);
      }
   }

   public void AddBuoy(Buoy b)
   {
      if (_buoys.Count < 6)
         _buoys.Add(b);
      else
         Debug.LogError("Exceed number of buoys");
   }

   private bool getCross(Vector3 p11, Vector3 p12, Vector3 p21, Vector3 p22, out Vector3 cross)
   {
      p11.y = 1;
      p12.y = 1;
      p21.y = 1;
      p22.y = 1;

      Vector3 f1 = Vector3.Cross(p11, p12);
      Vector3 f2 = Vector3.Cross(p21, p22);

      cross = Vector3.Cross(f1, f2);

      if (Mathf.Abs(cross.y) < 1f)
      {
         cross = Vector3.zero;
         return false;
      }

      cross.x = cross.x / cross.y;
      cross.z = cross.z / cross.y;
      cross.y = 0;

      float d = (_torpedo.position - cross).magnitude;

      if (d > VarSync.GetFloat(VarName.BuoysDetectRange))
         return false;

      return true;
   }

   private Vector3 getDir(Vector3 v, float a)
   {
      return Quaternion.AngleAxis(a, Vector3.up) * v;
   }

   private ZoneColor calcZoneColor(float radius)
   {
      if (radius <= 0 || radius > VarSync.GetFloat(VarName.YellowZoneD))
         return ZoneColor.Red;
      if (radius > VarSync.GetFloat(VarName.GreenZoneD))
         return ZoneColor.Yellow;
      return ZoneColor.Green;
   }

   private void refreshDetectionZoneColor(float curRaduis)
   {
      ZoneColor clr = calcZoneColor(curRaduis * 2);
      if (clr == _zoneColor)
         return;
      _zoneColor = clr;
      switch (_zoneColor)
      {
         case ZoneColor.Red:
            _detectionZone.SetColor(new Color(1, 0, 0, 0.2f)); //red
            break;
         case ZoneColor.Yellow:
            _detectionZone.SetColor(new Color(1, 0.64f, 0, 0.2f));
            break;
         case ZoneColor.Green:
            _detectionZone.SetColor(new Color(0, 1, 0, 0.2f));
            IsTorpedoFinallyDetected = true;
            break;
      }
   }
}
