using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum Wheather
{
   Good = 0,
   Bad = 1,
}

public class WheatherChanger : MonoBehaviour
{
   private Wheather _prevWheather;
   private Wheather _curWheather;

    private Crest.ShapeFFT _shapeFFT;

   void Start()
   {
      _prevWheather = Wheather.Good;
      _shapeFFT = GetComponent<Crest.ShapeFFT>();

   }

   private void Update()
   {
      if (VarSync.GetInt(VarName.Weather) == 0)
      {
         _curWheather = Wheather.Good;
         if (_curWheather != _prevWheather)
         {
            _shapeFFT.enabled = false;
            _prevWheather = _curWheather;
         }
      }
      else
      {
         _curWheather = Wheather.Bad;
         if (_curWheather != _prevWheather)
         {
            _shapeFFT.enabled = true;
            _prevWheather = _curWheather;
         }
      }
   }
}
