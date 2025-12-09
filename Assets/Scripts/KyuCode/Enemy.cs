using UnityEngine;

public class Enemy : MonoBehaviour
{
    /// <summary>
    /// 적의 최대 체력
    /// </summary>
    public float maxHealth = 100f;
    private EnemyMaker maker;
        private bool isDead = false;

    /// <summary>
    /// 현재 체력
    /// </summary>
    private float currentHealth;

    void OnEnable()
    {
        // 시작 시 현재 체력을 최대 체력으로 설정합니다.
        currentHealth = maxHealth;
            isDead = false;
    }
        public void SetMaker(EnemyMaker enemyMaker)  
    {
        // 1. maker 참조 저장
        maker = enemyMaker;

        
    }

    /// <summary>
    /// 외부(포탑 등)에서 호출하여 적에게 데미지를 줍니다.
    /// </summary>
    /// <param name="damage">입힐 데미지 양</param>
    public void TakeDamage(float damage)
    {
        if (isDead) return;
        currentHealth -= damage;

        // 체력이 0 이하가 되면 사망 처리
        if (currentHealth <= 0)
        {
            // TODO: 여기에 골드 획득, 파괴 이펙트 생성 등의 코드를 추가할 수 있습니다.
            Die();
        }

        
    }

        public void Setup(float health)
    {
        maxHealth = health;
        currentHealth = maxHealth;
    }


        public void Die()
    {
        if (isDead) return;
        isDead = true;
        // TODO: 여기에 골드 획득, 파괴 이펙트 생성 등의 코드를 추가할 수 있습니다.
    if (GameManager.instance != null)
        {
             GameManager.instance.EnemyDestroyed();
        }
        // 부모 오브젝트가 있다면 부모를 파괴하고, 없다면 자기 자신을 파괴합니다.
        // 이렇게 하면 적 모델과 체력바 등을 포함하는 부모 오브젝트 전체를 한번에 제거할 수 있습니다.
             ReturnToPool();
             
        
    }
public bool IsDead()
    {
        return isDead;
    }

    public void ReturnToPool()
    {
        if (maker == null) return;
        // EnemyMaker가 설정되어 있고, 오브젝트 풀링을 사용한다고 가정합니다.
        // 부모 오브젝트가 있다면 부모 오브젝트 전체를 풀에 반환합니다.
        GameObject objectToReturn = (transform.parent != null) ? transform.parent.gameObject : gameObject;
        maker.ReturnEnemy2Pool(objectToReturn);
    }
}

