using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  // UI image 기능을 사용하기 위한 네임스페이스
using UnityEngine.Video; // VideoPlayer를 제어하기 위한 네임스페이스

public class GazePointerCtrl : MonoBehaviour
{
    [Header("UI Settings")]
    public Transform uiCanvas;
    public Image gazeImg;
    public float uiScaleVal = 1f;
    
    [Header("Game Settings")]
    public string nextGameScene = "RandomTowerDefense";
    public float gazeChargeTime = 3f;

    [Header("Video Control")]
    // 제어할 외부 비디오 플레이어를 인스펙터에서 할당
    public VideoPlayer targetVideoPlayer; 

    // Internal Variables
    private Vector3 defaultScale;
    private float curGazeTime = 0;
    private GameObject currentHitObj;
    private Vector3 objectSize_OG;

    void Start()
    {
        defaultScale = uiCanvas.localScale;
        curGazeTime = 0;
    }

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hitInfo;

        if (Physics.Raycast(ray, out hitInfo))
        {
            if (uiCanvas != null)
            {
                uiCanvas.position = hitInfo.point;
                uiCanvas.localScale = defaultScale * uiScaleVal * hitInfo.distance;
                uiCanvas.LookAt(transform.position);
            }
            
            if (hitInfo.transform.CompareTag("GazeObj"))
            {
                // 새로운 오브젝트를 보거나, 기존에 아무것도 안 보고 있었을 때
                if (currentHitObj != hitInfo.transform.gameObject)
                {
                    if (currentHitObj != null)
                    {
                        GazeExit(); // 이전 오브젝트 정리
                    }
                    currentHitObj = hitInfo.transform.gameObject;
                    objectSize_OG = currentHitObj.transform.localScale;
                }
                GazeProcessing(); // 시선 처리 (비디오 일시정지 포함)
            }
            else
            {
                GazeExit(); // GazeObj가 아닌 다른 물체를 볼 때
            }
        }
        else
        {
            // 허공을 볼 때
            uiCanvas.localScale = defaultScale * uiScaleVal;
            uiCanvas.position = transform.position + (transform.forward * 2.0f); 
            GazeExit();
        }
        
        if (uiCanvas != null) uiCanvas.LookAt(transform.position);

        // 시선 시간 클램핑 및 UI 갱신
        curGazeTime = Mathf.Clamp(curGazeTime, 0, gazeChargeTime);
        if (gazeImg != null) gazeImg.fillAmount = curGazeTime / gazeChargeTime;
    }

    void GazeProcessing()
    {
        if (currentHitObj == null) return;

        // [추가됨] 시선이 머무는 동안 비디오 일시 정지
        if (targetVideoPlayer != null && targetVideoPlayer.isPlaying)
        {
            targetVideoPlayer.Pause();
        }

        // 1. 시간 누적
        curGazeTime += Time.deltaTime;
        float ratio = Mathf.Clamp01(curGazeTime / gazeChargeTime);

        // 2. 게이지 UI 채우기
        if (gazeImg != null) 
        {
            gazeImg.fillAmount = ratio;
        }

        // 3. 색상 변경
        Renderer rend = currentHitObj.GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.Lerp(Color.white, Color.green, ratio);

        // 4. 크기 변경
        currentHitObj.transform.localScale = Vector3.Lerp(objectSize_OG, objectSize_OG * 1.3f, ratio);

        // 5. 씬 이동
        if (curGazeTime >= gazeChargeTime)
        {
            // 씬 이동 직전 비디오 완전 정지 (선택 사항, 안전장치)
            if(targetVideoPlayer != null) targetVideoPlayer.Stop(); 
            
            SceneManager.LoadScene(nextGameScene);
        }
    }

    void GazeExit()
    {
        // [추가됨] 시선이 벗어나면 비디오 다시 재생
        if (targetVideoPlayer != null && !targetVideoPlayer.isPlaying)
        {
            targetVideoPlayer.Play();
        }

        if (currentHitObj == null) return;

        // 원래대로 복구
        currentHitObj.transform.localScale = objectSize_OG;
        Renderer rend = currentHitObj.GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.white;
        
        curGazeTime = 0f;
        if (gazeImg != null) gazeImg.fillAmount = 0f;
        
        currentHitObj = null;
    }
}