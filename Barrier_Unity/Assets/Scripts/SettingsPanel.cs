using UnityEngine;

public class SettingsPanel : MonoBehaviour
{
   // Start is called before the first frame update
   public void ApplySetings()
   {
      AttributeHelper.SerialzeToYaml("settings.yaml");
   }

   public void ClosePanel()
   {
      gameObject.SetActive(false);
   }

}
