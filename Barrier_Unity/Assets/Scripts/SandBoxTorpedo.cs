using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SandBoxTorpedo : MonoBehaviour
{
   [SerializeField] private float _speed;

   void Start()
   {
        
   }

   void Update()
   {
      transform.position = transform.position + transform.forward * _speed * Time.deltaTime;   
   }

   public float GetSpeed()
   {
      return _speed;
   }
}
