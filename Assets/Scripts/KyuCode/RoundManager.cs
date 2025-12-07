using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class RoundManager : MonoBehaviour
{
    public static RoundManager instance;

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // 20라운드 HP 데이터
    private int[] waveHPs = new int[] {
        100, 150, 220, 350, 1500,       // 1~5R
        500, 700, 950, 1300, 5000,      // 6~10R
        1800, 2400, 3200, 4200, 15000,  // 11~15R
        6000, 8500, 11000, 14000, 45000 // 16~20R
    };
    public int GetWaveHP(int waveIndex)
    {
        // waveIndex는 0부터 시작하므로 안전하게 처리
        if (waveIndex >= 0 && waveIndex < waveHPs.Length)
        {
            return waveHPs[waveIndex];
        }

        // 20라운드를 초과하면 마지막 라운드의 HP를 계속 사용하도록 처리
        if (waveIndex >= waveHPs.Length && waveHPs.Length > 0)
        {
            return waveHPs[waveHPs.Length - 1];
        }

        return 100; // 배열이 비어있거나 에러 발생 시 기본값
    }
}

