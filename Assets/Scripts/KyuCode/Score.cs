using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{
    /// <summary>
    /// 다른 스크립트에서 Score 기능에 쉽게 접근할 수 있도록 하는 싱글톤 인스턴스
    /// </summary>
    public static Score instance;

    /// <summary>
    /// 점수를 표시할 UI 텍스트 (TextMeshPro)
    /// </summary>
    public TextMeshPro scoreText;

    private int currentScore;

    void Awake()
    {
        // 싱글톤 패턴 구현
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        currentScore = 0;
        UpdateScoreUI();
    }

    public void AddScore(int amount)
    {
        currentScore += amount;
        UpdateScoreUI();
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
        {
            scoreText.text = "Score:\n" + currentScore;
        }
    }
}

