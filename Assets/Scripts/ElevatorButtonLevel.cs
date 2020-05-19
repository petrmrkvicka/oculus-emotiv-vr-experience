using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Collections.Generic;
using System.Linq;


public class ElevatorButtonLevel : MonoBehaviour
{
    // Start is called before the first frame update
    public TextMeshPro text;
    public int level;
    public bool isHighlighted;
    public Material highlightedMaterial;
    protected int isDoorOpened;
    protected GameObject doorLeft;
    protected GameObject doorRight;
    protected GameObject elevatorObj;
    protected float elevatorSpeed;
    private float timer;
    protected float moveElevatorToY;


    void Start()
    {
        transform.position = new Vector3(-8.14f, transform.position.y, transform.position.z);

        text.SetText(level.ToString());
        MeshRenderer thisRenderer = GetComponent<MeshRenderer>();

        if(isHighlighted)
        {
            thisRenderer.material = highlightedMaterial;
        }

        doorLeft = GameObject.Find("doorLeft");
        doorRight = GameObject.Find("doorRight");
        elevatorObj = GameObject.Find("Elevator");
        elevatorSpeed = 2.0f;

        isDoorOpened = 2;
    }

    // Update is called once per frame
    void Update()
    {
        if(moveElevatorToY > 0){
            elevatorObj.transform.position = Vector3.MoveTowards(elevatorObj.transform.position, new Vector3(elevatorObj.transform.position.x, moveElevatorToY, elevatorObj.transform.position.z), elevatorSpeed * Time.deltaTime);

            GameObject[] leftDoors = GameObject.FindGameObjectsWithTag("DoorLeft");
            int countLeft = 1;
            foreach (GameObject lDoor in leftDoors)
            {
                if(Mathf.Abs(lDoor.transform.position.y - moveElevatorToY) < 2){
                    doorLeft = lDoor;
                    countLeft = 0;
                } else {
                    countLeft++;
                }
                
            }
            GameObject[] rightDoors = GameObject.FindGameObjectsWithTag("DoorRight");
            int countRight = 1;
            foreach (GameObject rDoor in rightDoors)
            {
                if(Mathf.Abs(rDoor.transform.position.y - moveElevatorToY) < 2){
                    doorRight = rDoor;
                    countRight = 0;
                } else {
                    countRight++;
                }
                
            }

            if(elevatorObj.transform.position.y == moveElevatorToY){
                moveElevatorToY = 0;
                isDoorOpened = 1;
            }
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
            GameObject[] leftDoors = GameObject.FindGameObjectsWithTag("DoorLeft");
            foreach (GameObject lDoor in leftDoors)
            {
                lDoor.transform.position = Vector3.MoveTowards(lDoor.transform.position, new Vector3(lDoor.transform.position.x, lDoor.transform.position.y, -2.92f), elevatorSpeed * Time.deltaTime);
            }
            GameObject[] rightDoors = GameObject.FindGameObjectsWithTag("DoorRight");
            foreach (GameObject rDoor in rightDoors)
            {
                rDoor.transform.position = Vector3.MoveTowards(rDoor.transform.position, new Vector3(rDoor.transform.position.x, rDoor.transform.position.y, -1.825f), elevatorSpeed * Time.deltaTime);
            }
            timer += Time.deltaTime;
            if(timer > 2.0f && timer < 3.0f){
                MoveToLevel();
            }

            if(timer > 5.0f){
                isDoorOpened = 2;
                timer = 0f;
            }
        }
    }

    void OnTriggerStay(Collider col) {
        if(col.gameObject.name == "LeftHandSphereCollider" || col.gameObject.name == "RightHandSphereCollider")
        {
            Transform trans = transform;
            //Debug.Log(Mathf.Abs(col.transform.position.z - trans.position.z));
            if(Mathf.Abs(col.transform.position.z - trans.position.z) < 0.04f && Mathf.Abs(col.transform.position.y - trans.position.y) < 0.04f){
                OVRInput.Controller whichTouch;
                whichTouch = col.gameObject.name == "LeftHandSphereCollider" ? OVRInput.Controller.LTouch : OVRInput.Controller.RTouch;

                if(col.transform.position.x < -8.14f && col.transform.position.x > -8.18f) {
                    transform.position = new Vector3(col.transform.position.x, transform.position.y, transform.position.z);
                    OVRInput.SetControllerVibration(2f, 0.3f, whichTouch);
                } else if(col.transform.position.x <= -8.18f) {
                    transform.position = new Vector3(-8.18f, transform.position.y, transform.position.z);
                    OVRInput.SetControllerVibration(2f, 0.7f, whichTouch);
                } else {
                    transform.position = new Vector3(-8.14f, transform.position.y, transform.position.z);
                    OVRInput.SetControllerVibration(0, 0, whichTouch);
                }

                if(col.transform.position.x < -8.15f && col.transform.position.x > -8.18f) {
                    Debug.Log("Close Door, go to level");
                    isDoorOpened = 0;
                    
                }
            }
        }
    }

    private void MoveToLevel(){
        GameObject[] elevatorPositions = GameObject.FindGameObjectsWithTag("Elevator").OrderBy(g=>g.transform.position.y).ToArray();
        int count = 1;

        foreach (GameObject elPos in elevatorPositions)
        {
            //Debug.Log(elPos.transform.position.y);

            if(count == level){
                moveElevatorToY = elPos.transform.position.y;
            }

            count++;
        }
    }

    void OnTriggerExit(Collider col){
        OVRInput.SetControllerVibration(0, 0,OVRInput.Controller.LTouch);
        OVRInput.SetControllerVibration(0, 0,OVRInput.Controller.RTouch);
        transform.position = new Vector3(-8.14f, transform.position.y, transform.position.z);
    }
}
