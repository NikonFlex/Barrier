using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberFloatingObjectActivation : MonoBehaviour
{
   private Crest.SimpleFloatingObject _floatObjectScript;

   private void Start()
   {
      _floatObjectScript = GetComponent<Crest.SimpleFloatingObject>();   
   }

   public void StartFloating()
   {
      gameObject.GetComponent<Rigidbody>().isKinematic = false;
      _floatObjectScript.enabled = true;
   }
}
