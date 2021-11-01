using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BuoyGuard : MonoBehaviour
{
   public Transform m_torpedo;
   public Material m_material;

   private Buoy m_bouy1;
   private Buoy m_bouy2;
   private float m_errorBeg;
   private float m_errorEnd;
   private float m_errorCur;
   private float m_errorTime;
   private GameObject m_rombZone;
   private GameObject m_elipseZone;
   private GameObject m_splineZone;
   private bool _startDrawTrackedPosition = false;

   private float m_bearingError => VarSync.GetFloat(VarName.BuoysBearingError);
   private float m_detectRange => VarSync.GetFloat(VarName.BuoysDetectRange);


   void Start()
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

      //m_rombZone.transform.localPosition = new Vector3(0, 0.25f, 0);


      m_errorTime = Time.time;

      StartCoroutine(timerCoroutine());
   }

   IEnumerator timerCoroutine()
   {
      while (true)
      {
         yield return new WaitForSeconds(1);

         m_errorBeg = m_errorEnd;
         m_errorEnd = Random.Range(-m_bearingError / 2f, m_bearingError / 2f);
         m_errorTime = Time.time;
      }
   }

   // Update is called once per frame
   void Update()
   {
      if (m_bouy1 == null || m_bouy2 == null)
         return;
      
      if (_startDrawTrackedPosition)
      {
         StartCoroutine(drawTrackedTargetPosition());
         _startDrawTrackedPosition = false;
      }

      m_errorCur = Mathf.Lerp(m_errorBeg, m_errorEnd, Time.time - m_errorTime);

      Vector3 vc = Quaternion.AngleAxis(m_errorCur, Vector3.up) * (m_torpedo.position - m_bouy1.transform.position).normalized;
      Vector3 vr = getDir(vc, m_bearingError / 2);
      Vector3 vl = getDir(vc, -m_bearingError / 2);

      Vector3 p1 = m_bouy1.transform.position;
      Vector3 p1r = m_bouy1.transform.position + vr * m_detectRange + Vector3.up;
      Vector3 p1l = m_bouy1.transform.position + vl * m_detectRange + Vector3.up;

      vc = Quaternion.AngleAxis(m_errorCur, Vector3.up) * (m_torpedo.position - m_bouy2.transform.position).normalized;
      vr = getDir(vc, m_bearingError / 2);
      vl = getDir(vc, -m_bearingError / 2);

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

      if (!(f1 && f2 && f3 && f4))
      {
         m_rombZone.GetComponent<MeshFilter>().mesh = Utils.CreateRombusMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
         m_elipseZone.GetComponent<MeshFilter>().mesh = Utils.CreateEllipseMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
         m_splineZone.GetComponent<MeshFilter>().mesh = Utils.CreateSplineMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
         return;
      }
      
      Vector3[] vertices = { c1 - c1, c2 - c1, c3 - c1, c4 - c1 };

      m_rombZone.GetComponent<MeshFilter>().mesh = Utils.CreateRombusMesh(vertices[0], vertices[1], vertices[2], vertices[3]);
      m_rombZone.transform.position = new Vector3(c1.x, 10, c1.z);
      m_rombZone.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, Mathf.PingPong(Time.time, 0.5f));

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
      m_elipseZone.GetComponent<MeshRenderer>().material.color = new Color(0, 1, 0, Mathf.PingPong(Time.time, 0.5f));

      m_splineZone.GetComponent<MeshFilter>().mesh = Utils.CreateSplineMesh(vertices[0], vertices[1], vertices[2], vertices[3]);
      m_splineZone.transform.position = new Vector3(c1.x, 10, c1.z);
      m_splineZone.GetComponent<MeshRenderer>().material.color = new Color(0, 0, 1, Mathf.PingPong(Time.time, 0.5f));
   }

   private IEnumerator drawTrackedTargetPosition()
   {
      while (true)
      {
         Vector3 b1Bearing = (m_torpedo.position - m_bouy1.transform.position).normalized;
         Vector3 b1BearingWithError = getDir(b1Bearing, Random.Range(-m_bearingError / 2, m_bearingError / 2));

         //Vector3 p1 = m_bouy1.transform.position;
         //Vector3 p1r = m_bouy1.transform.position + b1BearingWithError * m_detectRange + Vector3.up;

         //Vector3 b2Bearing = (m_torpedo.transform.position - m_bouy2.transform.position).normalized;
         //Vector3 b2BearingWithError = getDir(b2Bearing, Random.Range(-m_bearingError / 2, m_bearingError / 2));

         //Vector3 p2 = m_bouy2.transform.position;
         //Vector3 p2r = m_bouy2.transform.position + b2BearingWithError * m_detectRange + Vector3.up;

         Vector3 p1 = m_bouy1.transform.position;
         Vector3 p1r = calcBearingDeviatedPoint(m_bouy1.transform.position, m_torpedo.transform.position, m_detectRange, Random.Range(-m_bearingError / 2, m_bearingError / 2));
         Vector3 p2 = m_bouy2.transform.position;
         Vector3 p2r = calcBearingDeviatedPoint(m_bouy2.transform.position, m_torpedo.transform.position, m_detectRange, Random.Range(-m_bearingError / 2, m_bearingError / 2));

         Vector3 bouysBearingIntersection = Vector3.zero;
         bool f1 = getCross(p1, p1r, p2, p2r, out bouysBearingIntersection);

         GameObject TrackedPoint = new GameObject("Track Point " + string.Format("{0:0.00}", Scenario.Instance.ScenarioTime));
         TrackedPoint.AddComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(10, 30);
         TrackedPoint.AddComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
         TrackedPoint.transform.position = new Vector3(bouysBearingIntersection.x, 10, bouysBearingIntersection.z);
  
         yield return new WaitForSeconds(1f);
      }
   }

   private Vector3 calcBearingDeviatedPoint(Vector3 posFrom, Vector3 posTo, float bearingRange, float deviation)
   {

      Vector3 bearing = (posTo - posFrom).normalized;
      Vector3 BearingWithError = getDir(bearing, deviation);
      return posFrom + BearingWithError * bearingRange + Vector3.up;
   }

   private void OnDrawGizmos()
   {
      Gizmos.color = Color.red;
      Gizmos.DrawSphere(m_torpedo.position, 100);

      Gizmos.color = Color.green;
      if(m_bouy1 != null)
         Gizmos.DrawSphere(m_bouy1.GetComponent<Transform>().position, 25);
      if (m_bouy2 != null)
         Gizmos.DrawSphere(m_bouy2.GetComponent<Transform>().position, 25);
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
      m_errorBeg = Random.Range(-m_bearingError / 2f, m_bearingError / 2);
      m_errorCur = m_errorBeg;
      m_errorEnd = Random.Range(-m_bearingError / 2f, m_bearingError / 2);
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
}
