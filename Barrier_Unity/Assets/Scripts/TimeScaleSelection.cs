using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TimeScaleSelection : MonoBehaviour
{
   void Start()
   {
      var dd = GetComponent<TMP_Dropdown>();
      Time.timeScale = float.Parse(dd.options[dd.value].text);
      dd.onValueChanged.AddListener(delegate
      {
         dropdownValueChanged(dd);
      });
   }

   void dropdownValueChanged(TMP_Dropdown change) => 
      Time.timeScale = float.Parse(change.options[change.value].text);
}
