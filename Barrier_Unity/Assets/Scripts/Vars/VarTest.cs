using UnityEngine;

public class VarTest : MonoBehaviour
{
   // Start is called before the first frame update
   void Start()
   {
      AttributeHelper.SerialzeToYaml("settings.yaml");
//       AttributeHelper.DeserializeFromYaml("settings.yaml");
// 
//       VarSync.OnVariableUpdate += onVariableUpdate;
// 
//       VarSync.Set(VarName.SOME_FLOAT, 0.5f);
   }

   private void onVariableUpdate(VarName v, object value)
   {
   }

   // Update is called once per frame
   void Update()
   {

   }
}
