//
//Filename: maxCamera.cs
//
// original: http://www.unifycommunity.com/wiki/index.php?title=MouseOrbitZoom
//
// --01-18-2010 - create temporary target, if none supplied at start

using UnityEngine;
using System.Collections;


[AddComponentMenu("Camera-Control/3dsMax Camera Style")]
public class maxCamera : MonoBehaviour
{
   public enum ControlType
   {
      Free,
      PinPositon,
      PinTarget
   }

   private Transform target;
   public ControlType controlType = ControlType.Free;

   public Vector3 targetOffset;
   public float distance = 5.0f;
   public float maxDistance = 400;
   public float minDistance = 1f;
   public float xSpeed = 500.0f;
   public float ySpeed = 500.0f;
   public int yMinLimit = -80;
   public int yMaxLimit = 80;
   public int zoomRate = 100;
   public float panSpeed = 5f;
   public float zoomDampening = 5.0f;

   public Vector3 minBox = new Vector3(-167, 0, 283);
   public Vector3 maxBox = new Vector3(175, 40, 348);

   private float xDeg = 0.0f;
   private float yDeg = 0.0f;
   private float currentDistance;
   private float desiredDistance;
   private Quaternion currentRotation;
   private Quaternion desiredRotation;
   private Quaternion rotation;
   private Vector3 currentPosition;
   private Vector3 desiredPosition;

   public GameObject objectToFollow;
   private Vector3 followingObjectHitOffset;
   private Camera cam;

   float mouseAxisX;
   float mouseAxisY;
   Vector3 mousePosSave;

   void Start() { Init(); }
   void OnEnable() { Init(); }

   public void Init()
   {
      Init(transform.position + (transform.forward * distance));
   }

   public void Init(Vector3 posTarget)
   {
      //If there is no target, create a temporary target at 'distance' from the cameras current viewpoint
      if (!target)
      {
         string name = "Cam Target (" + gameObject.name + ")";
         GameObject go = GameObject.Find(name);
         if (go == null)
            go = new GameObject(name);
         target = go.transform;
      }

      target.position = posTarget;
      distance = Vector3.Distance(transform.position, target.position);
      currentDistance = distance;
      desiredDistance = distance;

      //be sure to grab the current rotations as starting points.
      currentPosition = transform.position;
      desiredPosition = target.position - (transform.forward * distance);
      rotation = transform.rotation;
      currentRotation = transform.rotation;
      desiredRotation = transform.rotation;

      xDeg = rotation.eulerAngles.y;
      yDeg = rotation.eulerAngles.x;

      cam = GetComponent<Camera>();
   }


   void zoomOld(float val)
   {
      desiredDistance -= val * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
      //clamp the zoom min/max
      desiredDistance = Mathf.Clamp(desiredDistance, minDistance, maxDistance);
   }


   Vector3 clampPos(Vector3 pos)
   {
      //print( "clamp  " + pos ); 
      Vector3 p = new Vector3(pos.x, pos.y, pos.z);
      p.x = Mathf.Clamp(p.x, minBox.x, maxBox.x);
      p.y = Mathf.Clamp(p.y, minBox.y, maxBox.y);
      p.z = Mathf.Clamp(p.z, minBox.z, maxBox.z);
      return p;
   }

   void zoom(float val)
   {
      if (val == 0)
         return;

      if (controlType != ControlType.Free || objectToFollow != null)
      {
         zoomOld(val);
         return;
      }


      //desiredDistance -= val * Time.deltaTime * zoomRate * Mathf.Abs(desiredDistance);
      float delta = val * val * val;

      float dForward = 200;
      float dDown = 200;
      RaycastHit hit;
      if (Physics.Raycast(transform.position, rotation * Vector3.forward, out hit, 200f))
      {
         if (hit.collider.gameObject != null)
         {
            dForward = Vector3.Distance(hit.point, transform.position);
         }
      }

      if (Physics.Raycast(transform.position, Vector3.down, out hit, 200f))
      {
         if (hit.collider.gameObject != null)
         {
            dDown = Vector3.Distance(hit.point, transform.position);
         }
      }

      float d = Mathf.Max(dForward, dDown);
      if (d > 10)
         delta *= d / 10;


      target.position -= rotation * Vector3.forward * delta;
      Vector3 clamped = clampPos(target.position);
      if (target.position == clamped)
         currentDistance = Vector3.Distance(target.position, transform.position);
      target.position = clamped;

      //desiredDistance += delta;
      //print ( desiredDistance );

   }

   /*
      * Camera logic on LateUpdate to only update after all character movement logic has been handled. 
      */
   void LateUpdate()
   {
      if (target == null)
         return;

      if (!cam || !cam.enabled)
         return;

      if (Input.GetMouseButton(0) || Input.GetMouseButton(1) || Input.GetMouseButton(2))
      {
         Vector3 diff = Input.mousePosition - mousePosSave;

         mouseAxisX = Mathf.Clamp(0.5f * diff.x / Screen.width, -1, 1);
         mouseAxisY = Mathf.Clamp(0.5f * diff.y / Screen.height, -1, 1);
      }
      else
      {
         mouseAxisX = 0;
         mouseAxisY = 0;
         mousePosSave = Input.mousePosition;
      }

      float realMouseAxisX = Input.GetAxis("Mouse X");
      float realMouseAxisY = Input.GetAxis("Mouse Y");

      bool realMouseMovementDetected = realMouseAxisX != 0 || realMouseAxisY != 0;
      if (realMouseMovementDetected)
      {
         mouseAxisX = realMouseAxisX;
         mouseAxisY = realMouseAxisY;
      }

      if (controlType == ControlType.Free)
      {
         if (Input.GetKeyDown("space"))
         {
            if (objectToFollow == null)
            {
               RaycastHit hit;
               if (Physics.Raycast(cam.ScreenPointToRay(Input.mousePosition), out hit))
               {
                  if (hit.collider.gameObject != null && Vector3.Distance(hit.point, transform.position) < maxDistance)
                  {
                     //            followingObjectScreenOffset = cam.WorldToScreenPoint(hit.collider.gameObject.transform.position);
                     followingObjectHitOffset = hit.point - hit.collider.gameObject.transform.position;
                     objectToFollow = hit.collider.gameObject;
                     Init(hit.point);
                  }
               }
            }
            else
            {
               objectToFollow = null;
            }
         }
      }


      if (objectToFollow != null)
      {
         target.position = objectToFollow.transform.position + followingObjectHitOffset;
      }

      // If Control and Alt and Middle button? ZOOM!
      if (Input.GetMouseButton(2) && Input.GetKey(KeyCode.LeftAlt) && Input.GetKey(KeyCode.LeftControl))
         zoom(mouseAxisY * 0.125f);
      // If middle mouse and left alt are selected? ORBIT
      else if ((Input.GetMouseButton(1) && Input.GetKey(KeyCode.LeftControl) && !realMouseMovementDetected) ||
                (Input.GetMouseButton(1) && realMouseMovementDetected))
      {
         if (controlType != ControlType.PinPositon)
         {
            xDeg += mouseAxisX * xSpeed * 0.02f;
            yDeg -= mouseAxisY * ySpeed * 0.02f;

            ////////OrbitAngle

            //Clamp the vertical axis for the orbit
            yDeg = ClampAngle(yDeg, yMinLimit, yMaxLimit);
            // set camera rotation 
            desiredRotation = Quaternion.Euler(yDeg, xDeg, 0);
            currentRotation = transform.rotation;

            rotation = Quaternion.Lerp(currentRotation, desiredRotation, Time.deltaTime * zoomDampening);
            transform.rotation = rotation;
         }
         else
         {
            float dx = mouseAxisX * 0.2f * Mathf.Deg2Rad;
            float dy = mouseAxisY * 0.1f * Mathf.Deg2Rad;

            Quaternion qx = new Quaternion(dy, 0, 0, 1);
            Quaternion qy = new Quaternion(0, dx, 0, 1);

            transform.rotation = Quaternion.Lerp(transform.rotation, qy * qx, Time.deltaTime * 10);
            return;
         }
      }
      // otherwise if middle mouse is selected, we pan by way of transforming the target in screenspace
      else if ((Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftControl) && !realMouseMovementDetected) ||
               (Input.GetMouseButton(2) && realMouseMovementDetected))
      {
         if (controlType == ControlType.Free)
         {
            //grab the rotation of the camera so we can move in a psuedo local XY space
            target.rotation = transform.rotation;
            target.Translate(Vector3.right * -mouseAxisX * panSpeed);
            target.Translate(transform.up * -mouseAxisY * panSpeed, Space.World);
         }
      }

      // affect the desired Zoom distance if we roll the scrollwheel
      zoom(Input.GetAxis("Mouse ScrollWheel") * 5);


      // For smoothing of the zoom, lerp distance
      currentDistance = Mathf.Lerp(currentDistance, desiredDistance, Time.deltaTime * zoomDampening);

      // calculate position based on the new currentDistance 
      desiredPosition = target.position - (rotation * Vector3.forward * currentDistance + targetOffset);
      transform.position = clampPos(Vector3.Lerp(currentPosition, desiredPosition, Time.deltaTime * zoomDampening));
      currentPosition = transform.position;
   }

   private static float ClampAngle(float angle, float min, float max)
   {
      if (angle < -360)
         angle += 360;
      if (angle > 360)
         angle -= 360;
      return Mathf.Clamp(angle, min, max);
   }
}