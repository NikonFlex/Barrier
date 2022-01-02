using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BuoyState
{
   None,
   PreparingToWork,
   Working
}

public class Buoy : MonoBehaviour
{
   public BuoyState State => _state;
   private BuoyState _state = BuoyState.None;
   void Start()
   {
      Scenario.Instance.OnBouyHatched(this);
   }

   // Update is called once per frame
   void Update()
   {
      if (_state == BuoyState.None)
         StartCoroutine(preparingToWork());
   }

   IEnumerator preparingToWork()
   {
      _state = BuoyState.PreparingToWork;
      float startTime = Scenario.Instance.ScenarioTime;
      Scenario.Instance.AddMessage($"Буй '{gameObject.name}' готовится");
      while (Scenario.Instance.ScenarioTime - startTime < VarSync.GetFloat(VarName.BuoyReadyTime))
         yield return null;
      _state = BuoyState.Working;
      Scenario.Instance.AddMessage($"Буй '{gameObject.name}' начал сканирование");
      FindObjectOfType<BuoyGuard>().AddBuoy(this);
      yield return null;
   }
}
