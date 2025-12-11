using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrabObject_2 : MonoBehaviour
{
    [Header("Hand Settings")]
    // 인스펙터에서 이 스크립트가 왼손용인지 오른손용인지 결정
    public ARAVRInput.Controller handSide = ARAVRInput.Controller.RTouch; 

    [Header("Physics Settings")]
    public LayerMask grabbedLayer;      // 잡을 수 있는 물체의 레이어 (Bomb)
    public float grabRange = 0.3f;      // 잡기 감지 반경
    public float throwPower = 1.3f;     // 던질 때 힘 배율 (1.3배 정도가 적당)
    public float rotPower = 1.0f;       // 던질 때 회전력 배율

    // 내부 상태 변수
    private bool isGrabbing = false;    // 현재 잡고 있는지 여부
    private GameObject grabbedObject;   // 잡고 있는 물체
    
    // 부드러운 던지기를 위한 속도 저장소 (Queue)
    // 순간 속도만 쓰면 손이 멈칫할 때 바닥에 떨어지므로, 최근 기록의 평균을 사용함
    private Queue<Vector3> recentVelocities = new Queue<Vector3>();
    private int maxVelocitySamples = 10; // 최근 10프레임의 속도를 기억

    // 물리 계산을 위한 이전 프레임 데이터
    Vector3 prevPos;
    Quaternion prevRot;

    // [헬퍼 프로퍼티] 현재 설정된 손(Left/Right)의 위치를 자동으로 반환
    Vector3 CurrentHandPosition
    {
        get
        {
            return (handSide == ARAVRInput.Controller.LTouch) ? ARAVRInput.LHandPosition : ARAVRInput.RHandPosition;
        }
    }

    // [헬퍼 프로퍼티] 현재 설정된 손의 트랜스폼(부모 지정용) 반환
    Transform CurrentHandTransform
    {
        get
        {
            return (handSide == ARAVRInput.Controller.LTouch) ? ARAVRInput.LHand : ARAVRInput.RHand;
        }
    }

    void Update()
    {
        // 잡지 않은 상태면 잡기 시도, 잡은 상태면 놓기/던지기 처리
        if (isGrabbing == false)
        {
            TryGrab();
        }
        else
        {
            HoldingAndUnGrab();
        }
    }

    // ============================
    //  1. 물체 잡기 시도 (TryGrab)
    // ============================
    private void TryGrab()
    {
        // 지정된 손(handSide)의 버튼이 눌렸는지 확인
        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, handSide))
        {
            // 손 주변의 물체들 탐색 (OverlapSphere)
            Collider[] hitObjects = Physics.OverlapSphere(CurrentHandPosition, grabRange, grabbedLayer);
            
            GameObject closestObject = null;
            float closestDistance = float.MaxValue;

            // 가장 가까운 물체 찾기
            for (int i = 0; i < hitObjects.Length; i++)
            {
                // 물리 컴포넌트(Rigidbody)가 없으면 잡을 수 없음
                if (hitObjects[i].GetComponent<Rigidbody>() == null) continue;

                // 거리 비교
                float distance = Vector3.Distance(hitObjects[i].transform.position, CurrentHandPosition);
                if (distance < closestDistance)
                {
                    closestObject = hitObjects[i].gameObject;
                    closestDistance = distance;
                }
            }

            // 잡을 물체가 확정되면
            if (closestObject != null)
            {
                isGrabbing = true;
                grabbedObject = closestObject;
                
                // [중요] 물리를 끄고(Kinematic), 손의 자식으로 만듦
                grabbedObject.GetComponent<Rigidbody>().isKinematic = true;
                grabbedObject.transform.parent = CurrentHandTransform;
                
                // 손 위치로 착 붙이기 (선택 사항)
                grabbedObject.transform.localPosition = Vector3.zero; 
                grabbedObject.transform.localRotation = Quaternion.identity;

                // 물리 연산 변수 초기화
                prevPos = CurrentHandPosition;
                prevRot = CurrentHandTransform.rotation;
                recentVelocities.Clear(); // 이전 속도 기록 삭제
            }
        }
    }

    // ==========================================
    //  2. 잡고 있는 중 & 놓기 처리 (Holding)
    // ==========================================
    private void HoldingAndUnGrab()
    {
        // 현재 프레임의 데이터 가져오기
        Vector3 currentHandPos = CurrentHandPosition;
        Quaternion currentHandRot = CurrentHandTransform.rotation;

        // 순간 속도 계산: (현재위치 - 과거위치) / 시간
        Vector3 currentVelocity = (currentHandPos - prevPos) / Time.deltaTime;
        
        // [속도 버퍼링] 큐에 현재 속도 저장 (던질 때 평균 내려고)
        recentVelocities.Enqueue(currentVelocity);
        if (recentVelocities.Count > maxVelocitySamples)
        {
            recentVelocities.Dequeue(); // 너무 오래된 데이터는 버림
        }

        // 회전 변화량 계산 (쿼터니온 차이)
        Quaternion deltaRotation = currentHandRot * Quaternion.Inverse(prevRot);

        // 다음 프레임을 위해 현재 상태 저장
        prevPos = currentHandPos;
        prevRot = currentHandRot;

        // 버튼을 떼면 놓기(던지기) 실행
        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, handSide))
        {
            isGrabbing = false;

            Rigidbody rb = grabbedObject.GetComponent<Rigidbody>();
            
            // [중요] 물리 다시 켜고, 부모 관계 끊기(독립)
            rb.isKinematic = false;
            grabbedObject.transform.parent = null;

            // [평균 속도 계산] 큐에 저장된 속도들의 평균을 구함
            Vector3 averageVelocity = Vector3.zero;
            foreach (Vector3 v in recentVelocities)
            {
                averageVelocity += v;
            }
            if(recentVelocities.Count > 0) 
                averageVelocity /= recentVelocities.Count;

            // 계산된 평균 속도로 던짐 (Move Average)
            rb.velocity = averageVelocity * throwPower;

            // 회전력(Angular Velocity) 적용 로직
            float angle;
            Vector3 axis;
            deltaRotation.ToAngleAxis(out angle, out axis); // 회전축과 각도 추출
            if (angle > 180) angle -= 360; // 180도 넘어가면 음수로 변환 (최단 경로)
            
            // 라디안 단위로 변환하여 적용
            Vector3 angularVelocity = (angle * Mathf.Deg2Rad * axis) / Time.deltaTime;
            rb.angularVelocity = angularVelocity * rotPower;

            // 변수 초기화
            grabbedObject = null;
            recentVelocities.Clear();
        }
    }


}