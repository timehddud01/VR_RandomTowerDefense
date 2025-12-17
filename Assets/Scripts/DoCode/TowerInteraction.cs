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

    [Header("Gaze Settings")]
    public Transform playerHead;
    public Image gazeLoadingBar;
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
        if (gazeLoadingBar != null)
        {
            gazeLoadingBar.gameObject.SetActive(false);
            gazeLoadingBar.fillAmount = 0f;
        }
    }

    void Update()
    {
        // 1. 잡기 전
        if (!isGrabbing)
        {
            TryGrab();
        }
        // 2. 잡은 후
        else
        {
            HandleHoldingAndRelease();

            // 물건을 아직 들고 있다면 시선 처리 시도
            if (isGrabbing)
            {
                HandleGazeMerging();
            }
        }
    }

    // ==========================================
    //  1. 타워 잡기 (기존과 동일)
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
                    Debug.Log($"[GRAB] 잡기 성공: {slot.currentTower.name}");
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
            originalSlot.currentTower = null;
            originalSlot.isOccupied = false;
        }

        originalLayer = heldObject.layer;
        SetLayerRecursively(heldObject, 2); // Ignore Raycast 등으로 변경 추천

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        heldObject.transform.SetParent(CurrentHandTransform);
        heldObject.transform.localPosition = holdOffset;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    // ==========================================
    //  2. 놓기 (기존과 동일)
    // ==========================================
    void HandleHoldingAndRelease()
    {
        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, handSide))
        {
            ReturnTowerToOriginalSlot();
        }
    }

    void ReturnTowerToOriginalSlot()
    {
        if (heldObject != null && originalSlot != null)
        {
            heldObject.transform.SetParent(null);
            SetLayerRecursively(heldObject, originalLayer);

            Transform loc = originalSlot.transform.Find("platform_location");
            heldObject.transform.position = (loc != null) ? loc.position : originalSlot.transform.position;
            heldObject.transform.rotation = Quaternion.identity;

            originalSlot.SetTower(heldObject);
        }
        ResetState();
        ResetGazeUI();
    }

    // ==========================================
    //  3. 시선 처리 (이곳에 디버그 집중)
    // ==========================================
    void HandleGazeMerging()
    {
        if (playerHead == null) return;

        Debug.DrawRay(playerHead.position, playerHead.forward * maxGazeDistance, Color.blue);

        Ray ray = new Ray(playerHead.position, playerHead.forward);
        RaycastHit hit;
        bool foundValidTarget = false;

        // [STEP 1] 레이캐스트 발사
        if (Physics.Raycast(ray, out hit, maxGazeDistance, interactionLayer))
        {
            // 충돌한 물체 이름 확인 (LayerMask 설정이 맞는지 확인용)
            Debug.Log($"[STEP 1] Ray Hit: {hit.collider.name} (Layer: {hit.collider.gameObject.layer})");

            PlatformSlot targetSlot = hit.collider.GetComponent<PlatformSlot>();
            
            if (targetSlot != null)
            {
                // [STEP 2] 슬롯 상태 확인
                if (targetSlot.isOccupied && targetSlot.currentTower != null)
                {
                    if (heldObject != null)
                    {
                        // [STEP 3] 합성 조건 정밀 검사
                        CheckMergeCondition(targetSlot.currentTower, hit.point, hit.normal, ref foundValidTarget);
                    }
                    else
                    {
                        Debug.LogWarning("[STEP 2 Fail] 슬롯은 찾았으나 손에 든 오브젝트(heldObject)가 Null입니다.");
                    }
                }
                else
                {
                    // 너무 자주 뜨면 주석 처리
                    Debug.Log($"[STEP 2 Fail] 슬롯({targetSlot.name})이 비어있거나 타워가 없습니다.");
                }
            }
        }

        if (!foundValidTarget)
        {
            if (curGazeTime > 0)
            {
                Debug.Log("[RESET] 타겟을 벗어나 게이지 초기화");
                ResetGazeData();
            }
            ResetGazeUI();
        }
    }

    void CheckMergeCondition(GameObject targetTower, Vector3 hitPoint, Vector3 hitNormal, ref bool foundValidTarget)
    {
        // 1. 자기 자신 확인
        if (targetTower == heldObject) return; 

        TowerData heldData = heldObject.GetComponent<TowerData>();
        TowerData targetData = targetTower.GetComponent<TowerData>();

        // 2. 데이터 컴포넌트 확인
        if (heldData == null)
        {
            Debug.LogError($"[STEP 3 Error] 손에 든 타워({heldObject.name})에 TowerData 스크립트가 없습니다!");
            return;
        }
        if (targetData == null)
        {
            Debug.LogError($"[STEP 3 Error] 바닥 타워({targetTower.name})에 TowerData 스크립트가 없습니다!");
            return;
        }

        // 3. ID 비교
        if (heldData.towerId != targetData.towerId) 
        {
             Debug.Log($"[STEP 3 Fail] ID 불일치 -> 손: {heldData.towerId} vs 바닥: {targetData.towerId}");
             return; 
        }

        // 4. 등급 비교
        if (heldData.grade != targetData.grade) 
        {
             Debug.Log($"[STEP 3 Fail] 등급 불일치 -> 손: {heldData.grade} vs 바닥: {targetData.grade}");
             return; 
        }

        // 5. 최고 등급 확인
        if (heldData.grade == TowerData.TowerGrade.Epic) 
        {
            Debug.Log("[STEP 3 Fail] 이미 최고 등급(Epic)입니다.");
            return; 
        }

        // [SUCCESS] 모든 조건 통과
        foundValidTarget = true;
        
        // 게이지 진행 상황 로그 (너무 빠르면 주석 처리)
        // Debug.Log($"[STEP 4] 합성 진행 중... {curGazeTime}/{gazeChargeTime}");
        
        ProcessGazeLogic(targetTower, hitPoint, hitNormal);
    }

    void ProcessGazeLogic(GameObject targetObj, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (currentGazeTarget != targetObj)
        {
            Debug.Log($"[TARGET CHANGE] 새로운 타겟 감지: {targetObj.name}");
            currentGazeTarget = targetObj;
            curGazeTime = 0f;
        }

        curGazeTime += Time.deltaTime;

        if (gazeLoadingBar != null)
        {
            gazeLoadingBar.gameObject.SetActive(true);
            gazeLoadingBar.fillAmount = curGazeTime / gazeChargeTime;
            gazeLoadingBar.transform.position = hitPoint + (hitNormal * 0.2f); 
            gazeLoadingBar.transform.LookAt(playerHead);
        }

        if (curGazeTime >= gazeChargeTime)
        {
            Debug.Log("[STEP 5] ✨ 합성 조건 달성! PerformMerge 호출");
            PerformMerge(targetObj);
        }
    }

    void PerformMerge(GameObject targetObj)
    {
        TowerData heldData = heldObject.GetComponent<TowerData>();
        TowerData targetData = targetObj.GetComponent<TowerData>();
        PlatformSlot targetSlot = targetData.ownerSlot;

        TowerData.TowerGrade nextGrade = heldData.grade + 1;

        Debug.Log($"[MERGE] 합성 수행: {heldData.grade} -> {nextGrade}");

        Destroy(heldObject);
        if (targetSlot != null) 
        {
            targetSlot.currentTower = null;
            targetSlot.isOccupied = false;
        }
        Destroy(targetObj);

        if (TowerGenerator.Instance != null)
        {
            TowerGenerator.Instance.CreateMergedTower(nextGrade, targetSlot);
        }
        else
        {
            Debug.LogError("[CRITICAL] TowerGenerator 인스턴스를 찾을 수 없습니다!");
        }

        ResetState();
        ResetGazeData();
        ResetGazeUI();
    }

    void ResetState()
    {
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
        if (gazeLoadingBar != null)
        {
            gazeLoadingBar.fillAmount = 0f;
            gazeLoadingBar.gameObject.SetActive(false);
        }
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
}