using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberFloatingObjectActivation : MonoBehaviour
{
   public void StartFloating()
   {
      gameObject.GetComponent<Rigidbody>().isKinematic = false;
      gameObject.GetComponent<Crest.SimpleFloatingObject>().enabled = true;
   }
}
