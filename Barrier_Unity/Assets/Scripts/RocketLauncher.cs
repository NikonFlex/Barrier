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

   private bool _isStarted = false;

   void Start()
   {
        
   }

   void Update()
   {
      if (_isStarted == false)
      {
         LaunchRockets();
         _isStarted = true;
      }
   }

   private void LaunchRockets()
   {
      float S = (gameObject.transform.position - _torpedo.transform.position).magnitude;
      float V = _speed;
      float T = S / V;
      Vector3 target = _torpedo.transform.position + _torpedo.transform.forward * T * _torpedo.GetComponent<SandBoxTorpedo>().GetSpeed();

      float offset = -1 * _rocketNumber / 2 * _offestBeetweenRockets;
      for (int i = 0; i < _rocketNumber; i++)
      {
         Vector3 cur_target = target - Utils.PerpTo(_torpedo.transform.forward) * offset;
         GameObject rocket = GameObject.CreatePrimitive(PrimitiveType.Cube);
         rocket.transform.position = gameObject.transform.position;
         rocket.name = $"{i}";
         rocket.AddComponent<SandBoxRocket>().AimToTarget(cur_target, _speed, _killRadius);
         offset += _offestBeetweenRockets;
      }
   }
}
