using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Tower : MonoBehaviour
{
    public Transform damageUI;
    public Image damageImage;

    public float damageTime = 0.1f;
    public int initialHP = 10;
    int _hp =0;

    public int HP{

        get {
            return _hp;
        }

        set {
            _hp = value;
            StopAllCoroutines();

            StartCoroutine(DamageEvent());
            if(_hp <= 0)
            {
                Destroy(gameObject);
            }
        }
    }

    public static Tower Instance;
    void Awake(){
        if(Instance == null){
            Instance = this;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        _hp = initialHP;
        float z = Camera.main.nearClipPlane + 0.01f;

        damageUI.parent = Camera.main.transform;
        damageUI.localPosition = new Vector3(0,0,z);
        damageUI.localRotation = Quaternion.identity;
        damageImage.enabled = false;


        
    }

    // Update is called once per frame
    void Update()
    {
        
    }


    IEnumerator DamageEvent(){

        damageImage.enabled = true;
        yield return new WaitForSeconds(damageTime);
        damageImage.enabled = false;
    }
}
