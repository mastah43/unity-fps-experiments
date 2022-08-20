using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieController : MonoBehaviour
{
    private static readonly ILogger Logger = Debug.unityLogger;

    private struct WalkMovement
    {
        public float LegRotate;
        public float ArmRotate;
        public float BodyUp;
    }

    [Tooltip("Position the zombie should walk to")]
    public Transform goal;

    [Tooltip("Walk leg angle maximum")]
    public double legWalkAngle = 15;

    private Transform body;
    private Transform neck;
    private Transform head;
    private Transform legLeftJoint;
    private Transform legRightJoint;
    private Transform legLeft;
    private Transform legRight;
    private Transform armLeftJoint;
    private Transform armRightJoint;
    private Transform armLeft;
    private Transform armRight;
    private Transform footLeftJoint;
    private Transform footRightJoint;
    private Transform footLeft;
    private Transform footRight;

    private double timeWalkStarted = -1;

    private float legLength;
    private NavMeshAgent agent;
    private Vector2 smoothDeltaPosition = Vector2.zero;
    private Vector2 velocity = Vector2.zero;

    void Start()
    {
        body = transform.Find("Body");
        neck = body.Find("Neck");
        head = body.Find("Head");

        legLeftJoint = body.Find("LegLeftJoint");
        legRightJoint = body.Find("LegRightJoint");
        legLeft = legLeftJoint.Find("Leg");
        legRight = legRightJoint.Find("Leg");

        armLeftJoint = body.Find("ArmLeftJoint");
        armRightJoint = body.Find("ArmRightJoint");
        armLeft = armLeftJoint.Find("Arm");
        armRight = armRightJoint.Find("Arm");

        footLeftJoint = legLeftJoint.Find("FootJoint");
        footRightJoint = legRightJoint.Find("FootJoint");
        footLeft = footLeftJoint.Find("Foot");
        footRight = footRightJoint.Find("Foot");
        legLength = legLeft.localScale.y + footLeft.localScale.y;

        agent = GetComponent<NavMeshAgent>();
        agent.destination = goal.position;

        // since we run animations for walking, position will be updated by controller
        agent.updatePosition = false;
    }

    void Update()
    {
        Walk();
    }

    private void Walk()
    {
        var worldDeltaPosition = agent.nextPosition - transform.position;

        // Map 'worldDeltaPosition' to local space
        var dx = Vector3.Dot(transform.right, worldDeltaPosition);
        var dy = Vector3.Dot(transform.forward, worldDeltaPosition);
        var deltaPosition = new Vector2(dx, dy);

        // Low-pass filter the deltaMove
        var smooth = Mathf.Min(1.0f, Time.deltaTime / 0.15f);
        smoothDeltaPosition = Vector2.Lerp(smoothDeltaPosition, deltaPosition, smooth);

        /*
        // TODO remove if not needed to stop the animation for walking upon reaching the target
        // Update velocity if time advances
        if (Time.deltaTime > 1e-5f)
            velocity = smoothDeltaPosition / Time.deltaTime;

        bool shouldMove = velocity.magnitude > 0.5f && agent.remainingDistance > agent.radius;

        // TODO move head in the direction of turning in a separate controller
        //GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;
        */

        if (timeWalkStarted < 0)
        {
            timeWalkStarted = Time.time;
        }

        var footLeftDistanceBefore = LeftFootFrontDistance();
        var footRightDistanceBefore = RightFootFrontDistance();

        // rotate legs
        var walkMovement = GetWalkMovement();
        var legRotate = walkMovement.LegRotate;
        legLeftJoint.Rotate(legRotate, 0, 0, Space.Self);
        legRightJoint.Rotate(-legRotate, 0, 0, Space.Self);

        // rotate feet
        footLeftJoint.Rotate(-legRotate, 0, 0, Space.Self);
        footRightJoint.Rotate(legRotate, 0, 0, Space.Self);

        // rotate arms
        armLeftJoint.Rotate(-walkMovement.ArmRotate, 0, 0, Space.Self);
        armRightJoint.Rotate(walkMovement.ArmRotate, 0, 0, Space.Self);

        // body move forward / backward
        // agent moves already forward but we need to correct the movement of the body
        // since the body is not moving linearly but more like in a sine curve
        var footLeftDistanceDelta = Math.Abs(LeftFootFrontDistance() - footLeftDistanceBefore);
        var footRightDistanceDelta = Math.Abs(RightFootFrontDistance() - footRightDistanceBefore);
        var walkDistance = Math.Max(footLeftDistanceDelta, footRightDistanceDelta);
        var walkDelta = walkDistance - agent.velocity.magnitude*Time.deltaTime;
        body.Translate(Vector3.forward * walkDelta);

        /* TODO remove or use log levels
        logger.Log($"Walk at time={Time.time}: agent speed={agent.velocity.magnitude}, foot delta: left={footLeftDistanceDelta}, right={footRightDistanceDelta},"
            + $" foot before: left={footLeftDistanceBefore}, right={footRightDistanceBefore},"
            + $" leg join rotation x: left={legLeftJoint.localRotation.x}, right={legRightJoint.localRotation.x }");
        */

        // move body up / down
        body.Translate(Vector3.up * walkMovement.BodyUp);

        // TODO
        //Logger.Log($"body move: fwd/bckwd: walkDelta={walkDelta}, walkDistance={walkDistance}, agent.velocity.magnitude={agent.velocity.magnitude}; up/down: {walkMovement.BodyUp}");

        transform.position = agent.nextPosition;
    }

    private WalkMovement GetWalkMovement()
    {
        var walkMovement = new WalkMovement();

        var footFrontDistanceMax = Math.Sin(legWalkAngle) * legLength;
        var stepMoveForward = footFrontDistanceMax * 2;
        var stepsPerSec = agent.velocity.magnitude / stepMoveForward;
        var stepRadPerSec = (double)stepsPerSec * Math.PI * 2d;
        var timeWalked = Time.time - timeWalkStarted;
        var timeWalkedBeforeUpdate = timeWalked - Time.deltaTime;
        double legRotation = this.walkLegRotation(stepRadPerSec, timeWalked);
        double legRotationBeforeUpdate = this.walkLegRotation(stepRadPerSec, timeWalkedBeforeUpdate);

        // TODO
        //Logger.Log($"walk movement: footFrontDistanceMax={footFrontDistanceMax}, walkStepsPerSec={stepsPerSec}");

        walkMovement.LegRotate = (float)(legRotationBeforeUpdate - legRotation);
        walkMovement.ArmRotate = walkMovement.LegRotate;

        // when legs split for walk step forward / backward then body moves down
        var bodyMoveYBeforeUpdate = -legLength * (1 - Math.Cos(degreesToRand(legRotationBeforeUpdate)));
        var bodyMoveY = -legLength * (1 - Math.Cos(degreesToRand(legRotation)));
        walkMovement.BodyUp = (float)(bodyMoveY - bodyMoveYBeforeUpdate);

        return walkMovement;
    }

    private static double degreesToRand(double degrees)
    {
        return degrees / 180d * Math.PI;
    }

    private float walkLegRotation(double stepRadPerSec, double timeWalked)
    {
        return (float)legWalkAngle * (float)Math.Sin(stepRadPerSec * timeWalked);
    }

    private float LeftFootFrontDistance()
    { 
        return FootDistance(legLeftJoint);
    }

    private float RightFootFrontDistance()
    {
        return FootDistance(legLeftJoint);
    }

    private float FootDistance(Transform legJoint)
    {
        return (float)Math.Sin(legJoint.localRotation.x) * legLength;
    }
}
