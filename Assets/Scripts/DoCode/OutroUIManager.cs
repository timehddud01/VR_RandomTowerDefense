using UnityEngine;
using UnityEngine.UI; // 이미지 제어를 위해 필요

public class OutroUIManager : MonoBehaviour
{
    [Header("UI Objects")]
    public GameObject successUI; // 성공했을 때 띄울 이미지/패널
    public GameObject failUI;    // 실패했을 때 띄울 이미지/패널

    void Start()
    {
        // GameManager에 저장해둔 결과를 가져옵니다.
        bool result = GameManager.isGameClearedResult;

        // 결과에 따라 UI 활성화/비활성화
        if (result == true)
        {
            if (successUI != null) successUI.SetActive(true);
            if (failUI != null) failUI.SetActive(false);
        }
        else
        {
            if (successUI != null) successUI.SetActive(false);
            if (failUI != null) failUI.SetActive(true);
        }
    }
}