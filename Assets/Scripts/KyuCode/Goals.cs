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
            // GameManager에 라이프 감소를 요청합니다.
            if (GoalManager.instance != null)
            {
                GoalManager.instance.EnemyReachedGoal();
            }

            // 목표에 도달한 적 오브젝트를 파괴합니다.
            Destroy(other.gameObject);
        }
    }
}

