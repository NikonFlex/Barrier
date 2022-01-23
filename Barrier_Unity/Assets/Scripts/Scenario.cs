using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Cinemachine;

public enum ScenarioPhaseState
{
   Idle, // цель не обнаружена.
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
   public Torpedo Target;
}

public class Scenario : MonoBehaviour
{
   [SerializeField] GameObject _ship;
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
   public Transform Ship => _ship.transform;

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
   public void OnBouyHatched(Buoy b) => _buoys.Add(b);

   public void OnRocketLaunched(Rocket r)
   {
      _rockets.Add(r);
      VirtualCameraHelper.AddMemberToTargetGroup("vcam_TorpedoZone", r.transform);
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
      _ship.transform.GetComponent<Ship>().SetUpMscSettings(VarSync.GetBool(VarName.MSC_USE), VarSync.GetFloat(VarName.MSC_DISTANCE));
      LabelHelper.ShowLabel(_ship);
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
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.Idle, "Цель не обнаружена", 1f, _ship.transform));
      phases.Add(new PhaseTargetDetected());
      phases.Add(new PhaseLaunchBouys());
      phases.Add(new PhaseBouysPreparingReady());
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.BuoysStartScan, "Начало сканирования буями", 2f));
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
      return new TargetInfo()
      {
         Distance = (_ship.transform.position - _torpedo.transform.position).magnitude,
         // TODO: use ship direction and 
         Target = _torpedo,
         Bearing = Vector3.SignedAngle(_ship.transform.forward, (_torpedo.transform.position - transform.position).normalized, Vector3.up)
      };
   }
}


class PhaseTargetDetected : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.TargetDetectedByAntenna;
   public override string Title => "Цель обнаружена МСЦ";
   public override bool IsFinished => Scenario.Instance.ScenarioTime > _startTime + _duration;
   private float _startTime;
   private float _duration = 1f;

   public override void Start()
   {
      _startTime = Scenario.Instance.ScenarioTime;
      var camera = VirtualCameraHelper.Activate("vCam_ShipGroup");
      VirtualCameraHelper.AddMemberToTargetGroup(camera, Scenario.Instance.TargetInfo.Target.transform);
   }
   public override void Update() { }
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
   enum BuoyLifeCicle
   {
      Nap,
      Fly,
      Break,
      PrepapreFloat,
      Diving,
      PrepareWork,
      Working
   }
   private BuoyLifeCicle _firstBuoyLifeCicle = BuoyLifeCicle.Nap;

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
         _firstBuoyLifeCicle = BuoyLifeCicle.Fly;
      }
      else
      {
         var buoyPacket = Scenario.Instance.BuoyPackets.First();
         if (_allBouysLaunchedTime + _delay2 < Scenario.Instance.ScenarioTime
               && _bouyCamera == null
               //&& _buoyCameraHeight > buoyPacket.transform.position.y)
               && buoyPacket.CalcTimeToTarget() < 3 )
         {
            if (_firstBuoyLifeCicle == BuoyLifeCicle.Fly)
            {
               LabelHelper.ShowLabels(false);
               _firstBuoyLifeCicle = BuoyLifeCicle.Break;
               // попытка следить за тормозящим буем
               _bouyCamera = VirtualCameraHelper.Activate("vcam_Buoy");
               VirtualCameraHelper.SetTarget(_bouyCamera, buoyPacket.Bobber);
               buoyPacket.Trail.SetActive(false);
            }
         }
         else if (buoyPacket.transform.position.y < -40 && _firstBuoyLifeCicle != BuoyLifeCicle.Diving)
         {
            _firstBuoyLifeCicle = BuoyLifeCicle.Diving;
            VirtualCameraHelper.SetTarget(_bouyCamera, buoyPacket.transform);
         }
      }

   }

   private bool checkFinished()
   {
      if (Scenario.Instance.BuoyPackets.Length == 0)
         return false;
      return Scenario.Instance.BuoyPackets.All(p => p.IsOnWater);
   }
}

class PhaseBouysPreparingReady : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.BuoysOnPlace;
   public override string Title => "Буи готовятся";
   public override bool IsFinished => checkFinished();

   public override void Start(){}
   public override void Update(){}

   private bool checkFinished()
   {
      if (Scenario.Instance.Buoys.Length == 0)
         return false;

      return Scenario.Instance.Buoys.All(b => b.State == BuoyState.Working);
   }
}

class PhaseBouysTargetDetected : IScenarioPhase
{
   private BuoyGuard _bg;

   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.TargetDetectedByBuoys;
   public override string Title => "Цель запеленгована буями";
   public override bool IsFinished => checkFinished();


   public override void Start()
   {
      _bg = GameObject.FindObjectOfType<BuoyGuard>();
      var cam = VirtualCameraHelper.Activate("vcam_TorpedoZone");
      var zone = _bg.RealZone;
      float radius = Math.Max((zone[0] - zone[2]).magnitude, (zone[1] - zone[3]).magnitude);
      VirtualCameraHelper.AddMemberToTargetGroup(cam, _bg.DetectZone, 1, radius);

      foreach (var b in _bg.Bouys)
         VirtualCameraHelper.AddMemberToTargetGroup(cam, b.transform);

      LabelHelper.ShowLabels(true);
   }
   public override void Update() {}


   private bool checkFinished() => _bg.ScanningError < 0.1f;
}

class PhaseLaunchRockets : IScenarioPhase
{
   private RocketLauncher _rocketLauncher;
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
      _rocketLauncher = GameObject.FindObjectOfType<RocketLauncher>();
      _rocketLauncher.LaunchRockets();
      VirtualCameraHelper.AddMemberToTargetGroup("vcam_TorpedoZone", Scenario.Instance.Ship);
      _rocketsLaunched = false;
      _cam = null;
   }
   public override void Update() 
   {
      if (_cam == null && _rocketsLaunched && _launchTime + _cameraDelay < Scenario.Instance.ScenarioTime)
      {
         //          // убираем все из камеры и оставляем только ракеты и цель
         //          VirtualCameraHelper.ClearTargetGroup(_cam);
         _cam = VirtualCameraHelper.Activate("vcam_Rockets");
         foreach (var r in Scenario.Instance.Rockets)
            VirtualCameraHelper.AddMemberToTargetGroup(_cam, r.transform);
      }
      if (!_rocketsLaunched && _rocketLauncher.IsAllRocketsLaunched)
      {
         _launchTime = Scenario.Instance.ScenarioTime;
         _rocketsLaunched = true;
      }

      if (_rocketLauncher.IsAllRocketsExploded && Scenario.Instance.TargetInfo.Target.IsActive && !_rocketsMissed)
      {
         _rocketsMissed = true;
         Scenario.Instance.AddMessage("Ракеты не попали");
      }
      else if (_rocketLauncher.IsAllRocketsExploded && !Scenario.Instance.TargetInfo.Target.IsActive)
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