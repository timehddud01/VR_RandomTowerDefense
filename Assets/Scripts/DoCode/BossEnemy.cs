using UnityEngine;

public class BossEnemy : MonoBehaviour
{
    public float maxHealth = 100f;
    
    private EnemyMaker maker;
    private bool isDead = false;
    private int waveIndex = 0;
    private float currentHealth;

    void OnEnable()
    {
        currentHealth = maxHealth;
        isDead = false;
    }

    public void SetMaker(EnemyMaker enemyMaker)  
    {
        maker = enemyMaker;
    }

    public void TakeDamage(float damage)
    {
        // 이미 죽었다면 데미지 처리 무시 (중복 사망 방지)
        if (isDead) return;
        
        currentHealth -= damage;
        // print("boss health now is "+ currentHealth); // 로그가 너무 많으면 주석 처리

        if (currentHealth <= 0)
        {
            Die(true);
        }
    }

    public void Setup(float health, int wave)
    {
        maxHealth = health;
        currentHealth = maxHealth;
        waveIndex = wave;
    }

    public void Die(bool killedByPlayer = true)
    {
        if (isDead) return;
        isDead = true;

        // 1. 점수 추가
        if (killedByPlayer && Score.instance != null)
        {
            Score.instance.AddScore((waveIndex + 1) * 100);
            Debug.Log("Boss killed");
        }

        // 2. 게임 매니저에게 알림 (승리 조건 체크 및 적 카운트 감소)
        if (GameManager.instance != null)
        {
            // 보스 처치 시 전체 라운드 승리 조건 체크
            if (GameManager.instance.currentRound >= GameManager.instance.totalRounds)
            {
                Debug.Log("최종 보스 처치! GameManager에게 승리 신호 전송.");
                GameManager.instance.GameOver(true); 
            }

            // 적 숫자 감소
            GameManager.instance.EnemyDestroyed();
        }

        // 3. 오브젝트 제거/반환
        ReturnToPool();
    }

    public bool IsDead()
    {
        return isDead;
    }

    // [수정됨] 부모 오브젝트 확인 및 전체 제거 로직
    public void ReturnToPool()
    {
        // 1. 삭제(반환)할 타겟 설정: 부모가 있으면 부모, 없으면 나 자신
        GameObject targetObject = (transform.parent != null) ? transform.parent.gameObject : gameObject;
        Destroy(targetObject);

    }
}