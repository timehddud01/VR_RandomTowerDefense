using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MoveManager : MonoBehaviour
{

    private NavMeshAgent agent;
   private Transform[] waypoints;




    private int currentWaypointIndex = 0;


    void Awake()
    {
        // NavMesh Agent 컴포넌트를 가져옵니다.
        agent = GetComponent<NavMeshAgent>();


    }


     public void SetPath(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypointIndex = 0;

        // 경로가 유효하면 첫 번째 경유지로 이동을 시작합니다.
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.destination = waypoints[currentWaypointIndex].position;
        }
    }

    void Update()
    {
        // 모든 경유지를 방문했다면 더 이상 처리하지 않습니다.
        if (waypoints == null || currentWaypointIndex >= waypoints.Length)
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
}

