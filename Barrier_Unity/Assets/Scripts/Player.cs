using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VerticalThrowedCube : MonoBehaviour
{
    [SerializeField] private float V0;
    const float g = 9.8f;
    bool action_started = false;
    float t = 0;
    float _currentSpeed = 0;
    Vector3 _start_pos;

    void Start()
    {
        _start_pos = transform.position;
    }

    void Update()
    {
        Vector3 dir = Vector3.Normalize(transform.parent.forward);
        dir.y = 0;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("start");
            _currentSpeed = V0;
            action_started = true;
        }

        _currentSpeed -= g * Time.deltaTime;
        float delta = _currentSpeed * Time.deltaTime;
        Vector3 pos = transform.position;
        pos += dir * 10 * Time.deltaTime;
        pos.y += delta;
        if (pos.y > 0.5f)
            transform.position = pos;
    }
}