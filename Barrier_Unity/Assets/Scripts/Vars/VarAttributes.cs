using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using UnityEngine;

public class VarNamePersistAttribute
{
   public string DisplayText;
   public object DefaultValue;
}

public class VarNamePersist
{
   public Dictionary<VarName, VarNamePersistAttribute> Data;
};

public enum VarType
{
   Default = 0,
   Float,
   FloatArray,
   String,
   Bool,
   Enum
};

[AttributeUsage(AttributeTargets.Field)]
public class VarNameAttribute : Attribute
{
   public string DisplayText;
   public object DefaultValue;
   public float Precision = 0;
   public float FormatPrecision = 0;
   public bool HideInInspector;
   public bool Persist;
   public string Units;
   public VarType Vt = VarType.Default;
   public string[] Variants;
};

public static class AttributeHelper
{
   private static VarNamePersist m_Persist = new VarNamePersist();

   static AttributeHelper()
   {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

      m_Persist.Data = new Dictionary<VarName, VarNamePersistAttribute>();

      for (VarName varName = VarName.UNDEFINED; varName < VarName.VARSYNC_LAST; varName++)
      {
         VarNameAttribute va = getAttributes(varName);
         if (va == null || va.Persist == false)
            continue;

         VarNamePersistAttribute vp = new VarNamePersistAttribute
         {
            DisplayText = va.DisplayText,
            DefaultValue = va.DefaultValue
         };
         m_Persist.Data.Add(varName, vp);
      }
   }

   private static VarNameAttribute getAttributes(VarName varName)
   {
      object[] attrs = typeof(VarName).GetMember(varName.ToString()).First().GetCustomAttributes(typeof(VarNameAttribute), false);

      if (attrs.Length == 0)
         return null;

      var attr = attrs[0] as VarNameAttribute;
      if (attr.Vt == VarType.Default && attr.DefaultValue != null)
      {
         if (attr.DefaultValue is bool) attr.Vt = VarType.Bool;
         else if (attr.DefaultValue is int) attr.Vt = VarType.Float;
         else if (attr.DefaultValue is float) attr.Vt = VarType.Float;
         else if (attr.DefaultValue is string) attr.Vt = VarType.String;
         else if (attr.DefaultValue is float[]) attr.Vt = VarType.FloatArray;
      }

      return attr;
   }

   public static VarNameAttribute GetAttributes(this VarName varName) => getAttributes(varName);

   public static VarType GetVarType(this VarName varName)
   {
      VarNameAttribute va = getAttributes(varName);
      return va == null ?  VarType.Default : va.Vt;
   }

   public static string GetDisplayText(this VarName varName)
   {
      VarNamePersistAttribute vp;
      if (!m_Persist.Data.TryGetValue(varName, out vp))
      {
         VarNameAttribute va = getAttributes(varName);
         if (va == null || string.IsNullOrEmpty(va.DisplayText))
            return varName.ToString();

         return va.DisplayText;
      }

      if (string.IsNullOrEmpty(vp.DisplayText))
         return varName.ToString();

      return vp.DisplayText;
   }

   public static void SetDisplayText(this VarName varName, string displayText)
   {
      VarNamePersistAttribute vp;
      if (!m_Persist.Data.TryGetValue(varName, out vp))
         return;

      vp.DisplayText = displayText;
   }

   public static object GetDefValue(this VarName varName)
   {
      VarNamePersistAttribute vp;
      if (!m_Persist.Data.TryGetValue(varName, out vp))
      {
         VarNameAttribute va = getAttributes(varName);
         if (va == null)
            return null;

         return va.DefaultValue;
      }

      return vp.DefaultValue;
   }

   public static float GetPrecision(this VarName varName)
   {
      VarNameAttribute va = getAttributes(varName);
      if (va == null)
         return 0;

      return va.Precision;
   }

   public static float GetFormatPrecision(this VarName varName)
   {
      VarNameAttribute va = getAttributes(varName);
      if (va == null)
         return 0;

      return va.FormatPrecision;
   }

   public static bool IsHideInInspector(this VarName varName)
   {
      VarNameAttribute va = getAttributes(varName);
      if (va == null)
         return false;

      return va.HideInInspector;
   }

   public static bool IsPerist(this VarName varName)
   {
      VarNameAttribute va = getAttributes(varName);
      if (va == null)
         return false;

      return va.Persist;
   }
   public static bool SerialzeToYaml(string path)
   {
      try
      {
         System.Yaml.Serialization.YamlSerializer yaml = new System.Yaml.Serialization.YamlSerializer();
         yaml.SerializeToFile(path, m_Persist);
         return true;
      }
      catch (System.Exception e)
      {
         Debug.LogErrorFormat("SerialzeToYaml '{0}'\n{1}", m_Persist.ToString(), e.ToString());
      }

      return false;
   }

   static public bool DeserializeFromYaml(string path)
   {
      try
      {
         System.Yaml.Serialization.YamlSerializer yaml = new System.Yaml.Serialization.YamlSerializer();

         VarNamePersist persist = yaml.DeserializeFromFile(path)[0] as VarNamePersist;

         foreach (var item in persist.Data)
         {
            VarNamePersistAttribute vp;
            if (m_Persist.Data.TryGetValue(item.Key, out vp))
            {
               m_Persist.Data[item.Key] = item.Value;
            }
         }

         return true;
      }
      catch (System.Exception e)
      {
         Debug.LogErrorFormat("DeserializeFromYaml '{0}'\n{1}", typeof(VarNamePersist).Name, e.ToString());
      }

      return true;
   }
};