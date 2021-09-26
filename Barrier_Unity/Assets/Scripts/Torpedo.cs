using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Torpedo : MonoBehaviour
{
   [SerializeField] float m_speed = 15;
   [SerializeField] float m_circleSize = 500;
   [SerializeField] float m_arrowLength = 6000;
   [SerializeField] float m_arrowWidth = 100;
   [SerializeField] float m_advanceDistance = 100;
   [SerializeField] Material m_circleMaterial;
   [SerializeField] GameObject m_arrow;
   [SerializeField] GameObject m_targetShip;

   GameObject m_circleObject;
   GameObject m_arrowedLineObject;

   // Start is called before the first frame update
   void Start()
   {
      m_circleObject = createCircle();
      m_arrowedLineObject = createForwardArrowedLine();
   }

   // Update is called once per frame
   void Update()
   {


      float step = m_speed * Time.deltaTime;
      Vector3 pos = transform.position;

      Vector3 targetAdvancePos = m_targetShip.transform.position + m_targetShip.transform.forward * m_advanceDistance;
      var relativePos = targetAdvancePos - transform.position;
      var targetRotation = Quaternion.LookRotation(relativePos);
      transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 0.5f);
//      transform.Rotate(Time.deltaTime, Time.deltaTime, Time.deltaTime);
      //transform.LookAt(targetAdvancePos);
      //gameObject.transform.LookAt(m_targetShip.transform);
      pos += transform.forward*step;
      transform.position = pos;

      m_circleObject.GetComponent<MeshRenderer>().material.color = 
         new Color(1, 0, 0, Mathf.PingPong(Time.time, 0.5f));
   }

   GameObject createCircle()
   {
      var g = new GameObject("Circle");
      Mesh mesh = new Mesh();
      g.AddComponent<MeshFilter>().mesh = mesh;
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material = m_circleMaterial;
      meshReneder.material.color = new Color(1, 0, 0, 0.4f);

      var vertexList = new List<Vector3>();
      vertexList.Add(new Vector3(0, 0, 0));
      float deltaAngle = 5;
      for (float a = 0; a <= 360; a += deltaAngle)
      {
         Quaternion rotation = Quaternion.Euler(0, a, 0);
         Vector3 v = rotation * Vector3.forward * m_circleSize;
         vertexList.Add(v);
      }

      var idxList = new List<int>();
      for (int itr = 1; itr < vertexList.Count - 1; itr ++)
      {
         idxList.Add(0);
         idxList.Add(itr);
         idxList.Add(itr + 1);
      }

      mesh.vertices = vertexList.ToArray();
      mesh.normals = Enumerable.Repeat(Vector3.up, mesh.vertices.Length).ToArray();
      mesh.triangles = idxList.ToArray();
      g.transform.SetParent(transform, false);
      g.transform.localPosition = new Vector3(0, 0.5f, 0);
      return g;
   }

   GameObject createForwardArrowedLine()
   {
      var g = new GameObject("ArrowLine");
      Mesh mesh = new Mesh();
      g.AddComponent<MeshFilter>().mesh = mesh;
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material = m_arrow.GetComponent<MeshRenderer>().material;

//      meshReneder.material = m_circleMaterial;
//      meshReneder.material.color = new Color(1, 0, 0, 0.4f);

      var vertexList = new List<Vector3>();
      var uvList = new List<Vector2>();
      float step = m_arrowWidth*2;
      float halfWidth = m_arrowWidth/2;

      float uv = 0;
      for (float x = 0; x <= m_arrowLength; x += step)
      {
         vertexList.Add(new Vector3(-halfWidth, 0, x));
         vertexList.Add(new Vector3(halfWidth, 0, x));

         uvList.Add(new Vector2(-uv, 0f));
         uvList.Add(new Vector2(-uv, 1f));
         uv += 1;
      }

      var idxList = new List<int>();
      for (int itr = 0; itr < vertexList.Count - 4; itr+=2)
      {
         idxList.Add(itr);
         idxList.Add(itr + 2);
         idxList.Add(itr + 1);

         idxList.Add(itr + 1);
         idxList.Add(itr + 2);
         idxList.Add(itr + 3);
      }

      mesh.vertices = vertexList.ToArray();
      mesh.normals = Enumerable.Repeat(Vector3.up, mesh.vertices.Length).ToArray();
      mesh.triangles = idxList.ToArray();
      mesh.uv = uvList.ToArray();
      g.transform.SetParent(transform, false);
      g.transform.localPosition = new Vector3(0, 0.5f, 0);
      return g;
   }

}
