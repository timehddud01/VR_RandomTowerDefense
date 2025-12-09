
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerGenerator : MonoBehaviour
{  
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
        // 항상 Hover 감지
        HandleHoverRaycast();

        // 버튼 One → 랜덤 일반 타워 설치
        if (ARAVRInput.GetDown(ARAVRInput.Button.One))
        {
            TryPlaceRandomCommonTower();
        }

        // 버튼 Two → 타워 합성
        if (ARAVRInput.GetDown(ARAVRInput.Button.Two))
        {
            TryMergeTower();
        }
    }

    // ============================
    //  플랫폼 Hover 처리
    // ============================
    void HandleHoverRaycast()
    {
        Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
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
        if (commonTowers == null || commonTowers.Length == 0)
        {
            Debug.LogWarning("Common 등급 타워 프리팹이 설정되지 않았습니다.");
            return;
        }

        Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
        RaycastHit hitInfo;

        if (!Physics.Raycast(ray, out hitInfo, 200f, platformLayer))
            return;

        PlatformSlot slot = hitInfo.collider.GetComponent<PlatformSlot>();

        if (slot == null)
        {
            Debug.LogWarning("PlatformSlot 스크립트가 플랫폼에 없습니다.");
            return;
        }

        if (slot.isOccupied)
        {
            Debug.Log("❌ 이미 설치된 플랫폼입니다! 추가 설치 불가.");
            return;
        }

        Transform location = slot.transform.Find("platform_location");
        if (location == null)
        {
            Debug.LogWarning("platform_location 이 존재하지 않습니다.");
            return;
        }

        // 일반 등급 중 하나를 랜덤으로 선택
        GameObject prefab = GetRandomPrefabByGrade(TowerData.TowerGrade.Common);
        if (prefab == null)
        {
            Debug.LogWarning("Common 등급 타워 프리팹이 비어 있습니다.");
            return;
        }

        GameObject tower = Instantiate(prefab, location.position, Quaternion.identity);

        // PlatformSlot과 연결
        slot.SetTower(tower);

        Debug.Log("✅ 랜덤 일반 타워 설치 완료!");
    }

    // ============================
    //  타워 합성 시도 (Button.Two)
    // ============================
    void TryMergeTower()
    {
        Ray ray = new Ray(ARAVRInput.RHandPosition, ARAVRInput.RHandDirection);
        RaycastHit hit;

        if (!Physics.Raycast(ray, out hit, 200f, platformLayer))
            return;

        PlatformSlot slot = hit.collider.GetComponent<PlatformSlot>();
        if (slot == null || !slot.isOccupied || slot.currentTower == null)
        {
            Debug.Log("⚠ 합성할 타워가 없습니다.");
            return;
        }

        TowerData targetTowerData = slot.currentTower.GetComponent<TowerData>();
        if (targetTowerData == null)
        {
            Debug.LogWarning("선택한 타워에 TowerData가 없습니다.");
            return;
        }

        // 에픽 등급이면 더 이상 합성 불가
        if (targetTowerData.grade == TowerData.TowerGrade.Epic)
        {
            Debug.Log("⚠ 에픽 등급은 더 이상 합성할 수 없습니다.");
            return;
        }

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
            Debug.Log("❌ 동일한 타워가 배치되지 않았습니다!");
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

        if (nextPrefab == null)
        {
            Debug.LogWarning("다음 등급 타워 프리팹이 설정되지 않았습니다. 등급: " + nextGrade);
            return;
        }

        Transform location = slot.transform.Find("platform_location");
        if (location == null)
        {
            Debug.LogWarning("platform_location 이 존재하지 않습니다.");
            return;
        }

        GameObject newTower = Instantiate(nextPrefab, location.position, Quaternion.identity);
        slot.SetTower(newTower);

        Debug.Log("✨ 타워 합성 성공! 등급 업: " + targetTowerData.grade + " → " + nextGrade);
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
        warningUI.SetActive(true);
        yield return new WaitForSeconds(0.3f);
        warningUI.SetActive(false);
    }

    void ShowWarningUI()
    {
        if (warningUI == null) return;
        StartCoroutine(ShowWarningUIRoutine());
    }
}
