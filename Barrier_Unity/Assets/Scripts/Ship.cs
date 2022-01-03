using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
   public float m_Speed = 5;
   GameObject m_directionLine;

   [SerializeField] GameObject m_arrow;
   [SerializeField] private MSC _msc; 

   public void SetUpMscSettings(bool isActive, float distToShip)
   {
      _msc.SetUpSettings(isActive, distToShip);
   }

   void Start()
   {
      m_directionLine = createDirectionLine();
   }

   void Update()
   {
      if (!Scenario.IsRunning)
         return;
      float step = m_Speed * Time.deltaTime;
      Vector3 pos = transform.position;
      pos += transform.forward * step;
      transform.position = pos;
      
      _msc.SetPosition(transform.position, transform.forward);
   }

   GameObject createDirectionLine()
   {
      var g = new GameObject("ArrowLine");
      g.layer = 6;
      g.AddComponent<MeshFilter>().mesh = Utils.CreateLinedMesh(1000, 50, 100);
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material = m_arrow.GetComponent<MeshRenderer>().material;

      g.transform.SetParent(transform, false);
      g.transform.localPosition = new Vector3(0, 0.5f, 0);
      return g;
   }
}
