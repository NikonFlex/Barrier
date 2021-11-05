using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FolowCamera : MonoBehaviour
{
   [SerializeField] float _height = 50;
   [SerializeField] float _offset = 50;
   [SerializeField] float _smoothTime = 2;
   [SerializeField] float _minDistance= 5;

   private Transform _target;
   private Transform _prevTarget;
   private bool _targetChanged = false;
   private IEnumerator _coroutine;
   private int n = 0;
   private float _curDistance;

   public void StartFollow(Transform trg, float distance = -1)
   {
      _target = trg;
      _targetChanged = true;
      _curDistance = distance > 0 ? distance : _minDistance;
      n++;
   }
   
   IEnumerator follow()
   {
      _targetChanged = false;
      int idx = n;
      //print($"start follow {idx}");

      Vector3 curPoint = _prevTarget != null ? _prevTarget.position : transform.position;
      //Vector3 nextPoint = 0.5f * (curPoint + _target.position) + Vector3.up * _height + transform.right * _offset;
      Vector3 nextPoint = _target.position + Vector3.up * _height + transform.right * (_offset + _height);

      Quaternion finishRotation = Quaternion.LookRotation(_target.position - transform.position);


      Vector3 velocity = Vector3.zero;
      float rotationTime = 0;

      //print($"follow {idx} {n}");
      bool needMove = (transform.position - _target.position).magnitude > _curDistance * 2;
      if (idx != n)
         yield break;
         //            var pos = gameObject.transform.position + Speed * Time.deltaTime * (nextPoint - gameObject.transform.position);

      //print($"follow {idx}");
      float moveTime = 0;
      Vector3 startPos = transform.position;
      Quaternion startRotation = transform.rotation;
      float firstPhaseTime = _smoothTime / 2;
      while (idx == n)
      {
         //print($"next {idx}");
         if (moveTime < firstPhaseTime)
         {
            //var pos = Vector3.SmoothDamp(transform.position, nextPoint, ref velocity, _smoothTime/2);
            float t = moveTime / firstPhaseTime;
            if (needMove)
            {
               //print($"Move {idx} {(transform.position - _target.position).magnitude}");
               var pos = Vector3.Lerp(startPos, nextPoint, t);
               velocity = (pos - transform.position) / Time.deltaTime;
               transform.position = pos;
            }
            //print($"Rotate {idx} {(transform.position - _target.position).magnitude}");
            gameObject.transform.rotation = Quaternion.Slerp(startRotation, Quaternion.LookRotation(_target.position - transform.position), t);
            moveTime += Time.deltaTime;
            yield return null;
         }
         else
            break;
      }

      float fixedDistance = (transform.position - _target.position).magnitude;
      while (idx == n)
      {
         //print($"trg {idx} dist {(transform.position - _target.position).magnitude} ");
         if ((transform.position - _target.position).magnitude > _curDistance)
         {
            //print($"trg {idx} SmoothDamp {(transform.position - _target.position).magnitude} ");
            var pos = Vector3.SmoothDamp(transform.position, _target.position, ref velocity, _smoothTime / 2);
            transform.position = pos;
            fixedDistance = (transform.position - _target.position).magnitude;
         }

         transform.position = _target.position - transform.forward * fixedDistance;
         transform.RotateAround(_target.position, Vector3.up, 20 * Time.deltaTime);
         yield return null;
      }

      yield return null;
   }

   // Start is called before the first frame update
   void Start()
   {
      //_coroutine = follow();
   }

   // Update is called once per frame

   void Update()
   {
      if (_targetChanged)
      {
         StartCoroutine(follow());
      }
   }
}
