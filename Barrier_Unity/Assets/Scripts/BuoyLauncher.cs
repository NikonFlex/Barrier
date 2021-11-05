using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyLauncher : MonoBehaviour
{
   [SerializeField] private float _launchSpeed; // m/s
   [SerializeField] private float _angleSpeed; // grad/s
   [SerializeField] Transform _packetPos;
   private Quaternion _startRotation;
   private Quaternion _finishRotation;
   private bool _haveTarget = false;
   private bool _busy = false;
   private int _buoysCounter = 0;

   public bool ShootToTarget(Vector3 targetPos)
   {
      if (_haveTarget)
         return false;
      if (_busy)
         return false;
      _busy = true;
      ScenarioLog.Instance.AddMessage("Shoot buoy start");
      //print("Shoot buoy start");
      _haveTarget = true;
      StartCoroutine(shoot(targetPos));
      return true;
   }


   void Start()
   {
      _startRotation = transform.rotation;
   }

   public void LaunchBuouys()
   {
      FindObjectOfType<CameraController>().FollowObject(transform);
      var trg = Scenario.Instance.TargetInfo;
      if (trg != null)
      {
         float d = VarSync.GetFloat(VarName.BouysDistanceBetween);
         float distance = trg.Distance - d * Mathf.Sqrt(3) / 2f;
         Mathf.Clamp(distance, 0, VarSync.GetFloat(VarName.BuoysShootRange));
         Vector3 dirToTarget = (trg.TargetPos - transform.position).normalized;
         Vector3 left = Vector3.Cross(dirToTarget, Vector3.up).normalized;
         Vector3 openConeHeightPos = Vector3.up * VarSync.GetFloat(VarName.BuoysOpenConeHeight);
         Vector3 p1 = left * d / 2 + dirToTarget * distance + openConeHeightPos;
         Vector3 p2 = -left * d / 2 + dirToTarget * distance + openConeHeightPos;
         var o1 = GameObject.CreatePrimitive(PrimitiveType.Cube);
         o1.transform.position = p1;
         var o2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
         o2.transform.position = p2;
         ShootToTarget(p1);
         StartCoroutine(nextShoot(p2));
      }
   }

   void Update()
   {
      if (_busy)
         return;

      if (Input.GetKeyDown("space"))
      {
         LaunchBuouys();
      }
   }

   private IEnumerator nextShoot(Vector3 p)
   {
      while (_busy)
         yield return null;
      ScenarioLog.Instance.AddMessage("next shoot");
      //FindObjectOfType<CameraController>().FollowObject(transform);
      ShootToTarget(p);
      yield return null;
   }

   private IEnumerator shoot(Vector3 pos)
   {
      _busy = true;
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
         rotationTime += Time.deltaTime;
         transform.rotation = Quaternion.Slerp(startRotation, finishRotation, rotationTime / rotationPeriod);
         yield return null;
      }


      GameObject packetPrefab = Resources.Load("packet") as GameObject;
      var packetInstance = Instantiate(packetPrefab, _packetPos.position, _packetPos.rotation);
      packetInstance.name = $"bouy {_buoysCounter++}";
      Packet packet = packetInstance.GetComponent<Packet>();
      packet.Target = pos;
      packet.Launch(_launchSpeed, transform.forward);
      Scenario.Instance.OnPacketLaunch(packet);

      //FindObjectOfType<CameraController>().FollowObject(packetInstance.transform);


      _busy = false;
      _haveTarget = false;

      yield return null;
   }
}