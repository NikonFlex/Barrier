using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberFloatingObjectActivation : MonoBehaviour
{
   [SerializeField] Crest.SimpleFloatingObject _floating;
   private Crest.SampleHeightHelper _heightHelper;
   private bool _isFloating = false;

   private void Update()
   {
      if (_isFloating)
         setPosOnWaterSurface();
   }

   public void StartFloating()
   {
      _heightHelper = new Crest.SampleHeightHelper();
      var pos = gameObject.transform.position;
      transform.position = new Vector3(pos.x, 0, pos.z);
      _heightHelper.Init(transform.position);
      _isFloating = true;

//       _floating.gameObject.SetActive(true);
//       GetComponent<Rigidbody>().isKinematic = false;
      //Time.timeScale = 0.1f;
   }

   private void setPosOnWaterSurface()
   {
      Vector3 curPos = transform.position;
      _heightHelper.Sample(out float height);
      //transform.position = new Vector3(curPos.x, height, curPos.z);
   }
}
