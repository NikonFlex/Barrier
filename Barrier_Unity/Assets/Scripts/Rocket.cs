using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum RocketState
{
   None = 0,
   Fly = 1,
   Explode = 2,
   Exploded = 3,
}

public class Rocket : MonoBehaviour
{
   private Vector3 _speedVector;
   private bool _isAimed = false;
   private float _killRadius;
   private RocketState _state;

   private void Update()
   {
      if (_isAimed == true)
      {
         _isAimed = false;
         StartCoroutine(Coorutine());
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
      _isAimed = true;
      _state = RocketState.Fly;
   }

   private IEnumerator Coorutine()
   {
      while (_state == RocketState.Fly)
      {
         Vector3 pos = transform.position;
         pos += _speedVector * Time.deltaTime;
         _speedVector.y -= 9.8f * Time.deltaTime;
         transform.rotation = Quaternion.LookRotation(_speedVector); // куда смотрит снаряд
         if (transform.position.y < 1)
            _state = RocketState.Explode;
         transform.position = pos;
         yield return null;
      }

      float cur_radius = 0;
      float adding_radius = _killRadius / 60f * 1.25f;
      while (cur_radius < _killRadius)
      {
         cur_radius += adding_radius;
         GetComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(cur_radius, 30);
         GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
         yield return null;
      }
      _state = RocketState.Exploded;
   }
}
