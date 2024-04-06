using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Target : MonoBehaviour
{
    private int carried, inBase;

    Vector3 base1co, base2co, startingCo;

    Vector3 positionInBase = Vector3.zero;

    protected GameObject[] players, bases;

    private float yPos = 0.5f;

    private int spotInBase;
    Rigidbody rb;



    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        SetMass(0);
        startingCo = new Vector3(transform.localPosition.x, transform.localPosition.y, transform.localPosition.z);
        players = GameObject.FindGameObjectsWithTag("Player");
        bases = GameObject.FindGameObjectsWithTag("HomeBase");
        base1co = bases[0].transform.localPosition;
        base2co = bases[1].transform.localPosition;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(transform.localPosition.y < 0.5f){
            transform.localPosition = new Vector3(transform.localPosition.x, 0.5f, transform.localPosition.z);
        }
        
        if (carried != 0 || inBase != 0){
            rb.useGravity = false;
        }
        else {
            rb.useGravity = true;
        }

        if(carried == 0 && inBase != 0){
            this.transform.localPosition = positionInBase;
        }

    }

    public void ZeroRotation(){
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
    }

    public void Explode(){
        SetMass(1);
        SetCarry(0);
        rb.velocity = new Vector3(Random.Range(-10f, 10f), 1, Random.Range(-10f, 10f));
        rb.angularVelocity = Vector3.zero;
    }
    
    void OnCollisionEnter(Collision collision){
        if(collision.gameObject.CompareTag("Floor") || collision.gameObject.CompareTag("Player")){
            ZeroRotation();
            SetMass(0);
        }
    }

     void OnTriggerEnter(Collider collision){

     }
     
     void OnTriggerExit(Collider collision){
         if(collision.gameObject.CompareTag("HomeBase")){ 
             inBase = 0;
         }
         
     }

    public void AddToBase(int spot, int playerTeam, Vector3 position){
        SetInBase(playerTeam);
        SetCarry(0);
        positionInBase = position;
        SetSpotInBase(spot);
        ZeroRotation();
     }

     public void Carry(int team){
         SetCarry(team);
         SetMass(0);
         ZeroRotation();
     }

     public void AddToSpotInbase(int spot, Vector3 position){
         positionInBase = position;
         spotInBase = spot;
     }

     public void ResetGame(){
        transform.localPosition = startingCo;
        ZeroRotation();
        SetCarry(0);
        SetInBase(0);
        SetSpotInBase(0);
     }
    
    

    // --------------GETTERS----------------
    public int GetCarried(){return carried;}
    public int GetInBase(){return inBase;}
    public int GetSpotInBase(){return spotInBase;}


    // --------------SETTERS----------------
    public void SetCarry(int team){carried = team;}
    public void SetMass(float mass){rb.mass = mass;}
    public void SetInBase(int baseNum){inBase = baseNum;}
    public void SetSpotInBase(int spot){spotInBase = spot;}
    public void SetYPos(float y){yPos = y;}
}