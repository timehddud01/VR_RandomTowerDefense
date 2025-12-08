using UnityEngine;
using System.Collections;

public class EnemyDie : MonoBehaviour
{
    // EnemyMaker 참조 변수 (풀 반환 요청 시 필요)
    private EnemyMaker maker; 
    
    // [설정] 3초 후 사망 처리
    private const float TEST_DEATH_TIME = 5.0f; 

    // === 1. EnemyMaker로부터 초기 정보를 받는 함수 ===

    public void SetMakerAndTarget(EnemyMaker enemyMaker, Transform targetEndPoint) 
    {    
    
        // EnemyDie는 배열의 마지막 요소를 최종 목표로 사용합니다.

        // 1. maker 참조 저장
        maker = enemyMaker;
        
        // [수정 1] 새로운 타이머를 시작하기 전에 기존 타이머를 모두 정리합니다.
        //         (이전에 풀에서 사용하던 코루틴이 남아있을 수 있기 때문에 안전 장치)
        StopAllCoroutines(); 
        
        // 2. 객체가 활성화될 때마다 새로운 타이머를 시작합니다.
        StartCoroutine(StartDeathTimer()); 
    }
    
    // === 2. 3초 후 사망 처리 코루틴 ===

    private IEnumerator StartDeathTimer()
    {
        // [수정 2] StopAllCoroutines()을 여기서 제거. SetMakerAndTarget에서 이미 처리됨.
        
        // 3초 대기
        yield return new WaitForSeconds(TEST_DEATH_TIME);
        
        // 3초 후 사망 처리
        Die();
    }

    // === 3. 최종 사망 처리 (풀 반환) ===

    public void Die()
    {
        // 사망 시 타이머(코루틴)가 혹시라도 남아있다면 중지합니다.
        StopAllCoroutines(); 
        
        if (maker != null)
        {
            Debug.Log($"[EnemyDie] {gameObject.name} 3초 경과, 풀에 반환 요청.");
            
            // EnemyMaker에게 풀에 반환을 요청하여 카운트를 줄입니다.
            maker.ReturnEnemy2Pool(gameObject); 
        }
        else
        {
            Destroy(gameObject);
        }
    }
}