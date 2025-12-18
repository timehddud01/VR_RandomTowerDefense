using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video; //VideoPlayer기능을 사용하기 위한 네임스페이스

public class Video360Play : MonoBehaviour
{
    VideoPlayer vp;
    public VideoClip[] vcList;
    int curVCidx;
    // Start is called before the first frame update
    void Start()
    {
        vp=GetComponent<VideoPlayer>();
        vp.clip = vcList[0]; //첫번째 값을 임의로 넣어줌
        curVCidx = 0; //현재 실행되고 있는 비디오클립의 index를 의미하는 변수 설정
        vp.Play();
        //vp.Stop();


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftBracket))
        {
            // curVCidx = curVCidx -1;
            // if(curVCidx>=vcList.Length){
            //     curVCidx = curVCidx + vcList.Length;
            // }
            //위 if문을 대체할 수 있는 % 활용 코드
            curVCidx = (curVCidx - 1 + vcList.Length) % vcList.Length;

            SetVideoPlay(curVCidx);
            // vp.clip = vcList[curVCidx]; //실행 코드는 한 함수로 통일, 메모리 누수
            //vp.Play();


        }
        else if (Input.GetKeyDown(KeyCode.RightBracket))
        {
            curVCidx = curVCidx + 1;
            if (curVCidx >= vcList.Length)
            {
                curVCidx = curVCidx - vcList.Length;
            }


            SetVideoPlay(curVCidx);
            // vp.clip = vcList[curVCidx]; //실행 코드는 한 함수로 통일, 메모리 누수
            //vp.Play();
        }



    }
    public void SetVideoPlay(int num) //GazePointerCtrl에서 호출하는 함수
    {   
        num = num % vcList.Length; //안전벨트. 범위 안에 들어있는 비디오만 볼 수 있게
        //현재 재생중인 번호가 전달받은 번호와 다를때만 실행
        if (curVCidx != num)
        {
            vp.Stop(); //멈추고
            vp.clip = vcList[num]; //새로운 클립으로 교체
            curVCidx = num;
            vp.Play();
        }
    }

    // public void SwapVideoClip(bool isNext)
    // {
    //     int setVCnum = curVCidx;
    //     vp.Stop();
    //     if (isNext)
    //     {

    //         setVCnum = (curVCidx + 1) % vcList.Length;
    //     }
    //     else
    //     {
    //         curVCidx = (curVCidx - 1 + vcList.Length) % vcList.Length;
    //     }
    //     vp.clip = vcList[setVCnum];
    //     vp.Play();
    //     curVCidx = setVCnum;
        
    // }

}