using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class BuoyGuard : MonoBehaviour
{
   [SerializeField] private Transform m_torpedo;
   [SerializeField] private Material m_material;
   [SerializeField] private TorpedoDetectionModel _torpedoDetectionModel;
   [SerializeField] private float _accumalatedTorpedoWayLength = 10000;

   private List<Buoy> m_bouys = new List<Buoy>();
   public Buoy[] Bouys => m_bouys.ToArray();


   private GameObject m_rombZone;
   private GameObject m_elipseZone;
   private GameObject m_splineZone;

   private GameObject _accumalatedTorpedoWay;

   private float _startScanTime = -1;
   private float _scanningError = 1f;

   private bool _drawDebugLines = true;

   public Vector3[] RealZone { get; private set; }
   public Transform DetectZone => m_splineZone.transform;
   public float ScanningError => _scanningError;
   private float m_detectRange => VarSync.GetFloat(VarName.BuoysDetectRange);


   void Start()
   {
      createZoneObject();
      _accumalatedTorpedoWay = createAccumalatedTorpedoWay();

      StartCoroutine(trackTargetPosition());
   }

   void Update()
   {
      if (m_bouys.Count > 1)
         activateZone();
      else
         return;

      var target = Scenario.Instance.TargetInfo;
      if (target == null || !Scenario.Instance.TargetInfo.Target.IsActive)
      {
         deactivateZone();
         _drawDebugLines = false;
         return;
      }

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
      m_rombZone = new GameObject("romb_zone");
      m_rombZone.AddComponent<MeshFilter>().mesh = Utils.CreateRombusMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
      m_rombZone.AddComponent<MeshRenderer>().material = m_material;

      m_elipseZone = new GameObject("elipse_zone");
      m_elipseZone.AddComponent<MeshFilter>().mesh = Utils.CreateEllipseMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
      m_elipseZone.AddComponent<MeshRenderer>().material = m_material;

      m_splineZone = new GameObject("spline_zone");
      m_splineZone.AddComponent<MeshFilter>().mesh = Utils.CreateSplineMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
      m_splineZone.AddComponent<MeshRenderer>().material = m_material;
   }

   private void activateZone()
   {
      m_rombZone.SetActive(true);
      m_elipseZone.SetActive(true);
      m_splineZone.SetActive(true);
   }

   private void deactivateZone()
   {
      m_rombZone.SetActive(false);
      m_elipseZone.SetActive(false);
      m_splineZone.SetActive(false);
   }

   private Vector3[] updateZonePoints()
   {
      float b1Error = m_bouys[0].Error;
      float b2Error = m_bouys[1].Error;

      Vector3 vb = (m_torpedo.position - m_bouys[0].transform.position).normalized;
      vb = new Vector3(vb.x, 0, vb.z).normalized;
      Vector3 vr = getDir(vb, b1Error + Buoy.GetBearingError() / 2f);
      Vector3 vl = getDir(vb, b1Error - Buoy.GetBearingError() / 2f);

      Vector3 p1 = new Vector3(m_bouys[0].transform.position.x, 0, m_bouys[0].transform.position.z);
      Vector3 p1r = p1 + vr * m_detectRange;
      Vector3 p1l = p1 + vl * m_detectRange;

      vb = (m_torpedo.position - m_bouys[1].transform.position).normalized;
      vb = new Vector3(vb.x, 0, vb.z).normalized;
      vr = getDir(vb, b2Error + Buoy.GetBearingError() / 2f);
      vl = getDir(vb, b2Error - Buoy.GetBearingError() / 2f);

      Vector3 p2 = new Vector3(m_bouys[1].transform.position.x, 0, m_bouys[1].transform.position.z);
      Vector3 p2r = p2 + vr * m_detectRange;
      Vector3 p2l = p2 + vl * m_detectRange;

      Vector3 c1 = Vector3.zero;
      bool f1 = getCross(p1, p1l, p2, p2l, out c1);
      Vector3 c2 = Vector3.zero;
      bool f2 = getCross(p1, p1r, p2, p2l, out c2);
      Vector3 c3 = Vector3.zero;
      bool f3 = getCross(p1, p1r, p2, p2r, out c3);
      Vector3 c4 = Vector3.zero;
      bool f4 = getCross(p1, p1l, p2, p2r, out c4);

      if (!(f1 && f2 && f3 && f4))
         return new Vector3[0];
      else
         return new Vector3[] { c1, c2, c3, c4 };
   }

   private void updateZoneMesh(Vector3 c1, Vector3 c2, Vector3 c3, Vector3 c4, Vector3[] vertices)
   {
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

   private IEnumerator trackTargetPosition()
   {
      while (true)
      {
         if (m_bouys.Count < 2)
         {
            yield return null;
            continue;
         }

         if (Scenario.Instance.TargetInfo != null && Scenario.Instance.TargetInfo.Target.IsActive)
         {
            int idx = 0;
            
            for(int i = 0; i < m_bouys.Count-1; i++)
            {
               for(int j = i+1; j < m_bouys.Count; j++)
               {
                  //calculate real tracked point
                  Vector3 b1Bearing = (m_torpedo.position - m_bouys[i].transform.position).normalized;
                  Vector3 b2Bearing = (m_torpedo.position - m_bouys[j].transform.position).normalized;

                  Vector3 b1BearingWithError = getDir(b1Bearing, m_bouys[i].Error);
                  Vector3 b2BearingWithError = getDir(b2Bearing, m_bouys[j].Error);

                  Vector3 p11 = m_bouys[i].transform.position;
                  Vector3 p12 = p11 + b1BearingWithError * m_detectRange;
                  Vector3 p21 = m_bouys[j].transform.position;
                  Vector3 p22 = p21 + b2BearingWithError * m_detectRange;

                  Vector3 bouysBearingIntersection = Vector3.zero;
                  bool crossed = getCross(p11, p12, p21, p22, out bouysBearingIntersection);
                  //if (!crossed)
                  //{
                  //   yield return null;
                  //   continue;
                  //}

                  Vector3 trackedPosition = new Vector3(bouysBearingIntersection.x, 0, bouysBearingIntersection.z);

                  _torpedoDetectionModel.AddTrackPoint(idx, trackedPosition);

                  idx++;
               }
            }

            //draw accumalated way
            _accumalatedTorpedoWay.GetComponent<MeshFilter>().mesh = Utils.CreateOfssetedLinedMesh(calculateAccumalatedTorpedoWayPoints(), 50);

            yield return new WaitForSeconds(1f);
         }

         yield return null;
      }
   }

   private void OnDrawGizmos()
   {
      Vector3 t1 = m_torpedo.rotation * Vector3.forward * 100 + Vector3.up * 5;
      Vector3 t2 = m_torpedo.rotation * Quaternion.Euler(new Vector3(0, 180, 0)) * Vector3.forward * 500 + Vector3.up * 5;
      Vector3 t3 = m_torpedo.rotation * Quaternion.Euler(new Vector3(0, 90, 0)) * Vector3.forward * 250 + Vector3.up * 5;
      Vector3 t4 = m_torpedo.rotation * Quaternion.Euler(new Vector3(0, -90, 0)) * Vector3.forward * 250 + Vector3.up * 5;

      Gizmos.color = new Color(1, 0, 1, 0.5f);
      Gizmos.DrawLine(m_torpedo.position, m_torpedo.position + t1);
      Gizmos.DrawLine(m_torpedo.position, m_torpedo.position + t2);
      Gizmos.DrawLine(m_torpedo.position, m_torpedo.position + t3);
      Gizmos.DrawLine(m_torpedo.position, m_torpedo.position + t4);

      if (!_drawDebugLines)
         return;

      Gizmos.color = new Color(1, 0, 0, 0.25f);

      foreach(var b in m_bouys)
      {
         Vector3 bearing = (m_torpedo.position - b.transform.position).normalized;
         bearing = new Vector3(bearing.x, 0, bearing.z).normalized;
         Vector3 bCenter = getDir(bearing, b.Error);
         Vector3 bLeft = getDir(bearing, b.Error - Buoy.GetBearingError() /2f);
         Vector3 bRight = getDir(bearing, b.Error + Buoy.GetBearingError() / 2f);
         Vector3 p = new Vector3(b.transform.position.x, 2, b.transform.position.z);
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
      if(m_bouys.Count < 6)
         m_bouys.Add(b);
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
