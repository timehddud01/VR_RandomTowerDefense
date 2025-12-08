using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabObject : MonoBehaviour
{

    bool isGrabbing = false;
    GameObject grabbedObj;
    public LayerMask grabbedLayer;
    public float grabRange = .2f;

    Vector3 prevPos;
    float throwPower = 1000;

    Quaternion prevRot;
    public float rotPower =5;
    public bool isRemoteGrab = true;
    public float remoteGrabDustance = 20;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(isGrabbing == false){
            TryGrab();
        }
        else{
            TryUngrab();
        }
    }

    private void TryGrab(){
        if(ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.RTouch)){

            Collider[] hitObjects = Physics.OverlapSphere(ARAVRInput.RHandPosition, grabRange,grabbedLayer);

            int closest = -1;
            float closestDistance = float.MaxValue;

            for (int i = 0; i< hitObjects.Length; i ++){
                var rigid = hitObjects[i].GetComponent<Rigidbody>();
                if(rigid == null) continue;

                Vector3 nextPos = hitObjects[i].transform.position;
                float nextDistance = Vector3.Distance(nextPos, ARAVRInput.RHandPosition);

                if(nextDistance < closestDistance){
                    closest = i;
                    closestDistance = nextDistance;
                }


                if(closest > -1){
                    isGrabbing = true;
                    grabbedObj = hitObjects[closest].gameObject;
                    grabbedObj.transform.parent = ARAVRInput.RHand;
                    Debug.Log("grabbed");
                
                    grabbedObj.GetComponent<Rigidbody>().isKinematic =true;
                    prevPos = ARAVRInput.RHandPosition;
                    prevRot = ARAVRInput.RHand.rotation;
                
                }
            }
        }
    }

    private void TryUngrab(){
        Vector3 throwDirection = (ARAVRInput.RHandPosition - prevPos);
        prevPos = ARAVRInput.RHandPosition;

        Quaternion deltaRotation = ARAVRInput.RHand.rotation * Quaternion.Inverse(prevRot);

        prevRot = ARAVRInput.RHand.rotation;

        if(ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, ARAVRInput.Controller.RTouch)){
        isGrabbing = false;
        grabbedObj.GetComponent<Rigidbody>().isKinematic = false;
        grabbedObj.transform.parent = null;

        grabbedObj.GetComponent<Rigidbody>().velocity = throwDirection * throwPower;


        float angle;
        Vector3 axis;
        deltaRotation.ToAngleAxis(out angle, out axis);
        Vector3 angularVelocity = (1.0f/Time.deltaTime) * angle * axis;
        grabbedObj.GetComponent<Rigidbody>().angularVelocity = angularVelocity;


                grabbedObj = null;
        }


    }
}
