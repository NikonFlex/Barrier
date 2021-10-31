using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioLog : MonoBehaviour
{
   private static ScenarioLog _instance;
   private string _log;
   private TMPro.TMP_Text _last_message;

   public static ScenarioLog Instance => _instance;

   private void Awake() => _instance = this;

   private void Start()
   {
      _last_message = GetComponent<TMPro.TMP_Text>();
   }
   public void AddMessage(string message)
   {
      _log += string.Format("{0:0.00}", Scenario.Instance.ScenarioTime) + " " + message + "\n";
      _last_message.text = _log;
   }
}
