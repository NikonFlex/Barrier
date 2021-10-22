using System;
using System.Linq;

public struct VarData
{
   public enum Type { Undefined, String, Float, FloatArray, Bool };
   public Type type;
   public string value;

   public VarData(object o = null)
   {
      value = Convert.ToString(o);

      type = VarData.Type.Undefined;
      if      (o is bool)   { type = Type.Bool; }
      else if (o is float)  { type = Type.Float; }
      else if (o is string) { type = Type.String; }
      else if (o is float[])
      {
         type = Type.FloatArray;
         value = string.Join(",", (o as float[]).Select(f => f.ToString()).ToArray());
      }
   }

   public object ToObject()
   {
      switch (type)
      {
         case VarData.Type.Float: return Convert.ToSingle(value);
         case VarData.Type.Bool: return Convert.ToBoolean(value);
         case VarData.Type.FloatArray: return value == "" ? new float[0] : Array.ConvertAll(value.Split(','), new Converter<string, float>(float.Parse));
         default: return value;
      }
   }
}
