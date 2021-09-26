using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

struct sector
{
   public Color color;
   public float width; // in degrees
   public float range_min; // in meters
   public float range_max; // in meters
};

public class SectorDrawer : MonoBehaviour
{
   [SerializeField] GameObject m_origin;
   [SerializeField] int m_sectorSubdivNum = 100;
   [SerializeField] Material m_sectorMaterial;
   sector[] m_sectors;
   GameObject[] m_sectorMeshes;
   // Start is called before the first frame update
   void Awake()
   {
      sector s1;
      s1.range_min = 100;
      s1.range_max = 200;
      s1.width = 80;
      s1.color = new Color(0.9f, 0.1f, 0.1f, 0.5f);

      sector s2;
      s2.range_min = 1000;
      s2.range_max = 2000;
      s2.width = 80;
      s2.color = new Color(0.5f, 0.1f, 0.8f, 0.5f);


      m_sectors = new sector[] { s1, s2 };

      var meshesList = new List<GameObject>();
      foreach (var sec in m_sectors)
         meshesList.Add(createSectorMesh(sec));

      m_sectorMeshes = meshesList.ToArray();

   }
   void Start()
   {
   }

   // Update is called once per frame
   void Update()
   {

   }

   public void ShowSectors(bool draw)
   {
      foreach (var m in m_sectorMeshes)
         m.SetActive(draw);
   }

   GameObject createSectorMesh(sector s)
   {
      var g = new GameObject("Sector");
      Mesh mesh = new Mesh();
      g.AddComponent<MeshFilter>().mesh = mesh;
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material = m_sectorMaterial;
      meshReneder.material.color = s.color;

      var vertexList = new List<Vector3>();
      float deltaAngle = s.width / 2 / m_sectorSubdivNum;
      for (float a = -s.width / 2; a <= s.width / 2; a += deltaAngle)
      {
         Quaternion rotation = Quaternion.Euler(0, a, 0);
         Vector3 v0 = rotation * Vector3.back * s.range_min + Vector3.up / 2;
         Vector3 v1 = rotation * Vector3.back * s.range_max + Vector3.up / 2;
         //Vector3 v0 = Vector3.back * s.range_min + Vector3.up / 2;
         //Vector3 v1 = Vector3.back * s.range_max + Vector3.up / 2;

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

      Color[] colors = new Color[vertexList.Count];
      int i = 0;
      while (i < vertexList.Count)
      {
         colors[i] = s.color;
         i++;
      }
      mesh.vertices = vertexList.ToArray();
      mesh.normals = Enumerable.Repeat(Vector3.up, mesh.vertices.Length).ToArray();
      mesh.triangles = idxList.ToArray();
      //mesh.colors = colors;
      g.transform.SetParent(m_origin.transform, false);
      return g;
   }

   private void drawSector(sector s)
   {
   }
}
