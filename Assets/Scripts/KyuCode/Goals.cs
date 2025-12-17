using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Goals : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        // 충돌한 오브젝트가 Enemy인지 확인합니다.
     if (other.CompareTag("Enemy"))
        {
            Enemy enemy = other.GetComponent<Enemy>();

            // GameManager에 라이프 감소를 요청합니다.
            if (enemy != null && !enemy.IsDead())
            {

                                if (GoalManager.instance != null)
                {
                    GoalManager.instance.EnemyReachedGoal();
                }
            }

            enemy.Die(false);
        }
    }
}

