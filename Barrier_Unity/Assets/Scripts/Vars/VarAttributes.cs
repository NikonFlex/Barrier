using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Globalization;
using UnityEngine;

public class VarNamePersistAttribute
{
   public string displayText;
   public object defaultValue;
}

public class VarNamePersist
{
   public Dictionary<VarName, VarNamePersistAttribute> data;
};

[AttributeUsage(AttributeTargets.Field)]
public class VarNameAttribute : Attribute
{
   public string displayText;
   public object defaultValue;
   public float  precision;
   public bool   hideInInspector;
   public bool   persist;
};

public static class AttributeHelper
{
   private static VarNamePersist m_persist = new VarNamePersist();

   static AttributeHelper()
   {
      Thread.CurrentThread.CurrentCulture = CultureInfo.InvariantCulture;

      m_persist.data = new Dictionary<VarName, VarNamePersistAttribute>();

      for (VarName varName = VarName.UNDEFINED; varName < VarName.VARSYNC_LAST; varName++)
      {
         VarNameAttribute va = getAttributes(varName);
         if (va == null || va.persist == false)
            continue;

         VarNamePersistAttribute vp = new VarNamePersistAttribute
         {
            displayText = va.displayText,
            defaultValue = va.defaultValue
         };
         m_persist.data.Add(varName, vp);
      }
   }

   private static VarNameAttribute getAttributes(VarName varName)
   {
      object[] attrs = typeof(VarName).GetMember(varName.ToString()).First().GetCustomAttributes(typeof(VarNameAttribute), false);

      if (attrs.Length == 0)
         return null;

      return attrs[0] as VarNameAttribute;
   }

   public static string GetDisplayText(this VarName varName)
   {
      VarNamePersistAttribute vp;
      if (!m_persist.data.TryGetValue(varName, out vp))
      {
         VarNameAttribute va = getAttributes(varName);
         if(va == null || string.IsNullOrEmpty(va.displayText))
            return varName.ToString();

         return va.displayText;
      }

      if (string.IsNullOrEmpty(vp.displayText))
         return varName.ToString();

      return vp.displayText;
   }

   public static void SetDisplayText(this VarName varName, string displayText)
   {
      VarNamePersistAttribute vp;
      if (!m_persist.data.TryGetValue(varName, out vp))
         return;
      
      vp.displayText = displayText;
   }

   public static object GetDefValue(this VarName varName)
   {
      VarNamePersistAttribute vp;
      if (!m_persist.data.TryGetValue(varName, out vp))
      {
         VarNameAttribute va = getAttributes(varName);
         if (va == null)
            return null;

         return va.defaultValue;
      }

      return vp.defaultValue;
   }

   public static float GetPrecision(this VarName varName)
   {
      VarNameAttribute va = getAttributes(varName);
      if (va == null)
         return 0;

      return va.precision;
   }

   public static bool IsHideInInspector(this VarName varName)
   {
      VarNameAttribute va = getAttributes(varName);
      if (va == null)
         return false;
         
      return va.hideInInspector;
   }

   public static bool IsPerist(this VarName varName)
   {
      VarNameAttribute va = getAttributes(varName);
      if (va == null)
         return false;
         
      return va.persist;
   }
   public static bool SerialzeToYaml(string path)
   {
      try
      {
         System.Yaml.Serialization.YamlSerializer yaml = new System.Yaml.Serialization.YamlSerializer();
         yaml.SerializeToFile(path, m_persist);
         return true;
      }
      catch (System.Exception e)
      {
         Debug.LogErrorFormat("SerialzeToYaml '{0}'\n{1}", m_persist.ToString(), e.ToString());
      }
      
      return false;
   }

   static public bool DeserializeFromYaml(string path)
   {
      try
      {
         System.Yaml.Serialization.YamlSerializer yaml = new System.Yaml.Serialization.YamlSerializer();
         
         VarNamePersist persist = yaml.DeserializeFromFile(path)[0] as VarNamePersist;

         foreach (var item in persist.data)
         {
            VarNamePersistAttribute vp;
            if(m_persist.data.TryGetValue(item.Key, out vp))
            {
               m_persist.data[item.Key] = item.Value;
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