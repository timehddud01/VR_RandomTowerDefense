using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수 추가

public class GameManager : MonoBehaviour
{
    [Header("Game Settings")]
    public int totalRounds = 10; 
    public int currentRound = 1; 
    public int bossRoundCycle = 5; 
    public int userLife = 10; 

    float waitingTime2NextRound = 3.0f; 
    private int enemiesAlive = 0;
    
    [Header("References")]
    public EnemyMaker enemyMaker;
    public TextMeshPro roundtext;
    
    // [변경] SceneChange 스크립트 연결 (인스펙터에서 할당)
    public SceneChange sceneChangeLoader; 

    // [변경] 기존 로컬 UI 변수는 제거하거나 안 씀 (Outro 씬에서 처리하므로)
    // public GameObject gameClearUI; 
    // public float uiSpawnDistance = 2.0f; 

    // [추가] 다음 씬(Outro)으로 승리/패배 여부를 넘겨주기 위한 정적 변수
    public static bool isGameClearedResult = false;

    public static GameManager instance;
    
    void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        StartGame();
    }

    void StartRound()
    {   
        bool isBossRound = (currentRound % bossRoundCycle == 0);
         
         if (isBossRound)
        {
            enemiesAlive = 1;
        }
        else
        {
            if(roundtext != null)
                roundtext.text = "Round:\n" + currentRound;
            
            int waveSize = enemyMaker.firstEnemyPoolSize + (currentRound - 1);
            enemiesAlive = waveSize;
        }

        if ( isBossRound ) Debug.Log("보스 라운드");
        else Debug.Log(currentRound + "라운드 시작");

        if (enemyMaker != null)
        {
            enemyMaker.CallEnemy(currentRound, isBossRound);
        }
    }

    public void EndRound()
    {
        // 마지막 라운드를 클리어했다면 승리 처리
        if ( currentRound >= totalRounds ) 
        {
            GameOver(true); 
            return;
        }
        else
        {
             if (currentRound % 2 == 0 && BombPool.instance != null)
            {
                BombPool.instance.ReplenishBomb();
            }
            Debug.Log(currentRound + "라운드 종료");
            StartCoroutine(WaitUntilNextRound(waitingTime2NextRound)); 
        }
    }

    public void EnemyDestroyed()
    {
        enemiesAlive--;
        if (enemiesAlive <= 0)
        {
            EndRound();
        }
    }

    IEnumerator WaitUntilNextRound(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);
        currentRound++; 
        
        if (TowerCount.Instance != null)
        {
            TowerCount.Instance.AddMaxLimit();
        }

        StartRound();
    }

    void StartGame()
    {
        currentRound = 1;
        if (TowerCount.Instance != null)
        {
            TowerCount.Instance.InitTowerCount();
        }
        
        // gameClearUI 관련 코드는 Outro 씬으로 이관되었으므로 삭제 혹은 주석 처리
        // if (gameClearUI != null) gameClearUI.SetActive(false);
        
        StartRound();
    }
    
    public void LifeDecrease(int damage) 
    {
        userLife -= damage;
        if (userLife <= 0)
        {
            GameOver(false); 
        }
    }

    // =========================================================
    // [수정] 게임 종료 처리 (씬 이동 로직으로 변경)
    // =========================================================
    public void GameOver(bool isWin)
    {
        // 1. 결과 상태 저장 (static 변수) -> Outro 씬에서 읽어감
        isGameClearedResult = isWin;

        if (isWin)
        {
            Debug.Log("게임 승리! Outro 씬으로 이동합니다.");
            LoadOutroScene();
        }
        else
        {
            Debug.Log("게임 패배! Outro 씬으로 이동합니다.");
            LoadOutroScene();
        }
    }

    // 씬 이동 헬퍼 함수
    void LoadOutroScene()
    {
        // SceneChange 스크립트가 연결되어 있다면 그것을 사용
        if (sceneChangeLoader != null)
        {
            sceneChangeLoader.LoadNextScene("Outro");
        }
        else
        {
            // 연결 안 되어 있어도 작동하도록 안전장치
            SceneManager.LoadScene("Outro");
        }
    }
}