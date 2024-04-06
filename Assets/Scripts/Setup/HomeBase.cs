using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomeBase : MonoBehaviour
{
    public bool training;
    
    public int team;
    private GameObject player;

    private List<Vector3> positionsInBase = new List<Vector3>();
    private List<bool> openSpots = new List<bool>();
    private List<GameObject> capturedTargets;
    private GameObject[] players;

    void Start()
    {
        player = GameObject.Find("Agent " + team);

        for (int i = 0; i < 9; i++){
            openSpots.Add(true);
        }

        float x = transform.localPosition.x;
        float z = transform.localPosition.z;
        float y = 0.5f;
        
        positionsInBase.Add(new Vector3(x + 3,y,z + 3));
        positionsInBase.Add(new Vector3(x + 3,y,z));
        
        positionsInBase.Add(new Vector3(x,y,z + 3));
        
        positionsInBase.Add(new Vector3(x + 3,y,z - 3));

        positionsInBase.Add(new Vector3(x,y,z));

        positionsInBase.Add(new Vector3(x - 3,y,z + 3));
        
        positionsInBase.Add(new Vector3(x,y,z - 3));
        
        positionsInBase.Add(new Vector3(x - 3,y,z));
        positionsInBase.Add(new Vector3(x - 3,y,z - 3));

        if(team == 1){
            positionsInBase.Reverse();
        }
        

        Material mat;
        string id;
        if (team == 1) id = WorldConstants.agent1ID;           
        else id = WorldConstants.agent2ID;

        mat = (Material) Resources.Load<Material>(id + "/Resources/HomeBaseMat"); 
        gameObject.GetComponentInChildren<Renderer>().material = mat;
        

        capturedTargets = new List<GameObject>();
        
    }

    public void Reset(){
        for (int i = 0; i < 9; i++){
            openSpots[i] = true;
        }
        capturedTargets.Clear();
    }

    public int AddToFirstSpotInBase(){
        for (int i = 0; i < 9; i++){
                if(openSpots[i]){
                    openSpots[i] = false;
                    return i;
                }
            }
        return -1;
    }

    void OnTriggerEnter(Collider collision){
        if (collision.gameObject.CompareTag("Player"))
        {
            GameObject player = collision.gameObject;

            CogsAgent agentScript = player.GetComponent(WorldConstants.agent1ID) as CogsAgent;
            if (agentScript == null) agentScript = player.GetComponent(WorldConstants.agent2ID) as CogsAgent;
            if (agentScript.GetTeam() == team){
            for (int i = agentScript.GetCarrying() - 1; i > -1; i--)
                {
                    GameObject currentTarget = agentScript.GetCarry(i);
                    capturedTargets.Add(currentTarget);
                    int spot = AddToFirstSpotInBase();
                    Vector3 position = GetPosition(spot);
                    currentTarget.GetComponent<Target>().AddToBase(spot, team, position);
                    agentScript.RemoveCarry(currentTarget);

                }
            }
        }
    }
    void OnTriggerExit(Collider collision){
         if(collision.gameObject.CompareTag("Target")){ 
            capturedTargets.Remove(collision.gameObject);
            openSpots[collision.gameObject.GetComponent<Target>().GetSpotInBase()] = true;             
         }

         
    }

    public Vector3 GetPosition(int i){ return positionsInBase[i];}



    // --------------GETTERS----------------
    public int GetCaptured() {return capturedTargets.Count;}
}