using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public Camera topCamera;
    public Camera P1Camera;
    public Camera P2Camera;

    // Start is called before the first frame update
    void Start()
    {
        topCamera.enabled = true;
        P1Camera.enabled = false;
        P2Camera.enabled = false;
    }

    private bool closeUp1 = false;
    private bool closeUp2 = false;
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1)) {
            topCamera.enabled = true;
            P1Camera.enabled = false;
            P2Camera.enabled = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2)) {
            topCamera.enabled = false;
            P1Camera.enabled = true;
            P2Camera.enabled = false;

            P1Camera.rect = new Rect(0f, 0f, 1f, 1f);

            if (closeUp1) {
                P1Camera.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            }
            else {
                P1Camera.transform.localPosition = new Vector3(0f, 1.5f, -5f);
            }

            closeUp1 = !closeUp1;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3)) {
            topCamera.enabled = false;
            P1Camera.enabled = false;
            P2Camera.enabled = true;

            P2Camera.rect = new Rect(0f, 0f, 1f, 1f);     

            if (closeUp2) {
                P2Camera.transform.localPosition = new Vector3(0f, 1.5f, 0f);
            }
            else {
                P2Camera.transform.localPosition = new Vector3(0f, 1.5f, -5f);
            }

            closeUp2 = !closeUp2;      
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4)) {
            topCamera.enabled = false;
            P1Camera.enabled = true;
            P2Camera.enabled = true;

            P1Camera.rect = new Rect(0f, 0f, 0.5f, 1f);
            P2Camera.rect = new Rect(0.5f, 0f, 0.5f, 1f);
        }
    }
}
