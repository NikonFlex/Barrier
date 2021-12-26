using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BuoyGuard : MonoBehaviour
{
   public Transform m_torpedo;
   public Material m_material;

   private Buoy m_bouy1;
   private Buoy m_bouy2;

   private float[] m_errorBeg = new float[2];
   private float[] m_errorEnd = new float[2];
   private float m_errorTime;

   private GameObject m_rombZone;
   private GameObject m_elipseZone;
   private GameObject m_splineZone;

   private bool _startDrawTrackedPosition = false;
   private float _startScanTime = -1;

   private GameObject _accumalatedTorpedoWay;
   [SerializeField] private float _accumalatedTorpedoWayLength = 10000;

   [SerializeField] private TorpedoDetectionModel _torpedoDetectionModel;

   private float m_detectRange => VarSync.GetFloat(VarName.BuoysDetectRange);
   public Transform DetectZone => m_splineZone.transform;
   
   public float _scanningError = 1f;
   public float ScanningError => _scanningError;

   public Vector3[] RealZone { get; private set; }

   private float getBearingError()
   {
      if (VarSync.GetInt(VarName.Weather) == 0)
         return VarSync.GetFloat(VarName.BuoysBearingError);
      else
         return VarSync.GetFloat(VarName.BuoysBearingError) * VarSync.GetFloat(VarName.BuoysBearingMultplier);
   }

   private float b1Error => Mathf.Lerp(m_errorBeg[0], m_errorEnd[0], Time.time - m_errorTime);
   private float b2Error => Mathf.Lerp(m_errorBeg[1], m_errorEnd[1], Time.time - m_errorTime);

   void Start()
   {
      UnityEngine.Random.InitState(DateTime.UtcNow.GetHashCode());

      m_rombZone = new GameObject("romb_zone");
      m_rombZone.AddComponent<MeshFilter>().mesh = Utils.CreateRombusMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
      m_rombZone.AddComponent<MeshRenderer>().material = m_material;

      m_elipseZone = new GameObject("elipse_zone");
      m_elipseZone.AddComponent<MeshFilter>().mesh = Utils.CreateEllipseMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
      m_elipseZone.AddComponent<MeshRenderer>().material = m_material;

      m_splineZone = new GameObject("spline_zone");
      m_splineZone.AddComponent<MeshFilter>().mesh = Utils.CreateSplineMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
      m_splineZone.AddComponent<MeshRenderer>().material = m_material;
      m_errorTime = Time.time;

      StartCoroutine(timerCoroutine());

      _accumalatedTorpedoWay = createAccumalatedTorpedoWay();
   }

   IEnumerator timerCoroutine()
   {
      while (true)
      {
         yield return new WaitForSeconds(1);

         m_errorBeg[0] = m_errorEnd[0];
         m_errorBeg[1] = m_errorEnd[1];
         m_errorEnd[0] = Utils.GaussRandom(getBearingError() / 2f);
         m_errorEnd[1] = Utils.GaussRandom(getBearingError() / 2f);
         m_errorTime = Time.time;
      }
   }

   // Update is called once per frame
   void Update()
   {
      var target = Scenario.Instance.TargetInfo;
      if (target == null || !Scenario.Instance.TargetInfo.Target.IsAlive)
      {
         m_rombZone.SetActive(false);
         m_elipseZone.SetActive(false);
         m_splineZone.SetActive(false);
         return;
      }

      if (m_bouy1 == null || m_bouy2 == null)
         return;

      m_rombZone.SetActive(true);
      m_elipseZone.SetActive(true);
      m_splineZone.SetActive(true);

      if (_startScanTime < 0)
         _startScanTime = Scenario.Instance.ScenarioTime;
      else
      {
         _scanningError = Mathf.Clamp01(1 - (Scenario.Instance.ScenarioTime - _startScanTime) / 5);
      }

      if (_startDrawTrackedPosition)
      {
         StartCoroutine(drawTrackedTargetPosition());
         _startDrawTrackedPosition = false;
      }

      Vector3 vc = Quaternion.AngleAxis(b1Error, Vector3.up) * (m_torpedo.position - m_bouy1.transform.position).normalized;
      Vector3 vr = getDir(vc, getBearingError() / 2);
      Vector3 vl = getDir(vc, -getBearingError() / 2);

      Vector3 p1 = m_bouy1.transform.position;
      Vector3 p1r = m_bouy1.transform.position + vr * m_detectRange + Vector3.up;
      Vector3 p1l = m_bouy1.transform.position + vl * m_detectRange + Vector3.up;

      vc = Quaternion.AngleAxis(b2Error, Vector3.up) * (m_torpedo.position - m_bouy2.transform.position).normalized;
      vr = getDir(vc, getBearingError() / 2);
      vl = getDir(vc, -getBearingError() / 2);

      Vector3 p2 = m_bouy2.transform.position;
      Vector3 p2r = m_bouy2.transform.position + vr * m_detectRange + Vector3.up;
      Vector3 p2l = m_bouy2.transform.position + vl * m_detectRange + Vector3.up;

      Vector3 c1 = Vector3.zero;
      bool f1 = getCross(p1, p1l, p2, p2l, out c1);
      Vector3 c2 = Vector3.zero;
      bool f2 = getCross(p1, p1r, p2, p2l, out c2);
      Vector3 c3 = Vector3.zero;
      bool f3 = getCross(p1, p1r, p2, p2r, out c3);
      Vector3 c4 = Vector3.zero;
      bool f4 = getCross(p1, p1l, p2, p2r, out c4);

      RealZone = new Vector3[] { c1, c2, c3, c4 };

      if (!(f1 && f2 && f3 && f4))
      {
         m_rombZone.GetComponent<MeshFilter>().mesh = Utils.CreateRombusMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
         m_elipseZone.GetComponent<MeshFilter>().mesh = Utils.CreateEllipseMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
         m_splineZone.GetComponent<MeshFilter>().mesh = Utils.CreateSplineMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
         return;
      }

      Vector3[] vertices = { c1 - c1, c2 - c1, c3 - c1, c4 - c1 };

      //draw rombus
      m_rombZone.GetComponent<MeshFilter>().mesh = Utils.CreateRombusMesh(vertices[0], vertices[1], vertices[2], vertices[3]);
      m_rombZone.transform.position = new Vector3(c1.x, 10, c1.z);
      m_rombZone.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, Mathf.PingPong(Time.time, 0.5f));

      //draw ellipse
      float magn1 = (vertices[0] - vertices[2]).magnitude / 2;
      float magn2 = (vertices[1] - vertices[3]).magnitude / 2;
      if (magn2 > magn1)
      {
         m_elipseZone.GetComponent<MeshFilter>().mesh = Utils.CreateEllipseMesh(vertices[0], vertices[1], vertices[2], vertices[3]);
         m_elipseZone.transform.eulerAngles = new Vector3(0, 180 - Vector3.SignedAngle((c2 - c4).normalized, Vector3.forward, Vector3.up), 0);
      }
      else
      {
         m_elipseZone.GetComponent<MeshFilter>().mesh = Utils.CreateEllipseMesh(vertices[1], vertices[2], vertices[3], vertices[0]);
         m_elipseZone.transform.eulerAngles = new Vector3(0, 180 - Vector3.SignedAngle((c1 - c3).normalized, Vector3.forward, Vector3.up), 0);
      }
      m_elipseZone.transform.position = new Vector3((c2.x + c4.x) / 2f, 10, (c2.z + c4.z) / 2f);
      m_elipseZone.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1, Mathf.PingPong(Time.time, 0.5f));

      //draw spline
      m_splineZone.GetComponent<MeshFilter>().mesh = Utils.CreateSplineMesh(vertices[0], vertices[1], vertices[2], vertices[3]);
      m_splineZone.transform.position = new Vector3(c1.x, 10, c1.z);
      m_splineZone.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, Mathf.PingPong(Time.time, 0.5f));
   }

   private IEnumerator drawTrackedTargetPosition()
   {
      while (Scenario.Instance.TargetInfo != null && Scenario.Instance.TargetInfo.Target.IsAlive)
      {
         //calculate real tracked point
         Vector3 b1Bearing = (m_torpedo.position - m_bouy1.transform.position).normalized;
         Vector3 b2Bearing = (m_torpedo.position - m_bouy2.transform.position).normalized;

         Vector3 b1BearingWithError = getDir(b1Bearing, b1Error);
         Vector3 b2BearingWithError = getDir(b2Bearing, b2Error);

         Vector3 p11 = m_bouy1.transform.position;
         Vector3 p12 = p11 + b1BearingWithError * m_detectRange;
         Vector3 p21 = m_bouy2.transform.position;
         Vector3 p22 = p21 + b2BearingWithError * m_detectRange;

         Vector3 bouysBearingIntersection = Vector3.zero;
         bool crossed = getCross(p11, p12, p21, p22, out bouysBearingIntersection);
         if (!crossed)
         {
            yield return null;
            continue;
         }

         p12 = p11 + b1Bearing * m_detectRange;
         p22 = p21 + b2Bearing * m_detectRange;

         Vector3 realCross = Vector3.zero;
         crossed = getCross(p11, p12, p21, p22, out realCross);

         Vector3 realPosition = new Vector3(realCross.x, 4, realCross.z);

         //draw real points
         GameObject realPointMesh = new GameObject("Real Track Point " + string.Format("{0:0.00}", Scenario.Instance.ScenarioTime));
         realPointMesh.AddComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(5, 30);
         realPointMesh.AddComponent<MeshRenderer>().material.color = new Color(1, 0, 1, 0.5f);
         realPointMesh.transform.position = realPosition;
         Destroy(realPointMesh, 12.0f);

         Vector3 trackedPosition = new Vector3(bouysBearingIntersection.x, 0, bouysBearingIntersection.z);

         _torpedoDetectionModel.AddTrackPoint(trackedPosition);

         //draw tracked points
         GameObject trackedPointMesh = new GameObject("Track Point " + string.Format("{0:0.00}", Scenario.Instance.ScenarioTime));
         trackedPointMesh.AddComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(5, 30);
         trackedPointMesh.AddComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
         trackedPointMesh.transform.position = trackedPosition + Vector3.up * 5;
         Destroy(trackedPointMesh, 12.0f);

         //draw tracked points after kalman filter
         GameObject kalmanTrackedPointMesh = new GameObject("Kalman Track Point " + string.Format("{0:0.00}", Scenario.Instance.ScenarioTime));
         kalmanTrackedPointMesh.AddComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(5, 30);
         kalmanTrackedPointMesh.AddComponent<MeshRenderer>().material.color = new Color(0, 1, 0, 0.5f);
         kalmanTrackedPointMesh.transform.position = _torpedoDetectionModel.CalcPos() + Vector3.up * 5; 
         Destroy(kalmanTrackedPointMesh, 12.0f);

         //draw accumalated way
         _accumalatedTorpedoWay.GetComponent<MeshFilter>().mesh = Utils.CreateOfssetedLinedMesh(calculateAccumalatedTorpedoWayPoints(), 50);

         yield return new WaitForSeconds(1f);
      }
   }

   //private Vector3 calcBearingDeviatedPoint(Vector3 posFrom, Vector3 posTo, float bearingRange, float deviation)
   //{

   //   Vector3 bearing = (posTo - posFrom).normalized;
   //   Vector3 BearingWithError = getDir(bearing, deviation);
   //   return posFrom + BearingWithError * bearingRange + Vector3.up;
   //}

   private void OnDrawGizmos()
   {
      Gizmos.color = new Color(1, 0, 0, 0.25f);

      if (m_bouy1 != null)
      {
         Vector3 bearing = (m_torpedo.position - m_bouy1.transform.position).normalized;
         Vector3 bCenter = getDir(bearing, b1Error);
         Vector3 bLeft = getDir(bearing, b1Error - getBearingError()/2f);
         Vector3 bRight = getDir(bearing, b1Error + getBearingError() / 2f);
         Vector3 p = new Vector3(m_bouy1.transform.position.x, 2, m_bouy1.transform.position.z);
         Vector3 pCenter = p + bCenter * m_detectRange;
         Vector3 pLeft = p + bLeft * m_detectRange;
         Vector3 pRight = p + bRight * m_detectRange;
         Gizmos.DrawLine(p, pCenter);
         Gizmos.DrawLine(p, pLeft);
         Gizmos.DrawLine(p, pRight);
      }

      if (m_bouy2 != null)
      {
         Vector3 bearing = (m_torpedo.position - m_bouy2.transform.position).normalized;
         Vector3 bCenter = getDir(bearing, b2Error);
         Vector3 bLeft = getDir(bearing, b2Error - getBearingError() / 2f);
         Vector3 bRight = getDir(bearing, b2Error + getBearingError() / 2f);
         Vector3 p = new Vector3(m_bouy2.transform.position.x, 2, m_bouy2.transform.position.z);
         Vector3 pCenter = p + bCenter * m_detectRange;
         Vector3 pLeft = p + bLeft * m_detectRange;
         Vector3 pRight = p + bRight * m_detectRange;
         Gizmos.DrawLine(p, pCenter);
         Gizmos.DrawLine(p, pLeft);
         Gizmos.DrawLine(p, pRight);
      }
   }

   public void AddBuoy(Buoy b)
   {
      if (m_bouy1 == null)
         m_bouy1 = b;
      else if (m_bouy2 == null)
      {
         m_bouy2 = b;
         startWork();
      }
      else
         Debug.LogError("Exceed number of buoys");
   }


   private void startWork()
   {
      m_errorBeg[0] = Utils.GaussRandom(getBearingError() / 2f);
      m_errorBeg[1] = Utils.GaussRandom(getBearingError() / 2f);
      m_errorEnd[0] = Utils.GaussRandom(getBearingError() / 2f);
      m_errorEnd[1] = Utils.GaussRandom(getBearingError() / 2f);
      
      _startDrawTrackedPosition = true;
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

      if (Mathf.Abs(cross.y) < 0.0001f)
      {
         cross = Vector3.zero;
         return false;
      }

      cross.x = cross.x / cross.y;
      cross.z = cross.z / cross.y;
      cross.y = 0;

      float d = (m_torpedo.position - cross).magnitude;

      if (d > VarSync.GetFloat(VarName.BuoysDetectRange))
         return false;

      return true;
   }

   private Vector3 getDir(Vector3 v, float a)
   {
      return Quaternion.AngleAxis(a, Vector3.up) * v;
   }

   private GameObject createAccumalatedTorpedoWay()
   {
      var g = new GameObject("AccumalatedTorpedoTail");
      g.AddComponent<MeshFilter>().mesh = Utils.CreateOfssetedLinedMesh(new List<Vector3>(), 50);
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material.color = new Color(0, 1, 0, 0.5f);

      return g;
   }

   private List<Vector3> calculateAccumalatedTorpedoWayPoints()
   {
      Vector3 firstPos = _torpedoDetectionModel.CalcPrognosisPos(5f) + Vector3.up * 5f;

      List<Vector3> points = new List<Vector3>();
      for (int i = 0; i < _accumalatedTorpedoWayLength; i += 20)
         points.Add(firstPos + _torpedoDetectionModel.CalcCourse() * i);

      return points;
   }
}
