using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ZombieController : MonoBehaviour
{
    private static readonly ILogger Logger = Debug.unityLogger;

    private struct WalkMovement
    {
        public float LegRotateDelta;
        public float LegRotateTotal;
        public float ArmRotateDelta;
        public float FootRotateDelta;
        public float BodyUpDelta;
        public float BodyForwardDelta;
        public float BodyForwardTotal;
        public double StepsWalked;
    }

    [Tooltip("Position the zombie should walk to")]
    public Transform goal;

    [Tooltip("Walk leg angle maximum")]
    public double legWalkAngle = 20;

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

    private WalkMovement walkMovement;

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
                   + $", bodyUp={walkMovement.BodyUpDelta}, bodyForward={walkMovement.BodyForwardDelta}"  
                   + $", BodyForwardTotal={walkMovement.BodyForwardTotal}");
        
        ApplyWalkMovement();
    }

    private void ApplyWalkMovement()
    {
        // rotate legs
        legLeftJoint.Rotate(walkMovement.LegRotateDelta, 0, 0, Space.Self);
        legRightJoint.Rotate(-walkMovement.LegRotateDelta, 0, 0, Space.Self);

        // rotate feet
        footLeftJoint.Rotate(walkMovement.FootRotateDelta, 0, 0, Space.Self);
        footRightJoint.Rotate(-walkMovement.FootRotateDelta, 0, 0, Space.Self);

        // rotate arms
        armLeftJoint.Rotate(walkMovement.ArmRotateDelta, 0, 0, Space.Self);
        armRightJoint.Rotate(-walkMovement.ArmRotateDelta, 0, 0, Space.Self);

        // body move forward / backward
        body.Translate(Vector3.forward * walkMovement.BodyForwardDelta);

        // move body up / down
        body.Translate(Vector3.up * walkMovement.BodyUpDelta);
    }

    private void UpdateWalkMovement(ref WalkMovement movement)
    {
        // TODO move walking to separate controller or class and also reduce complexity in this method
        
        var agentSpeedRatio = 1; // agent.velocity.magnitude / agent.speed;
        // TODO adapt the walk angle depending on the agent speed - there were problems with not continuous leg rotation
        //var agentSpeedRatio = agent.velocity.magnitude / agent.speed;
        if (agentSpeedRatio == 0) return; // no walking on standing
        
        var legWalkAngleForCurrentSpeed = agentSpeedRatio * legWalkAngle;
        var stepMoveForward = Math.Sin(legWalkAngleForCurrentSpeed) * legLength;
        var stepsPerSec = agent.velocity.magnitude / stepMoveForward;
        
        double legRotationBeforeUpdate = movement.LegRotateTotal;
        
        var stepsWalkDelta = stepsPerSec * Time.deltaTime;
        var stepsWalkedBefore = movement.StepsWalked; 
        movement.StepsWalked += stepsWalkDelta;
        
        // One step is considered the movement of a leg from center to front / front to center / center to back or back to center
        double legRotation = (float)legWalkAngleForCurrentSpeed * (float)Math.Sin(Math.PI * movement.StepsWalked);

        // TODO here already minor error is added when leg rotation surpasses the max walk angle
        movement.LegRotateDelta = (float)(legRotation - legRotationBeforeUpdate);
        movement.LegRotateTotal += movement.LegRotateDelta;
        movement.FootRotateDelta = movement.LegRotateDelta;
        movement.ArmRotateDelta = -movement.LegRotateDelta;

        // when legs split for walk step forward / backward then body moves down
        var bodyUpBeforeUpdate = -legLength * (1 - Math.Cos(DegreesToRand(legRotationBeforeUpdate)));
        var bodyUp = -legLength * (1 - Math.Cos(DegreesToRand(legRotation)));
        movement.BodyUpDelta = (float)(bodyUp - bodyUpBeforeUpdate);
        
        // realistically move body forward / backward to adapt continues agent movement by realistic 
        // alternating walking speed due to steps
        
        // TODO some loss occurs here because foot is moving front and back - how to detect it?
        var footForwardPassed = (long)((movement.StepsWalked - 0.5) / 1L) - (long)((stepsWalkedBefore - 0.5) / 1L) == 1; 
        var footDistanceDelta = footForwardPassed ? 
            Math.Abs(FootDistanceByLegRotation(legWalkAngleForCurrentSpeed) - FootDistanceByLegRotation(legRotation)) + 
            Math.Abs(FootDistanceByLegRotation(legWalkAngleForCurrentSpeed) - FootDistanceByLegRotation(legRotationBeforeUpdate)): 
            Math.Abs(FootDistanceByLegRotation(legRotation) - FootDistanceByLegRotation(legRotationBeforeUpdate));
        var agentDistanceDelta = agent.velocity.magnitude * Time.deltaTime;
        movement.BodyForwardDelta = footDistanceDelta - agentDistanceDelta;
        var bodyForwardTotalBefore = movement.BodyForwardTotal; 
        movement.BodyForwardTotal += movement.BodyForwardDelta;
        if (Math.Abs(bodyForwardTotalBefore) < Math.Abs(movement.BodyForwardTotal))
        {
            // TODO there is some problem with the walking speed too slow (maybe due to sin driving leg rotation which does not consider
            // the slow leg rotation on changing direction (foot forward and then backward)
            movement.BodyForwardTotal -= movement.BodyForwardDelta;
            movement.BodyForwardDelta = 0;
        }
        
        Logger.Log($"walk update: footDistanceDelta={footDistanceDelta}, agentDistanceDelta={agentDistanceDelta}, stepNowCompleted={footForwardPassed}, legRotation={legRotation}, legRotationBeforeUpdate={legRotationBeforeUpdate}");
        
    }

    private static double DegreesToRand(double degrees) => degrees / 180d * Math.PI;
    
    private float FootDistanceByLegRotation(double legRotation) => (float)Math.Abs(Math.Sin(DegreesToRand(legRotation)) * legLength);
    
}
