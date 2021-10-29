using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RocketLauncher : MonoBehaviour
{
   [SerializeField] private GameObject _torpedo;
   [SerializeField] private float _speed;
   [SerializeField] private int _rocketNumber;
   [SerializeField] private float _offestBeetweenRockets;
   [SerializeField] private float _killRadius;
   [SerializeField] private float _angleSpeed;

   private List<GameObject> _rockets = new List<GameObject>();

   private bool _isStarted = false;

   void Start()
   {
        
   }

   void Update()
   {
      if (_isStarted)
      {
         StartCoroutine(LaunchRocketsCoroutine());
         _isStarted = false;
      }
   }

   public bool IsAllRocketsExploded()
   {
      for (int i = 0; i < _rockets.Count; i++)
      {
         if (_rockets[i].GetComponent<Rocket>().IsExploded() == false)
            return false;
      }
      return true;
   }

   public void LaunchRockets()
   {
      _isStarted = true;
   }

   private IEnumerator LaunchRocketsCoroutine()
   {
      var startRotation = transform.rotation;
      float horDelta = Utils.CalculateHorAngleDelta(gameObject.transform, _torpedo.transform.position);
      float vertDelta = Utils.CalculateVertAngle(gameObject.transform.position, _torpedo.transform.position, _speed);
      var finishRotation = Quaternion.Euler(-vertDelta, startRotation.eulerAngles.y + horDelta, transform.rotation.z);
      float vertRotationPeriod = Mathf.Abs(vertDelta) / _angleSpeed;
      float horRotationPeriod = Mathf.Abs(horDelta) / _angleSpeed;
      var rotationPeriod = Mathf.Max(vertRotationPeriod, horRotationPeriod);

      float rotationTime = 0;
      while (rotationTime < rotationPeriod)
      {
         rotationTime += Time.deltaTime;
         transform.rotation = Quaternion.Slerp(startRotation, finishRotation, rotationTime / rotationPeriod);
         yield return null;
      }

      float S = (gameObject.transform.position - _torpedo.transform.position).magnitude;
      float V = _speed;
      float T = S / V;
      Vector3 target = _torpedo.transform.position + _torpedo.transform.forward * T * _torpedo.GetComponent<Torpedo>().GetSpeed();

      float offset = -1 * _rocketNumber / 2 * _offestBeetweenRockets;
      for (int i = 0; i < _rocketNumber; i++)
      {
         Vector3 cur_target = target - Utils.PerpTo(_torpedo.transform.forward) * offset;
         GameObject rocket = GameObject.CreatePrimitive(PrimitiveType.Cube);
         rocket.transform.position = gameObject.transform.position;
         rocket.name = $"{i}";
         rocket.AddComponent<Rocket>().AimToTarget(cur_target, _speed, _killRadius);
         _rockets.Add(rocket);
         offset += _offestBeetweenRockets;
      }
   }
}
