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

    void Start()
    {
        animator = GetComponent<Animator>();
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
        // 타겟이 유효한지 검사 및 리스트 정리
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
            // 타겟을 바라봄
            transform.LookAt(currentTarget);

            if (fireCooldown <= 0f)
            {
                Attack();
                fireCooldown = 1f / fireRate;
            }

        }
        else if(currentTarget == null)
        {
            // animator.ResetTrigger("Attack");
            // animator.Play("Idle");
        }

        fireCooldown -= Time.deltaTime;
    } // <--- 여기에 닫는 괄호가 빠져 있었습니다!

    // 이제 이 함수들은 Update 밖으로 나와서 정상적으로 작동합니다.
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
        if (currentTarget == null) return; // 안전장치 추가

        Enemy enemyComponent = currentTarget.GetComponent<Enemy>();
        if (enemyComponent != null)
        {
            enemyComponent.TakeDamage(damage);
            Debug.Log(currentTarget.name + "을(를) 공격!");
            
            if (attackVFX != null)
            {
                Instantiate(attackVFX, currentTarget.position, Quaternion.identity);
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