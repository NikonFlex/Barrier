﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ship : MonoBehaviour
{
   public float m_Speed = 100;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
      float step = m_Speed * Time.deltaTime;
      Vector3 pos = transform.position;
      pos.x += step;
      transform.position = pos;
    }
}
