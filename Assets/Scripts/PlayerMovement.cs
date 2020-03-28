﻿using UnityEngine;
using UnityEngine.UI;

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

    public Image[] dashCharges;
    public Image shootTimerDisplay;
    public Image damageMeterDisplay;


    [Header("Player modifiable variables ")]
    public float desiredRotationSpeed;
    public float groundDistance = 0.4f;
    public float normalSpeed;
    public float rollSpeed;
    public float rollTimerMax;
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
    public float dashFillTime;
    public float maxDashFillTime;

    public ChannelingState cState;

    [HideInInspector] // Internal script variables
    private float inputX;
    private float inputZ;    
    private float animationSpeedMagnitude;
    private float playerSpeed;
    private bool playerIsShooting;
    private bool playerInAnimation;
    private float rollTimer;
    private bool rollCooldown;
    private bool rollAnimationActive;

    





    private void Start()
    {
        attackPower = StatsScript.ProjectileBaseDamage;
        attackPowerModifier = attackPowerModifierStage1;
        controller = GetComponent<CharacterController>();
        anim = transform.GetComponentInChildren<Animator>();
        rollTimer = rollTimerMax;
        shootingEffect1.SetActive(false);
        shootingEffect2.SetActive(false);      
        shootingEffect3.SetActive(false);
        shootingEffect4.SetActive(false);


        projectileToShoot = projectile1;
        cState = ChannelingState.PHASE_ZERO;
        availableDashes = 1;
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

    /* Method gets called by animation event on "Pushback" animation on  Player*/
    public void shootingModeExit()
    {
        playerInAnimation = false;
    }
    // Method gets called by animation event on "Roll", and is used to notify that roll animation is over
    public void rollModeExit()
    {
        rollAnimationActive = false;
        playerInAnimation = false;
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
        anim.ResetTrigger("PushbackTrigger");

        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(inputX, 0f, inputZ);
        moveDirection = moveDirection.normalized;

        if (playerIsShooting)
        {
            

            monitorChannelEffects();

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            float zVel = transform.InverseTransformDirection(moveDirection).z;
            float xVel = transform.InverseTransformDirection(moveDirection).x;
            anim.SetFloat("zVel", zVel, .1f, Time.deltaTime);
            anim.SetFloat("xVel", xVel, .1f, Time.deltaTime);
            if (!rollAnimationActive)
            {
                if (Physics.Raycast(ray, out hit, 100))
                {
                    transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
                }
                controller.Move(moveDirection * playerSpeed / 3 * Time.deltaTime);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(moveDirection), desiredRotationSpeed);
                controller.Move(moveDirection * playerSpeed * Time.deltaTime);
            }
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
        
        if (!playerInAnimation && !rollCooldown) 
        {
            if (Input.GetMouseButtonDown(1))
            {
                playerChannelAttack();              
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (playerIsShooting)
                {
                    playerReleaseAttack();
                    for (int i = 0; i < dashCharges.Length; i++)
                    {
                        dashCharges[i].fillAmount = 0f;
                    }
                }                            
            }
        }

        if (moveDirection != Vector3.zero)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !rollCooldown && availableDashes >= 1 && dashCharges[0].fillAmount == 1) 
            {
                //playerIsShooting = false;    
                dashCharges[availableDashes-1].fillAmount = 0f;
                availableDashes--;
                RollMode();
            }
        }

        if (rollCooldown)
        {
            manageDash();
        }

        if (rollAnimationActive)
            playerSpeed = rollSpeed;
        else
            playerSpeed = normalSpeed;
        

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
        if(availableDashes >= 1)
            dashCharges[availableDashes-1].fillAmount = dashFillTime / maxDashFillTime;
    }

    private void manageDash()
    {     
        rollTimer -= Time.deltaTime;
        if (rollTimer <= 0)
        {
            rollTimer = rollTimerMax;
            rollCooldown = false;
        }
    }

    private void playerChannelAttack()
    {
        playerIsShooting = true;
        shootingEffect1.SetActive(true);
    }

    private void playerReleaseAttack()
    {
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
        anim.SetTrigger("PushbackTrigger");
        playerIsShooting = false;
        playerInAnimation = true;       
    }


    private void RollMode()
    {
        anim.SetTrigger("Roll");
        rollCooldown = true;
        rollAnimationActive = true;
    }

    private void Update()
    {
        playerMovement();
    }
}
