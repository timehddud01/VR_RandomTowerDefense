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
            originalSlot.currentTower = null;
            originalSlot.isOccupied = false;
        }

        originalLayer = heldObject.layer;
        SetLayerRecursively(heldObject, 2); // Ignore Raycast 레이어 등으로 변경

        Rigidbody rb = heldObject.GetComponent<Rigidbody>();
        if (rb) rb.isKinematic = true;

        heldObject.transform.SetParent(CurrentHandTransform);
        heldObject.transform.localPosition = holdOffset;
        heldObject.transform.localRotation = Quaternion.identity;
    }

    // ==========================================
    //  2. 놓기 (보완된 로직)
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
        if (heldObject != null)
        {
            // [보완] 부모 관계를 먼저 끊어서 손에서 즉시 분리
            heldObject.transform.SetParent(null);
            SetLayerRecursively(heldObject, originalLayer);

            // 물리 상태 확정 (움직이지 않게)
            Rigidbody rb = heldObject.GetComponent<Rigidbody>();
            if (rb) rb.isKinematic = true;

            if (originalSlot != null)
            {
                Transform loc = originalSlot.transform.Find("platform_location");
                heldObject.transform.position = (loc != null) ? loc.position : originalSlot.transform.position;
                heldObject.transform.rotation = Quaternion.identity;

                originalSlot.SetTower(heldObject);
            }
        }
        
        // 상태 완전 초기화
        ResetState();
        ResetGazeUI();
        ResetGazeData();
    }

    // ==========================================
    //  3. 시선 처리 (Gaze Logic)
    // ==========================================
    void HandleGazeMerging()
    {
        if (playerHead == null) return;

        Debug.DrawRay(playerHead.position, playerHead.forward * maxGazeDistance, Color.blue);

        Ray ray = new Ray(playerHead.position, playerHead.forward);
        RaycastHit hit;
        bool foundValidTarget = false;

        if (Physics.Raycast(ray, out hit, maxGazeDistance, interactionLayer))
        {
            PlatformSlot targetSlot = hit.collider.GetComponent<PlatformSlot>();
            
            if (targetSlot != null && targetSlot.isOccupied && targetSlot.currentTower != null)
            {
                if (heldObject != null)
                {
                    CheckMergeCondition(targetSlot.currentTower, hit, ref foundValidTarget);
                }
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
            uiCanvas.position = hitInfo.point;
            uiCanvas.forward = playerHead.forward * -1;
            uiCanvas.localScale = defaultScale * uiCanvasVal * hitInfo.distance;
        }

        if (curGazeTime >= gazeChargeTime)
        {
            PerformMerge(targetObj);
        }
    }

    void PerformMerge(GameObject targetObj)
    {
        TowerData heldData = heldObject.GetComponent<TowerData>();
        TowerData targetData = targetObj.GetComponent<TowerData>();
        PlatformSlot targetSlot = targetData.ownerSlot;

        TowerData.TowerGrade nextGrade = heldData.grade + 1;

        // [중요] 합성 시 손에 든 물체와 타겟을 모두 확실히 제거
        GameObject towerToDestroy = heldObject;
        ResetState(); // 파괴 전 상태를 먼저 초기화하여 참조 오류 방지

        Destroy(towerToDestroy);
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

        ResetGazeData();
        ResetGazeUI();
    }

    // ==========================================
    //  4. 상태 초기화 및 유틸리티
    // ==========================================
    void ResetState()
    {
        // 손에 자식으로 남아있는 경우를 대비한 최종 확인
        if (heldObject != null && heldObject.transform.parent == CurrentHandTransform)
        {
            heldObject.transform.SetParent(null);
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
                Debug.Log($"<color=cyan>[LOOKING]</color> Tower: {slot.currentTower.name} (Grade: {td?.grade})");
            }
        }
    }
}