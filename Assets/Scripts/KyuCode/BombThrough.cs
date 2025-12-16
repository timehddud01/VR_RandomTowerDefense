using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombThrough : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        int terrainLayer = LayerMask.NameToLayer("Terrain");
        int myLayer = gameObject.layer;

         if (terrainLayer >= 0)
        {
            Physics.IgnoreLayerCollision(myLayer, terrainLayer, true);
        }

        Collider myCollider = GetComponent<Collider>();
        if (myCollider != null)
        {
            // 태그로 찾기
            GameObject[] taggedTerrains = GameObject.FindGameObjectsWithTag("Terrain");
            foreach (GameObject t in taggedTerrains)
            {
                Collider tc = t.GetComponent<Collider>();
                if (tc != null) Physics.IgnoreCollision(myCollider, tc);
            }

            // TerrainCollider로 찾기 (Unity Terrain 사용 시)
            TerrainCollider[] terrainColliders = FindObjectsOfType<TerrainCollider>();
            foreach (TerrainCollider tc in terrainColliders)
            {
                Physics.IgnoreCollision(myCollider, tc);
            }
    }

    // Update is called once per frame
    void Update()
    {

    }


}
}

