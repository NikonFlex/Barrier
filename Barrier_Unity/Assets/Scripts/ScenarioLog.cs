using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScenarioLog : MonoBehaviour
{
   private string _log;
   [SerializeField] TMPro.TMP_Text _last_message;

   public void AddMessage(string message)
   {
      _log += string.Format("{0:0.00}", Scenario.Instance.ScenarioTime) + "  " + message + "\n";
      _last_message.text = _log;
   }
}
