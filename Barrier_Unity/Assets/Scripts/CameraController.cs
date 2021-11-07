using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
   [SerializeField] Camera _topCamera;
   [SerializeField] Camera _isoCamera;
   [SerializeField] Camera _freeCamera;
   [SerializeField] Camera _followCamera;
   [SerializeField] Camera _torpedoCamera;

   // Use this for initialization
   void Start()
   {
      _isoCamera.gameObject.SetActive(true);
      _topCamera.gameObject.SetActive(false);
      _freeCamera.gameObject.SetActive(false);
      _followCamera.gameObject.SetActive(false);
      _torpedoCamera.gameObject.SetActive(false);
   }

   public void FollowObject(Transform t, float distance = -1)
   {
      print($"Follow object {t.gameObject.name}");
      //_followCamera.GetComponent<MouseOrbit>().target = t;
      _followCamera.GetComponent<FolowCamera>().StartFollow(t, distance);
      ChangeView(ViewType.FollowObject);
   }

   public void ChangeView(ViewType vt)
   {
      switch(vt)
      {
         case ViewType.Iso:
         {
            _isoCamera.gameObject.SetActive(true);
            _topCamera.gameObject.SetActive(false);
            _freeCamera.gameObject.SetActive(false);
            _followCamera.gameObject.SetActive(false);
            _torpedoCamera.gameObject.SetActive(false);
            break;
         }
         case ViewType.Top:
         {
            _topCamera.gameObject.SetActive(true);
            _isoCamera.gameObject.SetActive(false);
            _freeCamera.gameObject.SetActive(false);
            _followCamera.gameObject.SetActive(false);
            _torpedoCamera.gameObject.SetActive(false);
            break;
         }
         case ViewType.FollowObject:
         {
            _followCamera.gameObject.SetActive(true);
            _topCamera.gameObject.SetActive(false);
            _isoCamera.gameObject.SetActive(false);
            _freeCamera.gameObject.SetActive(false);
            _torpedoCamera.gameObject.SetActive(false);
            break;
         }
         case ViewType.Torpedo:
         {
            _torpedoCamera.gameObject.SetActive(true);
            _followCamera.gameObject.SetActive(false);
            _topCamera.gameObject.SetActive(false);
            _isoCamera.gameObject.SetActive(false);
            _freeCamera.gameObject.SetActive(false);
            break;
         }
      }
   }
}