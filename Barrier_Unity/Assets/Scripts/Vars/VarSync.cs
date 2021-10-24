﻿using UnityEngine;
using System;


public static class VarSync
{
   static private VarData[] _vars = new VarData[(int)VarName.VARSYNC_LAST];
 
   static public VarData[] vars { get { return _vars; } }

   static VarSync()
   {
      ClearVariables();
   }

   public delegate void VariableUpdateEventHandler(VarName v, object value);
   public static event VariableUpdateEventHandler OnVariableUpdate;

   private static void fireUpdate(VarName v, VarData data)
   {
      if (OnVariableUpdate != null)
         OnVariableUpdate(v, data.ToObject());
   }

   private static VarData defVar(this VarData.Type t)
   {
      switch (t)
      {
         case VarData.Type.Bool: return new VarData(false);
         case VarData.Type.Float: return new VarData(0f);
         case VarData.Type.String: return new VarData("");
         default: return new VarData(null);
      }
   }

   static public void ClearVariables()
   {
      for (VarName i = VarName.UNDEFINED; i < VarName.VARSYNC_LAST; i++)
      {
         _vars[(int)i].type = VarData.Type.Undefined;
         _vars[(int)i].value = null;

         object defVal = i.GetDefValue();

         if(defVal != null)
         {
            _vars[(int)i] = new VarData(defVal);
            fireUpdate(i, _vars[(int)i]);
         }
      }
   }

   // helpers
   static public T Get<T>(VarName v, T def)
   {
      object o = GetObject(v);
      return (o != null && o is T) ? (T)o : def;
   }

   static public float GetFloat(VarName v)
   {
      object o = GetObject(v);
      if (o == null)
         o = v.GetDefValue();
      if (o != null && (o is float || o is int))
         return (float)o;
      return 0f;
   }

   static public bool GetBool(VarName v)
   {
      return Get(v, false);
   }

   static public int GetInt(VarName v)
   {
      return (int)Get(v, 0f);
   }
   static public string GetString(VarName v)
   {
      return Get(v, "");
   }

   static public Vector2 Get(VarName v, Vector2 def)
   {
      float[] arr = Get(v, new float[0]);
      if (arr == null || arr.Length < 2)
         return def;
      return new Vector2(arr[0], arr[1]);
   }

   static public Vector3 Get(VarName v, Vector3 def)
   {
      float[] arr = Get(v, new float[0]);
      if (arr == null || arr.Length < 3)
         return def;
      return new Vector3(arr[0], arr[1], arr[2]);
   }

   static public object GetObject(VarName v)
   {
      return _vars[(int)v].ToObject();
   }

   static private bool isChanged(VarName v, float val)
   {
      float prec = v.GetPrecision();
      float oldVal = Get(v, 0f);
      if (prec == 0)
         return oldVal != val;

      if (val == 0 && oldVal != 0) 
         return true;

      return Mathf.Abs(oldVal - val) >= prec;
   }

   static private bool isChanged(VarName v, float[] val)
   {
      float[] oldVal = Get(v, new float[0]);
      if (oldVal.Length != val.Length)
         return true;
      float prec = v.GetPrecision();
      if (prec == 0)
         return oldVal != val;
      for (int i = 0; i < val.Length; i++)
         if (Mathf.Abs(oldVal[i] - val[i]) >= prec)
            return true;
      return false;
   }

   static public void Set(VarName v, bool value, bool forceUpdate = false)
   {
      if (Get(v, false) != value || forceUpdate)
         updateVariableInternal(v, new VarData(value), forceUpdate);
   }

   static public void Set(VarName v, string value, bool forceUpdate = false)
   {
      if (Get(v, "") != value || forceUpdate)
         updateVariableInternal(v, new VarData(value), forceUpdate);
   }

   static public void Set(VarName v, float value, bool forceUpdate = false)
   {
      if (isChanged(v, value))
         updateVariableInternal(v, new VarData(value), forceUpdate);
   }

   static public void Set(VarName v, float[] value, bool forceUpdate = false)
   {
      if (isChanged(v, value))
         updateVariableInternal(v, new VarData(value), forceUpdate);
   }

   static public void Set(VarName v, Vector2 value, bool forceUpdate = false)
   {
      float[] arr = new float[2] { value.x, value.y };
      if (isChanged(v, arr))
         updateVariableInternal(v, new VarData(value), forceUpdate);
   }

   static public void Set(VarName v, Vector3 value, bool forceUpdate = false)
   {
      float[] arr = new float[3] { value.x, value.y, value.z };
      if (isChanged(v, arr))
         updateVariableInternal(v, new VarData(value), forceUpdate);
   }

   static public void updateVariableInternal(VarName v, VarData data, bool forceUpdate)
   {
      bool diff = _vars[(int)v].value != data.value;

      _vars[(int)v] = data;

      if (diff || forceUpdate)
         fireUpdate(v, data);
   }

}
