using System.Collections.Generic;
using UnityEngine;

public enum PropCategory
{
   Scenario,
   Common
}

public class UIProperty
{
   public VarName Var;
   public string Title;
};

public class NumericProperty : UIProperty
{
   public NumericProperty(VarName v, string t, string u, float val)
   {
      Var = v;
      Title = t;
      Units = u;
      Val = val;
   }
   public string Units;
   public float Val;
}

public class ToggleProperty : UIProperty
{
   public ToggleProperty(VarName v, string t, bool val)
   {
      Var = v;
      Title = t;
      Val = val;
   }
   public bool Val;
}

public class DropdownProperty : UIProperty
{
   public DropdownProperty(VarName v, string t, string[] options)
   {
      Var = v;
      Title = t;
      Options = options;
   }
   public string[] Options;
}

static public class ScenarioProperties
{
   static private UIProperty[] _props = null;
//   private static readonly UIProperty[] props = new UIProperty[]
//   {
//          new NumericProperty( "Расстояние до цели", "м"),
//          new NumericProperty( "Пеленг на цель", "м"),
//          new ToggleProperty( "МСЦ, использовать"),
//          new NumericProperty( "МСЦ, дальность", "м"),
//          new DropdownProperty( "Погода", new string[] { "Штиль", "Шторм" }),
//          new DropdownProperty( "Количество буев", new string[] { "1", "2", "3", "4", "5", "6"}),
//          new NumericProperty( "Погрешность постановки буя", "м"),
//          new NumericProperty( "Погрешность пеленга буя", "град")
//   };

   public static UIProperty[] GetProps()
   {
      if (_props == null)
         createProperties();
      return _props;
   }

   private static void createProperties()
   {
      var propList = new List<UIProperty>();
      for (VarName varName = VarName.UNDEFINED; varName < VarName.VARSYNC_LAST; varName++)
      {
         UIProperty prop = createPropertyFromVariable(varName);
         if (prop != null)
            propList.Add(prop);
      }
      _props = propList.ToArray();
   }

   private static UIProperty createPropertyFromVariable(VarName vn)
   {
      if (!vn.IsPerist())
         return null;
      switch(vn.GetVarType())
      {
         case VarType.Float:
            return new NumericProperty(vn, vn.GetDisplayText(), vn.GetUnits(), VarSync.GetFloat(vn));
         case VarType.Bool:
            return new ToggleProperty(vn, vn.GetDisplayText(), VarSync.GetBool(vn));
         case VarType.Enum:
            return new DropdownProperty(vn, vn.GetDisplayText(), vn.GetVariants());
         default:
            Debug.LogError($"Can't create UIProperty for {vn}");
         return null;
      }
   }
};


// static public class CommonProperties
// {
//    static public UIProperty[] Props = new UIProperty[]
//    {
//          new NumericProperty( "Точность пеленга антенны", "град"),
//          new NumericProperty( "Высота раскрытия парашюта", "м")
//    };
// }
