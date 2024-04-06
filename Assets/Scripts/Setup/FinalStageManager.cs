using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class FinalStageManager : MonoBehaviour
{
    // Start is called before the first frame update
    public bool isTraining = false;
    public Text winnerTextbox;
    public GameObject timer;

    public GameObject agent1;
    public GameObject agent2;
    public GameObject base1;
    public GameObject base2;
    public Text base1CountTxt;
    public Text base2CountTxt;
    public Camera cam1, cam2;

    GameObject[] targets;
    GameObject[] players;

    CogsAgent agent1Script, agent2Script;

    void Awake() {
        if (GameObject.FindGameObjectsWithTag("Player").Length == 0) {
            agent1 = Resources.Load<GameObject>(WorldConstants.agent1ID + "/" + WorldConstants.agent1ID);
            agent2 = Resources.Load<GameObject>(WorldConstants.agent2ID + "/" + WorldConstants.agent2ID);
            agent1 = Instantiate(agent1);
            agent2 = Instantiate(agent2);
            
            agent1.name = "Agent 1";
            agent2.name = "Agent 2";
        }
        else {
            players = GameObject.FindGameObjectsWithTag("Player"); 
            agent1 = players[0];
            agent2 = players[1];

            agent1.name = "Agent 1";
            agent2.name = "Agent 2";
        }
    }
    
    void Start()
    {
        targets = GameObject.FindGameObjectsWithTag("Target");

        agent1.transform.SetParent(transform);
        agent2.transform.SetParent(transform);
        cam1.transform.SetParent(agent1.transform);
        cam2.transform.SetParent(agent2.transform);
        cam1.transform.localPosition = new Vector3(0f, 1.5f, -5f);
        cam2.transform.localPosition = new Vector3(0f, 1.5f, -5f);
        cam1.transform.localRotation = Quaternion.identity;
        cam2.transform.localRotation = Quaternion.identity;
        
        winnerTextbox.enabled = false;
        agent1Script = agent1.GetComponent(WorldConstants.agent1ID) as CogsAgent;
        agent2Script = agent2.GetComponent(WorldConstants.agent2ID) as CogsAgent;
    }


    // Update is called once per frame
    void FixedUpdate()
    { 
        bool timerIsRunning = timer.GetComponent<Timer>().GetTimerIsRunning();

        int base1Num = base1.GetComponent<HomeBase>().GetCaptured();
        int base2Num = base2.GetComponent<HomeBase>().GetCaptured();
        int agent1Carry = agent1Script.GetCarrying();
        int agent2Carry = agent2Script.GetCarrying();

        float agent1BaseDist = agent1Script.DistanceToBase();
        float agent2BaseDist = agent2Script.DistanceToBase();

        base1CountTxt.text = "[A1] " + WorldConstants.agent1ID + ": " + base1Num.ToString();
        base2CountTxt.text = "[A2] " + WorldConstants.agent2ID + ": " + base2Num.ToString();
     
        if (!timerIsRunning)
        {
            if (base1Num > base2Num)
            {
                agent1Script.SetReward(1f);
                agent2Script.SetReward(-1f);
                Debug.Log("Agent 1 wins by capture");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (base2Num > base1Num)
            {
                agent1Script.SetReward(-1f);
                agent2Script.SetReward(1f);
                Debug.Log("Agent 2 wins by capture");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            else if (agent1Carry > agent2Carry)
            {
                agent1Script.SetReward(1f);
                agent2Script.SetReward(-1f);
                Debug.Log("Agent 1 wins by carry");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (agent2Carry > agent1Carry)
            {
                agent1Script.SetReward(-1f);
                agent2Script.SetReward(1f);
                Debug.Log("Agent 2 wins by carry");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            else if (agent1BaseDist < agent2BaseDist && agent1Carry != 0)
            {
                agent1Script.SetReward(1f);
                agent2Script.SetReward(-1f);
                Debug.Log("Agent 1 wins by distance");
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 1 wins";
            }
            
            else if (agent2BaseDist < agent1BaseDist && agent2Carry != 0)
            {
                agent1Script.SetReward(-1f);
                agent2Script.SetReward(1f);
                Debug.Log("Agent 2 wins by distance");                
                winnerTextbox.enabled = true;
                winnerTextbox.text = "Agent 2 wins";
            }
            
            else {
                agent1Script.SetReward(0f);
                agent2Script.SetReward(0f);
                Debug.Log("Draw!");

                winnerTextbox.enabled = true;
                winnerTextbox.text = "Draw";
            }

            if (isTraining) {
                Reset();
            }
            else {
                StopGame();
            }
            
        }
    }

    void Reset() {
        timer.GetComponent<Timer>().Reset();
        base1.GetComponent<HomeBase>().Reset();
        base2.GetComponent<HomeBase>().Reset();
        foreach (GameObject target in targets)
        {
            target.GetComponent<Target>().ResetGame();
        }
        
        agent1Script.EndEpisode();
        agent2Script.EndEpisode();
    }

    void StopGame() {
        Time.timeScale = 0;
    }
}