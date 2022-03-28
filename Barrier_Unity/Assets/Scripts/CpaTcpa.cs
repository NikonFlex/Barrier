using UnityEngine;
static class CpaTcpa
{
   static float norm360(float c)
   {
      if (c > 360)
         c -= 360;
      if (c < 0)
         c += 360;
      return c;
   }

   public class PolarPoint
   {
      float _r = 0; // m
      float _fi = 0; // deg, zero to noth

      public PolarPoint(float r, float fi)
      {
         _r = r;
         _fi = fi;
      }
      public PolarPoint(PolarPoint p)
      {
         _r = p._r;
         _fi = p._fi;
      }

      public float x => _r * Mathf.Sin(_fi * Mathf.Deg2Rad);
      public float z => _r * Mathf.Cos(_fi * Mathf.Deg2Rad);

      public static PolarPoint operator -(PolarPoint p1, PolarPoint p2)
      {
         float dx = p1.x - p2.x;
         float dz = p1.z - p2.z;
         float r = Mathf.Sqrt(dx * dx + dz * dz);
         float fi = (r > 0.001f) ? Mathf.Atan2(dx, dz)*Mathf.Rad2Deg : p1._fi - p2._fi;
         return new PolarPoint(r, norm360(fi));
      }
   }
   public static void Calc(PolarPoint own_v, PolarPoint tgt_v, PolarPoint tgt, out float cpa, out float tcpa)
   {
      PolarPoint rel_v = new PolarPoint(own_v - tgt_v);

      float vx = rel_v.x;
      float vz = rel_v.z;
      float rx = tgt.x;
      float rz = tgt.z;

      if((vx * vx + vz * vz) < 0.001f)
      {
         tcpa = -1f;
         cpa = -1f;
         return;
      }

      tcpa = (rx * vx + rz * vz)/(vx*vx + vz*vz);

      float d1 = tcpa * rel_v.x - rx;
      float d2 = tcpa * rel_v.z - rz;

      cpa = Mathf.Sqrt(d1* d1 + d2* d2);
   }                                
}