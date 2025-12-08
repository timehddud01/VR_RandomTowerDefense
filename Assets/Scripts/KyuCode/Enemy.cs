using UnityEngine;

public class Enemy : MonoBehaviour
{
    /// <summary>
    /// 적의 최대 체력
    /// </summary>
    public float maxHealth = 100f;
    private EnemyMaker maker;
    /// <summary>
    /// 현재 체력
    /// </summary>
    private float currentHealth;
    public static GameManager instance;
    void Awake()
    {
        // 시작 시 현재 체력을 최대 체력으로 설정합니다.
        currentHealth = maxHealth;
    }
        public void SetMakerAndTarget(EnemyMaker enemyMaker, Transform targetEndPoint) 
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
        // TODO: 여기에 골드 획득, 파괴 이펙트 생성 등의 코드를 추가할 수 있습니다.
    if (EnemySpawners.instance != null)
        {
            EnemySpawners.instance.EnemyDestroyed();
        }
        // 부모 오브젝트가 있다면 부모를 파괴하고, 없다면 자기 자신을 파괴합니다.
        // 이렇게 하면 적 모델과 체력바 등을 포함하는 부모 오브젝트 전체를 한번에 제거할 수 있습니다.
        GameObject objectToDestroy = (transform.parent != null) ? transform.parent.gameObject : gameObject;
        maker.ReturnEnemy2Pool(gameObject); 
        Destroy(objectToDestroy);
        
    }
}

