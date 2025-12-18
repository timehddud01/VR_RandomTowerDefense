using UnityEngine;
using UnityEngine.SceneManagement; // 씬 관리를 위해 필수

public class SceneChange : MonoBehaviour
{
    [Header("설정")]
    [Tooltip("이동하고자 하는 씬의 이름을 입력하세요.")]
    public string nextGameScene = "RandomTowerDefense";

    // 1. 외부(UI 버튼 등)에서 호출하거나, 특정 조건 만족 시 호출할 함수
    public void LoadNextScene()
    {
        // 씬 이름이 유효한지 체크
        if (!string.IsNullOrEmpty(nextGameScene))
        {
            SceneManager.LoadScene(nextGameScene);
        }
        else
        {
            Debug.LogWarning("이동할 씬의 이름이 설정되지 않았습니다!");
        }
    }

    // 2. 만약 스크립트에서 직접 씬 이름을 넣어 이동하고 싶다면 이 함수 사용 (오버로딩)
    public void LoadNextScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    // (테스트용) 키보드 스페이스바를 누르면 이동 (필요 없으면 Update 전체 삭제 가능)
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            LoadNextScene();
        }
    }
}