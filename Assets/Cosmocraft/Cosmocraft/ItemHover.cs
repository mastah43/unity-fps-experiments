using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Cosmocraft
{

    public class ItemHover : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            transform.position += Vector3.up * 0.25f * (float)Math.Sin(Time.time) * Time.deltaTime;
            transform.Rotate(0, 90f * Time.deltaTime, 0, Space.Self);
        }
    }

}