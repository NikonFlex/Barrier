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
   private float _killRadius;
   private bool _isExploded = false;

   public bool IsExploded => _isExploded;

   public void AimToTarget(Vector3 target, float speed, float killRadius)
   {
      _killRadius = killRadius;
      float horAngle = transform.rotation.eulerAngles.y + Utils.CalculateHorAngleDelta(transform, target);
      float vertAngle = Utils.CalculateVertAngle(transform.position, target, speed);
      gameObject.transform.rotation = Quaternion.Euler(-vertAngle, horAngle, transform.rotation.z);
      _speedVector = gameObject.transform.forward * speed;
      StartCoroutine(fly());
   }

   private IEnumerator fly()
   {
      while (transform.position.y > 1)
      {
         if (!Scenario.IsRunning)
         {
            yield return null;
            continue;
         }

         Vector3 pos = transform.position;
         pos += _speedVector * Time.deltaTime;
         _speedVector.y -= 9.8f * Time.deltaTime;
         transform.rotation = Quaternion.LookRotation(_speedVector.normalized);
         transform.position = pos;
         
         yield return null;
      }

      yield return StartCoroutine(explode());
   }

   private IEnumerator explode()
   {
      //Time.timeScale = 0.1f;
      LabelHelper.DestroyLabel(gameObject);
      transform.forward = Vector3.down;
      Utils.SetHeight(transform, 2);
      float cur_radius = 0;
      float adding_radius = _killRadius / 60f * 1.25f;
      GameObject strikeEffect = Instantiate(Resources.Load("RocketStrikeFx"), transform.position, Quaternion.identity) as GameObject;

      while (cur_radius < _killRadius)
      {
          cur_radius += adding_radius;
          strikeEffect.transform.localScale = new Vector3(cur_radius / 50, cur_radius / 50, cur_radius / 50);
//          GetComponent<MeshFilter>().mesh = Utils.CreateCircleMesh(cur_radius, 30);
//          GetComponent<MeshRenderer>().material.color = new Color(1, 0, 0, 0.5f);
          yield return null;
      }
      var delta = transform.position - Scenario.Instance.TargetInfo.Target.transform.position;
      delta.y = 0;
      if (delta.magnitude <= _killRadius)
      {
         Scenario.Instance.TargetInfo.Target.Kill();
         Scenario.Instance.AddMessage("Торпеда взорванна");
      }

      gameObject.transform.GetChild(0).gameObject.SetActive(false);
      _isExploded = true;
   }
}
