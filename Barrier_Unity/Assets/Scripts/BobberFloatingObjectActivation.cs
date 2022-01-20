using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BobberFloatingObjectActivation : MonoBehaviour
{
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
      _heightHelper.Init(gameObject.transform.position);
      _isFloating = true;
   }

   private void setPosOnWaterSurface()
   {
      Vector3 curPos = gameObject.transform.position;
      float height;
      _heightHelper.Sample(out height);
      gameObject.transform.position = new Vector3(curPos.x, height, curPos.y);
   }
}
