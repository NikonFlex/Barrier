using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;

public enum ScenarioPhaseState
{
   Alert, // поиск цели
   TargetDetectedByAntenna,// - цель обнаружена антенной
   StartAiming,// - начало нацеливания
   BuoysLaunched,// - Буи выпущены
   BuoysOnPlace,  // - Буи приводнились
   BuoysStartScan, // - Начало сканирования буями
   TargetDetectedByBuoys, // - Цель обнаружена буями
   MissilesLaunched, // - Ракеты выпущены
   MissilesStrike, //- Ракеты взорвались
   ScenarioFinished //- Окончание упражения
}

public enum TargetDetectStatus
{
   NoDetect,
   MPCOnly,
   MPCAndBuoys,
   Buoys
}

public abstract class IScenarioPhase
{
   public abstract ScenarioPhaseState ScenarioState { get; }
   public abstract string Title { get; }
   public abstract void Start();
   public abstract bool IsFinished { get; }
   public abstract void Update();
}

public class ScnenarioPhaseStub : IScenarioPhase
{
   public ScnenarioPhaseStub(ScenarioPhaseState state, string title, float duration, Transform followObject = null)
   {
      _scenarioState = state;
      _title = title;
      _duration = duration;
      _followObject = followObject;
   }
   public override void Start()
   {
      _startTime = Scenario.Instance.ScenarioTime;
      if (_followObject != null)
         GameObject.FindObjectOfType<CameraController>().FollowObject(_followObject);

   }

   public override void Update() {}

   public override ScenarioPhaseState ScenarioState => _scenarioState;
   public override string Title => _title;
   public override bool IsFinished => Scenario.Instance.ScenarioTime > _startTime + _duration;

   private readonly string _title;
   private readonly float _duration;
   private float _startTime;
   private ScenarioPhaseState _scenarioState;
   private Transform _followObject;
}

public class TargetInfo
{
   public float Bearing = 0;
   public float Distance = -1;
   public float Tcpa = -1;
   public Torpedo Target;
}

public class Scenario : MonoBehaviour
{
   [SerializeField] Ship _ship;
   [SerializeField] Torpedo _torpedo;
   [SerializeField] private CameraController _cameraController;
   [SerializeField] private ScenarioLog _log;

   private List<Packet> _buoyPackets = new List<Packet>();
   private List<Rocket> _rockets = new List<Rocket>();
   private List<Buoy> _buoys = new List<Buoy>();

   private IScenarioPhase[] _phases;
   private Mode _currentMode = Mode.Stoped;
   private float _startTime;
   private float _currentTime;
   private int _currentPhaseIndex;

   private static Scenario _instance;

   private IScenarioPhase currentPhase => _phases[(int)_currentPhaseIndex];
   private bool isRunning => _currentMode == Mode.Running;
   private bool isAlive => _currentMode != Mode.Stoped && _currentMode != Mode.Finished;



   public enum Mode
   {
      Stoped,
      Paused,
      Running,
      Finished
   }

   public static Scenario Instance => _instance;
   public ScenarioPhaseState State => currentPhase.ScenarioState;
   public Mode CurrentMode => _currentMode;
   public float ScenarioTime => _currentTime - _startTime;
   public IScenarioPhase CurrentPhase => currentPhase;
   public bool IsAlive => isAlive;
   public static bool IsRunning => Instance.isRunning;
   public TargetInfo TargetInfo => isRunning ? calcTargetInfo() : null;
   public Packet[] BuoyPackets => _buoyPackets.ToArray();
   public Buoy[] Buoys => _buoys.ToArray();
   public Rocket[] Rockets => _rockets.ToArray();
   public Ship Ship => _ship;
   public static bool IsTargetAlive => 
      Instance.TargetInfo != null &&
      Instance.TargetInfo.Target.IsActive;
   public TargetDetectStatus TargetDetectStatus { get; set; } = TargetDetectStatus.NoDetect;
   public Vector3 PointOfFirstDetectionByMPC { get; set; }


   public void OnPacketLaunched(Packet p)
   {
      p.BuoyIndex = _buoyPackets.Count;
      _buoyPackets.Add(p);
      LabelHelper.AddLabel(p.gameObject, $"РГАБ {_buoyPackets.Count}");
      VirtualCameraHelper.AddMemberToTargetGroup("vcam_Launch", p.transform);
      if (_buoyPackets.Count == 1) // first buoy
      {
         VirtualCameraHelper.AddMemberToTargetGroup("vcam_Buoy", p.transform);
      }
   }
   public void OnBouyBorn(Buoy b) => _buoys.Add(b);

   public void OnRocketLaunched(Rocket r)
   {
      _rockets.Add(r);
      VirtualCameraHelper.AddMemberToTargetGroup("vcam_Rockets", r.transform);
      LabelHelper.AddLabel(r.gameObject, $"РАКЕТА {_rockets.Count}");
   }

   public void StartScenario()
   {
      OffScreenIndicator.ShowIndecators();
      setUpTargetPosition();
      setUpShipSettings();
      _currentTime = _startTime = Time.time;
      _currentMode = Mode.Running;
      _currentPhaseIndex = 0;

      _buoyPackets.Clear();
      _rockets.Clear();
      _buoys.Clear();
      TargetDetectStatus = TargetDetectStatus.NoDetect;

      currentPhase.Start();
   }

   private void setUpTargetPosition()
   {
      float bearing = VarSync.GetFloat(VarName.StartBearingToTarget);
      float distance = VarSync.GetFloat(VarName.StartDistanceToTarget);

      _torpedo.transform.position = Quaternion.AngleAxis(bearing, Vector3.up) * _ship.transform.forward * distance;
   }

   private void setUpShipSettings()
   {
      _ship.transform.GetComponent<Ship>().SetUpMpcSettings(VarSync.GetBool(VarName.MPC_USE), VarSync.GetFloat(VarName.MPC_DISTANCE));
      LabelHelper.ShowLabel(_ship.gameObject);
   }

   public void PauseScenario()
   {
      print("PauseScenario");
      if (_currentMode == Mode.Running)
         _currentMode = Mode.Paused;
   }

   public void ResumeScenario()
   {
      print("ResumeScenario");
      if (_currentMode == Mode.Paused)
         _currentMode = Mode.Running;
   }

   public void StopScenario()
   {
      _currentMode = Mode.Stoped;
   }

   public void AddMessage(string message)
   {
      _log.AddMessage(message);
   }

   void Awake() => _instance = this;

   void Start()
   {
      AttributeHelper.DeserializeFromYaml("settings.yaml");
      LabelHelper.ShowLabels(true);
      // add stub phases
      var phases = new List<IScenarioPhase>();
      phases.Add(new PhaseAlert());
      phases.Add(new PhaseTargetDetectedByMPC());
      phases.Add(new PhaseLaunchBouys());
      phases.Add(new PhaseBouysPreparingReady());
      phases.Add(new PhaseBouysStartScan());
      //phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.BuoysStartScan, "Начало сканирования буями", 2f));
      phases.Add(new PhaseBouysTargetDetected());
      phases.Add(new PhaseLaunchRockets());
      //phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.MissilesStrike, "Ракеты достигли цели", 2)); НЕ НУЖНА
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.ScenarioFinished, "Сценарий закончен", 2));
      
      _phases = phases.ToArray();

      VirtualCameraHelper.Activate("vCam_ShipGroup");
   }
   // Update is called once per frame
   void Update()
   {
      VarSync.Set(VarName.CurrentTime, _currentTime);
      VarSync.Set(VarName.ScenarioPhaseName, currentPhase.Title, true);
      
      if (_currentMode != Mode.Running)
         return;

      TargetInfo trg = calcTargetInfo();
      VarSync.Set(VarName.TargetBearing, trg != null ? trg.Bearing : 0f);
      VarSync.Set(VarName.TargetDistance, trg != null ? trg.Distance : 0f);
      VarSync.Set(VarName.TargetTCPA, trg != null ? trg.Tcpa.ToString("N1") : "--");

      _currentTime += Time.deltaTime;

      currentPhase.Update();

      //взрыв корабля
      if (Scenario.Instance.TargetInfo.Distance <= 15 && _ship.transform.GetComponent<Ship>().IsAlive)
      {
         StartCoroutine(_ship.transform.GetComponent<Ship>().Explode());
         Scenario.Instance.AddMessage("Корабль уничтожен");
         Scenario.Instance.TargetInfo.Target.Kill();
      }

      if (!currentPhase.IsFinished)
         return;

      int nextIndex = (int)_currentPhaseIndex + 1;
      if (nextIndex >= _phases.Length)
      {
         _currentMode = Mode.Finished;
         AddMessage("Сценарий закончен");
         return;
      }
      _currentPhaseIndex = nextIndex;
      AddMessage($"Старт фазы: '{currentPhase.Title}'");
      currentPhase.Start();
   }


   private TargetInfo calcTargetInfo()
   {
      CpaTcpa.PolarPoint own_v = new CpaTcpa.PolarPoint(_ship.m_Speed, _ship.transform.rotation.eulerAngles.y);
      CpaTcpa.PolarPoint tgt_v = new CpaTcpa.PolarPoint(_torpedo.Speed, _torpedo.transform.rotation.eulerAngles.y);
      
      float distance = (_ship.transform.position - _torpedo.transform.position).magnitude;
      float bearing = Vector3.SignedAngle(_ship.transform.forward, (_torpedo.transform.position - transform.position).normalized, Vector3.up);

      CpaTcpa.PolarPoint tgt = new CpaTcpa.PolarPoint(distance, bearing);

      float tcpa = 0;
      float cpa = 0;
      CpaTcpa.Calc(own_v, tgt_v, tgt, out cpa, out tcpa);

      return new TargetInfo()
      {
         Distance = distance,
         // TODO: use ship direction and 
         Target = _torpedo,
         Bearing = bearing,
         Tcpa = tcpa
      };
   }
}

class PhaseAlert: IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.Alert;
   public override string Title => "Поиск цели";
   public override bool IsFinished => Scenario.Instance.TargetInfo.Distance <= VarSync.GetFloat(VarName.MPC_RANGE)*1000;

   public override void Start() { }
   public override void Update() { }
}


class PhaseTargetDetectedByMPC : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.TargetDetectedByAntenna;
   public override string Title => "Первичное обнаружение";
   public override bool IsFinished => Scenario.Instance.ScenarioTime > _startTime + _durationLift + _durationDetect + _finishDelay;
   private float _startTime;
   private float _durationLift = 3f;
   private float _durationDetect = 1f;
   private float _finishDelay = 1f;
   private CinemachineVirtualCamera _camera;

   public override void Start()
   {
      Scenario.Instance.PointOfFirstDetectionByMPC = Scenario.Instance.TargetInfo.Target.transform.position;

      _startTime = Scenario.Instance.ScenarioTime;
      _camera = VirtualCameraHelper.Activate("vCam_ShipGroup");
      VirtualCameraHelper.AddMemberToTargetGroup(_camera, Scenario.Instance.TargetInfo.Target.transform, 1);
//      VirtualCameraHelper.AddMemberToTargetGroup(camera, Scenario.Instance.Ship.MPC.transform);
      Scenario.Instance.TargetDetectStatus = TargetDetectStatus.MPCOnly;
   }
   public override void Update() 
   {
      var t = Scenario.Instance.ScenarioTime - _startTime;
      if (t < _durationLift)
         Utils.SetHeight(_camera.transform, Mathf.SmoothStep(0, Scenario.Instance.TargetInfo.Distance, t / _durationLift));
      else
      {
         t -= _durationLift;
         Scenario.Instance.Ship.MPC.BeamLengthCoef = Mathf.SmoothStep(0, 1, t / _durationDetect);
      }
         
   }
}

class PhaseLaunchBouys : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.BuoysLaunched;
   public override string Title => "Запуск буев";
   public override bool IsFinished => checkFinished();
   public float _delay1 = 2;
   public float _delay2 = 1;
   public float _buoyCameraHeight = 15;
   private float _startTime;
   private float _allBouysLaunchedTime = 0;
   private CinemachineVirtualCamera _launchCamera = null;
   private CinemachineVirtualCamera _bouyCamera = null;
   BuoyLauncher _launcher;

   public override void Start()
   {
      _startTime = Scenario.Instance.ScenarioTime;
      _launcher = GameObject.FindObjectOfType<BuoyLauncher>();
      _launcher.LaunchBuouys();
   }
   public override void Update() 
   {
      if (_startTime + _delay1 >= Scenario.Instance.ScenarioTime && _launchCamera == null)
         _launchCamera = VirtualCameraHelper.Activate("vcam_Launch");

      if (_allBouysLaunchedTime == 0)
      {
         if (Scenario.Instance.BuoyPackets.Length == _launcher.NumBuoys)
            _allBouysLaunchedTime = Scenario.Instance.ScenarioTime;
      }
      else
      {
         var buoyPacket = Scenario.Instance.BuoyPackets.First();
         if (_allBouysLaunchedTime + _delay2 < Scenario.Instance.ScenarioTime
               && _bouyCamera == null
               //&& _buoyCameraHeight > buoyPacket.transform.position.y)
               && buoyPacket.CalcTimeToTarget() < 3 )
         {
            if (buoyPacket.State == Packet.PacketState.Fly || buoyPacket.State == Packet.PacketState.Break)
            {
               LabelHelper.ShowLabels(false);
               // попытка следить за тормозящим буем
               _bouyCamera = VirtualCameraHelper.Activate("vcam_Buoy");
               VirtualCameraHelper.SetTarget(_bouyCamera, buoyPacket.Bopper.transform);
               buoyPacket.Trail.SetActive(false);
            }
         }
         else if (buoyPacket.transform.position.y < -buoyPacket.WorkingDepth/2 
            && VirtualCameraHelper.GetTarget(_bouyCamera) != buoyPacket.transform)
         {
            // переключаемся на ныряющий буй 
            VirtualCameraHelper.SetTarget(_bouyCamera, buoyPacket.transform);
         }
      }

   }

   private bool checkFinished()
   {
      if (Scenario.Instance.Buoys.Length == 0)
         return false;
      return Scenario.Instance.Buoys.Any(x => x.State == BuoyState.PreparingToWork);
   }
}

class PhaseBouysPreparingReady : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.BuoysOnPlace;
   public override string Title => "Буи готовятся";
   public override bool IsFinished => checkFinished();
   private CinemachineVirtualCamera _bouyCamera = null;
   private const float _delay = 2f;
   private float _timeStartWork = float.MaxValue;
   private Buoy targetBuoy => Scenario.Instance.Buoys.First();

   public override void Start()
   {
      _bouyCamera = VirtualCameraHelper.Find("vcam_Buoy");
      VirtualCameraHelper.SetTarget(_bouyCamera, targetBuoy.transform);
   }

   public override void Update()
   {
      if (_timeStartWork == float.MaxValue  && targetBuoy.State == BuoyState.Working)
         _timeStartWork = Scenario.Instance.ScenarioTime;
   }

   private bool checkFinished()
   {
      return Scenario.Instance.ScenarioTime - _delay > _timeStartWork;
   }
}

class PhaseBouysStartScan : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.BuoysStartScan;
   public override string Title => "Буи начали сканирование";
   public override bool IsFinished => _timeAllBuoysStartWork > 0 && Scenario.Instance.ScenarioTime > _timeAllBuoysStartWork + _delay;
   private float _delay = 3;
   private float _timeAllBuoysStartWork = -1;

   public override void Start() 
   {
      var cam = VirtualCameraHelper.Activate("vcam_TorpedoZone");
      VirtualCameraHelper.AddMemberToTargetGroup(cam, Scenario.Instance.TargetInfo.Target.transform, 2);

      foreach (var b in Scenario.Instance.Buoys)
         VirtualCameraHelper.AddMemberToTargetGroup(cam, b.transform, 1, VarSync.GetFloat(VarName.BuoysDetectRange));

      LabelHelper.ShowLabels(true);

      Utils.SetHeight(cam.transform, VarName.BuoysDetectRange.GetFloat());
   }
   public override void Update() 
   {
      var numWorkBuoys = Scenario.Instance.Buoys.Count(b => b.State == BuoyState.Working);
      if (numWorkBuoys == 1)
         Scenario.Instance.TargetDetectStatus = TargetDetectStatus.MPCAndBuoys;
      else if (numWorkBuoys > 1)
         Scenario.Instance.TargetDetectStatus = TargetDetectStatus.Buoys;

      if (_timeAllBuoysStartWork < 0 && Scenario.Instance.Buoys.All(b => b.State == BuoyState.Working))
         _timeAllBuoysStartWork = Scenario.Instance.ScenarioTime;
   }
}

class PhaseBouysTargetDetected : IScenarioPhase
{
   private BuoyGuard _bg;

   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.TargetDetectedByBuoys;
   public override string Title => "Цель запеленгована буями";
   public override bool IsFinished => _bg.IsTorpedoFinallyDetected;
   private CinemachineVirtualCamera _cam;

   public override void Start() 
   {
      _bg = GameObject.FindObjectOfType<BuoyGuard>();
      _cam = VirtualCameraHelper.Find("vcam_TorpedoZone");
      VirtualCameraHelper.ClearTargetGroup(_cam);
      foreach (var b in Scenario.Instance.Buoys)
         VirtualCameraHelper.AddMemberToTargetGroup(_cam, b.transform, 1, 100);


      LabelHelper.ShowLabels(true);
      _cam.Follow = _bg.DetectZone;
      _cam.transform.position = _bg.DetectZone.position;
      Utils.SetHeight(_cam.transform, 1000);
   }

   public override void Update() 
   {
      var zone = _bg.RealZone;
      if (zone == null)
         return;

      float radius = Math.Max((zone[0] - zone[2]).magnitude, (zone[1] - zone[3]).magnitude);
      VirtualCameraHelper.RemoveMemberFromTargetGroup(_cam, _bg.DetectZone);
      VirtualCameraHelper.AddMemberToTargetGroup(_cam, _bg.DetectZone, 1, radius);
   }
}

class PhaseLaunchRockets : IScenarioPhase
{
   private bool _rocketsMissed = false;

   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.MissilesLaunched;
   public override string Title => "Запуск ракет";
   public override bool IsFinished => checkFinished();
   private float _launchTime;
   private float _cameraDelay = 2;
   private bool _rocketsLaunched = false;
   CinemachineVirtualCamera _cam;

   public override void Start()
   {
      Scenario.Instance.Ship.Launcher.LaunchRockets();
      _rocketsLaunched = false;
      //    _cam = null;
      _cam = VirtualCameraHelper.Activate("vcam_Rockets");
      VirtualCameraHelper.SetTarget(_cam, Scenario.Instance.Ship.Launcher.transform);
   }

   public override void Update() 
   {
      if (Scenario.Instance.Ship.Launcher.IsAllRocketsLaunched)
      {
         if (!_rocketsLaunched)
         {
            VirtualCameraHelper.RemoveMemberFromTargetGroup(_cam, Scenario.Instance.Ship.Launcher.transform);
            VirtualCameraHelper.AddMemberToTargetGroup(_cam, Scenario.Instance.TargetInfo.Target.transform);
            _rocketsLaunched = true;
         }
      }
//       if (_cam == null && _rocketsLaunched && _launchTime + _cameraDelay < Scenario.Instance.ScenarioTime)
//       {
//          //          // убираем все из камеры и оставляем только ракеты и цель
//          //          VirtualCameraHelper.ClearTargetGroup(_cam);
//          //_cam = VirtualCameraHelper.Activate("vcam_Rockets");
//          foreach (var r in Scenario.Instance.Rockets)
//             VirtualCameraHelper.AddMemberToTargetGroup(_cam, r.transform);
//       }
//       if (!_rocketsLaunched && _rocketLauncher.IsAllRocketsLaunched)
//       {
//          _launchTime = Scenario.Instance.ScenarioTime;
//          _rocketsLaunched = true;
//       }

      if (Scenario.Instance.Ship.Launcher.IsAllRocketsExploded && Scenario.IsTargetAlive && !_rocketsMissed)
      {
         _rocketsMissed = true;
         Scenario.Instance.AddMessage("Ракеты не попали");
      }
      else if (Scenario.Instance.Ship.Launcher.IsAllRocketsExploded && !Scenario.IsTargetAlive)
      {
         Scenario.Instance.AddMessage("Ракеты попали");
      }
   }


   private bool checkFinished()
   {
      return Scenario.Instance.TargetInfo.Target.IsActive == false;
   }
}

static class VirtualCameraHelper
{
   public static void Activate(CinemachineVirtualCamera cam)
   {
      Debug.Log($"Activate camera {cam.name}");
      // up priority of cam and down priority of siblings
      for (int i = 0; i < cam.transform.parent.childCount; ++i)
      {
         if (cam.transform.parent.GetChild(i) == cam.transform)
            cam.m_Priority = 11;
         else 
         {
            var camSibl = cam.transform.parent.GetChild(i).GetComponent<CinemachineVirtualCamera>();
            if (camSibl != null && camSibl.m_Priority > 10)
            {
               Debug.Log($"Deactivate camera {camSibl.name}");
               camSibl.m_Priority = 10;
            }

         }
      }
      cam.MoveToTopOfPrioritySubqueue();
   }

   public static CinemachineVirtualCamera Activate(string camName)
   {
      var g = GameObject.Find(camName);
      if (g == null)
         return null;
      var cam = g.GetComponent<CinemachineVirtualCamera>();
      if (cam != null)
         Activate(cam);
      return cam;
   }


   public static CinemachineVirtualCamera Find(string name)
   {
      var o = GameObject.Find(name);
      return o != null ? o.GetComponent<CinemachineVirtualCamera>() : null;
   }

   public static Transform GetTarget(CinemachineVirtualCamera cam)
   {
      var lookAtGroup = cam.LookAt.GetComponent<CinemachineTargetGroup>();
      return lookAtGroup.m_Targets.Length > 0 ? lookAtGroup.m_Targets.First().target : null;
   }

   public static bool AddMemberToTargetGroup(CinemachineVirtualCamera cam, Transform t, float w = 1, float r = 1)
   {
      if (cam.LookAt == null)
      {
         Debug.LogError($"Null lookat  of camera {cam.name}");
         return false;
      }
         
      var lookAtGroup = cam.LookAt.GetComponent<CinemachineTargetGroup>();
      if (lookAtGroup == null)
      {
         Debug.LogError($"Can't get  CinemachineTargetGroup from lookat in camera {cam.name}");
         return false;
      }

      Debug.Log($"Add {t.name} to target group of camera {cam.name}");

      int index = lookAtGroup.FindMember(t);
      if (index != -1)
      {
         lookAtGroup.m_Targets[index].weight = w;
         lookAtGroup.m_Targets[index].radius = r;
      }
      else
         lookAtGroup.AddMember(t, w, r);

      return true;
   }

   public static bool SetTarget(CinemachineVirtualCamera cam, Transform t, float w = 1, float r = 1)
   {
      ClearTargetGroup(cam);
      return AddMemberToTargetGroup(cam, t, w, r);
   }


   public static bool AddMemberToTargetGroup(string name, Transform t, float w = 1, float r = 1)
   {
      var cam = Find(name);
      return cam != null ? AddMemberToTargetGroup(cam, t, w, r) : false;
   }

   public static void RemoveMemberFromTargetGroup(CinemachineVirtualCamera cam, Transform t)
   {
      var lookAtGroup = cam.LookAt.GetComponent<CinemachineTargetGroup>();
      if (lookAtGroup == null)
      {
         Debug.LogError($"Can't get  CinemachineTargetGroup from lookat in camera {cam.name}");
         return;
      }

      //Debug.Log($"Remove {t.name} to target group of camera {cam.name}");
      lookAtGroup.RemoveMember(t);
   }

   public static void ClearTargetGroup(CinemachineVirtualCamera cam)
   {
      var lookAtGroup = cam.LookAt.GetComponent<CinemachineTargetGroup>();
      foreach (var t in lookAtGroup.m_Targets.ToList())
         lookAtGroup.RemoveMember(t.target);
   }

}