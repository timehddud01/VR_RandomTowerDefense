using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro; // TextMeshPro 사용

public class TowerCount : MonoBehaviour
{
    public static TowerCount Instance;

    [Header("Settings")]
    public int initialLimit = 4;      // 게임 시작 시 기본 제한
    public int bonusPerRound = 2;     // 라운드 당 추가 제한

    [Header("Status")]
    public int maxTowerLimit;         // 현재 최대 설치 가능 수
    public int currentPlacedTowers;   // 현재 설치된 타워 수

    [Header("UI")]
    public TextMeshProUGUI infoText;  // 정보를 표시할 텍스트 UI

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 게임 시작 시 초기화 (GameManager에서 호출)
    public void InitTowerCount()
    {
        maxTowerLimit = initialLimit;
        currentPlacedTowers = 0;
        UpdateUI();
    }

    // 라운드 종료 후 제한 증가 (GameManager에서 호출)
    public void AddMaxLimit()
    {
        maxTowerLimit += bonusPerRound;
        UpdateUI();
    }

    // 설치 가능 여부 확인
    public bool CanPlaceTower()
    {
        return currentPlacedTowers < maxTowerLimit;
    }

    // 타워 생성 시 호출
    public void AddCurrentCount()
    {
        currentPlacedTowers++;
        UpdateUI();
    }

    // 타워 삭제/합성 시 호출
    public void RemoveCurrentCount()
    {
        currentPlacedTowers--;
        if (currentPlacedTowers < 0) currentPlacedTowers = 0;
        UpdateUI();
    }

    // UI 업데이트
    private void UpdateUI()
    {
        if (infoText == null) return;

        // 양식:
        // -[포탑 제한]
        // -지금까지 설치된 타워 수 / 전체 설치가능한 타워 수
        // -다음 라운드에 설치가능한 타워 수 (다음 라운드 예상 최대치)
        
        infoText.text = $"[Tower Placed]\n" +
                        $"{currentPlacedTowers} / {maxTowerLimit}\n" +
                        $"Next Round:\n" +
                        $"you get\n" +
                        $"{bonusPerRound}";
    }
}