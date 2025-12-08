using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class DroneAI : MonoBehaviour
{


    enum DroneState{
        Idle,
        Move,
        Attack,
        Damage,
        Die
    }

    DroneState state = DroneState.Idle;
    public float idleDelayTime = 2f;
    float currentTime;

    public float moveSpeed = 1;
    //타워(목표)위치
    Transform tower;
    //길찾기 수행을 위한 내비메시에이전트
    NavMeshAgent agent;

    public float attackRange = 3;
    public float attackDelay = 2;


    Transform explosion;
    ParticleSystem expEffect;
    AudioSource expAudio;

    
    [SerializeField]
    int hp= 3;


    // Start is called before the first frame update
    void Start()
    {
        //타워찾기
        tower = GameObject.Find("Tower").transform;
        agent = GetComponent<NavMeshAgent>();
        agent.enabled = false;
        agent.speed = moveSpeed;

        explosion = GameObject.Find("Explosion").transform;
        expEffect = explosion.GetComponent<ParticleSystem>();
        expAudio = explosion.GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        print("Current State : " + state);
        switch (state){
            case DroneState.Idle:
            Idle();
            break;
            case DroneState.Move:
            Move();
            break;
            case DroneState.Attack:
            Attack();
            break;
            case DroneState.Damage:
            // Damage();
            break;
            case DroneState.Die:
            Die();
            break;

        }
    }

    private void Idle(){
        currentTime += Time.deltaTime;
        
        if(currentTime > idleDelayTime){
            state = DroneState.Move;
            agent.enabled = true;
        }
    }
    private void Move(){
        agent.SetDestination(tower.position);
        if(Vector3.Distance(transform.position, tower.position) < attackRange){
            state = DroneState.Attack;
            agent.enabled = false;
            currentTime = attackDelay;
        }
    }
    private void Attack(){
        currentTime += Time.deltaTime;
        if(currentTime > attackDelay){
            currentTime = 0;
            Tower.Instance.HP--;
        }
    }

    private void Die(){
        expEffect.Play();
        expAudio.Play();
    
        Destroy(gameObject,3);
    }

    IEnumerator Damage(){
        agent.enabled = false;
        Material mat = GetComponentInChildren<MeshRenderer>().material;

        Color originalColor = mat.color;

        mat.SetColor("_Color",Color.red);
        yield return new WaitForSeconds(0.1f);
        mat.SetColor("_Color", originalColor);
        state = DroneState.Idle;
        currentTime = 0;
    }

public void onDamagedProcess(){
    hp--;
    if(hp > 0){
        state = DroneState.Damage;
        StopAllCoroutines();
        StartCoroutine(Damage());
    }
    else if(hp<=0){
        state = DroneState.Die;
    }
}


}
