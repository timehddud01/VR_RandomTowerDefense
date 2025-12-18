using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoalManager : MonoBehaviour
{
    /// <summary>
    /// 다른 스크립트에서 GoalManager의 기능에 쉽게 접근할 수 있도록 하는 싱글톤 인스턴스
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
            Debug.Log("게임 오버! (라이프 소진)");
            
            // [수정] GameManager에게 패배 처리 요청 (false = 패배)
            // GameManager가 Outro 씬 이동과 static 변수 설정을 담당합니다.
            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver(false);
            }
            else
            {
                Debug.LogError("GameManager 인스턴스를 찾을 수 없습니다.");
            }

            // [삭제됨] 씬이 넘어가야 하므로 시간을 멈추지 않습니다.
            // Time.timeScale = 0; 
        }
    }
}