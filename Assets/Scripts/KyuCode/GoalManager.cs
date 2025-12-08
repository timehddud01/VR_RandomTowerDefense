using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    /// <summary>
    /// 다른 스크립트에서 GameManager의 기능에 쉽게 접근할 수 있도록 하는 싱글톤 인스턴스
    /// </summary>
    public static GoalManager instance;

    /// <summary>
    /// 플레이어의 라이프
    /// </summary>
    public int playerLives = 10;

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

    /// <summary>
    /// 적이 목표 지점에 도달했을 때 호출되어 플레이어 라이프를 감소시킵니다.
    /// </summary>
    public void EnemyReachedGoal()
    {
        playerLives--;
        Debug.Log("플레이어 라이프가 감소했습니다. 현재 라이프: " + playerLives);

        // 라이프가 0 이하가 되면 게임 오버 처리
        if (playerLives <= 0)
        {
            Debug.Log("게임 오버!");
            // TODO: 여기에 게임 오버 UI 표시, 게임 정지 등의 로직을 추가할 수 있습니다.
            Time.timeScale = 0; // 예시: 게임 시간 정지
        }
    }
}

