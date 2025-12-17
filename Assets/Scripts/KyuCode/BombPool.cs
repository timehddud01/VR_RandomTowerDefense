using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombPool : MonoBehaviour
{
    // Start is called before the first frame update
public static BombPool instance;
    public List<GameObject> bombs = new List<GameObject>();

    void Awake()
    {
        if (instance == null) instance = this;
    }

    void Start()
    {
        
        // 씬에 있는 모든 폭탄을 찾아서 리스트에 추가 (만약 인스펙터에서 할당하지 않았다면)
        if (bombs.Count == 0)
        {
            BombThrough[] foundBombs = FindObjectsOfType<BombThrough>();
            foreach (var bomb in foundBombs)
            {
                bombs.Add(bomb.gameObject);
            }
        }
    }

    // Update is called once per frame

    public void ReplenishBomb()
    {
        
        // 비활성화된 폭탄 하나를 찾아서 활성화 (보충)
        foreach (GameObject bomb in bombs)
        {
            if (!bomb.activeSelf)
            {
                bomb.SetActive(true);
                Debug.Log("폭탄이 보충되었습니다.");
                return;
            }
        }
        Debug.Log("보충할 폭탄이 없습니다 (모두 활성화 상태).");
    }
}
