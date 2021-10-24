using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum ScenarioPhaseState
{
   Idle, // цель не обнаружена.
   TargetDetectedByAntenna,// - цель обнаружена антенной
   BuoysLaunched,// - Буи выпущены
   BuoysOnPlace,  // - Буи приводнились
   BuoysStartScan, // - Начало сканирования буями
   TargetDetectedByBuoys, // - Цель обнаружена буями
   MissilesLaunched, // - Ракеты выпущены
   MissilesStrike //- Ракеты взорвались
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

   public enum Mode
   {
      Stoped,
      Paused,
      Running,
      Finished
   }

   public static Scenario Instance => _instance;
   public ScenarioPhaseState State => _currentState;
   public Mode CurrentMode => _currentMode;
   public float ScenarioTime => _currentTime - _startTime;
   public IScenarioPhase CurrentPhase => currentPhase;
   public bool IsAlive => isAlive;
   public bool IsRunning => isRunning;
   public TargetInfo TargetInfo => isRunning ? calcTargetInfo() : null;

   public void StartScenario()
   {
      _currentTime = _startTime = Time.time;
      _currentMode = Mode.Running;
      _currentState = ScenarioPhaseState.Idle;
      currentPhase.Start();
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
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.Idle, "Цель не обнаружена", 3));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.TargetDetectedByAntenna, "Цель обнаружена МСЦ", 3));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.BuoysLaunched, "Буи выстрелили", 5));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.BuoysOnPlace, "Буи приводнились", 5));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.BuoysStartScan, "Буи в работе", 3));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.TargetDetectedByBuoys, "Цель запеленгована буями", 2));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.MissilesLaunched, "Ракеты выпущены", 2));
      phases.Add(new ScnenarioPhaseStub(ScenarioPhaseState.MissilesStrike, "Ракеты достигли цели", 2));
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
      VarSync.Set(VarName.TargetBearing, trg != null ? trg.Bearing : 0f );
      VarSync.Set(VarName.TargetDistance, trg != null ? trg.Distance : 0f);

      _currentTime += Time.deltaTime;

      if (!currentPhase.IsFinished)
         return;

      int nextState = (int)_currentState + 1;
      if (nextState >= Enum.GetNames(typeof(ScenarioPhaseState)).Length)
      {
         _currentMode = Mode.Finished;
         return;
      }
      _currentState = (ScenarioPhaseState)(nextState);
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

   private IScenarioPhase currentPhase => _phases[(int)_currentState];
   private bool isRunning=> _currentMode == Mode.Running;
   private bool isAlive => _currentMode != Mode.Stoped && _currentMode != Mode.Finished;

   private IScenarioPhase[] _phases;
   private Mode _currentMode = Mode.Stoped;
   private float _startTime;
   private float _currentTime;
   private ScenarioPhaseState _currentState;

   private static Scenario _instance;
}
