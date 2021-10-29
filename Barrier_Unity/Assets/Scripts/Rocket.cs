using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rocket : MonoBehaviour
{
   private Vector3 _speedVector;
   private bool _isAimed = false;
   private float _killRadius;

   private void Update()
   {
      if (_isAimed == true)
      {
         StartCoroutine(Fly());
         _isAimed = false;
      }
   }

   public bool IsExploded()
   {
      return gameObject.transform.position.y < 0;
   }

   public void AimToTarget(Vector3 target, float speed, float killRadius)
   {
      _killRadius = killRadius;
      float horAngle = transform.rotation.eulerAngles.y + Utils.CalculateHorAngleDelta(transform, target);
      float vertAngle = Utils.CalculateVertAngle(transform.position, target, speed);
      gameObject.transform.rotation = Quaternion.Euler(-vertAngle, horAngle, transform.rotation.z);
      _speedVector = gameObject.transform.forward * speed;
      _isAimed = true;
   }

   private IEnumerator Fly()
   {
      bool break_flag = false;
      while (!break_flag)
      {
         Vector3 pos = transform.position;
         pos += _speedVector * Time.deltaTime;
         _speedVector.y -= 9.8f * Time.deltaTime;
         transform.rotation = Quaternion.LookRotation(new Vector3(0, 0, 0)); // куда смотрит снаряд
         if (transform.position.y < 0)
            break_flag = true;
         transform.position = pos;
         GetComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(_killRadius, 30);
         GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0);
         yield return null;
      }
   }
}
