using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;  
    private State state;

    public float chaseSpeed;
    public float patrolSpeed;
    public float speed;

    public Transform target;
    public Transform[] waypoints;
    public int waypointsIndex;

    public float stateMachineTimer;
    public float teleportTimer = 1;
    public float bulletHellWaves = 10;

    private Coroutine bulletHell1;

    public enum State
    {
        CHASE,
        PATROL,
        CASTING
    }

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        state = State.CASTING;
        agent.stoppingDistance = 3f;
        speed = agent.speed;
    }

    IEnumerator StateMachine()
    {
        yield return new WaitForSeconds(stateMachineTimer);
    }

    private void Update()
    {

        speed = agent.speed;
        BattleMonitor();
       

        if (state == State.CHASE)
            Chase();
        
        if (state == State.PATROL)
            Patrol();

        if (state == State.CASTING)
            BullethellStage1();
    }

    private void BattleMonitor()
    {
        
    }

    private void Chase()
    {
        agent.speed = chaseSpeed;
        if (Vector3.Distance(this.transform.position, target.transform.position) >= 3f)
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

    private void BullethellStage1()
    {
        transform.LookAt(target);
        if (bulletHell1 == null)
            bulletHell1 = StartCoroutine(BulletHell());
    }
    
    IEnumerator BulletHell()
    {
        agent.speed = 0f;
        for (int i = 0; i<bulletHellWaves; i++)
        {         
          
            transform.position = waypoints[waypointsIndex].transform.position;
            waypointsIndex = (waypointsIndex + 1) % waypoints.Length;
            yield return new WaitForSeconds(teleportTimer);
        }
        
    }
}
