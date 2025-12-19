using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // UI 텍스트 제어를 위해 필수

public class GoalManager : MonoBehaviour
{
    /// <summary>
    /// 다른 스크립트에서 GoalManager의 기능에 쉽게 접근할 수 있도록 하는 싱글톤 인스턴스
    /// </summary>
    public static GoalManager instance;

    [Header("Settings")]
    /// <summary>
    /// 플레이어의 라이프
    /// </summary>
    public int playerLives = 10;

    [Header("UI")]
    public TextMeshProUGUI lifeText; // 정보를 표시할 텍스트 UI 연결

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        // 게임 시작 시 초기 라이프 UI 표시
        UpdateUI();
    }

    /// <summary>
    /// 적이 목표 지점에 도달했을 때 호출되어 플레이어 라이프를 감소시킵니다.
    /// </summary>
    public void EnemyReachedGoal()
    {
        playerLives--;
        
        // 라이프 변경 즉시 UI 업데이트
        UpdateUI();

        Debug.Log("플레이어 라이프가 감소했습니다. 현재 라이프: " + playerLives);

        // 라이프가 0 이하가 되면 게임 오버 처리
        if (playerLives <= 0)
        {
            Debug.Log("게임 오버! (라이프 소진)");
            
            // GameManager에게 패배 처리 요청 (false = 패배)
            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver(false);
            }
            else
            {
                Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다.");
            }
        }
    }

    // UI 업데이트 함수 (TowerCount 스타일 적용)
    private void UpdateUI()
    {
        if (lifeText == null) return;

        // 양식:
        // [Player Life]
        // 현재 라이프
        lifeText.text = $"[Lifes]\n" +
                        $"{playerLives}";
    }
}