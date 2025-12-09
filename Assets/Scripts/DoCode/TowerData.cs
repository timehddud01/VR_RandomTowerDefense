
using UnityEngine;

public class TowerData : MonoBehaviour
{
    public enum TowerGrade
    {
        Common = 0,
        Magic = 1,
        Rare = 2,
        Unique = 3,
        Epic = 4
    }

    [Header("타워 기본 정보")]
    public string towerId;      // 예: "common_firetower"
    public TowerGrade grade;    // 일반, 매직, 레어, 유니크, 에픽

    [Header("경제 관련 정보")]
    public int sellPrice = 10;  // 판매 가격 (나중에 판매 기능에서 사용)

    [HideInInspector]
    public PlatformSlot ownerSlot;  // 이 타워가 배치된 플랫폼
}
