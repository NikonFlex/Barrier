using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public enum ScenarioPhaseState
{
   Idle, // цель не обнаружена.
   TargetDetectedByAntenna,// - цель обнаружена антенной
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
}

public class ScnenarioPhaseStub : IScenarioPhase
{
   public ScnenarioPhaseStub(ScenarioPhaseState state, string title, float duration)
   {
      _scenarioState = state;
      _title = title;
      _duration = duration;
   }
   public override void Start() => _startTime = Scenario.Instance.ScenarioTime;
   public override ScenarioPhaseState ScenarioState => _scenarioState;
   public override string Title => _title;
   public override bool IsFinished => Scenario.Instance.ScenarioTime > _startTime + _duration;

   private readonly string _title;
   private readonly float _duration;
   private float _startTime;
   private ScenarioPhaseState _scenarioState;
}

public class TargetInfo
{
   public float Bearing = 0;
   public float Distance = -1;
   public Vector3 TargetPos;
}

public class Scenario : MonoBehaviour
{
   [SerializeField] Transform _ship;
   [SerializeField] Transform _torpedo;
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

   public void OnPacketLaunch(Packet p) => _buoyPackets.Add(p);
   public void OnBouyHatched(Buoy b) => _buoys.Add(b);

   public void StartScenario()
   {
      setUpTargetPosition();
      _currentTime = _startTime = Time.time;
      _currentMode = Mode.Running;
      _currentPhaseIndex = 0;
      currentPhase.Start();
   }

   private void setUpTargetPosition()
   {
      float bearing = VarSync.GetFloat(VarName.StartBearingToTarget);
      float distance = VarSync.GetFloat(VarName.StartDistanceToTarget);

      _torpedo.position = Quaternion.AngleAxis(bearing, Vector3.up) * _ship.forward * distance;
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


   void Awake() => _instance = this;

   void Start()
   {
      // add stub phases
      var phases = new List<IScenarioPhase>();
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.Idle, "Цель не обнаружена", 0.1f));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.TargetDetectedByAntenna, "Цель обнаружена МСЦ", 0.5f));
      phases.Add(new PhaseLaunchBouys());
      phases.Add(new PhaseBouysReady());
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.TargetDetectedByBuoys, "Цель запеленгована буями", 2));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.MissilesLaunched, "Ракеты выпущены", 2));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.MissilesStrike, "Ракеты достигли цели", 2));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.ScenarioFinished, "Упражнение окончено", 2));
      
      _phases = phases.ToArray();
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

      if (!currentPhase.IsFinished)
         return;

      int nextIndex = (int)_currentPhaseIndex + 1;
      if (nextIndex >= _phases.Length)
      {
         _currentMode = Mode.Finished;
         return;
      }
      _currentPhaseIndex = nextIndex;
      currentPhase.Start();
   }

   private TargetInfo calcTargetInfo()
   {
      return new TargetInfo()
      {
         Distance = (_ship.position - _torpedo.position).magnitude,
         // TODO: use ship direction and 
         TargetPos = _torpedo.position,
         Bearing = Vector3.SignedAngle(_ship.forward, (_torpedo.position - transform.position).normalized, Vector3.up)
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

class PhaseLaunchBouys : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.BuoysLaunched;
   public override string Title => "Буи выстрелили";
   public override bool IsFinished => checkFinished();

   public override void Start()
   {
      GameObject.FindObjectOfType<BuoyLauncher>().LaunchBuouys();
   }

   private bool checkFinished()
   {
      if (Scenario.Instance.BuoyPackets.Length == 0)
         return false;
      return Scenario.Instance.BuoyPackets.All(p => p.State == PacketState.OnWater);
   }
}


class PhaseBouysReady : IScenarioPhase
{
   public override ScenarioPhaseState ScenarioState => ScenarioPhaseState.BuoysOnPlace;
   public override string Title => "Буи готовятся";
   public override bool IsFinished => checkFinished();

   public override void Start()
   {
   }

   private bool checkFinished()
   {
      if (Scenario.Instance.Buoys.Length == 0)
         return false;

      return Scenario.Instance.Buoys.All(b => b.State == BuoyState.Working);
   }
}
