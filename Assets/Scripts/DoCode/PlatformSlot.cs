using System.Collections;
using UnityEngine;

public class PlatformSlot : MonoBehaviour
{
    [Header("Condition")]
    public bool isOccupied = false;
    public GameObject currentTower;

    [Header("Hover time")]
    public float hoverDuration = 0.02f;

    // Hover 시 켰다가 끌 UI
    GameObject selectUI;
    Coroutine hoverRoutine;

    void Start()
    {
        // 자식(비활성 포함) 중에서 이름이 "SelectUI"인 오브젝트 찾기
        selectUI = FindChildByName(transform, "SelectUI");

        // 처음에는 보이지 않도록
        if (selectUI != null)
            selectUI.SetActive(false);
    }

    /// <summary>
    /// 부모 이하 트랜스폼에서 name과 같은 이름의 자식을 찾아 반환
    /// (비활성 오브젝트도 포함)
    /// </summary>
    GameObject FindChildByName(Transform parent, string name)
    {
        Transform[] children = parent.GetComponentsInChildren<Transform>(true);
        foreach (Transform t in children)
        {
            if (t.name == name)
                return t.gameObject;
        }
        return null;
    }

    // ==============================
    //      Hover 시 호출
    // ==============================
    public void OnHover()
    {
        if (selectUI == null)
            return;

        // 기존에 실행 중인 코루틴이 있으면 리셋
        if (hoverRoutine != null)
            StopCoroutine(hoverRoutine);

        hoverRoutine = StartCoroutine(HoverEffect());
    }

    IEnumerator HoverEffect()
    {
        // UI 켜기
        selectUI.SetActive(true);
        //print("ui show event work");

        // 지정된 시간만큼 유지
        yield return new WaitForSeconds(hoverDuration);

        // 추가 호출이 없으면 자동으로 끄기
        selectUI.SetActive(false);
        //print("ui now disappeared");
    }

    // ==============================
    //      타워 배치/삭제 관리
    // ==============================
    public void SetTower(GameObject tower)
    {
        currentTower = tower;
        //print(currentTower);
        isOccupied = (tower != null);

        if (tower != null)
        {
            TowerData data = tower.GetComponent<TowerData>();
            if (data != null)
            {
                data.ownerSlot = this;
            }
        }
    }

    public void RemoveTower()
    {
        if (currentTower != null)
        {
            TowerData data = currentTower.GetComponent<TowerData>();
            if (data != null)
            {
                data.ownerSlot = null;
            }

            Destroy(currentTower);
        }

        currentTower = null;
        isOccupied = false;
    }
}
