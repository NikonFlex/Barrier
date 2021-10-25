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
      yield return new WaitForSeconds(VarSync.GetFloat(VarName.BuoyReadyTime));
      _state = BuoyState.Working;
      print($"bouy '{gameObject.name}' start work");
      FindObjectOfType<BuoyGuard>().AddBuoy(this);
      yield return null;
   }
}
