using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

static class Utils
{
   static public Mesh CreateLinedMesh(float length, float width, float step)
   {
      Mesh mesh = new Mesh();

      var vertexList = new List<Vector3>();
      var uvList = new List<Vector2>();
      float halfWidth = width / 2;

      float uv = 0;
      for (float x = 0; x <= length; x += step)
      {
         vertexList.Add(new Vector3(-halfWidth, 0, x));
         vertexList.Add(new Vector3(halfWidth, 0, x));

         uvList.Add(new Vector2(-uv, 0f));
         uvList.Add(new Vector2(-uv, 1f));
         uv += 1;
      }

      var idxList = new List<int>();
      for (int itr = 0; itr < vertexList.Count - 4; itr += 2)
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

      return mesh;
   }

   static public Mesh CreateCircleMesh(float radius, int numSegments)
   {
      Mesh mesh = new Mesh();
      var vertexList = new List<Vector3>();
      vertexList.Add(new Vector3(0, 0, 0));
      float deltaAngle = 360/numSegments;
      for (float a = 0; a <= 360; a += deltaAngle)
      {
         Quaternion rotation = Quaternion.Euler(0, a, 0);
         Vector3 v = rotation * Vector3.forward * radius;
         vertexList.Add(v);
      }

      var idxList = new List<int>();
      for (int itr = 1; itr < vertexList.Count - 1; itr++)
      {
         idxList.Add(0);
         idxList.Add(itr);
         idxList.Add(itr + 1);
      }

      mesh.vertices = vertexList.ToArray();
      mesh.normals = Enumerable.Repeat(Vector3.up, mesh.vertices.Length).ToArray();
      mesh.triangles = idxList.ToArray();

      return mesh;
   }

   static public Mesh CreateSectorMesh(float width, float minRange, float maxRange, int numSegments)
   {
      Mesh mesh = new Mesh();

      var vertexList = new List<Vector3>();
      float deltaAngle = width / 2 / numSegments;
      for (float a = -width / 2; a <= width / 2; a += deltaAngle)
      {
         Quaternion rotation = Quaternion.Euler(0, a, 0);
         Vector3 v0 = rotation * Vector3.back * minRange + Vector3.up / 2;
         Vector3 v1 = rotation * Vector3.back * maxRange + Vector3.up / 2;

         vertexList.Add(v0);
         vertexList.Add(v1);


         //          GameObject sphere1 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
         //          sphere1.transform.SetParent(m_origin.transform, false);
         //          sphere1.transform.localScale = new Vector3(20, 20, 20);
         //          sphere1.transform.localPosition = v0;
         // 
         //          GameObject sphere2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
         //          sphere2.transform.parent = m_origin.transform;
         //          sphere2.transform.localScale = new Vector3(10, 10, 10);
         //          sphere2.transform.localPosition = v1;
      }

      var idxList = new List<int>();
      for (int itr = 0; itr < vertexList.Count - 2; itr += 2)
      {
         idxList.Add(itr + 0);
         idxList.Add(itr + 1);
         idxList.Add(itr + 2);

         idxList.Add(itr + 2);
         idxList.Add(itr + 1);
         idxList.Add(itr + 3);
      }

      mesh.vertices = vertexList.ToArray();
      mesh.normals = Enumerable.Repeat(Vector3.up, mesh.vertices.Length).ToArray();
      mesh.triangles = idxList.ToArray();
      return mesh;
   }

   static public Mesh CreateRombusMesh(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
   {
      Mesh mesh = new Mesh();

      var vertexList = new List<Vector3>();
      vertexList.Add(p1);
      vertexList.Add(p2);
      vertexList.Add(p3);
      vertexList.Add(p4);

      var idxList = new List<int>();
      idxList.Add(0);
      idxList.Add(1);
      idxList.Add(2);
      idxList.Add(0);
      idxList.Add(2);
      idxList.Add(3);

      mesh.vertices = vertexList.ToArray();
      mesh.normals = Enumerable.Repeat(Vector3.up, mesh.vertices.Length).ToArray();
      mesh.triangles = idxList.ToArray();
      return mesh;
   }

}