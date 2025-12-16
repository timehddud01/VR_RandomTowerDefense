using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportCurve : MonoBehaviour
{

    public Transform teleportCircleUI;
    LineRenderer lr;
    Vector3 originScale = Vector3.one *0.02f;
    public int lineSmooth = 40;
    public float curveLength = 50;
    public float gravity = -60;
    public float simulateTime = 0.02f;
    List<Vector3> lines = new List<Vector3>();
    public bool isWarp = false;
    public float warpTime = 0.1f;

    // Start is called before the first frame update
    void Start()
    {   
        teleportCircleUI.gameObject.SetActive(false);
        lr = GetComponent<LineRenderer>();
        lr.startWidth = 0.0f;
        lr.endWidth = 0.2f;
        
    }

    // Update is called once per frame
    void Update()
    {
        if(ARAVRInput.GetDown(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.RTouch)){
            lr.enabled = true;

        }
        else if(ARAVRInput.GetUp(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.RTouch)){
            lr.enabled = false;
            if(teleportCircleUI.gameObject.activeSelf){
                GetComponent<CharacterController>().enabled = false;
                transform.position = teleportCircleUI.position + Vector3.up;
                GetComponent<CharacterController>().enabled = true;
            }
            teleportCircleUI.gameObject.SetActive(false);
        }
        else if(ARAVRInput.Get(ARAVRInput.Button.IndexTrigger, ARAVRInput.Controller.RTouch)){
            MakeLines();
        }
    }



    void MakeLines(){
        lines.RemoveRange(0,lines.Count);
        Vector3 dir = ARAVRInput.RHandDirection * curveLength;
        Vector3 pos = ARAVRInput.RHandPosition;
        lines.Add(pos);

        for(int i = 0; i<lineSmooth; i++){
            Vector3 lastPos = pos;
            dir.y += gravity * simulateTime;
            pos += dir * simulateTime;
            if( CheckHitRay(lastPos, ref pos)){
                lines.Add(pos);
                break;
            }
            else {
                teleportCircleUI.gameObject.SetActive(false);
            }
            lines.Add(pos);
        }

        lr.positionCount = lines.Count;
        lr.SetPositions(lines.ToArray());

    }
//ref 참조에 의한 매개 변수 전달, 값이 할당된 변수를 사용해야 한다
    private bool CheckHitRay(Vector3 lastPos, ref Vector3 pos){
        Vector3 rayDir = pos - lastPos;
        Ray ray = new Ray(lastPos, rayDir);
        RaycastHit hitinfo;

        if(Physics.Raycast(ray, out hitinfo, rayDir.magnitude)){
            pos = hitinfo.point;


            int layer = LayerMask.NameToLayer("Terrain");
            if(hitinfo.transform.gameObject.layer == layer){
                teleportCircleUI.gameObject.SetActive(true);
                teleportCircleUI.position = pos;
                teleportCircleUI.forward = hitinfo.normal;
                float distance = (pos - ARAVRInput.RHandPosition).magnitude;

                teleportCircleUI.localScale = originScale * Mathf.Max(1, distance);
            }
            return true;
        }


        return false;
    }
}
