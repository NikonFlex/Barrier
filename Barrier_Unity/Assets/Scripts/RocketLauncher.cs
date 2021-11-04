using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RocketLauncher : MonoBehaviour
{
   [SerializeField] private GameObject _torpedo;
   [SerializeField] private float _speed;
   [SerializeField] private int _rocketNumber;
   [SerializeField] private float _offestBeetweenRockets;
   [SerializeField] private float _killRadius;
   [SerializeField] private float _angleSpeed;

   private List<Rocket> _rockets = new List<Rocket>();
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
      //return false;
      if (_rockets.Count == 0)
         return false;
      else
         return _rockets.All(x => x.IsExploded);
   }

   public void LaunchRockets()
   {
      _isStarted = true;
      _rocketNumber = VarSync.GetInt(VarName.RocketNum) + 1; //count from zero
      _offestBeetweenRockets = VarSync.GetFloat(VarName.RocketDistance);
   }

   private IEnumerator LaunchRocketsCoroutine()
   {
      yield return new WaitForSeconds(VarSync.GetInt(VarName.RocketPauseDuration));
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
      Vector3 target = _torpedo.transform.position + _torpedo.transform.forward * T * _torpedo.GetComponent<Torpedo>().Speed;

      float offset = -1 * _rocketNumber / 2 * _offestBeetweenRockets;
      for (int i = 0; i < _rocketNumber; i++)
      {
         Vector3 cur_target = target - Utils.PerpTo(_torpedo.transform.forward) * offset;

         GameObject rocketPrefab = Resources.Load("rocket") as GameObject;
         GameObject packetInstance = Instantiate(rocketPrefab, gameObject.transform.position, Quaternion.Euler(0, 0, 0));
         packetInstance.name = $"{i}";
         Rocket rocketComp = packetInstance.GetComponent<Rocket>();
         rocketComp.AimToTarget(cur_target, _speed, _offestBeetweenRockets / 2);
         _rockets.Add(rocketComp);

         offset += _offestBeetweenRockets;
      }
   }
}