using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{

    [Header("Inspector Objects")]
    public NavMeshAgent agent;
    public Transform target;
    public Transform[] waypoints;
    public Transform waypointMiddle;    
    public GameObject portalEffect;
    public Image healthDisplay;
    // Different field abilities
    public GameObject rotatingWalls;
    public GameObject targetCircle;
    public GameObject groundQuarterStatic;

    [Header("EnemyAI modifiable variables")]
    public float chaseSpeed;
    public float patrolSpeed;
    public float speed;
    public float teleportTimer = 1;
    public float bulletHellWaves = 10;
    public float healthPool;

    [HideInInspector] // Internal script variables
    public static EnemyAI enemyAI;    
    private State state;
    private int waypointsIndex;
    private float stateMachineTimer;
    private Coroutine bulletHell1;
    private float currentHealth;

    

    public enum State
    {
        CHASE,
        PATROL,
        CASTING
    }

    public State getState()
    {
        return state;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag.Equals(StatsScript.ProjectileBaseDamageTag))        
            takeDamage(other);
                  
    }

    public void takeDamage(Collider projectile)
    {
        currentHealth -= projectile.GetComponent<Projectile>().getProjectileDamage();
        healthDisplay.fillAmount = currentHealth / healthPool;
    }

    private void Start()
    {

        enemyAI = this;
        currentHealth = healthPool;
        agent = GetComponent<NavMeshAgent>();
        state = State.CASTING;
        agent.stoppingDistance = 3f;
        speed = agent.speed;
        rotatingWalls.SetActive(false);
    }

    IEnumerator StateMachine()
    {
        yield return new WaitForSeconds(stateMachineTimer);
    }

    private void Update()
    {
        if (GameMasterScript.gameRunning)
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
            bulletHell1 = StartCoroutine(FrozenOrbEnum());
    }
    
    IEnumerator FrozenOrbEnum()
    {
        agent.speed = 0f;
        Vector3 nextPos = waypoints[Random.Range(0, waypoints.Length)].transform.position;
        for (int i = 0; i<1; i++)
        {
            
            transform.position = nextPos;
            //waypointsIndex = (waypointsIndex + 1) % waypoints.Length;

            yield return new WaitForSeconds(teleportTimer);
            SpawnBullet(ObjectPool.FrozenOrb);
            nextPos = waypoints[Random.Range(0, waypoints.Length)].transform.position;
            // Bør pooles
            Destroy(Instantiate(portalEffect, new Vector3(nextPos.x, nextPos.y + .5f, nextPos.z), Quaternion.identity), 10f);
            
            yield return new WaitForSeconds(teleportTimer*3);
        }
        StartCoroutine(QuarterCircleZone());
        //StartCoroutine(RotatingCircleEnum());
    }

    IEnumerator QuarterCircleZone()
    {       
        for (int i = 0; i < 6; i++)
        {
            GameObject quarterCircleZone = Instantiate(groundQuarterStatic);
            Transform collider = quarterCircleZone.transform.Find("Collider").transform;
            Destroy(quarterCircleZone, 5f);
            /*
            RectTransform zoneCanvas = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas").GetComponent<RectTransform>();
            zoneCanvas.rotation = Quaternion.Euler(90f, i*90, zoneCanvas.rotation.z);
            */
            quarterCircleZone.transform.rotation = Quaternion.Euler(0f, i * 90, 0f);
            Image fillArea = quarterCircleZone.transform.Find("GroundQuarterStaticCanvas").transform.Find("Outer").Find("Inner").GetComponent<Image>();
            for (float j = 0; j < 1.01f; j+= .01f)
            {
                fillArea.fillAmount = j;
                yield return new WaitForSeconds(.01f);
            }
            collider.gameObject.SetActive(true);
            yield return new WaitForSeconds(3f);
        }
    }

    IEnumerator TargetCircle()
    {
        GameObject canvasCircle = Instantiate(targetCircle, new Vector3(target.position.x, 0f, target.position.z), Quaternion.identity);
        Destroy(canvasCircle, 1f);
        Transform collider = canvasCircle.transform.Find("TargetCircleCollider");
        RectTransform fillCircle = canvasCircle.transform.Find("TargetCircleCanvas").Find("Outer").Find("Inner").transform.GetComponent<RectTransform>();

        for (float i = 0; i <= 1.01f; i+=.02f)
        {          
            fillCircle.localScale = new Vector3(i, i, i);
            yield return new WaitForSeconds(.01f);
        }
        collider.gameObject.SetActive(true);
        
    }

    IEnumerator RotatingCircleEnum()
    {

        transform.position = waypointMiddle.transform.position;
        for (int i = 0; i < 10; i++)
        {
            SpawnBullet(ObjectPool.FrozenOrbStatic);
            yield return new WaitForSeconds(1f);
        }

    }

    private void SpawnBullet(string type)
    {
        GameObject obj = ObjectPool.objectPool.getStoredObject(type);
        if (obj != null)
        {
            obj.transform.position = transform.position;          
            obj.transform.rotation = transform.rotation;
            obj.SetActive(true);
            
        }
        else
        {
            Debug.Log("Utvide?");
        }
    }
}
