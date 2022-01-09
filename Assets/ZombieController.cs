using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

public class ZombieController : MonoBehaviour
{


    Transform head;
    Transform legLeftJoint;
    Transform legRightJoint;

    void Start()
    {
        head = transform.Find("Head");
        legLeftJoint = transform.Find("LegLeftJoint");
        legRightJoint = transform.Find("LegRightJoint");
    }

    void Update()
    {
        head.Rotate(0, 45f * (float)Math.Sin(Time.time) * Time.deltaTime, 0, Space.Self);
        legLeftJoint.Rotate(45f * (float)Math.Sin(Time.time) * Time.deltaTime, 0, 0, Space.Self);
        legRightJoint.Rotate(-45f * (float)Math.Sin(Time.time) * Time.deltaTime, 0, 0, Space.Self);
    }
}
