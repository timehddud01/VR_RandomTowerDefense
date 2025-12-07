using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveManagerForSpider : MonoBehaviour
{
    /// <summary>
    /// NavMesh Agent 컴포넌트
    /// </summary>
    private NavMeshAgent agent;

    /// <summary>
    /// 순서대로 방문할 경유지 목록.
    /// 마지막 요소가 최종 목적지(Goal)가 됩니다.
    /// </summary>
    public Transform[] waypoints;

    /// <summary>
    /// 현재 목표로 하는 경유지의 인덱스
    /// </summary>
    private int currentWaypointIndex = 0;

    void Start()
    {
        // NavMesh Agent 컴포넌트를 가져옵니다.
        agent = GetComponent<NavMeshAgent>();

        // 경유지가 설정되어 있고, 목록이 비어있지 않다면 첫 번째 경유지로 이동을 시작합니다.
        // if (waypoints != null && waypoints.Length > 0)
        // {
        //     agent.destination = waypoints[currentWaypointIndex].position;
        // }
    }

    void Update()
    {
        // 모든 경유지를 방문했다면 더 이상 처리하지 않습니다.
        if (currentWaypointIndex >= waypoints.Length)
        {
            return;
        }

        // 에이전트가 현재 목표 경유지에 도착했는지 확인합니다.
        // agent.pathPending: 경로 계산이 아직 진행 중인지 여부 (false여야 함)
        // agent.remainingDistance: 현재 목적지까지 남은 거리
        // agent.stoppingDistance: 목적지에 도착했다고 판단하는 거리
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // 다음 경유지로 인덱스를 증가시킵니다.
            currentWaypointIndex++;

            // 방문할 경유지가 더 남아있다면, 다음 목적지를 설정합니다.
            if (currentWaypointIndex < waypoints.Length)
            {
                agent.destination = waypoints[currentWaypointIndex].position;
            }
            else
            {
                // 모든 경유지 방문을 완료했을 때의 처리
                Debug.Log("모든 경유지를 거쳐 최종 목적지에 도착했습니다!");
            }
        }
    }




    
    //--------------------------------------------------------------------------추가 부분 
    public void SetPath(Transform[] path)
{
    // [핵심] 외부에서 전달받은 경로 배열을 내부 waypoints 변수에 할당합니다.
    waypoints = path;
    
    // 이동 인덱스를 초기화합니다. (필수)
    currentWaypointIndex = 0;

    // NavMesh Agent 컴포넌트를 가져옵니다. (Start() 로직을 여기에 통합)
    if (agent == null)
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // 경로가 설정되었으므로 첫 번째 경유지로 이동을 시작합니다.
    if (waypoints != null && waypoints.Length > 0)
    {
        agent.destination = waypoints[currentWaypointIndex].position;
    }
}

    
}

