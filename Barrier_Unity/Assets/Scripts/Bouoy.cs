using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BouoyState
{
   None,
   PreparingToWork,
   Working
}

public class Bouoy : MonoBehaviour
{
   public BouoyState State => _state;
   private BouoyState _state = BouoyState.None;
   void Start()
   {
      Scenario.Instance.OnBouyHatched(this);
   }

   // Update is called once per frame
   void Update()
   {
      if (_state == BouoyState.None)
         StartCoroutine(preparingToWork());
   }

   IEnumerator preparingToWork()
   {
      _state = BouoyState.PreparingToWork;
      yield return new WaitForSeconds(VarSync.GetFloat(VarName.BuoyReadyTime));
      _state = BouoyState.Working;
      print("bouy start work");
      BouoyGuard bouoy_guard = FindObjectOfType<BouoyGuard>();
      if (bouoy_guard.GetComponent<BouoyGuard>().IsBouoy1Null() == true)
         bouoy_guard.GetComponent<BouoyGuard>().SetBouoy1(gameObject);
      else if (bouoy_guard.GetComponent<BouoyGuard>().IsBouoy2Null() == true)
         bouoy_guard.GetComponent<BouoyGuard>().SetBouoy2(gameObject);
      yield return null;
   }
}
