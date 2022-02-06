using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyBopper : MonoBehaviour
{
   [SerializeField] Crest.SimpleFloatingObject _floating;
   [SerializeField] GameObject _signal;
   [SerializeField] GameObject _antenna;
   [SerializeField] GameObject _buoyancy;

   private Crest.SampleHeightHelper _heightHelper;
   private bool _isFloating = false;


   private void Update()
   {
      if (_isFloating)
         setPosOnWaterSurface();

      if (!Scenario.IsTargetAlive)
         _signal.GetComponent<ParticleSystem>().Stop();
      else 
      {
         var t = Scenario.Instance.TargetInfo;
         float distFromBuoyToTarget = (gameObject.transform.position - t.Target.transform.position).magnitude;
         float s = distFromBuoyToTarget;
         _signal.transform.localScale = new Vector3(s, s, s);
      }
   }

   public void StartWork()
   {
      transform.SetParent(null, true);

      transform.position = new Vector3(gameObject.transform.position.x, 1, gameObject.transform.position.z);
      transform.rotation = Quaternion.LookRotation(Vector3.down);

      _buoyancy.GetComponent<Animator>().SetBool("Inflate", true);
      _antenna.GetComponent<Animator>().SetBool("Raise", true);
      startFloating();
   }

   public void startFloating()
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

   public void StartPelleng()
   {
      _signal.GetComponent<ParticleSystem>().Play();
   }



   private void setPosOnWaterSurface()
   {
      Vector3 curPos = transform.position;
      _heightHelper.Sample(out float height);
      //transform.position = new Vector3(curPos.x, height, curPos.z);
   }
}