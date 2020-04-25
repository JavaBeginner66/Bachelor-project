using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{

    [Header("Inspector Objects")]
    public Animator anim;
    public CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask;

    public GameObject projectileToShoot;
    public GameObject projectile1;
    public GameObject projectile2;
    public GameObject projectile3;
    public GameObject projectile4;
    public GameObject projectileSpawnPoint;
    public GameObject shootingEffect1;
    public GameObject shootingEffect2;
    public GameObject shootingEffect3;
    public GameObject shootingEffect4;
    public GameObject teleportStartEffect;
    public GameObject teleportEndEffect;

    public Image[] dashCharges;
    public Image[] shieldCharges;
    public Image shootTimerDisplay;
    public Image damageMeterDisplay;

    public GameObject[] visualShieldCharges;


    [Header("Player modifiable variables ")]
    public float desiredRotationSpeed;
    public float groundDistance = 0.4f;
    public float runningSpeed;
    public float ChannelStateSpeed;
    public float attackPower;
    public float maxAttackPower;
    public float attackPowerModifier;
    public float attackPowerModifierStage1;
    public float attackPowerModifierStage2;
    public float attackPowerModifierStage3;
    public float attackPowerModifierStage4;
    public float channelStage2;
    public float channelStage3;
    public float channelStage4;

    public int availableDashes;
    public int availableShields;
    public float dashFillTime;
    public float maxDashFillTime;
    public float shieldFillTime;
    public float maxShieldFillTime;
    public float teleportDistance;
    public float attackCooldown;
    public float attackCooldownMax;

    public ChannelingState cState;

    //[HideInInspector] // Internal script variables
    private float inputX;
    private float inputZ;    
    private float animationSpeedMagnitude;
    private float playerSpeed;
    public bool playerIsShooting;
    public bool playerInTeleport;
    public float teleportTimer;
    public float teleportTimerMax;
    public float teleportLagDuration;
    public float teleportLagMaxDuration;
    public float maxShieldAmount;


    private void Start()
    {
        attackPower = StatsScript.ProjectileBaseDamage;
        playerSpeed = runningSpeed;
        attackPowerModifier = attackPowerModifierStage1;
        controller = GetComponent<CharacterController>();
        anim = transform.GetComponentInChildren<Animator>();
        shootingEffect1.SetActive(false);
        shootingEffect2.SetActive(false);      
        shootingEffect3.SetActive(false);
        shootingEffect4.SetActive(false);
        teleportStartEffect.SetActive(false);
        teleportEndEffect.SetActive(false);

        projectileToShoot = projectile1;
        cState = ChannelingState.PHASE_ZERO;
        availableDashes = 1;
        availableShields = 0;
        attackCooldown = attackCooldownMax;
        teleportLagDuration = teleportLagMaxDuration;
        maxShieldAmount = 3;

    }

    public enum ChannelingState
    {
        PHASE_ZERO = 0,
        PHASE1 = 1,
        PHASE2 = 2,
        PHASE3 = 3,
        PHASE4 = 4,
    }

    public ChannelingState getPlayerChannelingState()
    {
        return this.cState;
    }

    private void monitorChannelEffects()
    {
        if(attackPower <= maxAttackPower)
            attackPower += attackPowerModifier;

        damageMeterDisplay.fillAmount = attackPower / maxAttackPower;
        cState = ChannelingState.PHASE1;

        if (attackPower > channelStage2)
        {
            cState = ChannelingState.PHASE2;
            shootingEffect1.SetActive(false);
            attackPowerModifier = attackPowerModifierStage2;
            projectileToShoot = projectile2;
            if(!shootingEffect2.activeSelf)
                shootingEffect2.SetActive(true);
        }
        if(attackPower > channelStage3)
        {
            cState = ChannelingState.PHASE3;
            shootingEffect2.SetActive(false);
            attackPowerModifier = attackPowerModifierStage3;
            projectileToShoot = projectile3;
            if (!shootingEffect3.activeSelf)
                shootingEffect3.SetActive(true);
        }
        if (attackPower > channelStage4)
        {
            cState = ChannelingState.PHASE4;
            shootingEffect3.SetActive(false);
            attackPowerModifier = attackPowerModifierStage4;
            projectileToShoot = projectile4;
            if (!shootingEffect4.activeSelf)
                shootingEffect4.SetActive(true);
        }
    }

    private void playerMovement()
    {


        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(inputX, 0f, inputZ);
        moveDirection = moveDirection.normalized;

        // Stops player movement if in teleport
        if (!playerInTeleport)
        {
            if (playerIsShooting)
            {
                monitorChannelEffects();

                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                float zVel = transform.InverseTransformDirection(moveDirection).z;
                float xVel = transform.InverseTransformDirection(moveDirection).x;
                anim.SetFloat("zVel", zVel, .1f, Time.deltaTime);
                anim.SetFloat("xVel", xVel, .1f, Time.deltaTime);

                if (Physics.Raycast(ray, out hit, 100))
                {
                    transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
                }
                controller.Move(moveDirection * ChannelStateSpeed * Time.deltaTime);


            }
            else if (moveDirection != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), desiredRotationSpeed);
                controller.Move(moveDirection * playerSpeed * Time.deltaTime);
            }




            // Calculate input magnitude
            animationSpeedMagnitude = new Vector2(inputX, inputZ).sqrMagnitude;
            anim.SetFloat("Blend", animationSpeedMagnitude, 0f, Time.deltaTime);
            anim.SetFloat("InputX", inputX);
            anim.SetFloat("InputZ", inputX);
      
        

            if (Input.GetMouseButtonDown(1))
            {
                playerChannelAttack();              
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (playerIsShooting)
                {
                    playerReleaseAttack();                   
                }                            
            }        
        }

        if (playerIsShooting)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !playerInTeleport && availableDashes >= 1 && dashCharges[0].fillAmount == 1)
            {
                dashCharges[availableDashes - 1].fillAmount = 0f;
                availableDashes--;
                TeleportMode();
                RaycastHit hit;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out hit, 100f))
                {
                    StartCoroutine(TeleportLag(new Vector3(hit.point.x, transform.position.y, hit.point.z), teleportLagDuration));
                }
            }
        }

        IEnumerator TeleportLag(Vector3 newPos, float lagDuration)
        {
            teleportEndEffect.SetActive(false);
            teleportStartEffect.SetActive(true);
            yield return new WaitForSeconds(lagDuration);
            teleportStartEffect.SetActive(false);
            controller.enabled = false;
            transform.position = newPos;
            controller.enabled = true;
            teleportEndEffect.SetActive(true);
        }



        anim.SetBool("Shoot", playerIsShooting);

        dashFillTime += Time.deltaTime;
        if ((int)cState + 1 > availableDashes)
        {
            if (dashFillTime >= maxDashFillTime)
            {
                dashCharges[0].fillAmount = 1;
                availableDashes++;
                dashFillTime = 0;
            }
        }
        // Manage shield charges if player is isnt charging shot
        if (!playerIsShooting)
        {
            if(!(availableShields >= maxShieldAmount+1))
                shieldFillTime += Time.deltaTime;

            if (shieldFillTime >= maxShieldFillTime)
            {
                shieldCharges[0].fillAmount = 1;                   
                shieldFillTime = 0;
                if (availableShields <= maxShieldAmount)
                {
                    availableShields++;
                    calculateShields();                                 
                }
            }
            /*
            if (availableShields <= maxShieldAmount)
                shieldCharges[availableShields].fillAmount = shieldFillTime / maxShieldFillTime;
                */
            
        }


        if(availableDashes >= 1)
            dashCharges[availableDashes-1].fillAmount = dashFillTime / maxDashFillTime;

        if (playerInTeleport)
        {
            teleportTimer -= Time.deltaTime;
            if (teleportTimer <= 0)
            {
                teleportTimer = teleportTimerMax;
                playerInTeleport = false;
            }
        }
    }

    private void playerChannelAttack()
    {
        playerIsShooting = true;
        shootingEffect1.SetActive(true);
        // Remove the current charging shield charge
        if (availableShields <= maxShieldAmount)
        {
            shieldCharges[availableShields].fillAmount = 0f;
            shieldFillTime = 0f;
        }
    }

    private void calculateShields()
    {
        for (int i = 0; i < visualShieldCharges.Length; i++)
        {
            visualShieldCharges[i].SetActive(false);
        }
        for (int i = 0; i < availableShields; i++)
        {
            visualShieldCharges[i].SetActive(true);
        }
    }

    public void playerShieldDamage()
    {   
        if(availableShields >= maxShieldAmount+1)
            shieldCharges[availableShields-1].fillAmount = 0f;
        else
            shieldCharges[availableShields].fillAmount = 0f;

        
        availableShields--;
        calculateShields();
    }

    private void playerReleaseAttack()
    {
        // Reset dash displays
        for (int i = 0; i < dashCharges.Length; i++)
        {
            dashCharges[i].fillAmount = 0f;
        }
        // Reset everything to default or start value
        shootingEffect1.SetActive(false);
        shootingEffect2.SetActive(false);
        shootingEffect3.SetActive(false);
        shootingEffect4.SetActive(false);
        damageMeterDisplay.fillAmount = 0f;
        availableDashes = 0;
        dashFillTime = maxDashFillTime;
        attackPowerModifier = attackPowerModifierStage1;
        cState = ChannelingState.PHASE_ZERO;

        // Creating a projectile, and setting the projectile damage in this current instance
        GameObject proj = Instantiate(projectileToShoot, projectileSpawnPoint.transform.position, transform.rotation);
        Destroy(proj, 10f);
        proj.GetComponent<Projectile>().setProjectileDamage(attackPower);
        attackPower = StatsScript.ProjectileBaseDamage;
        projectileToShoot = projectile1;

        // Exit shooting mode
        playerIsShooting = false;    
    }


    private void TeleportMode()
    {      
        playerInTeleport = true;
    }

    private void Update()
    {
        playerMovement();
    }
}
