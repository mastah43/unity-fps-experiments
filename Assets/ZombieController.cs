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
        public float FootRotate;
        public float BodyUp;
        public float BodyForward;
        public float BodyForwardTotal;
        public double StepsWalked;
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

    private WalkMovement walkMovement = new WalkMovement();

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
    }

    void Update()
    {
        Walk();
    }

    private void Walk()
    {
        // TODO make actor stand with arms down and legs down and not split when movement comes to a stop
        // TODO make leg angle larger and shorter dependent on agent speed

        /*
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
        */

        // TODO move head in the direction of turning in a separate controller
        //GetComponent<LookAt>().lookAtTargetPosition = agent.steeringTarget + transform.forward;

        UpdateWalkMovement(ref walkMovement);
        Logger.Log($"Walk at time={Time.time}: agent velocity={agent.velocity.magnitude}"
                   + $", bodyUp={walkMovement.BodyUp}, bodyForward={walkMovement.BodyForward}"  
                   + $", BodyForwardTotal={walkMovement.BodyForwardTotal}");
        
        ApplyWalkMovement();
    }

    private void ApplyWalkMovement()
    {
        // rotate legs
        legLeftJoint.Rotate(walkMovement.LegRotate, 0, 0, Space.Self);
        legRightJoint.Rotate(-walkMovement.LegRotate, 0, 0, Space.Self);

        // rotate feet
        footLeftJoint.Rotate(walkMovement.FootRotate, 0, 0, Space.Self);
        footRightJoint.Rotate(-walkMovement.FootRotate, 0, 0, Space.Self);

        // rotate arms
        armLeftJoint.Rotate(walkMovement.ArmRotate, 0, 0, Space.Self);
        armRightJoint.Rotate(-walkMovement.ArmRotate, 0, 0, Space.Self);

        // body move forward / backward
        body.Translate(Vector3.forward * walkMovement.BodyForward);

        // move body up / down
        body.Translate(Vector3.up * walkMovement.BodyUp);
    }

    private void UpdateWalkMovement(ref WalkMovement movement)
    {
        var footFrontDistanceMax = Math.Sin(legWalkAngle) * legLength;
        var stepMoveForward = footFrontDistanceMax; 
        var stepsPerSec = agent.velocity.magnitude / stepMoveForward;
        
        double legRotationBeforeUpdate = this.WalkLegRotation(movement.StepsWalked);
        
        var stepsWalkDelta = stepsPerSec * Time.deltaTime;
        movement.StepsWalked += stepsWalkDelta;
        double legRotation = this.WalkLegRotation(movement.StepsWalked);

        movement.LegRotate = (float)(legRotation - legRotationBeforeUpdate);
        movement.FootRotate = movement.LegRotate;
        movement.ArmRotate = -movement.LegRotate;

        // when legs split for walk step forward / backward then body moves down
        var bodyMoveYBeforeUpdate = -legLength * (1 - Math.Cos(DegreesToRand(legRotationBeforeUpdate)));
        var bodyMoveY = -legLength * (1 - Math.Cos(DegreesToRand(legRotation)));
        movement.BodyUp = (float)(bodyMoveY - bodyMoveYBeforeUpdate);
        
        // realistically move body forward / backward to adapt continues agent movement by realistic 
        // alternating walking speed due to steps
        var footDistanceDelta = Math.Abs(FootDistanceByLegRotation(legRotation) - FootDistanceByLegRotation(legRotationBeforeUpdate));
        var agentDistanceDelta = agent.velocity.magnitude * Time.deltaTime;
        movement.BodyForward = footDistanceDelta - agentDistanceDelta;
        movement.BodyForwardTotal += movement.BodyForward;
    }

    private static double DegreesToRand(double degrees) => degrees / 180d * Math.PI;
    
    /**
     * One step is considered the movement of a leg from center to front and moving back to center
     */
    private float WalkLegRotation(double stepsWalked) => (float)legWalkAngle * (float)Math.Sin(Math.PI * stepsWalked);

    private float FootDistanceByLegRotation(double legRotation) => (float)Math.Sin(DegreesToRand(legRotation)) * legLength;
    
}
