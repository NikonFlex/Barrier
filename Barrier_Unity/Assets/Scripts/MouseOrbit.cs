using UnityEngine;
using System.Collections;

[AddComponentMenu("Camera-Control/Mouse Orbit with zoom")]
public class MouseOrbit : MonoBehaviour {
  
  public Transform target;
  public float distance = 5.0f;
  public float xSpeed = 20.0f;
  public float ySpeed = 60.0f;
  
  public float yMinLimit = 5f;
  public float yMaxLimit = 70f;
  
  public float distanceMin = 2f;
  public float distanceMax = 30f;
  
  float x = 0.0f;
  float y = 0.0f;
  
  // Use this for initialization
  void Start ()
  {
    Init();
  }

  public void Init()
  {
    Vector3 angles = transform.eulerAngles;
    x = angles.y;
    y = angles.x;

    // Make the rigid body not change rotation
    if (GetComponent<Rigidbody>())
      GetComponent<Rigidbody>().freezeRotation = true;
  }

  public void UpdateTransform()
  {
    if (target == null)
      return;
    
    y = ClampAngle(y, yMinLimit, yMaxLimit);

    Quaternion rotation = Quaternion.Euler(y, x, 0);

    distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * 5, distanceMin, distanceMax);

    //RaycastHit hit;
    //if (Physics.Linecast (target.position, transform.position, out hit)) {
    //  distance -=  hit.distance;
    //}
    Vector3 negDistance = new Vector3(0.0f, 0.0f, -distance);
    Vector3 position = rotation * negDistance + target.position;

    transform.rotation = rotation;
    transform.position = position;

  }

  void LateUpdate () 
  {
    if (target && ( Input.GetMouseButton( 1 ) || Input.GetAxis("Mouse ScrollWheel") != 0 ) ) 
    {
      x += Input.GetAxis("Mouse X") * xSpeed * distance * 0.02f;
      y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
    }
    UpdateTransform();
  }

  public static float ClampAngle(float angle, float min, float max)
  {
    if (angle < -360F)
      angle += 360F;
    if (angle > 360F)
      angle -= 360F;
    return Mathf.Clamp(angle, min, max);
  }
  
  
}