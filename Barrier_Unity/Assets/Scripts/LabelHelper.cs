using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

static class LabelHelper
{
   public static ScreenLabel AddLabel(GameObject o, string text)
   {
      var label = GameObject.Instantiate(Resources.Load<ScreenLabel>("ObjectLabel"), markersGroup);
      label.name = o.name + "_label";
      label.target = o.transform;
      label.LabelText = text;
      Debug.Log($"Add label '{label.name}' to object '{o.name}'");
      return label;
   }

   public static void DestroyLabel(GameObject o)
   {
      var scl = findLabel(o);
      if (scl)
      {
         if (scl.target == o.transform)
         {
            Debug.Log($"Destroy label '{scl.name}' of object '{o.name}'");
            GameObject.Destroy(scl.gameObject);
         }
      }
      else
      {
         Debug.LogError($"Can't find label of object '{o.name}'");
      }

   }
   public static void HideLabel(GameObject o)
   {
      var scl = findLabel(o);
      if (scl)
      {
         Debug.Log($"Hide label '{scl.name}' of object '{o.name}'");
         scl.gameObject.SetActive(false);
      }
      else
      {
         Debug.LogError($"Can't find label of object '{o.name}'");
      }
   }

   public static void ShowLabel(GameObject o)
   {
      var scl = findLabel(o);
      if (scl)
      {
         Debug.Log($"Show label '{scl.name}' of object '{o.name}'");
         scl.gameObject.SetActive(true);
      }
      else
      {
         Debug.LogError($"Can't find label of object '{o.name}'");
      }
   }

   public static string GetLabelText(GameObject o)
   {
      var scl = findLabel(o);
      if (scl)
      {
         return scl.LabelText;
      }
      else
      {
         Debug.LogError($"Can't find label of object '{o.name}'");
         return "";
      }
   }

   public static void SetLabelText(GameObject o, string text)
   {
      var scl = findLabel(o);
      if (scl)
         scl.LabelText = text;
      else
      {
         Debug.LogError($"Can't find label of object '{o.name}'");
      }
   }

   public static void ShowLabels(bool on = true)
   {
      markersGroup.gameObject.SetActive(on);
   }

   private static ScreenLabel findLabel(GameObject o)
   {
      return markersGroup.GetComponentsInChildren<ScreenLabel>(true).FirstOrDefault(scl => scl.target == o.transform);
   }

   private static Transform _markersGroup;
   private static Transform markersGroup => _markersGroup != null ? _markersGroup : _markersGroup = GameObject.Find("ObjectMarkers").transform;
}
