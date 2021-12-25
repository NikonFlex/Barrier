using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[RequireComponent(typeof(RectTransform))]

public class ScreenLabel : MonoBehaviour
{
    //public Transform target2;
    public Transform target;
    private Camera cam;
    public Vector3 offset;
    public string LabelText;
    public TextMeshProUGUI tMP;
    Rect canvasRect;
    //public bool oriented;
    
    RectTransform myRect;


    // Start is called before the first frame update
    void Start()
    {
        cam = Camera.main;
        myRect = transform.GetComponent<RectTransform>();
        canvasRect = FindObjectOfType<Canvas>().pixelRect;
        if (tMP!=null) tMP.text = LabelText;
    }

    private void Update()
    {
        if (target != null)
        {
            Vector3 targetPosition = cam.WorldToViewportPoint(target.position+offset);
            if (targetPosition.z>0)
            {
                Vector2 pos = new Vector2(targetPosition.x * canvasRect.width, targetPosition.y * canvasRect.height);
                myRect.position = pos;
            }
        }
    }
}
