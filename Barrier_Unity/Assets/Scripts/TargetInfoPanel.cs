using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TargetInfoPanel : MonoBehaviour
{
   [SerializeField] TMP_Text _distanceText;
   [SerializeField] TMP_Text _bearingText;
   void Start()
   {

   }

   // Update is called once per frame
   void Update()
   {
      var t = Scenario.Instance.TargetInfo;
      if (t != null)
      {
         _distanceText.SetText($"���������: {t.Distance:0.#} �");
         _bearingText.SetText($"������: {t.Bearing:0.#�}");
      }
   }
}
