using UnityEngine;

public class PlayerMove : MonoBehaviour
{   
    //이동 속도
    public float speed = 10;
    // //CharacterController 컴포넌트
    CharacterController cc;

    public float jumpPower = 5;


    // 중력 가속도의 크기
    public float gravity = -10;
    // 수직 속도
    float yVelocity = 0;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cc = GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        //사용자의 입력에 따라 전후좌우로 이동하고 싶다.
        //1. 사용자의 입력을 받는다.
        float h = ARAVRInput.GetAxis("Horizontal");
        float v = ARAVRInput.GetAxis("Vertical");
        // float e = ARAVRInput.GetAxis("");


        Vector3 dir = new Vector3(h,0,v);

        //사용자가 바라보는 방향으로 입력 값 변화시키기
        dir = Camera.main.transform.TransformDirection(dir);

        //2. 방향을 만든다
        //2.1 중력을 적용한 수직 방향 추가 v = v0 + at
        //가속도와 속도, 위치에 대한 개념 인지
        yVelocity += gravity * Time.deltaTime;

        if(cc.isGrounded)
        {
            yVelocity = 0;
        }
        if (ARAVRInput.GetDown(ARAVRInput.Button.Two,ARAVRInput.Controller.RTouch)){
            yVelocity = jumpPower;
        }
        dir.y = yVelocity;
        // //3. 이동한다.
        if (Input.GetKey(KeyCode.E))
        {
            dir.y += 10; // 월드 기준 위로
        }
        else if (Input.GetKey(KeyCode.Q))
        {
            dir.y -= 10; // 월드 기준 아래로
        }

        cc.Move(dir*speed*Time.deltaTime);
    }
}

//기말 프로젝트 : 디펜스의 개념(막는다) 1,2,3번째 프로젝트 포함하기
//Bullet : 오브젝트 풀 사용, 충돌 이펙트 적용 등 기능의 조합
//창의적, 도전성을 높이 평가