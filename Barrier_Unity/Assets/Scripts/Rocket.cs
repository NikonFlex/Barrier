using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum RocketState
{
   None = 0,
   Fly = 1,
   Explode = 2,
}

public class Rocket : MonoBehaviour
{
   private Vector3 _speedVector;
   private bool _newState = false;
   private float _killRadius;
   private RocketState _state;

   private void Update()
   {
      switch (_state)
      {
         case RocketState.Fly:
            if (_newState == true)
            {
               StartCoroutine(Fly());
               _newState = false;
            }
            break;

         case RocketState.Explode:
            if (_newState == true)
            {
               StartCoroutine(Explode());
               _newState = false;
            }
            break;
      }

      if (_newState == true)
      {
         StartCoroutine(Fly());
         _newState = false;
      }
   }

   public bool IsExploded => gameObject.transform.position.y < 0;

   public void AimToTarget(Vector3 target, float speed, float killRadius)
   {
      _killRadius = killRadius;
      float horAngle = transform.rotation.eulerAngles.y + Utils.CalculateHorAngleDelta(transform, target);
      float vertAngle = Utils.CalculateVertAngle(transform.position, target, speed);
      gameObject.transform.rotation = Quaternion.Euler(-vertAngle, horAngle, transform.rotation.z);
      _speedVector = gameObject.transform.forward * speed;
      _newState = true;
      _state = RocketState.Fly;
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
         yield return null;
      }
      _newState = true;
      _state = RocketState.Explode;
   }

   private IEnumerator Explode()
   {
      bool break_flag = false;
      float cur_radius = 0;
      float adding_radius = _killRadius / 60f * 1.25f;
      while (!break_flag)
      {
         if (cur_radius <= _killRadius)
         {
            cur_radius += adding_radius;
         }
         else
         {
            cur_radius = _killRadius;
            break_flag = true;
         }
         GetComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(cur_radius, 30);
         GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
         yield return null;
      }
   }
}
