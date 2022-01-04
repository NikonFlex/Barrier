using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Packet : MonoBehaviour
{
   private const float g = 9.8f;
   private Vector3 _speedVector;
   private Vector3 _target;
   [SerializeField] private float K = 0.001f;
   private bool _isOnWater = false;

   public bool IsOnWater => _isOnWater;

   public void Launch(float V0, Vector3 dir, Vector3 targetPos)
   {
      _speedVector = dir * V0;
      _target = targetPos;
      StartCoroutine(Fly());
   }

   private IEnumerator Fly()
   {
      bool break_flag = false;
      while (!break_flag)
      {
         if (!Scenario.IsRunning)
         {
            yield return null;
            continue;
         }

         Vector3 pos = transform.position;
         pos += _speedVector * Time.deltaTime;
         _speedVector.y -= g * Time.deltaTime;
         transform.rotation = Quaternion.LookRotation(_speedVector.normalized);
         transform.position = pos;

         if ((transform.position - _target).magnitude < 100 || 
            (_speedVector.y < 0 && pos.y < VarSync.GetFloat(VarName.BuoysOpenConeHeight)))
            break_flag = true;

         yield return null;
      }

      yield return StartCoroutine(SplashDown());
   }

   private IEnumerator SplashDown()
   {
      Scenario.Instance.AddMessage($"Раскрытие парашюта у '{gameObject.name}' на   высоте {transform.position.y}");
      while (transform.position.y > 0)
      {
         if (!Scenario.IsRunning)
         {
            yield return null;
            continue;
         }

         float PvMax = K * _speedVector.magnitude * _speedVector.magnitude; // максимальное сопротивление парашюта
         float PvMin = PvMax / 10; // минимальное сопротивление парашюта
         float speedMax = 250;
         float speed = _speedVector.magnitude;

         float Pv = Mathf.Lerp(PvMax, PvMin, speed / speedMax);

         Vector3 delta = _speedVector.normalized * Pv * Time.deltaTime;

         if (delta.magnitude > _speedVector.magnitude)
            _speedVector = Vector3.zero;
         else
            _speedVector -= delta;

         _speedVector.y -= g * Time.deltaTime;
         Vector3 pos = transform.position;
         pos += _speedVector * Time.deltaTime;
         transform.position = pos;
         transform.rotation = Quaternion.LookRotation(_speedVector.normalized);

         yield return null;
      }

      Scenario.Instance.AddMessage($"Буй '{gameObject.name}' приводнился");

      _isOnWater = true;
      gameObject.AddComponent<Buoy>();
   }
}
