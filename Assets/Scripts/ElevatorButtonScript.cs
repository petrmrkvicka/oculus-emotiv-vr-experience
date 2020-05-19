using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ElevatorButtonScript : MonoBehaviour
{

    //private GameObject sphere;
    private GameObject textBlock;
    private string thisPosition;
    private string colliderPosition;
    protected Text updatesText;
    protected GameObject elevator;
    public GameObject elevatorPosition;
    public GameObject doorLeft;
    public GameObject doorRight;
    protected bool isElevatorCalled;
    protected int isDoorOpened;
    public float elevatorSpeed;
    private float timer;


    void Start()
    {
        //sphere = GameObject.Find("HandSphereCollider");
        textBlock = GameObject.Find("TextOnHand");
        updatesText = textBlock.GetComponent<Text>();

        transform.position = new Vector3(-4.6f, transform.position.y, transform.position.z);

        elevator = GameObject.Find("Elevator");

        isElevatorCalled = false;
        isDoorOpened = 0;
        timer = 0.0f;
    }

    private void ElevatorIsDown() {
        if(isElevatorCalled == true){
            isDoorOpened = 1;
            Debug.Log("Open Door");
        }
        isElevatorCalled = false;
    }

    // Update is called once per frame
    private void Update() {
        if(isElevatorCalled == true)
            elevator.transform.position = Vector3.MoveTowards(elevator.transform.position, new Vector3(elevator.transform.position.x, elevatorPosition.transform.position.y, elevator.transform.position.z), elevatorSpeed * Time.deltaTime);

        if(elevator.transform.position == new Vector3(elevator.transform.position.x, elevatorPosition.transform.position.y, elevator.transform.position.z)){
            ElevatorIsDown();
        }

        if(isDoorOpened == 1) {
            doorLeft.transform.position = Vector3.MoveTowards(doorLeft.transform.position, new Vector3(doorLeft.transform.position.x, doorLeft.transform.position.y, -3.72f), elevatorSpeed * Time.deltaTime);
            doorRight.transform.position = Vector3.MoveTowards(doorRight.transform.position, new Vector3(doorRight.transform.position.x, doorRight.transform.position.y, -1.025f), elevatorSpeed * Time.deltaTime);
            
            timer += Time.deltaTime;
            if(timer > 5.0f){
                isDoorOpened = 2;
                timer = 0f;
            }
        } else if(isDoorOpened == 0) {
            doorLeft.transform.position = Vector3.MoveTowards(doorLeft.transform.position, new Vector3(doorLeft.transform.position.x, doorLeft.transform.position.y, -2.92f), elevatorSpeed * Time.deltaTime);
            doorRight.transform.position = Vector3.MoveTowards(doorRight.transform.position, new Vector3(doorRight.transform.position.x, doorRight.transform.position.y, -1.825f), elevatorSpeed * Time.deltaTime);
            timer += Time.deltaTime;
            if(timer > 5.0f){
                isDoorOpened = 2;
                timer = 0f;
            }
        }
            
    }

    void OnTriggerStay(Collider col) {
        

        if(col.gameObject.name == "LeftHandSphereCollider" || col.gameObject.name == "RightHandSphereCollider")
        {
            thisPosition = transform.position.ToString();
            colliderPosition = col.transform.position.ToString();
            updatesText.text = thisPosition + colliderPosition;
            //Debug.Log(updatesText.text);

            OVRInput.Controller whichTouch;
            whichTouch = col.gameObject.name == "LeftHandSphereCollider" ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;

            
            if(col.transform.position.x < -4.6f && col.transform.position.x > -4.612f) {
                transform.position = new Vector3(col.transform.position.x, transform.position.y, transform.position.z);
                OVRInput.SetControllerVibration(2f, 0.3f, whichTouch);
            } else if(col.transform.position.x <= -4.612f) {
                transform.position = new Vector3(-4.612f, transform.position.y, transform.position.z);
                OVRInput.SetControllerVibration(2f, 0.7f, whichTouch);
            } else {
                transform.position = new Vector3(-4.6f, transform.position.y, transform.position.z);
                OVRInput.SetControllerVibration(0, 0, whichTouch);
            }

            if(col.transform.position.x < -4.609f && col.transform.position.x > -4.612f) {
                Debug.Log("elevator called");
                isElevatorCalled = true;
            }

            
        }
    }

    void OnTriggerExit(Collider col){
        OVRInput.SetControllerVibration(0, 0,OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0,OVRInput.Controller.RTouch);

        if(col.gameObject.name == "HandSphereCollider")
        {
            transform.position = new Vector3(-4.6f, transform.position.y, transform.position.z);
        }
    }

}
