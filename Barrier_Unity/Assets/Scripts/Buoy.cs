using System;
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

   private float m_errorBeg;
   private float m_errorEnd;
   private float m_errorTime;

   public static float GetBearingError()
   {
      if (VarSync.GetInt(VarName.Weather) == 0)
         return VarSync.GetFloat(VarName.BuoysBearingError);
      else
         return VarSync.GetFloat(VarName.BuoysBearingError) * VarSync.GetFloat(VarName.BuoysBearingMultplier);
   }

   void Start()
   {
      Scenario.Instance.OnBouyHatched(this);

      UnityEngine.Random.InitState(DateTime.UtcNow.GetHashCode());

      m_errorBeg = 0;
      m_errorEnd = Utils.GaussRandom(GetBearingError() / 2f);

      m_errorTime = Time.time;
      StartCoroutine(timerCoroutine());
   }

   // Update is called once per frame
   void Update()
   {
      if (_state == BuoyState.None)
         StartCoroutine(preparingToWork());
   }

   public float Error => Mathf.Lerp(m_errorBeg, m_errorEnd, Time.time - m_errorTime);

   IEnumerator preparingToWork()
   {
      _state = BuoyState.PreparingToWork;
      float startTime = Scenario.Instance.ScenarioTime;
      Scenario.Instance.AddMessage($"��� '{gameObject.name}' ���������");
      while (Scenario.Instance.ScenarioTime - startTime < VarSync.GetFloat(VarName.BuoyReadyTime))
         yield return null;
      _state = BuoyState.Working;
      gameObject.GetComponent<Packet>().StartPelleng();
      Scenario.Instance.AddMessage($"��� '{gameObject.name}' ����� ������������");
      FindObjectOfType<BuoyGuard>().AddBuoy(this);
      yield return null;
   }

   IEnumerator timerCoroutine()
   {
      while (true)
      {
         yield return new WaitForSeconds(1);

         m_errorBeg = m_errorEnd;
         m_errorEnd = Utils.GaussRandom(GetBearingError() / 2f);
         m_errorTime = Time.time;
      }
   }
}
