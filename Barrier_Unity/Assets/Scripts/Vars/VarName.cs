using UnityEngine;

public enum VarName
{
   [VarName(hideInInspector = true)]
   UNDEFINED,

   [VarName(displayText = "Моя переменная", persist = true)]
   SOME_VAR_1,

   [VarName(displayText = "", persist = true)]
   SOME_VAR_2,

   [VarName(displayText = "Some float", persist = true, defaultValue = 0.33f)]
   SOME_FLOAT,

   [VarName(defaultValue = "some string")]
   SOME_VAR_3,    

   
   VARSYNC_LAST
};
