using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuoyLauncher : MonoBehaviour
{
    [SerializeField] private float _launchSpeed; // m/s
    [SerializeField] private float _angleSpeed; // grad/s
    [SerializeField] private Packet _packet;
    [SerializeField] private GameObject _target;
    private bool _isRotate = true;
    private Quaternion _startRotation;
    private Quaternion _finishRotation;
    private float _currentRotationTime;
    private float _rotationPeriod = 5f;

    void Start()
    {
        _startRotation = transform.rotation;
        _finishRotation = Quaternion.Euler(calculateVertAngleOnTarget(), calculateHorAngleOnTarget(), transform.rotation.z);
        float vertRotationPeriod = Mathf.Abs(Mathf.DeltaAngle(_startRotation.eulerAngles.x, _finishRotation.eulerAngles.x)) / _angleSpeed;
        float horRotationPeriod = Mathf.Abs(Mathf.DeltaAngle(_startRotation.eulerAngles.y, _finishRotation.eulerAngles.y)) / _angleSpeed;
        _rotationPeriod = Mathf.Max(vertRotationPeriod, horRotationPeriod);
    }

    void Update()
    {
        if (_isRotate)
        {
            _currentRotationTime += Time.deltaTime;
            if (_currentRotationTime > _rotationPeriod)
            {
                _isRotate = false;
                _packet.Launch(_launchSpeed, transform.forward);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(_startRotation, _finishRotation, _currentRotationTime / _rotationPeriod);
            }
            return;
        }
        //if (_isRotate)
        //{
        //    transform.rotation = _finishRotation;
        //    _packet.Launch(_launchSpeed, transform.forward);
        //    _isRotate = false;
        //}
    }

    private float calculateHorAngleOnTarget()
    {
        Vector3 dir_on_target = _target.transform.position - transform.position;
        return Vector3.SignedAngle(dir_on_target, transform.forward, Vector3.down);
    }

    private float calculateVertAngleOnTarget()
    {
        Vector3[] solutions = new Vector3[2];
        int numSolutions;
        numSolutions = fts.solve_ballistic_arc(transform.position, _launchSpeed, _target.transform.position, 9.8f, out solutions[0], out solutions[1]);
        if (numSolutions > 0)
            return Vector3.SignedAngle(solutions[0], new Vector3(solutions[0].x, 0, solutions[0].z), Vector3.left);
        return 0;
    }
}