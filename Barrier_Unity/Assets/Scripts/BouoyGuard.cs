using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class BouoyGuard : MonoBehaviour
{
   public Transform m_torpedo;
   public Transform m_bouy1;
   public Transform m_bouy2;
   public float m_range = 5000f;
   public float m_bearingRange;
   public float m_bearingError;

   public Material m_material;
   
   void Start()
   {
      m_mesh = new GameObject("zone");

      m_meshFilter = m_mesh.AddComponent<MeshFilter>();
      m_meshFilter.mesh = Utils.CreateRombusMesh(Vector3.zero, Vector3.zero, Vector3.zero, Vector3.zero);
      m_meshRenderer = m_mesh.AddComponent<MeshRenderer>();
      m_meshRenderer.material = m_material;
      //m_mesh.transform.localPosition = new Vector3(0, 0.25f, 0);


      m_errorBeg = Random.Range(-m_bearingError/2f, m_bearingError/2);
      m_errorCur = m_errorBeg;
      m_errorEnd = Random.Range(-m_bearingError / 2f, m_bearingError / 2);
      m_errorTime = Time.time;

      StartCoroutine(timerCoroutine());
   }

   IEnumerator timerCoroutine()
   {
      while(true)
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
      m_errorCur = Mathf.Lerp(m_errorBeg, m_errorEnd, Time.time - m_errorTime);

      Vector3 vc = Quaternion.AngleAxis(m_errorCur, Vector3.up) * (m_torpedo.position - m_bouy1.position).normalized;
      Vector3 vr = getDir(vc, m_bearingRange / 2);
      Vector3 vl = getDir(vc, -m_bearingRange / 2);

      Vector3 p1 = m_bouy1.position;
      Vector3 p1r = m_bouy1.position + vr * m_range + Vector3.up;
      Vector3 p1l = m_bouy1.position + vl * m_range + Vector3.up;

      vc = Quaternion.AngleAxis(m_errorCur, Vector3.up) * (m_torpedo.position - m_bouy2.position).normalized;
      vr = getDir(vc, m_bearingRange / 2);
      vl = getDir(vc, -m_bearingRange / 2);

      Vector3 p2 = m_bouy2.position;
      Vector3 p2r = m_bouy2.position + vr * m_range + Vector3.up;
      Vector3 p2l = m_bouy2.position + vl * m_range + Vector3.up;

      Vector3 c1 = getCross(p1, p1l, p2, p2l);
      Vector3 c2 = getCross(p1, p1r, p2, p2l);
      Vector3 c3 = getCross(p1, p1r, p2, p2r);
      Vector3 c4 = getCross(p1, p1l, p2, p2r);
      
      Vector3[] vertices = { c1-c1, c2-c1, c3-c1, c4-c1};
      
      m_meshFilter.mesh.vertices = vertices;

      m_mesh.transform.position = new Vector3(c1.x, 1, c1.z);

      m_meshRenderer.material.color =new Color(1, 0, 0, Mathf.PingPong(Time.time, 0.5f));
   }

   private void OnDrawGizmos()
   {
 
   //   Gizmos.color = Color.magenta;

   //   Vector3 vc = Quaternion.AngleAxis(m_errorCur, Vector3.up) * (m_torpedo.position - m_bouy1.position).normalized;
   //   Vector3 vr = getDir(vc, m_bearingRange/2);
   //   Vector3 vl = getDir(vc, -m_bearingRange/2);

   //   Vector3 p1 = m_bouy1.position;
   //   Vector3 p1r = m_bouy1.position + vr * m_range + Vector3.up;
   //   Vector3 p1l = m_bouy1.position + vl * m_range + Vector3.up;
      
   //   Gizmos.DrawLine(p1, p1r);
   //   Gizmos.DrawLine(p1, p1l);


   //   vc = Quaternion.AngleAxis(m_errorCur, Vector3.up) * (m_torpedo.position - m_bouy2.position).normalized;
   //   vr = getDir(vc, m_bearingRange / 2);
   //   vl = getDir(vc, -m_bearingRange / 2);

   //   Vector3 p2 = m_bouy2.position;
   //   Vector3 p2r = m_bouy2.position + vr * m_range + Vector3.up;
   //   Vector3 p2l = m_bouy2.position + vl * m_range + Vector3.up;
      
   //   Gizmos.DrawLine(p2, p2r);
   //   Gizmos.DrawLine(p2, p2l);

   //   Gizmos.color = Color.red;

   //   Vector3 c1 = getCross(p1, p1l, p2, p2l);
   //   Vector3 c2 = getCross(p1, p1r, p2, p2l);
   //   Vector3 c3 = getCross(p1, p1r, p2, p2r);
   //   Vector3 c4 = getCross(p1, p1l, p2, p2r);


   //   Gizmos.DrawCube(c1, new Vector3(50, 10, 50));
   //   Gizmos.DrawCube(c2, new Vector3(50, 10, 50));
   //   Gizmos.DrawCube(c3, new Vector3(50, 10, 50));
   //   Gizmos.DrawCube(c4, new Vector3(50, 10, 50));
   }

   private Vector3 getCross(Vector3 p11, Vector3 p12, Vector3 p21, Vector3 p22)
   {
      p11.y = 1;
      p12.y = 1;
      p21.y = 1;
      p22.y = 1;

      Vector3 f1 = Vector3.Cross(p11, p12);
      Vector3 f2 = Vector3.Cross(p21, p22);

      Vector3 cross = Vector3.Cross(f1, f2);

      cross.x = cross.x / cross.y;
      cross.z = cross.z / cross.y;
      cross.y = 0;

      return cross;
   }

   private Vector3 getDir(Vector3 v, float a)
   { 
      return Quaternion.AngleAxis(a, Vector3.up) * v;
   }

   private float m_errorBeg;
   private float m_errorEnd;
   private float m_errorCur;
   private float m_errorTime;

   GameObject m_mesh;
   MeshFilter m_meshFilter;
   MeshRenderer m_meshRenderer;
}
