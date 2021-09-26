using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Torpedo : MonoBehaviour
{
   [SerializeField] float m_speed = 15;
   [SerializeField] float m_circleSize = 500;
   [SerializeField] Material m_circleMaterial;

   GameObject m_circleObject;

   // Start is called before the first frame update
   void Start()
   {
      m_circleObject = createCircle();
   }

   // Update is called once per frame
   void Update()
   {
      float step = m_speed * Time.deltaTime;
      Vector3 pos = transform.position;
      pos.z += step;
      transform.position = pos;

      m_circleObject.GetComponent<MeshRenderer>().material.color = 
         new Color(1, 0, 0, Mathf.PingPong(Time.time, 0.8f));
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
}
