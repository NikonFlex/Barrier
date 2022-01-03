using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyLauncher : MonoBehaviour
{
   [SerializeField] private float _launchSpeed; // m/s
   [SerializeField] private float _angleSpeed; // grad/s
   [SerializeField] Transform _packetPos;
   private bool _inProgress = false;
   private Vector3[] _buoysTargets;
   private int _buoysCounter = 0;
   public readonly int NumBuoys = 2;

   public bool LaunchBuouys()
   {
      if (_inProgress)
      {
         Debug.LogWarning("Launch buoys already in progress");
         return false;
      }
         

      var trg = Scenario.Instance.TargetInfo;
      if (trg == null)
      {
         Debug.LogWarning("Can't launch buoys without target");
         return false;
      }

      _inProgress = true;
      var buoysTargets = new List<Vector3>();

      float d = VarSync.GetFloat(VarName.BouysDistanceBetween);
      float distance = trg.Distance - d * Mathf.Sqrt(3) / 2f;
      Mathf.Clamp(distance, 0, VarSync.GetFloat(VarName.BuoysShootRange));
      Vector3 dirToTarget = (trg.Target.transform.position - transform.position).normalized;
      Vector3 left = Vector3.Cross(dirToTarget, Vector3.up).normalized;
      Vector3 openConeHeightPos = Vector3.up * VarSync.GetFloat(VarName.BuoysOpenConeHeight);
      Vector3 p1 = left * d / 2 + dirToTarget * distance + openConeHeightPos;
      Vector3 p2 = -left * d / 2 + dirToTarget * distance + openConeHeightPos;
      buoysTargets.Add(p1);
      buoysTargets.Add(p2);
      _buoysTargets = buoysTargets.ToArray();
      //       var o1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      //       o1.transform.position = p1;
      //       var o2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
      //       o2.transform.position = p2;
      StartCoroutine(launchCoroutine());
      return true;
   }

   private IEnumerator launchCoroutine()
   {
      for (_buoysCounter = 0; _buoysCounter < _buoysTargets.Length; ++_buoysCounter)
      {
         Vector3 pos = _buoysTargets[_buoysCounter];
         yield return StartCoroutine(aimToTarget(pos));
         Scenario.Instance.AddMessage($"Запуск буя {_buoysCounter}");
         emitBuoy(pos, $"buoy {_buoysCounter}");
         yield return null;
      }
      _inProgress = false;
      yield return null;
   }

   private IEnumerator aimToTarget(Vector3 pos)
   {
      Scenario.Instance.AddMessage($"Прицеливание буя {_buoysCounter}");
      var startRotation = transform.rotation;
      float horDelta = Utils.CalculateHorAngleDelta(gameObject.transform, pos);
      float vertDelta = Utils.CalculateVertAngle(gameObject.transform.position, pos, _launchSpeed);
      var finishRotation = Quaternion.Euler(-vertDelta, startRotation.eulerAngles.y + horDelta, transform.rotation.z);
      float vertRotationPeriod = Mathf.Abs(vertDelta) / _angleSpeed;
      float horRotationPeriod = Mathf.Abs(horDelta) / _angleSpeed;
      var rotationPeriod = Mathf.Max(vertRotationPeriod, horRotationPeriod);

      float rotationTime = 0;
      while (rotationTime < rotationPeriod)
      {
         if (Scenario.IsRunning)
         {
            rotationTime += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, finishRotation, rotationTime / rotationPeriod);
         }
         yield return null;
      }
      yield return null;
   }

   private void emitBuoy(Vector3 targetPos, string name)
   {
      Packet packet = Instantiate(Resources.Load<Packet>("packet"), _packetPos.position, _packetPos.rotation);
      packet.name = name;
      packet.Target = targetPos;
      packet.Launch(_launchSpeed, transform.forward);
      Scenario.Instance.OnPacketLaunched(packet);
   }


   void Update()
   {
      if (_inProgress)
         return;

      if (Input.GetKeyDown("space"))
      {
         LaunchBuouys();
      }
   }

}