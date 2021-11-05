using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class SmoothTravel : MonoBehaviour
{
   // Start is called before the first frame update
   public GameObject[] _targets;
   public float Distance = 10;
   public float Height = 150;
   public float Offset = 10;
   public float Speed = 2;
   public float RotationPeriod = 5;
   public float SmoothTime = 1F;
   private int _currentIndex = 0;
   private bool _isMoving = false;
   [SerializeField] FolowCamera _camera;
   [SerializeField] float _duration = 5;
   void Start()
   {

   }

   // Update is called once per frame
   void Update()
   {
      if (!_isMoving)
         StartCoroutine(followWheel());
   }

   IEnumerator followWheel()
   {
      _isMoving = true;
      while (true)
      {
         Transform cur = _targets[(_currentIndex) % _targets.Length].transform;
         _camera.StartFollow(cur);
         _currentIndex++;
         yield return new WaitForSeconds(_duration);
      }

   }

   IEnumerator move()
   {
      _isMoving = true;
      Time.timeScale = 1;
      while (true)
      {
         Transform cur = _targets[(_currentIndex + 1) % _targets.Length].transform;
         Transform prev = _targets[_currentIndex % _targets.Length].transform;
         print($"Distance {(gameObject.transform.position - cur.transform.position).magnitude}");
         cur = _targets[_currentIndex % _targets.Length].transform;
         Vector3 nextPoint = 0.5f * (cur.position + prev.position) + Vector3.up * Height + transform.right * Offset;
         Quaternion startRotation = gameObject.transform.rotation;
         Quaternion finishRotation = Quaternion.LookRotation(cur.transform.position - transform.position);
         // 1.Rotation
         float rotationTime = 0;
//          while (rotationTime < RotationPeriod)
//          {
//             float t = rotationTime / RotationPeriod;
//             gameObject.transform.rotation = Quaternion.Slerp(startRotation, finishRotation, t * t);
// 
//             rotationTime += Time.deltaTime;
//             yield return null;
//          }

         Vector3[] spline = new Vector3[6];
         spline[0] = transform.position;
         spline[1] = (cur.position*0.8f + prev.position*0.2f) + Vector3.up * Random.Range(Height / 2, Height) + transform.right * Random.Range(Offset/2, Offset);
         spline[2] = (cur.position * 0.7f + prev.position * 0.3f) + Vector3.up * Random.Range(Height / 2, Height) + transform.right * Random.Range(Offset / 2, Offset);
         spline[3] = (cur.position * 0.5f + prev.position * 0.5f) + Vector3.up * Height + transform.right * Offset;
         spline[5] = (cur.position * 0.8f + prev.position * 0.2f) + Vector3.up * Random.Range(Height / 2, Height) + transform.right * Random.Range(Offset / 2, Offset);

         // 2. Move to nextPoint
         //          print($"Distance2 {(gameObject.transform.position - nextPoint).magnitude}");
         //          float MovePeriod = 5;
         //          float moveTime = 0;
         //          while (moveTime < MovePeriod)
         //          {
         //             float t = moveTime / MovePeriod;
         //             gameObject.transform.position = new CatmullRomSpline().Interpolate(spline, t);
         // 
         //             gameObject.transform.LookAt(cur);
         // 
         //             moveTime += Time.deltaTime;
         //             yield return null;
         //          }
         
         Vector3 velocity = Vector3.zero;
         while ((gameObject.transform.position - nextPoint).magnitude > 1)
         {
            //            var pos = gameObject.transform.position + Speed * Time.deltaTime * (nextPoint - gameObject.transform.position);
            var pos = Vector3.SmoothDamp(transform.position, nextPoint, ref velocity, SmoothTime);
            float t = rotationTime / SmoothTime;
            gameObject.transform.rotation = Quaternion.Slerp(startRotation, Quaternion.LookRotation(cur.transform.position - transform.position), t);

            rotationTime += Time.deltaTime;

            gameObject.transform.position = pos;
//            gameObject.transform.LookAt(cur);

            yield return null;
         }
         // 3. Move to target
         print($"Distance3 {(gameObject.transform.position - cur.position).magnitude}");
         
         velocity = Vector3.zero;
         while ((gameObject.transform.position - cur.position).magnitude > Distance)
         {
            //            var pos = gameObject.transform.position + Speed * Time.deltaTime * (cur.position - gameObject.transform.position);
            var pos = Vector3.SmoothDamp(transform.position, cur.position, ref velocity, SmoothTime);
            gameObject.transform.position = pos;
            gameObject.transform.LookAt(cur);
            yield return null;
         }
         yield return new WaitForSeconds(1);
         _currentIndex++;

         yield return null;
      }

      yield return null;

   }
}

class CatmullRomSpline
{
   readonly float[, ] C = new float[4, 4];
   public CatmullRomSpline()
   {
      C[0, 0] = -0.5f; C[0, 1] = 1.5f; C[0, 2] = -1.5f; C[0, 3] = 0.5f;
      C[1, 0] = 1.0f; C[1, 1] = -2.5f; C[1, 2] = 2.0f; C[1, 3] = -0.5f;
      C[2, 0] = -0.5f; C[2, 1] = 0.0f; C[2, 2] = 0.5f; C[2, 3] = 0.0f;
      C[3, 0] = 0.0f; C[3, 1] = 1.0f;  C[3, 2] = 0.0f; C[3, 3] = 0.0f;
   }

   public Vector3 Interpolate(Vector3[] points, float t)
   {
      if (t < 0) t = 0;
      if (t > 1) t = 1;

      //ASSERT(nPoints >= 4);

      int nSpans = points.Length - 3;
      float x = t * nSpans;
      int span = (int)x;
      span = Mathf.Min(span, nSpans);
      x -= span;
      int idxStart = span;

      Vector3[] c = new Vector3[4];
      for (int i = 0; i < 4; i++)
      {
         for (int j = 0; j < 4; j++)
            c[3 - i] += points[j + idxStart] * C[i, j];
      }

      return ((c[3] * x + c[2]) * x + c[1]) * x + c[0];
   }
}