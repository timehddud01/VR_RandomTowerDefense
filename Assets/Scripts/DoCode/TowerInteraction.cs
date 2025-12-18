using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TowerInteraction : MonoBehaviour
{
    [Header("Hand Settings")]
    public ARAVRInput.Controller handSide = ARAVRInput.Controller.RTouch;

    [Header("Grab Settings")]
    public float grabDistance = 10.0f;
    public LayerMask interactionLayer;
    public Vector3 holdOffset = new Vector3(0, 0, 0.2f);

    [Header("Gaze UI Settings")]
    public Transform uiCanvas;   
    public Image gazeImg;        
    public float uiCanvasVal = 1f; 
    private Vector3 defaultScale;  

    [Header("Gaze Logic Settings")]
    public Transform playerHead;
    public float gazeChargeTime = 2.0f;
    public float maxGazeDistance = 2000.0f;

    // --- 내부 상태 변수 ---
    private bool isGrabbing = false;
    private GameObject heldObject;
    private PlatformSlot originalSlot;
    private int originalLayer;

    private float curGazeTime = 0f;
    private GameObject currentGazeTarget;

    // 편의 속성
    Vector3 CurrentHandPosition => (handSide == ARAVRInput.Controller.LTouch) ? ARAVRInput.LHandPosition : ARAVRInput.RHandPosition;
    Vector3 CurrentHandDirection => (handSide == ARAVRInput.Controller.LTouch) ? ARAVRInput.LHandDirection : ARAVRInput.RHandDirection;
    Transform CurrentHandTransform => (handSide == ARAVRInput.Controller.LTouch) ? ARAVRInput.LHand : ARAVRInput.RHand;

    void Start()
    {
        if (uiCanvas != null)
        {
            defaultScale = uiCanvas.localScale;
            uiCanvas.gameObject.SetActive(false);
        }
        
        if (gazeImg != null)
        {
            gazeImg.fillAmount = 0f;
        }
    }

    void Update()
    {
        // [DEBUG] 시선이 닿는 곳 확인
        DebugCheckLookingAt();

        if (!isGrabbing)
        {
            TryGrab();
        }
        else
        {
            HandleHoldingAndRelease();

            // 물건을 잡고 있을 때만 시선 합성 로직 작동
            if (isGrabbing)
            {
                HandleGazeMerging();
            }
        }
    }

    // ==========================================
    //  1. 타워 잡기
    // ==========================================
    void TryGrab()
    {
        Debug.DrawRay(CurrentHandPosition, CurrentHandDirection * grabDistance, Color.red);

        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, handSide))
        {
            Ray ray = new Ray(CurrentHandPosition, CurrentHandDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, grabDistance, interactionLayer))
            {
                PlatformSlot slot = hit.collider.GetComponent<PlatformSlot>();
                if (slot != null && slot.isOccupied && slot.currentTower != null)
                {
                    GrabTower(slot.currentTower);
                }
            }
        }
    }

    void GrabTower(GameObject towerObj)
    {
        isGrabbing = true;
        heldObject = towerObj;

        TowerData data = heldObject.GetComponent<TowerData>();
        if (data != null && data.ownerSlot != null)
        {
            originalSlot = data.ownerSlot;
            originalSlot.currentTower = null; // 슬롯 비우기
            originalSlot.isOccupied = false;
        }

        originalLayer = heldObject.layer;
        SetLayerRecursively(heldObject, 2); // Ignore Raycast (충돌 방지)

        // 물리 끄기 (잡는 순간 덜덜거림 방지)
        ResetTowerPhysics(heldObject, true);

        heldObject.transform.SetParent(CurrentHandTransform);
        heldObject.transform.localPosition = holdOffset;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    // ==========================================
    //  2. 놓기 (강력하게 보강된 로직)
    // ==========================================
    void HandleHoldingAndRelease()
    {
        // 버튼 뗐을 때 혹은 예외적으로 물체가 사라졌을 때
        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, handSide) || heldObject == null)
        {
            ReturnTowerToOriginalSlot();
        }
    }

    void ReturnTowerToOriginalSlot()
    {
        // heldObject가 존재할 때만 복귀 로직 수행
        if (heldObject != null)
        {
            // 1. 부모 관계 즉시 해제 (손에서 분리)
            heldObject.transform.SetParent(null);

            // 2. 레이어 복구
            SetLayerRecursively(heldObject, originalLayer);

            // 3. 물리 상태 강제 리셋 (여기가 중요: 날아가지 않게 고정)
            ResetTowerPhysics(heldObject, true);

            // 4. 위치 강제 이동
            if (originalSlot != null)
            {
                Transform loc = originalSlot.transform.Find("platform_location");
                Vector3 targetPos = (loc != null) ? loc.position : originalSlot.transform.position;
                
                // 위치와 회전을 강제로 덮어씌움
                heldObject.transform.position = targetPos;
                heldObject.transform.rotation = Quaternion.identity;

                // 슬롯에 다시 등록
                originalSlot.SetTower(heldObject);
            }
            else
            {
                // 만약 돌아갈 슬롯 정보가 유실되었다면(예외상황), 
                // 일단 파괴하거나 근처에 두어야 에러가 안남. 여기선 파괴로 처리하거나 로그.
                Debug.LogWarning("돌아갈 슬롯을 찾지 못해 타워가 제자리에 멈춥니다.");
            }
        }
        
        // 5. 내부 상태 완전 초기화 (무조건 실행)
        ResetState();
        ResetGazeUI();
        ResetGazeData();
    }

    // [신규 기능] 물리력 초기화 헬퍼 함수
    void ResetTowerPhysics(GameObject obj, bool isKinematic)
    {
        if (obj == null) return;

        Rigidbody rb = obj.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;        // 이동 속도 제거
            rb.angularVelocity = Vector3.zero; // 회전 속도 제거
            rb.isKinematic = isKinematic;      // 물리 연산 여부 설정
        }

        // 혹시 자식 오브젝트에도 Rigidbody가 있다면 멈춰야 함 (Lava Guardian 특이사항 대비)
        Rigidbody[] childRbs = obj.GetComponentsInChildren<Rigidbody>();
        foreach(Rigidbody childRb in childRbs)
        {
            childRb.velocity = Vector3.zero;
            childRb.angularVelocity = Vector3.zero;
            childRb.isKinematic = isKinematic;
        }
    }

    // ==========================================
    //  3. 시선 처리 (Gaze Logic)
    // ==========================================
    void HandleGazeMerging()
    {
        if (playerHead == null || heldObject == null) return;

        // [방어 코드] 잡고 있는 물체가 파괴되었으면 즉시 리턴
        if (heldObject == null) 
        {
            ReturnTowerToOriginalSlot();
            return;
        }

        Debug.DrawRay(playerHead.position, playerHead.forward * maxGazeDistance, Color.blue);

        Ray ray = new Ray(playerHead.position, playerHead.forward);
        RaycastHit hit;
        bool foundValidTarget = false;

        if (Physics.Raycast(ray, out hit, maxGazeDistance, interactionLayer))
        {
            PlatformSlot targetSlot = hit.collider.GetComponent<PlatformSlot>();
            
            // 타겟 슬롯과 타워가 존재해야 함
            if (targetSlot != null && targetSlot.isOccupied && targetSlot.currentTower != null)
            {
                CheckMergeCondition(targetSlot.currentTower, hit, ref foundValidTarget);
            }
        }

        if (!foundValidTarget)
        {
            if (curGazeTime > 0) ResetGazeData();
            ResetGazeUI();
        }
    }

    void CheckMergeCondition(GameObject targetTower, RaycastHit hitInfo, ref bool foundValidTarget)
    {
        if (targetTower == heldObject) return; 

        TowerData heldData = heldObject.GetComponent<TowerData>();
        TowerData targetData = targetTower.GetComponent<TowerData>();

        if (heldData == null || targetData == null) return;
        if (heldData.towerId != targetData.towerId) return; 
        if (heldData.grade != targetData.grade) return; 
        if (heldData.grade == TowerData.TowerGrade.Epic) return; 

        foundValidTarget = true;
        ProcessGazeLogic(targetTower, hitInfo);
    }

    void ProcessGazeLogic(GameObject targetObj, RaycastHit hitInfo)
    {
        if (currentGazeTarget != targetObj)
        {
            currentGazeTarget = targetObj;
            curGazeTime = 0f;
        }

        curGazeTime += Time.deltaTime;

        if (uiCanvas != null && gazeImg != null)
        {
            uiCanvas.gameObject.SetActive(true);
            gazeImg.fillAmount = curGazeTime / gazeChargeTime;
            
            // UI 위치 보정
            uiCanvas.position = hitInfo.point;
            uiCanvas.forward = playerHead.forward * -1; // 플레이어를 보게 함
            
            // 거리에 따른 스케일 보정 (선택 사항)
            uiCanvas.localScale = defaultScale * uiCanvasVal * (hitInfo.distance * 0.5f); 
        }

        if (curGazeTime >= gazeChargeTime)
        {
            PerformMerge(targetObj);
        }
    }

    void PerformMerge(GameObject targetObj)
    {
        if (heldObject == null || targetObj == null) return;

        TowerData heldData = heldObject.GetComponent<TowerData>();
        TowerData targetData = targetObj.GetComponent<TowerData>();
        PlatformSlot targetSlot = targetData.ownerSlot; // 타겟의 슬롯

        TowerData.TowerGrade nextGrade = heldData.grade + 1;

        // [중요] 합성 시 손에 든 물체와 타겟을 모두 확실히 제거
        GameObject towerToDestroy = heldObject;
        
        // 1. 손 상태 먼저 초기화 (참조 끊기)
        ResetState(); 

        // 2. 오브젝트 파괴
        Destroy(towerToDestroy); // 손에 들고 있던 것
        
        if (targetSlot != null) 
        {
            targetSlot.currentTower = null;
            targetSlot.isOccupied = false;
        }
        Destroy(targetObj); // 바닥에 있던 것

        // 3. 새 타워 생성 요청
        if (TowerGenerator.Instance != null)
        {
            // 재료 소모에 대한 카운트 처리는 TowerGenerator 내부 혹은 TowerCount에서 수행되어야 함
            // 만약 TowerCount가 타워 파괴시 자동으로 줄어들지 않는다면 여기서 감소 로직 필요
            // (이전 코드 문맥상 TowerGenerator.CreateMergedTower 내부나 Destroy 로직에서 처리한다고 가정)
            
            // 하지만 안전을 위해 TowerCount 감소 로직을 명시적으로 호출하려면 아래와 같이 작성 가능
            if (TowerCount.Instance != null)
            {
                TowerCount.Instance.RemoveCurrentCount(); // 손에 든 거 삭제분
                TowerCount.Instance.RemoveCurrentCount(); // 바닥에 있는 거 삭제분
            }

            TowerGenerator.Instance.CreateMergedTower(nextGrade, targetSlot);
        }

        ResetGazeData();
        ResetGazeUI();
    }

    // ==========================================
    //  4. 상태 초기화 및 유틸리티
    // ==========================================
    void ResetState()
    {
        // 손에 자식으로 남아있는 경우를 대비한 최종 확인 (혹시 모르니)
        if (heldObject != null && heldObject.transform.parent == CurrentHandTransform)
        {
            heldObject.transform.SetParent(null);
            ResetTowerPhysics(heldObject, true);
        }

        isGrabbing = false;
        heldObject = null;
        originalSlot = null;
    }

    void ResetGazeData()
    {
        currentGazeTarget = null;
        curGazeTime = 0f;
    }

    void ResetGazeUI()
    {
        if (uiCanvas != null) uiCanvas.gameObject.SetActive(false);
        if (gazeImg != null) gazeImg.fillAmount = 0f;
    }

    void SetLayerRecursively(GameObject obj, int newLayer)
    {
        if (obj == null) return;
        obj.layer = newLayer;
        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }

    void DebugCheckLookingAt()
    {
        if (Time.frameCount % 30 != 0 || playerHead == null) return;

        Ray ray = new Ray(playerHead.position, playerHead.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxGazeDistance, interactionLayer))
        {
            PlatformSlot slot = hit.collider.GetComponent<PlatformSlot>();
            if (slot != null && slot.isOccupied && slot.currentTower != null)
            {
                TowerData td = slot.currentTower.GetComponent<TowerData>();
                // Debug.Log($"<color=cyan>[LOOKING]</color> Tower: {slot.currentTower.name} (Grade: {td?.grade})");
            }
        }
    }
}