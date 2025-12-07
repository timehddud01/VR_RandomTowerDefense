using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    // Start is called before the first frame update\

    public int totalRounds = 20; //전체 라운드 수
    int currentRound = 1; //현재 라운드
    public int bossRoundCycle = 5; //보스 생성 라운드 주기
    public int userLife = 10; //플레이 목숨 개수

    float waitingTime2NextRound = 3.0f; //다음 라운드까지 대기 시간

    public EnemyMaker enemyMaker;
    
    public static GameManager instance;//GameManager static으로 만듦
    
    void Awake()
    {
        //Awkake로 가장 먼저 실행되어 다른 스크립트의 start()와 겹치지 않게 하기
        if (instance == null)
        {
            instance = this;
        }
        else
        {   
            //중복 방지
            Destroy(gameObject);
        }
    }

    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //라운드 시작
    void StartRound()
    {   
         //보스 라운드인가??
        bool isBossRound = (currentRound % bossRoundCycle == 0);
       
        if ( isBossRound )
        {
            Debug.Log("보스 라운드");
        }
        else
        {
            Debug.Log(currentRound + "라운드 시작");
            
        }

        //적 생성 지시
        if (enemyMaker != null)
        {
            enemyMaker.CallEnemy(currentRound, isBossRound);
        }
    }

    //라운드 종료
    public void EndRound()
    {
        if ( currentRound >= totalRounds ) //마지막 라운드까지 가면
        {
            GameOver(true); //승리판정
            return;
        }
        else
        {
            Debug.Log(currentRound + "라운드 종료");

            StartCoroutine(WaitUntilNextRound(waitingTime2NextRound)); //5초 후 다음 라운드 시작
        }
    }

    IEnumerator WaitUntilNextRound(float waitingTime)
    {
        yield return new WaitForSeconds(waitingTime);
        currentRound++; //라운드 숫자 올리기
        StartRound();
    }



    //게임 시작
    void StartGame()
    {
        currentRound = 1;
        Debug.Log("게임 시작");
        StartRound();
    }


    //게임 종료
    void EndGame()
    {
        
    }

    public void GameOver(bool isWin)
    {
        Debug.Log("게임 승리!");
    }
    
    public void LifeDecrease(int damage) // endPoint에 닿으면 목숨 차감 함수--> enemyPrefab에서 호출
    {
        userLife -= damage;
        Debug.Log("남은 목숨: " + userLife);
        if (userLife <= 0)
        {
            GameOver(false); //패배판정
        }
    }
}
