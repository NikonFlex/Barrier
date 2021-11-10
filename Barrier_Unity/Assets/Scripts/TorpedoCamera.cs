using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TorpedoCamera : MonoBehaviour
{
   private Vector3 _offsetFromTorpedo;
   [SerializeField] GameObject _torpedo;

   void Start()
   {
      _offsetFromTorpedo = gameObject.transform.position - _torpedo.transform.position;
   }

   void Update()
   {
      gameObject.transform.position = _torpedo.transform.position + _offsetFromTorpedo;
      gameObject.transform.LookAt(_torpedo.transform);
   }
}
