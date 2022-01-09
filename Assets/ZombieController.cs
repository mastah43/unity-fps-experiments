using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ZombieController : MonoBehaviour
{


    Transform head;
    Transform legLeft;
    Transform legRight;

    void Start()
    {
        head = transform.Find("Body/Head");
        legLeft = transform.Find("Body/LegLeft");
        legRight = transform.Find("Body/LegRight");
    }

    void Update()
    {
        head.Rotate(0, 45f * (float)Math.Sin(Time.time) * Time.deltaTime, 0, Space.Self);
        legLeft.Rotate(45f * (float)Math.Sin(Time.time) * Time.deltaTime, 0, 0, Space.Self);
        legRight.Rotate(-45f * (float)Math.Sin(Time.time) * Time.deltaTime, 0, 0, Space.Self);
    }
}
