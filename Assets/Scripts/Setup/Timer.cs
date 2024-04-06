using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public const float timeLimit = 120;
    private float timeRemaining;
    private bool timerIsRunning = true;
    private Text timerTextUI;
    private float minutes, seconds;
    private string timerText = "";



    void Start(){
        timeRemaining = timeLimit;
        timerIsRunning = true;

        timerTextUI = GameObject.Find("TimerUI").GetComponent<Text>();
    }

    void Update()
    {
    
        if (timeRemaining > 0) {
            timeRemaining -= Time.deltaTime;
            minutes = Mathf.FloorToInt(timeRemaining / 60);
            seconds = Mathf.FloorToInt(timeRemaining % 60);
            if (Mathf.RoundToInt(seconds) < 10) {
                timerText = minutes + ":0" + seconds;
            }
            else {
                timerText = minutes + ":" + seconds;
            }
        }
        else {
            timerText = "Time's up!";
            timeRemaining = 0;
            timerIsRunning = false;
        }

        timerTextUI.text = timerText;
        
    }

    public void Reset(){
        timeRemaining = timeLimit;
        timerIsRunning = true;
    }
    


    // -------------GETTERS----------------
    public float GetTimeRemaning() {
        return timeRemaining;
    }

    public bool GetTimerIsRunning() {
        return timerIsRunning;
    }
}
