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
    public float maxGazeDistance = 20.0f;

    // --- 내부 상태 변수 ---
    private bool isGrabbing = false;
    private GameObject heldObject;
    private PlatformSlot originalSlot;
    private int originalLayer; 

    private float curGazeTime = 0f;
    private GameObject currentGazeTarget;

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
        if (isGrabbing == false)
        {
            TryGrab();
        }
        else
        {
            HandleHoldingAndRelease();
            HandleGazeMerging();
        }
    }

    // ==========================================
    //  1. 타워 잡기 (Raycast 방식)
    // ==========================================
    void TryGrab()
    {
        // 손 레이저 디버깅 (빨간색)
        Debug.DrawRay(CurrentHandPosition, CurrentHandDirection * grabDistance, Color.red);

        if (ARAVRInput.GetDown(ARAVRInput.Button.HandTrigger, handSide))
        {
            Ray ray = new Ray(CurrentHandPosition, CurrentHandDirection);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, grabDistance, interactionLayer))
            {
                GameObject targetToGrab = null;

                if (hit.collider.GetComponent<TowerData>() != null)
                {
                    targetToGrab = hit.collider.gameObject;
                }
                else if (hit.collider.GetComponent<PlatformSlot>() != null)
                {
                    PlatformSlot slot = hit.collider.GetComponent<PlatformSlot>();
                    if (slot.isOccupied && slot.currentTower != null)
                    {
                        targetToGrab = slot.currentTower;
                    }
                }

                if (targetToGrab != null)
                {
                    GrabTower(targetToGrab);
                }
            }
        }
    }

    void GrabTower(GameObject towerObj)
    {
        Debug.Log("[DEBUG] 4. GrabTower 실행");
        
        isGrabbing = true;
        heldObject = towerObj;

        TowerData data = heldObject.GetComponent<TowerData>();
        if (data != null && data.ownerSlot != null)
        {
            originalSlot = data.ownerSlot;

            // [수정 핵심] RemoveTower()를 호출하면 오브젝트가 파괴되므로,
            // 파괴하지 않고 슬롯의 연결 정보만 끊습니다.
            originalSlot.currentTower = null;
            originalSlot.isOccupied = false;
            
            // TowerData 쪽 정보는 유지 (나중에 취소 시 돌아가야 하므로)
        }

        originalLayer = heldObject.layer;
        SetLayerRecursively(heldObject, 2); // Ignore Raycast

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        heldObject.transform.SetParent(CurrentHandTransform);
        heldObject.transform.localPosition = holdOffset; 
        heldObject.transform.localRotation = Quaternion.identity;
    }

    // ==========================================
    //  2. 놓기 (수정됨)
    // ==========================================
    void HandleHoldingAndRelease()
    {
        if (ARAVRInput.GetUp(ARAVRInput.Button.HandTrigger, handSide))
        {
            Debug.Log("[DEBUG] 5. 놓기 시도 (Button Up)");
            ReturnTowerToOriginalSlot();
        }
    }

    void ReturnTowerToOriginalSlot()
    {
        // 이제 heldObject가 파괴되지 않았으므로 null이 아님
        if (heldObject != null && originalSlot != null)
        {
            Debug.Log($"[DEBUG] 6. 타워 복귀 성공: {heldObject.name}");

            heldObject.transform.SetParent(null);
            SetLayerRecursively(heldObject, originalLayer);

            Transform loc = originalSlot.transform.Find("platform_location");
            heldObject.transform.position = (loc != null) ? loc.position : originalSlot.transform.position;
            heldObject.transform.rotation = Quaternion.identity;

            // 슬롯에 다시 등록
            originalSlot.SetTower(heldObject);
        }
        else
        {
            // 만약 여전히 실패한다면 원인을 출력
            if (heldObject == null) Debug.LogWarning("[DEBUG] ⚠ 실패 원인: heldObject가 null (파괴됨?)");
            if (originalSlot == null) Debug.LogWarning("[DEBUG] ⚠ 실패 원인: originalSlot이 null");
        }

        ResetState();
        ResetGazeUI();
    }

    // ==========================================
    //  3. 시선 처리 (디버깅 추가)
    // ==========================================
    void HandleGazeMerging()
    {
        if (playerHead == null) return;

        // 시선 레이저 디버깅 (파란색) - 잡고 있을 때만 보임
        Debug.DrawRay(playerHead.position, playerHead.forward * maxGazeDistance, Color.blue);

        Ray ray = new Ray(playerHead.position, playerHead.forward);
        RaycastHit hit;
        bool foundValidTarget = false;

        if (Physics.Raycast(ray, out hit, maxGazeDistance, interactionLayer))
        {
            GameObject hitObj = hit.collider.gameObject;

            // [GAZE DEBUG] 시선이 무언가에 닿음
            // Debug.Log($"[GAZE HIT] {hitObj.name}"); 

            // 플랫폼이면 타워로 치환
            if (hitObj.GetComponent<PlatformSlot>() != null)
            {
                PlatformSlot slot = hitObj.GetComponent<PlatformSlot>();
                if (slot.isOccupied && slot.currentTower != null)
                {
                    hitObj = slot.currentTower;
                }
            }

            // 조건 검사
            if (hitObj != heldObject && hitObj.GetComponent<TowerData>() != null)
            {
                TowerData heldData = heldObject.GetComponent<TowerData>();
                TowerData targetData = hitObj.GetComponent<TowerData>();

                if (heldData != null && targetData != null)
                {
                    // 조건 로그 확인
                    if (heldData.towerId == targetData.towerId &&
                        heldData.grade == targetData.grade &&
                        heldData.grade != TowerData.TowerGrade.Epic)
                    {
                        foundValidTarget = true;
                        Debug.Log($"[GAZE MATCH] 합성 대상 발견! 게이지 증가 중... ({curGazeTime:F2})");
                        ProcessGazeLogic(hitObj, hit.point, hit.normal);
                    }
                    else
                    {
                        // 왜 합성이 안되는지 로그 (너무 자주 뜨면 주석 처리)
                        // Debug.Log($"[GAZE FAIL] 조건 불일치 - ID:{heldData.towerId}vs{targetData.towerId}, Grade:{heldData.grade}vs{targetData.grade}");
                    }
                }
            }
        }

        if (!foundValidTarget)
        {
            if (curGazeTime > 0) Debug.Log("[GAZE LOST] 대상에서 벗어남. 리셋.");
            ResetGazeData();
            ResetGazeUI();
        }
    }

    void ProcessGazeLogic(GameObject targetObj, Vector3 hitPoint, Vector3 hitNormal)
    {
        if (currentGazeTarget != targetObj)
        {
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
            Debug.Log("[DEBUG] ✨ 합성 완료! PerformMerge 실행");
            PerformMerge(targetObj);
        }
    }

    void PerformMerge(GameObject targetObj)
    {
        TowerData heldData = heldObject.GetComponent<TowerData>();
        TowerData targetData = targetObj.GetComponent<TowerData>();
        PlatformSlot targetSlot = targetData.ownerSlot;

        TowerData.TowerGrade nextGrade = heldData.grade + 1;

        // 기존 타워들 삭제
        Destroy(heldObject);
        // 타겟 타워가 있던 슬롯 비우기 (파괴는 아래 줄에서)
        if (targetSlot != null) 
        {
            targetSlot.currentTower = null;
            targetSlot.isOccupied = false;
        }
        Destroy(targetObj);

        // 생성 요청
        if (TowerGenerator.Instance != null)
        {
            TowerGenerator.Instance.CreateMergedTower(nextGrade, targetSlot);
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

    private void OnDrawGizmosSelected()
    {
        // 에디터에서도 Ray 확인 가능하게
        Gizmos.color = Color.red;
        Gizmos.DrawRay(transform.position, transform.forward * grabDistance);
    }
}