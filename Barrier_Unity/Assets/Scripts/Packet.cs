﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Packet : MonoBehaviour
{
   [SerializeField] private float K = 0.001f;
   [SerializeField] private float _stopSlowDownHeight;
   [SerializeField] private float _workingDepth;

   [SerializeField] private GameObject _slowDownEngine;
   [SerializeField] private GameObject _bobber;
   [SerializeField] private GameObject _trail;

   private LineRenderer _lineRenderer;

   private const float g = 9.8f;
   private const float M = 1550f;
   private const float V = 3f;
   private Vector3 _speedVector;
   private Vector3 _target;
   private float _divingSpeed = 0f;
   private bool _isOnWater = false;

   public bool IsOnWater => _isOnWater;
   public Transform Bobber => _bobber.transform;
   public int BuoyIndex = -1;

   void Start()
   {
      _lineRenderer = gameObject.GetComponent<LineRenderer>();
   }

   public void Launch(float V0, Vector3 dir, Vector3 targetPos)
   {
      _speedVector = dir * V0;
      _target = targetPos;
      StartCoroutine(fly());
   }
   
   public float CalcTimeToTarget()
   {
      return (new Vector2(_target.x, _target.z) - new Vector2(transform.position.x, transform.position.z)).magnitude /
         new Vector2(_speedVector.x, _speedVector.z).magnitude; 
   }

   public Vector3 TargetPos => _target;

   public GameObject Trail => _trail;

   private IEnumerator fly()
   {
      while (transform.position.y > 0)
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
         var nextPos = pos + (_speedVector + Vector3.down * g * Time.deltaTime) * Time.deltaTime;
         float breakAltitude = VarSync.GetFloat(VarName.BuoyBreakStartAltitude);
         if ((transform.position - _target).magnitude < 100 ||
            (_speedVector.y < 0 &&
            (pos.y < breakAltitude || nextPos.y < breakAltitude)))
            break;
         
         yield return null;
      }

      //yield return StartCoroutine(splashDown());
      yield return StartCoroutine(reactiveSlowDown());
   }

   private IEnumerator splashDown()
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

   private IEnumerator reactiveSlowDown()
   {
      Scenario.Instance.AddMessage($"Начало торможения у '{gameObject.name}' на   высоте {transform.position.y}");
      Debug.Log($"{name} time to target {CalcTimeToTarget()}");
      _slowDownEngine.SetActive(true);

      Vector3 slowDownS = _speedVector.normalized * ((transform.position.y - _stopSlowDownHeight) / _speedVector.normalized.y);
      float a = _speedVector.magnitude * _speedVector.magnitude / (2 * slowDownS.magnitude);
      a += g;

      while (transform.position.y > 1)
      {
         if (!Scenario.IsRunning)
         {
            yield return null;
            continue;
         }

         _speedVector += -1 * _speedVector.normalized * a * Time.deltaTime;
         _speedVector.y -= g * Time.deltaTime;
         Vector3 pos = transform.position;
         pos += _speedVector * Time.deltaTime;
         transform.position = pos;
         transform.rotation = Quaternion.LookRotation(_speedVector.normalized);
         yield return null;
      }

      Scenario.Instance.AddMessage($"Буй '{gameObject.name}' приводнился");
      yield return StartCoroutine(diving());
   }

   private IEnumerator diving()
   {
      _slowDownEngine.SetActive(false);
      _trail.SetActive(false);
      _bobber.transform.SetParent(null, true);

      Vector3 cur_pos = gameObject.transform.position;
      _bobber.transform.position = new Vector3(cur_pos.x, 1, cur_pos.z);
      _bobber.transform.LookAt(new Vector3(cur_pos.x, -_workingDepth, cur_pos.z));
      gameObject.transform.LookAt(new Vector3(cur_pos.x, -_workingDepth, cur_pos.z));

      _bobber.GetComponent<BobberFloatingObjectActivation>().StartFloating();

      string labelText = LabelHelper.GetLabelText(gameObject);
      if (BuoyIndex != 0) // следим только за первым буем. Потом решение можно переделать
         LabelHelper.HideLabel(gameObject);

      float a = (g * (1000 * V - M)) / M; //ускорение в воде

      while (gameObject.transform.position.y > -_workingDepth)
      {
         _divingSpeed += a * Time.deltaTime; 
         gameObject.transform.position += gameObject.transform.forward * _divingSpeed * Time.deltaTime;
         LabelHelper.SetLabelText(gameObject, $"H = {-gameObject.transform.position.y:0.#}");
         drawRopeToBopper();
         yield return null;
      }

      LabelHelper.HideLabel(gameObject);
      LabelHelper.AddLabel(_bobber, labelText);
      Scenario.Instance.AddMessage($"Буй '{gameObject.name}' погрузился");
      gameObject.AddComponent<Buoy>();
      _isOnWater = true;
      yield return null;
   }

   private void drawRopeToBopper()
   {
      if (!gameObject.activeSelf)
         return;

      List<Vector3> pos = new List<Vector3>();
      pos.Add(gameObject.transform.position);
      pos.Add(_bobber.transform.position);
      _lineRenderer.startWidth = 0.1f;
      _lineRenderer.endWidth = 0.1f;
      _lineRenderer.SetPositions(pos.ToArray());
      _lineRenderer.useWorldSpace = true;
   }
}
