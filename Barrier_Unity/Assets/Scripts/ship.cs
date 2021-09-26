using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
   public float m_Speed = 5;
   GameObject m_directionLine;
   [SerializeField] GameObject m_arrow;
   // Start is called before the first frame update
   void Start()
   {
      m_directionLine = createDirectionLine();
   }

   // Update is called once per frame
   void Update()
   {
      float step = m_Speed * Time.deltaTime;
      Vector3 pos = transform.position;
      pos.z += step;
      transform.position = pos;
   }

   GameObject createDirectionLine()
   {
      var g = new GameObject("ArrowLine");
      g.AddComponent<MeshFilter>().mesh = Utils.CreateLinedMesh(1000, 50, 100);
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material = m_arrow.GetComponent<MeshRenderer>().material;

      g.transform.SetParent(transform, false);
      g.transform.localPosition = new Vector3(0, 0.5f, 0);
      return g;

   }

}
