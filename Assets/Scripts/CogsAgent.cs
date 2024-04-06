using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Random = UnityEngine.Random;

public class CogsAgent : Agent
{
    // ------------------------VARIABLES----------------------------
    // public: direct access available for all classes
    // protected: direct access restricted to CogsAgent and MyAgent
    // private: direct access restricted to CogsAgent only;
    //          indirect access via Getters and Setters if they are provided

    // Basic objects where information about the agent and environment can be accessed
    protected Rigidbody rBody;
    protected GameObject timer, enemy, myBase;
    // Accessible from timer:
    // - GetTimeRemaining(): get time remaining for the current game
    // - GetTimerIsRunning(): whether the timer is running
    // Accessible from HomeBase:
    // - team: the team of the base
    // - GetCaptured(): get the number of captured targets

    protected Transform baseLocation; 
    protected int team; 
    
    protected GameObject[] targets; // list of all targets in the environment
    // Accessible for each target:
    // - GetCarried(): whether the target is being carried
    // - GetInBase(): whether the target is in a base

    protected List<GameObject> carriedTargets; // list of targets currently being carried by the agent

    protected Vector3 dirToGo, rotateDir; // vectors for direction to go towards and direction to rotate
    private const float maxMoveSpeed = 1.5f; 
    private float maxTurnSpeed = 100;
    
    private const float m_LaserLength = 20;
    private GameObject myLaser;
    private bool m_Shoot, frozen, invincible;
    private float frozenTime,invinceTime, moveSpeed, turnSpeed;

    private static float frozenDuration = 3f;
    private static float invinceDuration = frozenDuration + 1f;

    // This variable stores reward values for common actions
    // as key-value pairs. Example: <"frozen", -1f> specifies the
    // reward for being frozen as -1f. These values can be changed
    // in your own agent script
    protected Dictionary<string, float> rewardDict;



    // ---------------MONOBEHAVIOR FUNCTIONS------------------
    // Functions called at the beginning and update functions
    // INGORE

    protected void Awake()
    {
        rBody = GetComponent<Rigidbody>();
        targets = GameObject.FindGameObjectsWithTag("Target");

        timer = GameObject.FindGameObjectWithTag("Timer");
        
        

        myLaser = transform.Find("Laser").gameObject;
    }

    protected virtual void Start() {
        GameObject[] agents = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject agent in agents) {
            if (agent != gameObject) {
                enemy = agent;
            }
        }     
        if (transform.name == "Agent 1") {
            team = 1;
        }
        else team = 2;
        myBase = GameObject.Find("Base " + team);
        baseLocation = myBase.transform;

        dirToGo = Vector3.zero;
        rotateDir = Vector3.zero;
        carriedTargets = new List<GameObject>(); 
    }

    protected virtual void FixedUpdate() {
        moveSpeed = maxMoveSpeed - (0.05f * carriedTargets.Count);
        turnSpeed = maxTurnSpeed - (10f * carriedTargets.Count);
        
        if (turnSpeed < 0) {
            turnSpeed = 0;
         }


        if (rBody.velocity.sqrMagnitude > 25f) // slow it down
        {
            float maxVelocity = 0.95f - 0.03f * carriedTargets.Count;
            if (maxVelocity <= 0.6f) { maxVelocity = 0.6f; }
            rBody.velocity *= maxVelocity;

        }

        if (Time.time > frozenTime + frozenDuration && frozen)
        {
            frozen = false;
        }
        if (Time.time > invinceTime + invinceDuration && invincible){
            invincible = false;
        }

        int numCarry = 0;
        float xCo = this.transform.localPosition.x;
        float zCo = this.transform.localPosition.z;
        foreach (GameObject target in carriedTargets)
        {
            numCarry++;
            target.transform.localPosition = new Vector3(xCo, numCarry * 1.2f, zCo);
        }
    }
    


    // ------------------AGENT FUNCTIONS-----------------------
    
    // Called at the beginning of an episode to reset environment
    public override void OnEpisodeBegin()
    {
        rBody.velocity = Vector3.zero;
        transform.localPosition = new Vector3(baseLocation.localPosition.x, 0.5f, baseLocation.localPosition.z);
        if (team == 1)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f, -125f, 0f));
        }
        else if (team == 2)
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f, 50f, 0f));
        }
        
        carriedTargets.Clear();
        frozen = false;
        invincible = false;

    }



    // ---------------------GAME SETUP-----------------------
    
    // Freeze the agent (when hit by another agent's laser)
    private void Freeze() {
        if (rewardDict.ContainsKey("frozen")) AddReward(rewardDict["frozen"]);
        if (!invincible){
            frozen = true;
            invincible = true;
            frozenTime = Time.time;
            invinceTime = Time.time;
        }
    }
    
    // Drop carried targets (when hit by laser)
    private void DropCarrying() {
        if (rewardDict.ContainsKey("dropped-targets")) AddReward(rewardDict["dropped-targets"]);
        for (int i = carriedTargets.Count - 1; i > -1; i--)
            {
                GameObject currentTarget = carriedTargets[i];
                currentTarget.GetComponent<Target>().Explode();
                carriedTargets.Remove(currentTarget);
                
                if (rewardDict.ContainsKey("dropped-one-target")) AddReward(rewardDict["dropped-one-target"]);
            }
    }

    // IGNORE
    protected virtual void OnTriggerEnter(Collider collision)
    {
        
    }

    // ???
    protected virtual void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Target") && collision.gameObject.GetComponent<Target>().GetInBase() != team 
            && collision.gameObject.GetComponent<Target>().GetCarried() == 0 && !frozen)
        { 
            collision.gameObject.GetComponent<Target>().Carry(team);        
            carriedTargets.Add(collision.gameObject);
        }
        
    }


    
    // ----------------------USEFUL HELPERS------------------------
    // For controlling agent's processing and actions
    
    //Called within FixedUpdate to control the movement of the agent
    public void moveAgent(Vector3 dirToGo, Vector3 rotateDir){
        if(!IsFrozen()){
            if (!IsLaserOn()){
                rBody.AddForce(dirToGo * GetMoveSpeed(), ForceMode.VelocityChange);
            }
            transform.Rotate(rotateDir, Time.deltaTime * GetTurnSpeed());
        }
    }

    // try activating laser and return true if laser succesfully activated, false otherwise
    protected bool LaserControl() {
        if (IsLaserOn() && !IsFrozen()) 
        {
            if (rewardDict.ContainsKey("shooting-laser")) AddReward(rewardDict["shooting-laser"]);
            maxTurnSpeed = 50;
            var myTransform = transform;
            var rayDir = m_LaserLength * myTransform.forward;
            Debug.DrawRay(myTransform.position, rayDir, Color.red, 0f, true);
            RaycastHit hit;
            float laserHitDistance = m_LaserLength;
            if (Physics.SphereCast(transform.position, 0.25f, rayDir, out hit, m_LaserLength,1, QueryTriggerInteraction.Ignore))
            {
                laserHitDistance = hit.distance;
                if (hit.collider.gameObject.CompareTag("Player"))
                {
                    if (rewardDict.ContainsKey("hit-enemy")) AddReward(rewardDict["hit-enemy"]);
                    hit.collider.gameObject.GetComponent<CogsAgent>().DropCarrying();
                    hit.collider.gameObject.GetComponent<CogsAgent>().Freeze();
                }
            }
            myLaser.transform.localPosition = new Vector3(0f,0f,(laserHitDistance / 2f) + 0.5f);
            myLaser.transform.localScale = new Vector3(1f, 1f, laserHitDistance);
            return true;
        }
        else
        {
            myLaser.transform.localScale = new Vector3(0f, 0f, 0f);
            myLaser.transform.localPosition = new Vector3(0f,0f,0f);
            maxTurnSpeed = 100;
            return false;
        }
    }

    // return the distance from the agent and the home base
    public float DistanceToBase(){
        return Vector3.Distance(myBase.transform.localPosition, transform.localPosition);
    }


    // --------------GETTERS----------------
    // Can be used to get private information from CogsAgent
    public int GetTeam() {return team;}     
    protected float GetMoveSpeed() {return moveSpeed;}
    protected float GetTurnSpeed() {return turnSpeed;}
    public bool IsLaserOn() {return m_Shoot;}  // true if the laser command is set to 'on' (not necessarily that laser is being shot)  
    public bool IsFrozen() {return frozen;}
    public float GetFrozenTime() {return frozenTime;}
    public int GetCarrying() {return carriedTargets.Count;}   
    public GameObject GetCarry(int i){return carriedTargets[i];}

    
    
    // --------------SETTERS----------------
    // Can be used to set private values in CogsAgent

    // Set laser command (true = on, false = off)
    protected void SetLaser(bool on) {m_Shoot = on;}

    // Remove a target that is currently being carried
    public void RemoveCarry(GameObject target){carriedTargets.Remove(target);}

}
