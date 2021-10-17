using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum PacketState
{
    None = 0,
    Fly = 1,
    SplashDown = 2,
}

public class Packet : MonoBehaviour
{
    private const float g = 9.8f;
    private Vector3 _speedVector;
    private PacketState _state = PacketState.None;
    [SerializeField] private GameObject _target;
    [SerializeField] private float K = 0.001f;
    Vector3 _prevPos;
    bool _isOnParashut = false;

    void Start()
    {

    }

    void Update()
    {
        switch (_state)
        {
            case PacketState.Fly:
                StartCoroutine(Fly());
                //Fly();
                break;

            case PacketState.SplashDown:
                if (!_isOnParashut)
                    StartCoroutine(SplashDown());
                break;

            default:
                break;
        }       
    }

    public void Launch(float V0, Vector3 dir)
    {
        _speedVector = dir * V0;
        _state = PacketState.Fly;
        Time.timeScale = 0.5f;

    }

    private IEnumerator Fly()
    {
        bool break_flag = false;
        while (!break_flag)
        {
            Vector3 pos = transform.position;
            pos += _speedVector * Time.deltaTime;
            _speedVector.y -= g * Time.deltaTime;
            transform.rotation = Quaternion.LookRotation(_speedVector.normalized);
            if (new Vector2(pos.x, pos.z).magnitude >= new Vector2(_target.transform.position.x, _target.transform.position.z).magnitude)
                break_flag = true;
            transform.position = pos;
            yield return new WaitForSeconds(1f);
        }
        _state = PacketState.SplashDown;
    }

    private IEnumerator SplashDown()
    {
        _isOnParashut = true;
        _prevPos = transform.position;
        while (transform.position.y > 0)
        {
            float PvMax = K * _speedVector.magnitude * _speedVector.magnitude; // максимальное сопротиволение парашюта
            float PvMin = PvMax / 10; // минимальное сопротиволение парашюта
            float speedMax = 250;
            float speed = _speedVector.magnitude;

            float Pv = Mathf.Lerp(PvMax, PvMin, speed / speedMax);

            //print($"t = {Time.time} y = {transform.position.y}, speed = {_speedVector.magnitude}, Pv = {Pv}");

            Vector3 delta = _speedVector.normalized * Pv * Time.deltaTime;
            if (delta.magnitude > _speedVector.magnitude)
                _speedVector = Vector3.zero;
            else
                _speedVector -= delta;
            _speedVector.y -= g * Time.deltaTime;
            Vector3 pos = transform.position;
            pos += _speedVector * Time.deltaTime;
            transform.position = pos;
            transform.rotation = Quaternion.LookRotation(_speedVector.normalized);
            //if ((transform.position - _prevPos).magnitude > 1)
            //{
            //    GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            //    cube.transform.position = transform.position;
            //    _prevPos = transform.position;

            //}

            yield return null;
        }
        _state = PacketState.None;
    }
}
