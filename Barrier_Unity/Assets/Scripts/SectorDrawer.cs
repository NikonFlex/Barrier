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
    [SerializeField] public float a = 3;
    [SerializeField] public float b = 5;
    [SerializeField] public float h = 10;
    [SerializeField] public float k = 10;
    [SerializeField] public float theta = 45;
    [SerializeField] public int resolution = 1000;
    Vector3[] positions;
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
        g.AddComponent<MeshFilter>().mesh = Utils.CreateSectorMesh(s.width, s.range_min, s.range_max, m_sectorSubdivNum);
        var meshReneder = g.AddComponent<MeshRenderer>();
        meshReneder.material = m_sectorMaterial;
        meshReneder.material.color = s.color;

        g.transform.SetParent(m_origin.transform, false);
        return g;
    }

    Vector3[] CreateEllipse(float a, float b, float h, float k, float theta, int resolution)
    {

        positions = new Vector3[resolution + 1];
        Quaternion q = Quaternion.AngleAxis(theta, Vector3.forward);
        Vector3 center = new Vector3(h, k, 0.0f);

        for (int i = 0; i <= resolution; i++)
        {
            float angle = (float)i / (float)resolution * 2.0f * Mathf.PI;
            positions[i] = new Vector3(a * Mathf.Cos(angle), b * Mathf.Sin(angle), 0.0f);
            positions[i] = q * positions[i] + center;
        }

        return positions;
    }
}