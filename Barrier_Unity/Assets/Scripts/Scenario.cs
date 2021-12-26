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
   [SerializeField] Transform _ship;
   [SerializeField] Torpedo _torpedo;
   [SerializeField] private CameraController m_cameraController;
   [SerializeField] private ScenarioLog _log;

   private List<Packet> _buoyPackets = new List<Packet>();
   private List<Buoy> _buoys = new List<Buoy>();


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
   public bool IsRunning => isRunning;
   public TargetInfo TargetInfo => isRunning ? calcTargetInfo() : null;
   public Packet[] BuoyPackets => _buoyPackets.ToArray();
   public Buoy[] Buoys => _buoys.ToArray();

   public void OnPacketLaunch(Packet p)
   {
      _buoyPackets.Add(p);
      LabelHelper.AddLabel(p.gameObject, $"Буй {_buoyPackets.Count}" );
      VirtualCameraHelper.AddMemberToTargetGroup("vcam_Launch", p.transform);
   }
   public void OnBouyHatched(Buoy b) => _buoys.Add(b);

   public void StartScenario()
   {
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

      _torpedo.transform.position = Quaternion.AngleAxis(bearing, Vector3.up) * _ship.forward * distance;
   }

   private void setUpShipSettings()
   {
      _ship.GetComponent<Ship>().SetUpMscSettings(VarSync.GetBool(VarName.MSC_USE), VarSync.GetFloat(VarName.MSC_DISTANCE));
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
      // add stub phases
      var phases = new List<IScenarioPhase>();
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.Idle, "Цель не обнаружена", 1f, _ship));
      phases.Add(new PhaseTargetDetected());
      phases.Add(new PhaseLaunchBouys());
      phases.Add(new PhaseBouysPreparingReady());
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.BuoysStartScan, "Начало сканирования буями", 2f));
      phases.Add(new PhaseBouysTargetDetected());
      phases.Add(new PhaseLaunchRockets());
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.MissilesStrike, "Ракеты достигли цели", 2));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.ScenarioFinished, "Упражнение окончено", 2));
      
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
      currentPhase.Start();
      AddMessage(currentPhase.Title);

   }

   private TargetInfo calcTargetInfo()
   {
      return new TargetInfo()
      {
         Distance = (_ship.position - _torpedo.transform.position).magnitude,
         // TODO: use ship direction and 
         Target = _torpedo,
         Bearing = Vector3.SignedAngle(_ship.forward, (_torpedo.transform.position - transform.position).normalized, Vector3.up)
      };
   }

   private IScenarioPhase currentPhase => _phases[(int)_currentPhaseIndex];
   private bool isRunning => _currentMode == Mode.Running;
   private bool isAlive => _currentMode != Mode.Stoped && _currentMode != Mode.Finished;

   private IScenarioPhase[] _phases;
   private Mode _currentMode = Mode.Stoped;
   private float _startTime;
   private float _currentTime;
   private int _currentPhaseIndex;

   private static Scenario _instance;
}

class PhaseTargetDetected : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.TargetDetectedByAntenna;
   public override string Title => "Цель обнаружена МСЦ";
   public override bool IsFinished => Scenario.Instance.ScenarioTime > _startTime + _duration;
   private float _startTime;
   private float _duration = 2f;

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
   public override string Title => "Буи выстрелили";
   public override bool IsFinished => checkFinished();
   public float _delay = 2;
   private float _startTime;
   private CinemachineVirtualCamera _camera = null;

   public override void Start()
   {
      _startTime = Scenario.Instance.ScenarioTime;
      GameObject.FindObjectOfType<BuoyLauncher>().LaunchBuouys();
   }
   public override void Update() 
   {
      if (_startTime + _delay >= Scenario.Instance.ScenarioTime)
      {
         if (!_camera)
            _camera = VirtualCameraHelper.Activate("vcam_Launch");

      }
   }

   private bool checkFinished()
   {
      if (Scenario.Instance.BuoyPackets.Length == 0)
         return false;
      return Scenario.Instance.BuoyPackets.All(p => p.State == PacketState.OnWater);
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
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.TargetDetectedByBuoys;
   public override string Title => "Цель запеленгована буями";
   public override bool IsFinished => checkFinished();

   private BuoyGuard _bg;

   public override void Start()
   {
      _bg = GameObject.FindObjectOfType<BuoyGuard>();
      GameObject.FindObjectOfType<CameraController>().FollowObject(_bg.DetectZone, 2000);
   }
   public override void Update() {}


   private bool checkFinished()
   {
      return _bg.ScanningError < 0.1f;
   }
}

class PhaseLaunchRockets : IScenarioPhase
{
   private RocketLauncher _rocketLauncher;
   private CameraController m_cameraController;

   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.MissilesLaunched;
   public override string Title => "Запуск ракет";
   public override bool IsFinished => checkFinished();

   public override void Start()
   {
      _rocketLauncher = GameObject.FindObjectOfType<RocketLauncher>();
      _rocketLauncher.LaunchRockets();

      m_cameraController = GameObject.FindObjectOfType<CameraController>();
      m_cameraController.ChangeView(ViewType.Torpedo);
   }
   public override void Update() { }


   private bool checkFinished()
   {
      return _rocketLauncher.IsAllRocketsExploded();
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

   public static bool AddMemberToTargetGroup(string name, Transform t, float w = 1, float r = 1)
   {
      var cam = Find(name);
      return cam != null ? AddMemberToTargetGroup(cam, t, w, r) : false;
   }
}

static class LabelHelper
{

   public static ScreenLabel AddLabel(GameObject o, string text)
   {
      GameObject labelPrefab = Resources.Load("ObjectLabel") as GameObject;
      var label = GameObject.Instantiate(labelPrefab, markersGroup).GetComponent<ScreenLabel>();
      label.name = o.name + "_label";
      label.target = o.transform;
      label.LabelText = text;
      Debug.Log($"Add label '{label.name}' to object '{o.name}'");
      return label;
   }

   private static Transform markersGroup => GameObject.Find("ObjectMarkers").transform;

}