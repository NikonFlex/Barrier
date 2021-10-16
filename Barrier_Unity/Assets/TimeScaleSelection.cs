using UnityEngine;
using UnityEngine.UI;

public class TimeScaleSelection : MonoBehaviour
{
   void Start()
   {
      GetComponent<Dropdown>().onValueChanged.AddListener(delegate
      {
         dropdownValueChanged(GetComponent<Dropdown>());
      });
   }

   void dropdownValueChanged(Dropdown change) => 
      Time.timeScale = float.Parse(change.options[change.value].text);
}
