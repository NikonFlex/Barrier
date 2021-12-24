using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class MouseOrbitImproved : MonoBehaviour
{
    public Transform target;
    public float distance = 2.0f;
    public float xSpeed = 20.0f;
    public float ySpeed = 20.0f;
    public float yMinLimit = -90f;
    public float yMaxLimit = 90f;
    public float pinchSensitivity = 1.0f;
    public float touchSensitivity = 1.0f;

    public RectTransform swipeArea; 

    public float xMinLimit = -360f;
    public float xMaxLimit = 360f;
    public bool xClamped = false;

    public float distanceMin = 10f;
    public float distanceMax = 10f;
    public float smoothTime = 10f;
    public float ScrollSensitivity = 1.0f;
    float rotationYAxis = 0.0f;
    float rotationXAxis = 0.0f;
    float velocityX = 0.0f;
    float velocityY = 0.0f;

    private bool swipeEnabled = true;

    private Vector3 cameraPosition;
    private Quaternion cameraRotation;


    Vector2 delta;


    void Awake()
    {
        cameraPosition = transform.position;
        cameraRotation = transform.rotation;

        swipeEnabled = true;
        Vector3 angles = transform.eulerAngles;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;
        velocityX = 0f;
        velocityY = 0f;
    }

    void OnEnable()
    {
        swipeEnabled = true;
        Vector3 angles = transform.eulerAngles;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;
        velocityX = 0f;
        velocityY = 0f;
    }

    public void InitCameraPosition()
    {
        Vector3 angles = Camera.main.transform.rotation.eulerAngles;//cameraRotation.eulerAngles;
        transform.position = Camera.main.transform.position;//cameraPosition;
        rotationYAxis = angles.y;
        rotationXAxis = angles.x;
        velocityX = 0f;
        velocityY = 0f;
    }

    void Update()
    {
        distance = Vector3.Distance(target.position, transform.position);

#if UNITY_EDITOR
        distance = Mathf.Clamp(distance - Input.GetAxis("Mouse ScrollWheel") * ScrollSensitivity, distanceMin, distanceMax);
        if (swipeArea != null)
        {
            if (Input.GetMouseButtonDown(0))
            {
                swipeEnabled = IsOnArea(Input.mousePosition);
            }
        }
        else swipeEnabled = true;

        if (Input.GetMouseButton(0))
        {
            if (swipeEnabled)
            {
                velocityX += xSpeed * (Input.GetAxis("Mouse X")) * 0.02f;// 0.01f;
                velocityY += ySpeed * (Input.GetAxis("Mouse Y")) * 0.02f;//0.01f;
            }
        }
#elif (!UNITY_EDITOR&&(UNITY_ANDROID || UNITY_IOS))

        distance = Mathf.Clamp(distance, distanceMin, distanceMax);
        if (Input.touchCount > 0)
        {
            if (swipeArea!=null)
            {
                if (Input.GetTouch(0).phase == TouchPhase.Began)
                {
                        swipeEnabled=IsOnArea(Input.GetTouch(0).position);
                }
            }
            else swipeEnabled=true;
            if (swipeEnabled) ProceedTouch();
        }

#endif



        rotationYAxis += velocityX;
        rotationXAxis -= velocityY;
        rotationXAxis = ClampAngle(rotationXAxis, yMinLimit, yMaxLimit);

        if (xClamped) rotationYAxis = ClampAngle(rotationYAxis, xMinLimit, xMaxLimit);

        //Quaternion fromRotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y, 0);
        Quaternion toRotation = Quaternion.Euler(rotationXAxis, rotationYAxis, 0);
        Quaternion rotation = toRotation;
        Vector3 position = rotation * (distance * Vector3.back) + target.position;
        transform.position = position;
        transform.rotation = rotation;
        transform.position = position;
        velocityX = Mathf.Lerp(velocityX, 0, Time.deltaTime * smoothTime);
        velocityY = Mathf.Lerp(velocityY, 0, Time.deltaTime * smoothTime);
    }

    public static float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360F)
            angle += 360F;
        if (angle > 360F)
            angle -= 360F;
        return Mathf.Clamp(angle, min, max);
    }


    public bool IsOnArea(Vector3 ScreenPoint)
    {
        //this is a little adjustment in case the canvas is scaled
        Vector2 RealSize = new Vector2(swipeArea.rect.width, swipeArea.rect.height);
        RealSize = RealSize * GameObject.Find("Canvas").transform.localScale.x;

        //if the point is within the bounds of the button
        if (
            swipeArea.transform.position.x + (RealSize.x / 2f) >= ScreenPoint.x
            && swipeArea.transform.position.x - (RealSize.x / 2f) <= ScreenPoint.x
            && swipeArea.transform.position.y + (RealSize.y / 2f) >= ScreenPoint.y
            && swipeArea.transform.position.y - (RealSize.y / 2f) <= ScreenPoint.y
        )
        {
            return true; //return true
        }
        else
        {
            return false; //if not return false
        }
    }

    private void ProceedTouch()
    {
        if (Input.touchCount == 1 && Input.GetTouch(0).phase == TouchPhase.Moved)
        {
            delta = Input.GetTouch(0).deltaPosition;
            velocityX += xSpeed * delta.x * touchSensitivity / 1000.0f;
            velocityY += ySpeed * delta.y * touchSensitivity / 1000.0f;
        }

        if (Input.touchCount == 2)
        {
            // get current touch positions
            Touch tZero = Input.GetTouch(0);
            Touch tOne = Input.GetTouch(1);
            // get touch position from the previous frame
            Vector2 tZeroPrevious = tZero.position - tZero.deltaPosition;
            Vector2 tOnePrevious = tOne.position - tOne.deltaPosition;

            float oldTouchDistance = Vector2.Distance(tZeroPrevious, tOnePrevious);
            float currentTouchDistance = Vector2.Distance(tZero.position, tOne.position);

            // get offset value
            float deltaDistance = currentTouchDistance - oldTouchDistance;
            distance = Mathf.Clamp(distance - deltaDistance * pinchSensitivity, distanceMin, distanceMax);
        }
    }

}