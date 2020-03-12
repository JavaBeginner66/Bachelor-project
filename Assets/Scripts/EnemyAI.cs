using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;  
    private State state;

    public float chaseSpeed;
    public float patrolSpeed;

    public Transform target;
    public Transform[] waypoints;
    public int waypointsIndex;

    public float stateMachineTimer;

    public enum State
    {
        CHASE,
        PATROL,
        CASTING
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        state = State.CHASE;
    }

    IEnumerator StateMachine()
    {
        yield return new WaitForSeconds(stateMachineTimer);
    }

    private void Update()
    {
        
        BattleMonitor();

        if(state == State.CHASE)
            Chase();
        
        if (state == State.PATROL)
            Patrol();

        if (state == State.CASTING)
            Cast();
    }

    private void BattleMonitor()
    {

    }

    private void Chase()
    {
        agent.speed = chaseSpeed;
        if (Vector3.Distance(this.transform.position, target.transform.position) >= 2)
        {
            agent.SetDestination(target.transform.position);
        }        
    }

    private void Patrol()
    {
        agent.speed = patrolSpeed;

        if(Vector3.Distance(this.transform.position, waypoints[waypointsIndex].transform.position) >= 2)
        {
            agent.SetDestination(waypoints[waypointsIndex].transform.position);

        }
        else if(Vector3.Distance(this.transform.position, waypoints[waypointsIndex].transform.position) <= 2)
        {
            waypointsIndex = (waypointsIndex + 1) % waypoints.Length;
        }
    }

    private void Cast()
    {
        agent.speed = 0f;
    }
}
