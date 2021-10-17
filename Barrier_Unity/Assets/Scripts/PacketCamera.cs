using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PacketCamera : MonoBehaviour
{
    public GameObject following_obj;
    private float _offset = -10f;
    Vector3 _offsetVec;

    void Start()
    {
        _offsetVec = transform.position - following_obj.transform.position;
    }

    void Update()
    {
        transform.position = following_obj.transform.position + following_obj.transform.forward * _offset;
        transform.rotation = following_obj.transform.rotation;
    }
}
