using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;

public class Catacomb : CogsAgent
{
    // ------------------BASIC MONOBEHAVIOR FUNCTIONS-------------------
    
    // Initialize values

    private int carry;
    protected override void Start()
    {
        base.Start();
        AssignBasicRewards();
        carry = 0;
    }

    // For actual actions in the environment (e.g. movement, shoot laser)
    // that is done continuously
    protected override void FixedUpdate() {
        base.FixedUpdate();
        
        LaserControl();
        // Movement based on DirToGo and RotateDir
        moveAgent(dirToGo, rotateDir);

        //d=√((x_2-x_1)²+(y_2-y_1)²)

        DistanceIncentive(); 
        


        
    }


    
    // --------------------AGENT FUNCTIONS-------------------------

    // Get relevant information from the environment to effectively learn behavior
    public override void CollectObservations(VectorSensor sensor)
    {
        // Agent velocity in x and z axis 
        var localVelocity = transform.InverseTransformDirection(rBody.velocity);
        sensor.AddObservation(localVelocity.x);
        sensor.AddObservation(localVelocity.z);

        // Time remaning
        sensor.AddObservation(timer.GetComponent<Timer>().GetTimeRemaning());  

        // Agent's current rotation
        var localRotation = transform.rotation;
        sensor.AddObservation(transform.rotation.y);

        // Agent and home base's position
        sensor.AddObservation(this.transform.localPosition);
        sensor.AddObservation(baseLocation.localPosition);

        // for each target in the environment, add: its position, whether it is being carried,
        // and whether it is in a base
        foreach (GameObject target in targets){
            sensor.AddObservation(target.transform.localPosition);
            sensor.AddObservation(target.GetComponent<Target>().GetCarried());
            sensor.AddObservation(target.GetComponent<Target>().GetInBase());
        }
        
        // Whether the agent is frozen
        sensor.AddObservation(IsFrozen());
    }

    // For manual override of controls. This function will use keyboard presses to simulate output from your NN 
    public override void Heuristic(in ActionBuffers actionsOut)
{
        var discreteActionsOut = actionsOut.DiscreteActions;
        discreteActionsOut[0] = 0; //Simulated NN output 0
        discreteActionsOut[1] = 0; //....................1
        discreteActionsOut[2] = 0; //....................2
        discreteActionsOut[3] = 0; //....................3
        discreteActionsOut[4] = 0;

       
        if (Input.GetKey(KeyCode.UpArrow))
        {
            discreteActionsOut[0] = 1;
        }       
        if (Input.GetKey(KeyCode.DownArrow))
        {
            discreteActionsOut[0] = 2;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            discreteActionsOut[1] = 1;
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            discreteActionsOut[1] = 2;
        }
        

        //Shoot
        if (Input.GetKey(KeyCode.Space)){
            discreteActionsOut[2] = 1;
        }

        //GoToNearestTarget
        if (Input.GetKey(KeyCode.A)){
            discreteActionsOut[3] = 1;
        }


        if (Input.GetKey(KeyCode.B)){
            discreteActionsOut[4] = 1;
        }

    }

        // What to do when an action is received (i.e. when the Brain gives the agent information about possible actions)
        public override void OnActionReceived(ActionBuffers actions){

        

        int forwardAxis = (int)actions.DiscreteActions[0]; //NN output 0

        int rotateAxis = (int)actions.DiscreteActions[1]; 
        int shootAxis = (int)actions.DiscreteActions[2]; 
        int goToTargetAxis = (int)actions.DiscreteActions[3];
        int goToBaseAxis = (int)actions.DiscreteActions[4];
        
        MovePlayer(forwardAxis, rotateAxis, shootAxis, goToTargetAxis, goToBaseAxis);

        

    }


// ----------------------ONTRIGGER AND ONCOLLISION FUNCTIONS------------------------
    // Called when object collides with or trigger (similar to collide but without physics) other objects
    protected override void OnTriggerEnter(Collider collision)
    {
        base.OnTriggerEnter(collision);
        
        if (collision.gameObject.CompareTag("HomeBase") && collision.gameObject.GetComponent<HomeBase>().team == GetTeam())
        {
            AddReward(carry * 0.8f);
            Debug.Log("Adding this much reward: "+ carry * 0.8f);
            carry = 0;

            if (CheckWinning()) {
                Debug.Log("Winning? Yes!");
                Debug.Log("Adjusting attack rewards...");
                rewardDict["shooting-laser"] = 0.9f;
                rewardDict["hit-enemy"] = 1.0f;
            }
            else {
                Debug.Log("Winning? Nope!");
            }

            

            // stop re-picking up targets in base LOL
            //  if (collision.gameObject.CompareTag("Target") && collision.gameObject.GetComponent<Target>().GetInBase() != GetTeam() && collision.gameObject.GetComponent<Target>().GetCarried() == 0 && !IsFrozen())
            // {
            //     AddReward(-0.1f);
            // }

            if (collision.gameObject.CompareTag("HomeBase") && collision.gameObject.GetComponent<HomeBase>().team == GetTeam()) 
            {
            //     if (myBase.GetComponent<HomeBase>().GetCaptured() == 0) {
            //        AddReward(-0.4f);
            //        Debug.Log("Stop coming back without balls");
            //        Debug.Log("Reward added: " + -0.4f);
            //  }
            
            }



        }
        

        
    }

    protected override void OnCollisionEnter(Collision collision) 
    {
        base.OnCollisionEnter(collision);
        

        //target is not in my base and is not being carried and I am not frozen
        // if (collision.gameObject.CompareTag("Target") && collision.gameObject.GetComponent<Target>().GetInBase() != GetTeam() && collision.gameObject.GetComponent<Target>().GetCarried() == 0 && !IsFrozen())
        // {

            
        //     AddReward(0.5f);
        //     Debug.Log("You picked up a ball!");
        //     Debug.Log("Reward added: " + 0.5f);

        //     if(GetCarrying() > 3) {
        //         SetReward(-0.1f);
        //         Debug.Log("Try not to carry more than 3 balls");
        //         Debug.Log("Reward set: " + -0.1f);
        //     }

            

           
        // }

        if (collision.gameObject.CompareTag("Target") && collision.gameObject.GetComponent<Target>().GetInBase() != GetTeam() && !IsFrozen())
        {

            
            AddReward(0.5f);
            carry++;
            Debug.Log("You picked up a ball!");
            Debug.Log("Reward added: " + 0.5f);

            if(GetCarrying() > 3) {
                AddReward(-0.5f);
                Debug.Log("Try not to carry more than 3 balls");
                Debug.Log("Reward added: " + -0.5f);
            }

            

           
        }

        if (collision.gameObject.CompareTag("Wall"))
        {
            AddReward(-0.1f);
            Debug.Log("Avoid the wall");
            Debug.Log("Reward added: " + -0.1f);
        }
        
    }

    void OnTriggerExit(Collider collision)
    {
        if (collision.gameObject.CompareTag("HomeBase") && collision.gameObject.GetComponent<HomeBase>().team == GetTeam()) 
        {
            // if (myBase.GetComponent<HomeBase>().GetCaptured() == 0) {
            //     SetReward(0.3f);
            //     Debug.Log("Yay! You left your base!");
            // }
            
        }
        // base.OnTriggerExit(collision);
        
    }

    void OnCollisionStay(Collision collisionInfo)
    {
        if (myBase.GetComponent<HomeBase>().GetCaptured() == 0) {
                SetReward(-0.1f);
                Debug.Log("Leave bro");
            }
    }



    //  --------------------------HELPERS---------------------------- 
     private void AssignBasicRewards() {
        rewardDict = new Dictionary<string, float>();

        rewardDict.Add("frozen", -0.1f);
        rewardDict.Add("shooting-laser", 0f);
        rewardDict.Add("hit-enemy", 0.5f);
        rewardDict.Add("dropped-one-target", 0f);
        rewardDict.Add("dropped-targets", 0f);
        // rewardDict.Add("dropped-targets-base", 1.0f);
    }
    
    private void MovePlayer(int forwardAxis, int rotateAxis, int shootAxis, int goToTargetAxis, int goToBaseAxis)
    {
        dirToGo = Vector3.zero;
        rotateDir = Vector3.zero;

        Vector3 forward = transform.forward;
        Vector3 backward = -transform.forward;
        Vector3 right = transform.up;
        Vector3 left = -transform.up;

        //fowardAxis: 
            // 0 -> do nothing
            // 1 -> go forward
            // 2 -> go backward
        if (forwardAxis == 0){
            //do nothing. This case is not necessary to include, it's only here to explicitly show what happens in case 0
        }
        else if (forwardAxis == 1){
            dirToGo = forward;
        }
        else if (forwardAxis == 2){
            dirToGo = backward;
            
        }

        //rotateAxis: 
            // 0 -> do nothing
            // 1 -> go right
            // 2 -> go left
        if (rotateAxis == 0){
            //do nothing
        }
        else if (rotateAxis == 1){
            rotateDir = right;
        }
        else if (rotateAxis == 2){
            rotateDir = left;
        }
        
        


        //shoot
        if (shootAxis == 1){
            SetLaser(true);
        }
        else {
            SetLaser(false);
        }

        //go to the nearest target
        if (goToTargetAxis == 1){
            GoToNearestTarget();
        }

        if (goToBaseAxis == 1){
            GoToBase();
        }
        
        
    }

    // Go to home base
    private void GoToBase(){
        TurnAndGo(GetYAngle(myBase));
    }

    // Go to the nearest target
    private void GoToNearestTarget(){
        GameObject target = GetNearestTarget();
        if (target != null){
            float rotation = GetYAngle(target);
            TurnAndGo(rotation);
        }        
    }

    // Rotate and go in specified direction
    private void TurnAndGo(float rotation){

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
    protected GameObject GetNearestTarget(){
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

    private float GetYAngle(GameObject target) {
        
       Vector3 targetDir = target.transform.position - transform.position;
       Vector3 forward = transform.forward;

      float angle = Vector3.SignedAngle(targetDir, forward, Vector3.up);
      return angle; 
        
    }

    private bool CheckWinning() {

        CogsAgent enemyAgent = enemy.GetComponent<CogsAgent>();
        int enemyTeam = 0;
        if (enemyAgent != null) {
            enemyTeam = enemyAgent.GetTeam();
        // Now you can use the team variable as needed
        }

        int ourBaseNum = myBase.GetComponent<HomeBase>().GetCaptured();
        Debug.Log("We have: " + ourBaseNum);

        var enemyBase = GameObject.Find("Base " + enemyTeam);   
        int enemyBaseNum = enemyBase.GetComponent<HomeBase>().GetCaptured(); 
        Debug.Log("They have: " + enemyBaseNum);    

        if (ourBaseNum > enemyBaseNum) {
            return true;
        }

        return false; 
    }

    private void DistanceIncentive() {

        if (GetCarrying() < 2) {

        var distance = 1/DistanceToTarget() *0.1f;
        AddReward(distance);
        
        Debug.Log("Getting closer... + " + distance);

        }

        if (GetCarrying() >= 2) {
        var baseDist = 1/DistanceToBase() * 0.1f;
        AddReward(baseDist);

        Debug.Log("Heading home... + " + baseDist);

        }

        if (CheckWinning()) {

            CogsAgent enemyAgent = enemy.GetComponent<CogsAgent>();

            float dist = Vector3.Distance(enemyAgent.transform.localPosition, this.transform.localPosition);

            var enemDist = 1/dist * 0.1f;
            AddReward(enemDist);

            Debug.Log("ATTACKKKK!!!!!");

        }
    }

    private float DistanceToTarget() {


        float dist = Vector3.Distance(GetNearestTarget().transform.localPosition, this.transform.localPosition);

        if (dist <= 1.0f) {
            dist = 1.0f;
        }

        return dist;
    }

    

}
