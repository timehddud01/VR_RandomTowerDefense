using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class TowerAttack : MonoBehaviour
{
    [Header("타워 능력치")]
    public float attackRange = 15f;
    public float fireRate = 1f;
    public float damage = 25f;

    [Header("Unity 설정")]
    public string enemyTag = "Enemy";

    private List<Transform> enemiesInRange = new List<Transform>();
    private Transform currentTarget;
    private float fireCooldown = 0f;
    private Animator animator;
    public GameObject attackVFX;

    // --- 위치 보정용 변수 추가 ---
    private Vector3 initialPosition;
    private Quaternion initialRotation;
    private float positionResetTimer = 0f;
    private float resetInterval = 5f; // 5초 주기
    // -------------------------

    void Start()
    {
        // 처음 위치와 회전값을 저장
        initialPosition = transform.position;
        initialRotation = transform.rotation;

        animator = GetComponent<Animator>();
        
        // 애니메이션이 위치를 옮기지 못하도록 설정 (루트 모션 제어)
        if (animator != null)
        {
            animator.applyRootMotion = false; 
        }

        SphereCollider rangeCollider = GetComponent<SphereCollider>();
        if (rangeCollider == null)
        {
            rangeCollider = gameObject.AddComponent<SphereCollider>();
        }
        rangeCollider.isTrigger = true;
        rangeCollider.radius = attackRange;
    }

    void Update()
    {
        // 1. 위치 보정 로직 (5초마다 실행)
        positionResetTimer += Time.deltaTime;
        if (positionResetTimer >= resetInterval)
        {
            ResetPosition();
            positionResetTimer = 0f;
        }

        // 2. 타겟 관리 및 공격 로직
        if (currentTarget == null || !currentTarget.gameObject.activeInHierarchy)
        {
            enemiesInRange.RemoveAll(enemy => enemy == null || !enemy.gameObject.activeInHierarchy);
            if (enemiesInRange.Count > 0)
            {
                currentTarget = enemiesInRange.First();
            }
            else
            {
                currentTarget = null;
            }
        }

        if (currentTarget != null)
        {
            // 타겟을 바라봄 (Y축만 회전하게 하여 바닥에 파묻히는 것 방지)
            Vector3 targetDir = currentTarget.position - transform.position;
            targetDir.y = 0; // 타워가 위아래로 꺾이지 않게 함
            if (targetDir != Vector3.zero)
            {
                transform.rotation = Quaternion.LookRotation(targetDir);
            }

            if (fireCooldown <= 0f)
            {
                Attack();
                fireCooldown = 1f / fireRate;
            }
        }

        fireCooldown -= Time.deltaTime;
    }

    // 위치를 강제로 초기값으로 돌리는 함수
    void ResetPosition()
    {
        transform.position = initialPosition;
        // 회전은 타겟을 바라봐야 하므로 필요 시에만 초기화하거나 유지합니다.
        // transform.rotation = initialRotation; 
        Debug.Log(gameObject.name + " 위치 보정 완료");
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            enemiesInRange.Add(other.transform);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(enemyTag))
        {
            enemiesInRange.Remove(other.transform);
            if (currentTarget == other.transform)
            {
                currentTarget = null;
            }
        }
    }

    void Attack()
    {
        if (currentTarget == null) return;

        Enemy enemyComponent = currentTarget.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.TakeDamage(damage);
            
            if (attackVFX != null)
            {
                GameObject vfx = Instantiate(attackVFX, currentTarget.position, Quaternion.identity);
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                Destroy(vfx, ps != null ? ps.main.duration : 2f);
            }
        }

        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}