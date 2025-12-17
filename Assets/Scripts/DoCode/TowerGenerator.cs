using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerGenerator : MonoBehaviour
{
    // ==========================================
    // 외부 스크립트 접근용 싱글톤
    // ==========================================
    public static TowerGenerator Instance;

    private void Awake()
    {
        if (Instance == null) Instance = this;
    }

    [Header("Raycast")]
    public LayerMask platformLayer;

    [Header("Tower Prefab (class)")]
    public GameObject[] commonTowers;   // 일반 등급 4개
    public GameObject[] magicTowers;    // 매직 등급 4개
    public GameObject[] rareTowers;     // 레어 등급 4개
    public GameObject[] uniqueTowers;   // 유니크 등급 4개
    public GameObject[] epicTowers;     // 에픽 등급 4개

    // 현재 Hover 중인 PlatformSlot
    PlatformSlot currentHoverSlot = null;
    public GameObject warningUI;


    void Update()
    {
        // 항상 Hover 감지 (왼손 기준)
        HandleHoverRaycast();

        // [왼손(LTouch) - Button One] 일반 타워 생성
        if (ARAVRInput.GetDown(ARAVRInput.Button.One, ARAVRInput.Controller.LTouch))
        {
            TryPlaceRandomCommonTower();
        }

        // [오른손(RTouch) - Button Two] (구버전) 타워 합성 - 필요 시 유지
        // if (ARAVRInput.GetDown(ARAVRInput.Button.Two))
        // {
        //     TryMergeTower();
        // }
    }

    // ============================
    //  플랫폼 Hover 처리
    // ============================
    void HandleHoverRaycast()
    {
        // 왼손 기준 레이캐스트
        Ray ray = new Ray(ARAVRInput.LHandPosition, ARAVRInput.LHandDirection);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo, 200f, platformLayer))
        {
            PlatformSlot slot = hitInfo.collider.GetComponent<PlatformSlot>();

            if (slot != null)
            {
                currentHoverSlot = slot;
                slot.OnHover();
            }
        }
        else
        {
            currentHoverSlot = null;
        }
    }

    // ============================
    //  랜덤 일반 등급 타워 설치
    // ============================
    void TryPlaceRandomCommonTower()
    {
        if (commonTowers == null || commonTowers.Length == 0) return;

        // 왼손 위치에서 레이 발사
        Ray ray = new Ray(ARAVRInput.LHandPosition, ARAVRInput.LHandDirection);
        RaycastHit hitInfo;

        // 1. 플랫폼 레이어 감지 실패 시 리턴
        if (!Physics.Raycast(ray, out hitInfo, 200f, platformLayer)) return;

        PlatformSlot slot = hitInfo.collider.GetComponent<PlatformSlot>();

        // 2. 슬롯 컴포넌트 없거나, 이미 타워가 있으면 리턴
        if (slot == null || slot.isOccupied) return;

        Transform location = slot.transform.Find("platform_location");
        if (location == null) return;

        // 3. 프리팹 가져오기
        GameObject prefab = GetRandomPrefabByGrade(TowerData.TowerGrade.Common);
        if (prefab == null) return;

        // 4. 생성 및 슬롯 등록
        GameObject tower = Instantiate(prefab, location.position, Quaternion.identity);
        slot.SetTower(tower);
    }

    // ============================
    //  (구버전) 타워 합성 시도
    // ============================
    void TryMergeTower()
    {
        Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 200f, platformLayer))
            return;

        PlatformSlot slot = hit.collider.GetComponent<PlatformSlot>();
        if (slot == null || !slot.isOccupied || slot.currentTower == null) return;

        TowerData targetTowerData = slot.currentTower.GetComponent<TowerData>();
        if (targetTowerData == null) return;

        // 에픽 등급이면 더 이상 합성 불가
        if (targetTowerData.grade == TowerData.TowerGrade.Epic) return;

        string targetTag = slot.currentTower.tag;

        // --- 동일 태그를 가진 타워 찾기 ---
        TowerData[] allTowers = FindObjectsOfType<TowerData>();
        List<TowerData> sameTagTowers = new List<TowerData>();

        foreach (var t in allTowers)
        {
            if (t != null && t.gameObject.CompareTag(targetTag))
            {
                sameTagTowers.Add(t);
            }
        }

        // 합성을 위해서는 최소 2개 필요
        if (sameTagTowers.Count < 2)
        {
            ShowWarningUI();
            return;
        }

        // -------------------------
        //  합성 실행
        // -------------------------

        // 1. 현재 슬롯의 기존 타워 삭제
        slot.RemoveTower();

        // 2. 나머지 동일 태그 타워 중에서 하나 랜덤 삭제
        List<TowerData> candidates = new List<TowerData>();
        foreach (var t in sameTagTowers)
        {
            // 이미 지운 타워(슬롯의 타워)는 제외
            if (t == null) continue;
            if (slot.currentTower != null && t.gameObject == slot.currentTower) continue;

            candidates.Add(t);
        }

        if (candidates.Count > 0)
        {
            TowerData toRemove = candidates[Random.Range(0, candidates.Count)];
            if (toRemove != null)
            {
                PlatformSlot ownerSlot = toRemove.ownerSlot;
                if (ownerSlot != null)
                {
                    ownerSlot.RemoveTower();
                }
                else
                {
                    Destroy(toRemove.gameObject);
                }
            }
        }

        // 3. 다음 등급의 타워 중 하나를 랜덤으로 선택하여 현재 슬롯에 설치
        TowerData.TowerGrade nextGrade = targetTowerData.grade + 1;
        GameObject nextPrefab = GetRandomPrefabByGrade(nextGrade);

        if (nextPrefab == null) return;

        Transform location = slot.transform.Find("platform_location");
        if (location == null) return;

        GameObject newTower = Instantiate(nextPrefab, location.position, Quaternion.identity);
        slot.SetTower(newTower);
    }

    // =========================================================
    //  [TowerInteraction 호출용] 타워 합성 생성 함수
    // =========================================================
    public void CreateMergedTower(TowerData.TowerGrade grade, PlatformSlot targetSlot)
    {
        // 1. 프리팹 가져오기
        GameObject prefab = GetRandomPrefabByGrade(grade);
        if (prefab == null) return;

        // 2. 생성 위치 설정
        Transform spawnLoc = targetSlot.transform.Find("platform_location");
        Vector3 spawnPos = (spawnLoc != null) ? spawnLoc.position : targetSlot.transform.position;

        // 3. 생성 및 슬롯 등록
        GameObject newTower = Instantiate(prefab, spawnPos, Quaternion.identity);
        targetSlot.SetTower(newTower);
    }

    // ============================
    //  등급에 따른 랜덤 프리팹 반환
    // ============================
    GameObject GetRandomPrefabByGrade(TowerData.TowerGrade grade)
    {
        GameObject[] pool = null;

        switch (grade)
        {
            case TowerData.TowerGrade.Common:
                pool = commonTowers;
                break;
            case TowerData.TowerGrade.Magic:
                pool = magicTowers;
                break;
            case TowerData.TowerGrade.Rare:
                pool = rareTowers;
                break;
            case TowerData.TowerGrade.Unique:
                pool = uniqueTowers;
                break;
            case TowerData.TowerGrade.Epic:
                pool = epicTowers;
                break;
        }

        if (pool == null || pool.Length == 0)
            return null;

        int index = Random.Range(0, pool.Length);
        return pool[index];
    }

    IEnumerator ShowWarningUIRoutine()
    {
        if (warningUI != null)
        {
            warningUI.SetActive(true);
            yield return new WaitForSeconds(0.3f);
            warningUI.SetActive(false);
        }
    }

    void ShowWarningUI()
    {
        StartCoroutine(ShowWarningUIRoutine());
    }
}