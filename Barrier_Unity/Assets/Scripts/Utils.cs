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

   static public Mesh CreateOfssetedLinedMesh(List<Vector3> way_points, int width)
   {
      Mesh mesh = new Mesh();

      int half_width = width / 2;
      List<Vector3> vertex_list = new List<Vector3>();
      for (int i = 0; i < way_points.Count - 1; i++)
      {
         Vector3 p1 = way_points[i];
         Vector3 p2 = way_points[i + 1];
         Vector3 dir = p2 - p1;
         var prp = PerpTo(dir) * width / 2; // peprpendicular to dir
         vertex_list.Add(p1 - prp);
         vertex_list.Add(p1 + prp);
      }

      var idxList = new List<int>();
      for (int itr = 0; itr < vertex_list.Count - 2; itr += 2)
      {
         idxList.Add(itr);
         idxList.Add(itr + 2);
         idxList.Add(itr + 3);

         idxList.Add(itr);
         idxList.Add(itr + 3);
         idxList.Add(itr + 1);
      }

      mesh.vertices = vertex_list.ToArray();
      mesh.triangles = idxList.ToArray();
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
      return mesh;
   }

   static public Vector3 PerpTo(Vector3 dir)
   {
      return Quaternion.AngleAxis(90, Vector3.up) * dir.normalized;
   }

   static public Mesh CreateCircleMesh(float radius, int numSegments)
   {
      Mesh mesh = new Mesh();
      var vertexList = new List<Vector3>();
      vertexList.Add(new Vector3(0, 0, 0));
      float deltaAngle = 360 / numSegments;
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

   static public Mesh CreateEllipseMesh(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
   {
      List<Vector3> positions = new List<Vector3>();
      Quaternion q = Quaternion.AngleAxis(180, Vector3.forward);
      Vector3 center = new Vector3(0.0f, 0.0f, 0.0f);
      positions.Add(center);

      int resolution = 100;
      float a = (p1 - p3).magnitude / 2;
      float b = (p2 - p4).magnitude / 2;
      for (int i = 0; i <= resolution; i += 1)
      {
         float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
         positions.Add(new Vector3(a * Mathf.Cos(angle), 0.0f, b * Mathf.Sin(angle)));
         positions[i] = q * positions[i] + center;
      }

      Mesh mesh = new Mesh();
      mesh.vertices = positions.ToArray();
      var tris = new List<int>();
      for (int i = 1; i < positions.Count - 1; i += 1)
      {
         tris.Add(0);
         tris.Add(i);
         tris.Add(i + 1);
      }
      tris.Add(0);
      tris.Add(positions.Count - 2);
      tris.Add(1);
      mesh.triangles = tris.ToArray();
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
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
      idxList.Add(2);
      idxList.Add(1);
      idxList.Add(0);
      idxList.Add(3);
      idxList.Add(2);

      mesh.vertices = vertexList.ToArray();
      mesh.normals = Enumerable.Repeat(Vector3.up, mesh.vertices.Length).ToArray();
      mesh.triangles = idxList.ToArray();
      return mesh;
   }
   static public Mesh CreateSplineMesh(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
   {
      Mesh mesh = new Mesh();

      List<Vector3> spl = new List<Vector3>();
      int subdiv = 20;
      spl.AddRange(bsp(p1, p2, p3, p4, subdiv));
      spl.AddRange(bsp(p2, p3, p4, p1, subdiv));
      spl.AddRange(bsp(p3, p4, p1, p2, subdiv));
      spl.AddRange(bsp(p4, p1, p2, p3, subdiv));
      mesh.vertices = spl.ToArray();

      var tris = new List<int>();
      for (int i = 1; i < spl.Count - 1; i += 1)
      {
         tris.Add(0);
         tris.Add(i + 1);
         tris.Add(i);
      }
      tris.Add(0);
      tris.Add(spl.Count - 2);
      tris.Add(1);
      mesh.triangles = tris.ToArray();
      mesh.RecalculateNormals();
      mesh.RecalculateTangents();
      return mesh;
   }
   static public Vector3[] bsp(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4, int divisions)
   {
      Vector2 p1 = new Vector2(v1.x, v1.z);
      Vector2 p2 = new Vector2(v2.x, v2.z);
      Vector2 p3 = new Vector2(v3.x, v3.z);
      Vector2 p4 = new Vector2(v4.x, v4.z);
      var spline = new List<Vector3>();
      double[] a = new double[5];
      double[] b = new double[5];
      a[0] = (-p1.x + 3 * p2.x - 3 * p3.x + p4.x) / 6.0;
      a[1] = (3 * p1.x - 6 * p2.x + 3 * p3.x) / 6.0;
      a[2] = (-3 * p1.x + 3 * p3.x) / 6.0;
      a[3] = (p1.x + 4 * p2.x + p3.x) / 6.0;
      b[0] = (-p1.y + 3 * p2.y - 3 * p3.y + p4.y) / 6.0;
      b[1] = (3 * p1.y - 6 * p2.y + 3 * p3.y) / 6.0;
      b[2] = (-3 * p1.y + 3 * p3.y) / 6.0;
      b[3] = (p1.y + 4 * p2.y + p3.y) / 6.0;

      spline.Add(new Vector3((float)a[3], 0, (float)b[3]));

      int i;
      for (i = 1; i <= divisions - 1; i++)
      {
         float t = System.Convert.ToSingle(i) / System.Convert.ToSingle(divisions);
         spline.Add(new Vector3((float)((a[2] + t * (a[1] + t * a[0])) * t + a[3]), 0, (float)((b[2] + t * (b[1] + t * b[0])) * t + b[3])));
      }

      return spline.ToArray();
   }

   static public float CalculateHorAngleDelta(Transform from, Vector3 to)
   {
      Vector3 f = new Vector3(from.forward.x, 0, from.forward.z).normalized;
      Vector3 v = to - from.position;
      float a = Vector3.SignedAngle(f, new Vector3(v.x, 0, v.z).normalized, Vector3.up);
      return a;
   }

   static public float CalculateVertAngle(Vector3 from, Vector3 to, float launchSpeed)
   {
      float a1, a2;
      int numSolutions = fts.calc_vert_angle(from, launchSpeed, to, 9.8f, out a1, out a2);
      if (numSolutions > 0)
      {
         return a1;
      }
      return 0;
   }

   static public float GaussRandom(float halfRange)
   {
      float r = 0;
      for (int i = 0; i < 6; i++)
         r += Random.Range(0f, 1f);

      return halfRange/3f * Mathf.Sqrt(2f) * (r - 3f);
   }
}