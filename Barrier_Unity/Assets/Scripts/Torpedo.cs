using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Torpedo : MonoBehaviour
{
   [SerializeField] float m_speed = 30;
   [SerializeField] float m_advanceDistance = 100;
   
   [SerializeField] GameObject m_targetShip;
   
   [SerializeField] float m_predictTime = 4;
   [SerializeField] GameObject m_predictObject;

   private bool _isAlive = true;

   public bool IsActive => _isAlive;

   public void Kill()
   {
      gameObject.SetActive(false);
      _isAlive = false;
   }

   void Start()
   {
   }

   void Update()
   {

      if (!Scenario.IsRunning)
         return;


      float step = m_speed * Time.deltaTime;
      Vector3 pos = transform.position;

      Vector3 targetAdvancePos = m_targetShip.transform.position + m_targetShip.transform.forward * m_advanceDistance;
      var relativePos = targetAdvancePos - transform.position;
      var targetRotation = Quaternion.LookRotation(relativePos);
      transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 0.5f);
      pos += transform.forward*step;
      transform.position = pos;

      m_predictObject.transform.position = transform.position + transform.forward * m_speed * m_predictTime;
   }

   public float Speed => m_speed;

}
