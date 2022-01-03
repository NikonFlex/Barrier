using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PacketState
{
   None = 0,
   Fly = 1,
   SplashDown = 2,
   OnWater = 3
}

public class Packet : MonoBehaviour
{
   private const float g = 9.8f;
   private Vector3 _speedVector;
   private PacketState _state = PacketState.None;
   private Vector3 _target;
   [SerializeField] private float K = 0.001f;
   Vector3 _prevPos;
   bool _isOnParashut = false;
   private Vector3 _startPos;

   public PacketState State => _state;

   void Start()
   {
      _startPos = transform.position;
   }

   void Update()
   {
      //if (Target == null)
      //   return;

      //switch (_state)
      //{
      //   //case PacketState.Fly:
      //   //   StartCoroutine(Fly());
      //   //   break;

      //   case PacketState.SplashDown:
      //      if (!_isOnParashut)
      //         StartCoroutine(SplashDown());
      //      break;
      //}
   }

   public void Launch(float V0, Vector3 dir, Vector3 targetPos)
   {
      _speedVector = dir * V0;
      _state = PacketState.Fly;
      _target = targetPos;
      StartCoroutine(Fly());
   }

   private IEnumerator Fly()
   {
      bool break_flag = false;

      float startTime = Time.time;
      Vector3 startPos = transform.position;

      while (!break_flag)
      {
         Time.timeScale = 1;
         if (!Scenario.IsRunning)
         {
            yield return null;
            continue;
         }
         Vector3 pos = transform.position;
         pos += _speedVector * Time.deltaTime;
         _speedVector.y -= g * Time.deltaTime;
         transform.rotation = Quaternion.LookRotation(_speedVector.normalized);

         if ((transform.position - _target).magnitude < 100 || 
            (_speedVector.y < 0 && pos.y < VarSync.GetFloat(VarName.BuoysOpenConeHeight)))
            break_flag = true;
         //Debug.Log($"packet {(pos - transform.position).magnitude / Time.deltaTime} {Time.deltaTime}");
         transform.position = pos;
         yield return null;
      }
      Debug.Log($"packet {Time.time - startTime} {(startPos - transform.position).magnitude}");
      _state = PacketState.SplashDown;
      yield return StartCoroutine(SplashDown());
   }

   private IEnumerator SplashDown()
   {
      Scenario.Instance.AddMessage($"Раскрытие парашюта у '{gameObject.name}' на   высоте {transform.position.y}");
      //print($"packet '{gameObject.name}' on parashut on height {transform.position.y}");
      _isOnParashut = true;
      _prevPos = transform.position;
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
         //if ((transform.position - _prevPos).magnitude > 1)
         //{
         //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
         //    cube.transform.position = transform.position;
         //    _prevPos = transform.position;

         //}

         yield return null;
      }
      Scenario.Instance.AddMessage($"Буй '{gameObject.name}' приводнился");

      _state = PacketState.OnWater;
      gameObject.AddComponent<Buoy>();
   }
}
