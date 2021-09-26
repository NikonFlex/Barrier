using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
   [SerializeField] Camera _topCamera;
   [SerializeField] Camera _isoCamera;

   // Use this for initialization
   void Start()
   {
      _isoCamera.gameObject.SetActive(true);
      _topCamera.gameObject.SetActive(false);
   }

   public void ChangeView(ViewType vt)
   {
      switch(vt)
      {
         case ViewType.Iso:
         {
            _isoCamera.gameObject.SetActive(true);
            _topCamera.gameObject.SetActive(false);
            break;
         }
         case ViewType.Top:
         {
            _isoCamera.gameObject.SetActive(false);
            _topCamera.gameObject.SetActive(true);
            break;
         }
      }
   }
}