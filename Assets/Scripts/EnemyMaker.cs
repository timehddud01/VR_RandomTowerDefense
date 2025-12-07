using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Rendering;
using UnityEngine;

public class EnemyMaker : MonoBehaviour
{

    
    // Start is called before the first frame update
    public GameObject enemyFactory;
    public GameObject bossFactory;
    //1라운드 당 생성 적 수
    public int firstEnemyPoolSize = 20;
    
    // 아직 살아있는 적 수 확인
    int aliveEnemyCount;

    public Transform enemySpawnPoint;

    private static List<GameObject> enemyPool = new List<GameObject>();
    void Start()
    {
        if (enemyPool.Count == 0) //풀 만들기, 첫 라운드 수만큼만
        {
            for (int i = 0; i < firstEnemyPoolSize; i++)
            {
                GameObject enemy = Instantiate(enemyFactory); 
                enemy.SetActive(false);
                enemyPool.Add(enemy);
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CallEnemy(int currentRound, bool isBossRound)
    {
        

        StartCoroutine(SpawnEnemy(currentRound, isBossRound));
    }

    IEnumerator SpawnEnemy(int currentRound, bool isBossRound) 
    {
        int waveSize;//최종 웨이브 크기(보스일땐 1개, 일반 적일때는 풀사이즈 만큼 하기 위함)
        int targetPoolSize = firstEnemyPoolSize + (currentRound - 1); //라운드 올라갈수록 적 수 증가 -> 풀 사이즈를 늘리는 방향으로 설정
        
        Transform[] enemyWaypoints = null;



        ////////////////////////////포인트 전달
if (PathManager.Instance != null)
{
    enemyWaypoints = PathManager.Instance.EnemyPath;
}

// 2. [안전 체크] 경로가 없으면 생성 로직을 중단합니다.
if (enemyWaypoints == null || enemyWaypoints.Length == 0)
{
    Debug.LogError("PathManager에 웨이포인트가 설정되지 않았습니다!");
    yield break; // 코루틴 즉시 중단
}
///////////////////////////////////////////






        if (enemyPool.Count < targetPoolSize)
        {
            int addNeed = targetPoolSize - enemyPool.Count;
            for (int i = 0; i < addNeed; i++)
            {
                //새 적을 Instantiate하여 풀에 추가(풀을 라운듬마다 조금씩 확장하기)
                GameObject enemy = Instantiate(enemyFactory);
                enemy.SetActive(false);
                enemyPool.Add(enemy);
            }
        }


        //보스인지? 아닌지 판단하고 적 생성
        if (isBossRound)
        {
            waveSize = 1; //보스 1마리
        }
        else
        {
            waveSize = targetPoolSize; //일반 적은 풀 사이즈 만큼
        }

        aliveEnemyCount = waveSize; //살아있는 적 수 초기화

        for (int i = 0; i < waveSize; i++)
        {
            GameObject newEnemy = null;

            if (isBossRound)
            {
                newEnemy = Instantiate(bossFactory); //보스 생성
                newEnemy.transform.position = enemySpawnPoint.position;       
            }
            else
            {
                newEnemy = GetEnemyFromPool(); //풀에서 적 가져오기
            }

            if (newEnemy != null)
            { //////////////////////////////////////////

            
    MoveManagerForSpider moveManager = newEnemy.GetComponentInChildren<MoveManagerForSpider>();
    if (moveManager != null)
    {
        // 런타임에 경로를 전달하여 이동을 시작시킵니다.
        moveManager.SetPath(enemyWaypoints); 
    }
    
    
    EnemyDie enemyDie = newEnemy.GetComponentInChildren<EnemyDie>();
    if (enemyDie != null)
    {
        // EnemyDie는 배열의 마지막 요소를 최종 목표로 사용합니다.
        Transform finalTarget = enemyWaypoints[enemyWaypoints.Length - 1]; 
        enemyDie.SetMakerAndTarget(this, finalTarget); 
    }

    ///////////////////////////
            }
            
                
            yield return new WaitForSeconds(1f);   //0.5초 간격으로 적 생성
        }
    }

    public GameObject GetEnemyFromPool()
    {
        if (enemyPool.Count > 0)
        {
            GameObject enemy = enemyPool[0];
            enemyPool.RemoveAt(0);

            enemy.transform.position = enemySpawnPoint.position;
            enemy .SetActive(true);
            return enemy;
        }
        return null;
    }

    public void ReturnEnemy2Pool(GameObject enemy) //죽으면 다시 풀로 전환, enemyPrefab에서 호출
    {
        enemy.SetActive(false);
        enemyPool.Add(enemy);
        aliveEnemyCount--; //살아있는 적 수 줄이기
        if (aliveEnemyCount <=0)
        {
            
            GameManager.instance.EndRound(); // 라운드 종료 호출
        }
    }

}
