using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{

        public TextMeshPro timerText;
    private float time;
    // Start is called before the first frame update
    void Start()
    {
                time = 0;
    }

    // Update is called once per frame
    void Update()
    {
        time += Time.deltaTime;
        // 시간을 분:초 형식(00:00)으로 변환하여 텍스트에 적용
        timerText.text = string.Format("{0:00}:{1:00}", (int)time / 60, (int)time % 60);
    }
}
