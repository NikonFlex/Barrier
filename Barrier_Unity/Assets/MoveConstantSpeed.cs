using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveConstantSpeed : MonoBehaviour
{
   [SerializeField] float _speed = 1;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      transform.position += transform.forward * _speed * Time.deltaTime;
    }
}
