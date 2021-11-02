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
   [SerializeField] private GameObject _goodWheatherTexture;
   [SerializeField] private GameObject _badWheatherTexture;

   private Wheather _prevWheather;
   private Wheather _curWheather;

   void Start()
   {
      _prevWheather = Wheather.Good;
   }

   private void Update()
   {
      if (VarSync.GetInt(VarName.Weather) == 0)
      {
         _curWheather = Wheather.Good;
         if (_curWheather != _prevWheather)
         {
            gameObject.GetComponent<MeshRenderer>().material = _goodWheatherTexture.GetComponent<MeshRenderer>().material;
            _prevWheather = _curWheather;
         }
      }
      else
      {
         _curWheather = Wheather.Bad;
         if (_curWheather != _prevWheather)
         {
            gameObject.GetComponent<MeshRenderer>().material = _badWheatherTexture.GetComponent<MeshRenderer>().material;
            _prevWheather = _curWheather;
         }
      }
   }
}
