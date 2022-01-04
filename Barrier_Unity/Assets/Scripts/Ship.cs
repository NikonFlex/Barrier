using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ship : MonoBehaviour
{
   public float m_Speed = 5;

   private GameObject m_directionLine;
   private bool _isAlive = true;

   [SerializeField] private GameObject m_arrow;
   [SerializeField] private MSC _msc;

   public bool IsAlive => _isAlive;

   public void SetUpMscSettings(bool isActive, float distToShip)
   {
      _msc.SetUpSettings(isActive, distToShip);
   }

   public IEnumerator Explode()
   {
      _isAlive = false;

      GameObject explosion = GameObject.CreatePrimitive(PrimitiveType.Sphere);
      explosion.name = "explosion";
      explosion.GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
      explosion.transform.position = gameObject.transform.position;
      while (explosion.transform.localScale.magnitude < new Vector3(200, 200, 200).magnitude)
      {
         explosion.transform.localScale += new Vector3(2, 2, 2);
         yield return null;
      }
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
