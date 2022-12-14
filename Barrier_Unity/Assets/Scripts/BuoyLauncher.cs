using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class BuoyLauncher : MonoBehaviour
{
   [SerializeField] private float _launchSpeed; // m/s
   [SerializeField] private float _angleSpeed; // grad/s
   [SerializeField] Transform _packetPos;
   private bool _inProgress = false;
   private Vector3[] _buoysTargets;
   private int _buoysCounter = 0;
   public  int NumBuoys => int.Parse(VarSync.GetStringEnum(VarName.NumBuoys));

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

      float d = VarSync.GetFloat(VarName.BuoysDistanceBetween);
      float distance = trg.Distance/2 - d * Mathf.Sqrt(3) / 2f;
      float distance1 = distance + VarSync.GetFloat(VarName.BuoysShootRangeDiff)/2;
      Mathf.Clamp(distance1, 100, VarSync.GetFloat(VarName.BuoysShootRange));
      float distance2 = distance1 - VarSync.GetFloat(VarName.BuoysShootRangeDiff);
      Mathf.Clamp(distance2, 100, VarSync.GetFloat(VarName.BuoysShootRange));

      Vector3 dirToTarget = (trg.Target.transform.position - transform.position).normalized;
      Vector3 left = Vector3.Cross(dirToTarget, Vector3.up).normalized;
      //Vector3 targetHeight = Vector3.up * VarSync.GetFloat(VarName.BuoysOpenConeHeight);
      Vector3 targetHeight = Vector3.up * VarSync.GetFloat(VarName.BuoyBreakStartAltitude);
      Vector3 p1 = left * d / 2 + dirToTarget * distance1 + targetHeight;
      Vector3 p2 = -left * d / 2 + dirToTarget * distance2 + targetHeight;
      Vector3 p3 = left * d / 1 + dirToTarget * (distance1 * 1.3f) + targetHeight;
      Vector4 p4 = -left * d / 1 + dirToTarget * (distance2 * 1.3f) + targetHeight;
      Vector3 p5 = left * d / 1 + dirToTarget * (distance1 * 1f) + targetHeight;
      Vector4 p6 = -left * d / 1 + dirToTarget * (distance2 * 1f) + targetHeight;
      buoysTargets.Add(p1);
      buoysTargets.Add(p2);
      buoysTargets.Add(p3);
      buoysTargets.Add(p4);
      buoysTargets.Add(p5);
      buoysTargets.Add(p6);
      _buoysTargets = buoysTargets.Take(NumBuoys).ToArray();
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
      Quaternion startRotation = transform.rotation;
      float horDelta = Utils.CalculateHorAngleDelta(transform, pos);
      float vertAngle = Utils.CalculateVertAngle(transform.position, pos, _launchSpeed);

      Quaternion finishRotation = Quaternion.Euler(-vertAngle, startRotation.eulerAngles.y + horDelta, startRotation.eulerAngles.z);

      float a = Mathf.Abs(Quaternion.Angle(finishRotation, startRotation));
      float rotationPeriod = a / _angleSpeed;

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
      packet.Launch(_launchSpeed, transform.forward, targetPos);
      packet.Index = Scenario.Instance.BuoyPackets.Length;
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