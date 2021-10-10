using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ScenarioPanel : MonoBehaviour
{
   // Start is called before the first frame update
   [SerializeField] TMP_Text _timeText;
   [SerializeField] TMP_Text _phaseText;
   void Start()
   {
   }

   // Update is called once per frame
   void Update()
   {
      _timeText.SetText($"Время: {Scenario.Instance.ScenarioTime:0.#}");
      _phaseText.SetText(Scenario.Instance.CurrentPhase.Title);
   }
}
