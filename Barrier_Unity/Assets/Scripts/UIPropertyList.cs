using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UIPropertyList : MonoBehaviour
{
   [SerializeField] PropCategory Category;
   void Start()
   {
      GameObject numericPrefab = Resources.Load<GameObject>("TemplateNumericProperty");
      GameObject togglePrefab = Resources.Load<GameObject>("TemplateToggleProperty");
      GameObject dropdownPrefab = Resources.Load<GameObject>("TemplateDropdownProperty");
      foreach (UIProperty prop in ScenarioProperties.GetProps())
      {
         GameObject instance = null;
         if (prop is NumericProperty np)
         {
            instance = Instantiate(numericPrefab, transform.position, transform.rotation, transform);
            TextMeshProUGUI tu = instance.transform.Find("Units").GetComponent<TextMeshProUGUI>();
            tu.text = np.Units;

            TMP_InputField inp = instance.transform.Find("Input").GetComponent<TMP_InputField>();
            inp.text = VarSync.GetFloat(np.Var).ToString();
            }
         else if (prop is ToggleProperty tp)
            instance = Instantiate(togglePrefab, transform.position, transform.rotation, transform);
         else if (prop is DropdownProperty dp)
         {
            instance = Instantiate(dropdownPrefab, transform.position, transform.rotation, transform);
            TMP_Dropdown d = instance.transform.Find("Dropdown").GetComponent<TMP_Dropdown>();
            d.value = VarSync.GetInt(dp.Var);
            d.AddOptions(new List<string>(dp.Options));
         }

         TextMeshProUGUI t = instance.transform.Find("Text").GetComponent<TextMeshProUGUI>();
         t.text = prop.Title;
         instance.GetComponent<VarLinkUI>().V.Name = prop.Var.ToString();
      }

   }

   // Update is called once per frame
   void Update()
   {

   }
}
