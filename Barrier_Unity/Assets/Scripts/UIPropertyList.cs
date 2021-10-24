using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPropertyList : MonoBehaviour
{
   [SerializeField] PropCategory Category;
   void Start()
   {
      GameObject numericPrefab = Resources.Load("TemplateNumericProperty") as GameObject;
      GameObject togglePrefab = Resources.Load("TemplateToggleProperty") as GameObject;
      GameObject dropdownPrefab = Resources.Load("TemplateDropdownProperty") as GameObject;
      foreach (UIProperty prop in ScenarioProperties.GetProps())
      {
         GameObject instance = null;
         if (prop is NumericProperty np)
         {
            instance = Instantiate(numericPrefab, transform.position, transform.rotation);
            Text tu = instance.transform.Find("Units").GetComponent<Text>();
            tu.text = np.Units;

            InputField inp = instance.transform.Find("Input").GetComponent<InputField>();
            inp.text = VarSync.GetFloat(np.Var).ToString();
         }
         else if (prop is ToggleProperty tp)
            instance = Instantiate(togglePrefab, transform.position, transform.rotation);
         else if (prop is DropdownProperty dp)
         {
            instance = Instantiate(dropdownPrefab, transform.position, transform.rotation);
            Dropdown d = instance.transform.Find("Dropdown").GetComponent<Dropdown>();
            d.value = VarSync.GetInt(dp.Var);
            d.AddOptions(new List<string>(dp.Options));
         }

         Text t = instance.transform.Find("Text").GetComponent<Text>();
         t.text = prop.Title;
         instance.GetComponent<VarLinkUI>().V = prop.Var;
         instance.transform.SetParent(transform);
      }

   }

   // Update is called once per frame
   void Update()
   {

   }
}
