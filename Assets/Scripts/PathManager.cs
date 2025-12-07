using UnityEngine;

/// <summary>
/// 모든 적 유닛이 사용할 웨이포인트 경로를 중앙에서 관리합니다.
/// 싱글톤 패턴을 사용하여 어디서든 접근 가능합니다.
/// </summary>
public class PathManager : MonoBehaviour
{
    // 씬 전체에서 이 인스턴스에 접근할 수 있게 해주는 싱글톤 변수
    public static PathManager Instance { get; private set; } 

    /// <summary>
    /// Unity Inspector에서 직접 연결할 적의 이동 경로 목록
    /// 이 배열의 순서대로 적이 이동
    /// </summary>
    public Transform[] EnemyPath; 

    private void Awake()
    {
        // 싱글톤 초기화 로직
        if (Instance == null)
        {
            Instance = this;
            
        }
        else
        {
            
            Destroy(gameObject);
        }
    }
}