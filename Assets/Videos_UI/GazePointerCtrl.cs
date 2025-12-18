using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;  //UI image기능을 사용하기 위한 네임스페이스
using UnityEngine.Video; //VideoPlayer를 제어하기 위한 네임스페이스


//카메라의 시선을 처리하기 위한 기능
public class GazePointerCtrl : MonoBehaviour
{
    public Transform uiCanvas;
    public Image gazeImg;
    Vector3 defaultScale; //ui를 오브젝트화 하기 때문에, 카메라와의 거리가 중요하다.자동으로 private이 됨
    public float uiScaleVal = 1f; //1f, 1.0f 둘 다 가능

    public string nextGameScene ="RandomTowerDefense";

    
    
    float curGazeTime = 0 ; //시선이 머무르는 시간을 저장하기 위한 변수
    public float gazeChargeTime = 3f; //게이지가 차는 시간을 체크하기 위한 기준 시간 3초(필요에 따라 수정)
    private GameObject currentHitObj; //현재 시선이 머무르고 있는 오브젝트를 저장하기 위한 변수

    private Vector3 objectSize_OG; //원래 크기 기억

    // Start is called before the first frame update
    void Start()
    {
        defaultScale = uiCanvas.localScale;  //오브젝트가 갖는 기본 스케일 값
        curGazeTime = 0; //시선을 유지하는지 체크하기 위한 변수를 초기화
    }

    // Update is called once per frame
    void Update()
    {
        
        //2.카메라를 기준으로 전방의 레이를 설정한다.
        Ray ray = new Ray(transform.position,transform.forward); //위치와 방향정보
        RaycastHit hitInfo;//히트된 오브젝트의 정보를 담는다.

        //3.레이에 부딪힌 경우에는 거리 값을 이용해 uiCanvas의 크기를 조절한다.
        if (Physics.Raycast(ray, out hitInfo))
        {
            if (uiCanvas != null)
            {
                uiCanvas.position = hitInfo.point; // 닿은 위치 그대로
                uiCanvas.localScale = defaultScale * uiScaleVal * hitInfo.distance;
                uiCanvas.LookAt(transform.position); // 카메라는 쳐다보게 함
            }
            
            if (hitInfo.transform.tag == "GazeObj")
            {
                if(currentHitObj != hitInfo.transform.gameObject)
                {
                    if(currentHitObj != null)
                    {
                        GazeExit(); //이전 오브젝트에서 시선이 벗어났을 때 처리
                    }
                    currentHitObj = hitInfo.transform.gameObject; //현재 시선이 머무르고 있는 오브젝트 저장
                    objectSize_OG = currentHitObj.transform.localScale; //현재 오브젝트의 원래 크기 저장
                }
                GazeProcessing();
            }
            else
            {
                GazeExit();
            }
           
        }

        else
        {
           uiCanvas.localScale = defaultScale * uiScaleVal;
           uiCanvas.position = transform.position + (transform.forward * 2.0f); 
           GazeExit();
            
        }
        
        
        if (uiCanvas != null) uiCanvas.LookAt(transform.position);

        curGazeTime = Mathf.Clamp(curGazeTime, 0, gazeChargeTime); //시선이 머문 시간을 0과 최댓값(여기에선 3초) 사이로 한다.
        gazeImg.fillAmount = curGazeTime / gazeChargeTime; //0% ~100


    }

    //위에서 무엇을 봤는지 결정한 것이고
    //이제는 무엇을 할건지 결정하는 함수
    void GazeProcessing()
    {
        if (currentHitObj == null) return;

        // 1. 시간 누적
        curGazeTime += Time.deltaTime;
        float ratio = Mathf.Clamp01(curGazeTime / gazeChargeTime);

        // 2. 게이지 UI 채우기 (이미지 설정이 Filled여야 보임)
        if (gazeImg != null) 
        {
            gazeImg.fillAmount = ratio;
        }

        
        Renderer rend = currentHitObj.GetComponent<Renderer>();
        if (rend != null) rend.material.color = Color.Lerp(Color.white, Color.green, ratio);

        // 4. 크기 변경 (저장된 크기 기준 1.3배)
        currentHitObj.transform.localScale = Vector3.Lerp(objectSize_OG, objectSize_OG * 1.3f, ratio);

        // 5. 씬 이동
        if (curGazeTime >= gazeChargeTime)
        {
            SceneManager.LoadScene(nextGameScene);
        }
    }

    void GazeExit()
    {
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

