using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Torpedo : MonoBehaviour
{
   [SerializeField] float m_speed = 30;
   [SerializeField] float m_circleSize = 500;
   [SerializeField] float m_arrowLength = 6000;
   [SerializeField] float m_arrowWidth = 100;
   [SerializeField] float m_advanceDistance = 100;
   [SerializeField] Material m_circleMaterial;
   [SerializeField] GameObject m_arrow;
   [SerializeField] GameObject m_targetShip;
   
   [SerializeField] int _distBeetweenWaysPoints = 30;

   GameObject m_circleObject;
   GameObject m_arrowedLineObject;
   GameObject m_torpedoWay;

   Vector3 _lastPos;
   List<Vector3> _wayPoints;

   // Start is called before the first frame update
   void Start()
   {
      //m_circleObject = createCircle();
      m_arrowedLineObject = createForwardArrowedLine();
      _lastPos = gameObject.transform.position;
      _wayPoints = new List<Vector3>();
      _wayPoints.Add(_lastPos);
      m_torpedoWay = createWay();
   }

   // Update is called once per frame
   void Update()
   {

      if (!Scenario.Instance.IsRunning)
         return;


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

      //m_circleObject.GetComponent<MeshRenderer>().material.color = 
      //   new Color(1, 0, 0, Mathf.PingPong(Time.time, 0.5f));
      //float scaleFactor = Mathf.PingPong(Time.time * 0.5f, 0.7f) + 0.3f;
      //m_circleObject.transform.localScale = new Vector3(scaleFactor, 1, scaleFactor);

      if ((_wayPoints.Last() - gameObject.transform.position).magnitude > _distBeetweenWaysPoints)
      {
         _lastPos = gameObject.transform.position;
         _wayPoints.Add(_lastPos);
         m_torpedoWay.GetComponent<MeshFilter>().mesh = Utils.CreateOfssetedLinedMesh(_wayPoints, _distBeetweenWaysPoints);
      }
   }

   GameObject createCircle()
   {
      var g = new GameObject("Circle");
      g.layer = 6;
      g.AddComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(m_circleSize, 100);
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material = m_circleMaterial;
      meshReneder.material.color = new Color(1, 0, 0, 0.4f);
      g.transform.SetParent(transform, false);
      g.transform.localPosition = new Vector3(0, 0.5f, 0);
      return g;
   }

   GameObject createForwardArrowedLine()
   {
      var g = new GameObject("ArrowLine");
      g.layer = 6;
      g.AddComponent<MeshFilter>().mesh = Utils.CreateLinedMesh(m_arrowLength, m_arrowWidth, m_arrowWidth * 2);
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material = m_arrow.GetComponent<MeshRenderer>().material;

      g.transform.SetParent(transform, false);
      g.transform.localPosition = new Vector3(0, 0.5f, 0);
      return g;
   }

   private GameObject createWay()
   {
      var g = new GameObject("TorpedoTail");
      g.AddComponent<MeshFilter>().mesh = Utils.CreateOfssetedLinedMesh(_wayPoints, 50);
      var meshReneder = g.AddComponent<MeshRenderer>();
      meshReneder.material = m_arrow.GetComponent<MeshRenderer>().material;

      return g;
   }

   public float Speed => m_speed;

}
