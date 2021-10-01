using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BouoyGuard : MonoBehaviour
{
   public Transform _torpedo;
   public Transform _bouy1;
   public Transform _bouy2;
   public float _range;
   public float _angle;
   
   void Start()
   {

   }

   // Update is called once per frame
   void Update()
   {
      Vector3 v1 = (_torpedo.position - _bouy1.position);
   }

   private void OnDrawGizmos()
   {
      Gizmos.color = Color.red;
      Gizmos.DrawCube(_torpedo.position, new Vector3(5, 100, 5));

      Gizmos.color = Color.green;
      Gizmos.DrawCube(_bouy1.position, new Vector3(5, 100, 5));
      Gizmos.DrawCube(_bouy2.position, new Vector3(5, 100, 5));

      Gizmos.color = Color.magenta;

      Vector3 vc = (_torpedo.position - _bouy1.position).normalized;
      Vector3 vr = getDir(vc, -_angle);
      Vector3 vl = getDir(vc, _angle);
      Gizmos.DrawLine(_bouy1.position, _bouy1.position + vr * _range + Vector3.up);
      Gizmos.DrawLine(_bouy1.position, _bouy1.position + vl * _range + Vector3.up);

      vc = (_torpedo.position - _bouy2.position).normalized;
      vr = getDir(vc, -_angle);
      vl = getDir(vc, _angle);
      Gizmos.DrawLine(_bouy2.position, _bouy2.position + vr * _range + Vector3.up);
      Gizmos.DrawLine(_bouy2.position, _bouy2.position + vl * _range + Vector3.up);
      
   }

   private Vector3 getDir(Vector3 v, float a)
   { 
      return Quaternion.AngleAxis(a, Vector3.up) * v;
   }

   private float dir1; //radians
}
