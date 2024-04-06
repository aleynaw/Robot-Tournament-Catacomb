using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class TA_Example_1 : CogsAgent
{
    // -------------------AGENT FUNCTIONS--------------------

    // Get relevant information from the environment to effectively learn behavior
    public override void CollectObservations(VectorSensor sensor)
    {
        //Target and Agent positions
        var localVelocity = transform.InverseTransformDirection(rBody.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);

        sensor.AddObservation(timer.GetComponent<Timer>().GetTimeRemaning());
        var localRotation = transform.rotation;
        sensor.AddObservation(transform.rotation.y);

        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(baseLocation.localPosition);

        foreach (GameObject target in targets){
            sensor.AddObservation(target.transform.localPosition);
            sensor.AddObservation(target.GetComponent<Target>().GetCarried());
            sensor.AddObservation(target.GetComponent<Target>().GetInBase());
        }

        sensor.AddObservation(IsFrozen());
    }

    // What to do when an action is received (i.e. when the Brain gives the agent information about possible actions)
    public override void OnActionReceived(ActionBuffers actions)
    {
        var act = actions.DiscreteActions;
        AddReward(-0.0005f);
        int forwardAxis = (int)act[0]; //NN output 0

        movePlayer(forwardAxis, (int)act[1], (int)act[2], (int)act[3], (int)act[4]);

    }

    // For manual check of controls 
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0;
        discreteActionsOut[1] = 0;
        if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[1] = 2;
        }
        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 2;
        }

        discreteActionsOut[2] = Input.GetKey(KeyCode.Space) ? 1 : 0;

        discreteActionsOut[3] = Input.GetKey(KeyCode.A) ? 1:0;

        discreteActionsOut[4] = Input.GetKey(KeyCode.S) ? 1:0;
     }


    // ----------------------OVERRIDE FUNCTIONS------------------------
    // Functions that require being defined in both CogsAgent(as virtual functions) and MyAgent

    protected override void Start()
    {
        base.Start();
        Material mat;

            mat = (Material) Resources.Load<Material>("AgentMat"); 
        // }
        // else {
        //     mat = (Material) Resources.Load<Material>(WorldConstants.agent2ID + "/AgentMat"); 
        // }
        gameObject.GetComponent<Renderer>().material = mat;

        rewardDict = new Dictionary<string, float>();

        rewardDict.Add("frozen", -0.1f);
        rewardDict.Add("shooting-laser", 0f);
        rewardDict.Add("hit-enemy", 0.5f);
    }

    
    protected override void FixedUpdate() {
        base.FixedUpdate();
        

        LaserControl();

        moveAgent(dirToGo, rotateDir);
    }

    protected override void OnTriggerEnter(Collider collision)
    {
        base.OnTriggerEnter(collision);

        
        if (collision.gameObject.CompareTag("HomeBase") && collision.gameObject.GetComponent<HomeBase>().team == GetTeam())
        {
            AddReward(GetCarrying() * 0.1f); 
        }
    }

    protected override void OnCollisionEnter(Collision collision) 
    {
        base.OnCollisionEnter(collision);

        //target is not in my base and is not being carried and I am not frozen
        if (collision.gameObject.CompareTag("Target") && collision.gameObject.GetComponent<Target>().GetInBase() != GetTeam() && collision.gameObject.GetComponent<Target>().GetCarried() == 0 && !IsFrozen())
        {
            SetReward(0.5f);
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
        }
    }



    //  --------------------------HELPERS---------------------------- 
    private void movePlayer(int forwardAxis, int rotateAxis, int shootAxis, int goToTargetAxis, int goToBaseAxis)
    {
        dirToGo = Vector3.zero;
        rotateDir = Vector3.zero;

        // moveSpeed = maxMoveSpeed - (0.05f * carriedTargets.Count);
        // turnSpeed = maxTurnSpeed - (10f * carriedTargets.Count);


        SetLaser(false);

        switch (forwardAxis)
        {
            case 0: //do nothing
                break;
            case 1:
                dirToGo = transform.forward;
                break;
            case 2:
                dirToGo = -transform.forward;
                break;
        }
        switch (rotateAxis)
        {
            case 0: //do nothing
                break;
            case 1:
                rotateDir = -transform.up;
                break;
            case 2:
                rotateDir = transform.up;
                break;
        }


        switch (shootAxis)
        {
            case 0: //do nothing
                break;
            case 1:
                SetLaser(true);
                break;
        }
        
         switch (goToTargetAxis)
        {
             case 0: //do nothing
                break;
            case 1:
                goToNearestTarget();
                break;
        }

         switch (goToBaseAxis)
        {
             case 0: //do nothing
                break;
            case 1:
                goToBase();
                break;
        }

        
    }

    private void goToBase(){
        turnAndGo(getYAngle(myBase));
    }

    private void goToNearestTarget(){
        GameObject target = getNearestTarget();
        if (target != null){
            float rotation = getYAngle(target);
            turnAndGo(rotation);
        }        
    }

    private void turnAndGo(float rotation){

        if(rotation < -5f){
            rotateDir = transform.up;
        }
        else if (rotation > 5f){
            rotateDir = -transform.up;
        }
        else {
            dirToGo = transform.forward;
        }
    }

    // return reference to nearest target
    protected GameObject getNearestTarget(){
        float distance = 200;
        GameObject nearestTarget = null;
        foreach (var target in targets)
        {
            float currentDistance = Vector3.Distance(target.transform.localPosition, transform.localPosition);
            if (currentDistance < distance && target.GetComponent<Target>().GetCarried() == 0 && target.GetComponent<Target>().GetInBase() != team){
                distance = currentDistance;
                nearestTarget = target;
            }
        }
        return nearestTarget;
    }

    private float getYAngle(GameObject target) {
        
       Vector3 targetDir = target.transform.position - transform.position;
       Vector3 forward = transform.forward;

      float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
      return angle; 
        
    }
}


