using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BombThrough : MonoBehaviour
{
    // Start is called before the first frame update

     public GameObject BOmbVFX;

    private Vector3 startPos;
    private Quaternion startRot;

    void Start()
    {
        startPos = transform.position;
        startRot = transform.rotation;
    }
   private void OnCollisionEnter(Collision collision)
    {
        // Floor 태그에 닿았는지 확인
        if (collision.gameObject.CompareTag("Floor"))
        {
            ActivateBomb();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Floor 태그에 닿았는지 확인 (Trigger인 경우 대비)
        if (other.CompareTag("Floor"))
        {
            ActivateBomb();
        }
    }

    private void ActivateBomb()
    {
        // 맵에 있는 모든 Enemy 찾기
        Enemy[] enemies = FindObjectsOfType<Enemy>();

        foreach (Enemy enemy in enemies)
        {
            // 살아있는 적에게만 데미지 적용
            if (enemy != null && !enemy.IsDead())
            {
                enemy.TakeDamage(100f);
            }
        }
        GameObject vfx = Instantiate(BOmbVFX, new Vector3(0, 10, 0), Quaternion.identity);
                ParticleSystem ps = vfx.GetComponent<ParticleSystem>();
                Destroy(vfx, ps != null ? ps.main.duration : 2f);

        // 자신 파괴
        transform.position = startPos;
        transform.rotation = startRot;
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        gameObject.SetActive(false);
    }
}
