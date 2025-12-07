using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI; // UI 요소를 사용하기 위해 추가

public class EnemySpawners : MonoBehaviour
{
    public static EnemySpawners instance;

    [Header("스폰 설정")]
    [Tooltip("스폰할 적 프리팹")]
    public GameObject enemyPrefab;
    [Tooltip("적이 생성될 위치")]
    public Transform spawnPoint;
    [Tooltip("적이 따라갈 경로(경유지)")]
    public Transform[] waypoints;

    [Header("라운드 설정")]
    [Tooltip("라운드 사이의 대기 시간")]
    public float timeBetweenRounds = 5f;
    [Tooltip("라운드 시작 전 카운트다운")]
    private float roundCountdown = 2f;
    [Tooltip("현재 라운드 번호")]
    private int roundNumber = 0;

    [Header("적 스폰 설정")]
    [Tooltip("라운드당 스폰할 적의 수")]
    public int enemiesPerRound = 20;
    [Tooltip("적 스폰 간격")]
    public float timeBetweenSpawns = 0.5f;

    [Header("UI 요소 (선택 사항)")]
    [Tooltip("라운드 정보를 표시할 텍스트")]
    public Text roundText;

    private int enemiesAlive = 0;

    void Awake()
    {
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
        enemiesAlive = 0;
    }

    void Update()
    {
        // 살아있는 적이 없다면 다음 라운드를 준비합니다.
        if (enemiesAlive > 0)
        {
            return;
        }

        if (roundCountdown <= 0f)
        {
            StartCoroutine(SpawnRound());
            roundCountdown = timeBetweenRounds;
        }

        roundCountdown -= Time.deltaTime;
        UpdateRoundUI();
    }

    IEnumerator SpawnRound()
    {
        roundNumber++;
        enemiesAlive = enemiesPerRound;

        for (int i = 0; i < enemiesPerRound; i++)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
    }

    void SpawnEnemy()
    {
        GameObject enemyGO = Instantiate(enemyPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log("스폰댐");

        // 라운드에 따라 적의 체력을 강화시킵니다.
        Enemy enemy = enemyGO.GetComponent<Enemy>();
       if (enemy != null && RoundManager.instance != null)
        {
             float health = RoundManager.instance.GetWaveHP(roundNumber - 1);
            enemy.Setup(health);
        }

        // 생성된 적에게 경로를 설정해줍니다.
        MoveManager moveManager = enemyGO.GetComponentInChildren<MoveManager>();
        if (moveManager != null)
        {
            moveManager.SetPath(this.waypoints);
        }
    }

    public void EnemyDestroyed()
    {
        enemiesAlive--;
    }

    void UpdateRoundUI()
    {
        if (roundText != null)
        {
            if (enemiesAlive > 0)
            {
                roundText.text = "Round: " + roundNumber;
            }
            else
            {
                roundText.text = "Next Round in: " + Mathf.Ceil(roundCountdown).ToString();
            }
        }
    }
}
