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
        vp.clip = vcList[0]; 
        curVCidx = 0; 
        vp.Play();
    }

    // Update is called once per frame
    void Update()
    {
        

    }
    public void SetVideoPlay(int num) //GazePointerCtrl에서 호출하는 함수
    {   
        
    }

}