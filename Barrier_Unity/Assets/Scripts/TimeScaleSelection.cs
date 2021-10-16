using UnityEngine;
using UnityEngine.UI;

public class TimeScaleSelection : MonoBehaviour
{
   void Start()
   {
      var dd = GetComponent<Dropdown>();
      Time.timeScale = float.Parse(dd.options[dd.value].text);
      dd.onValueChanged.AddListener(delegate
      {
         dropdownValueChanged(dd);
      });
   }

   void dropdownValueChanged(Dropdown change) => 
      Time.timeScale = float.Parse(change.options[change.value].text);
}
