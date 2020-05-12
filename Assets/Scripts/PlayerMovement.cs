using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class PlayerMovement : MonoBehaviour
{

    [Header("Inspector Objects")]
    public Animator anim;
    public CharacterController controller;
    public Transform groundCheck;
    public LayerMask groundMask;

    // Prefabs   
    public GameObject projectileSpawnPoint;
    public GameObject teleportStartEffect;
    public GameObject teleportEndEffect;
    public GameObject[] projectileList;
    public GameObject[] shootingEffectList;
    public GameObject projectileToShoot;

    public Image[] dashCharges;
    public Image[] shieldCharges;
    public Image shootTimerDisplay;

    public GameObject[] visualShieldCharges;

    public TextMeshProUGUI attackPowerText;
    public TextMeshProUGUI shieldChargesText;
    public TextMeshProUGUI dashChargesText;

    public Transform shieldBreakPosition;
    public GameObject playerHitEffect;

    [Header("Player modifiable variables ")]
    public float desiredRotationSpeed;
    public float groundDistance = 0.4f;
    public float runningSpeed;
    public float ChannelStateSpeed;
    public float attackPower;
    public float maxAttackPower;
    public float attackPowerModifier;
    public float[] attackPowerModifierStages;
    public float nextPhaseTimer;

    public int availableDashes;
    public int availableShields;
    public float dashFillTime;
    public float maxDashFillTime;
    public float shieldFillTime;
    public float maxShieldFillTime;
    public float teleportDistance;
    public float attackCooldown;
    public float attackCooldownMax;
    public float teleportLagMaxDuration;

    public ChannelingState cState;

    //[HideInInspector] // Internal script variables
    private float inputX;
    private float inputZ;    
    private float animationSpeedMagnitude;
    private float playerSpeed;
    private bool playerIsShooting;
    private bool playerInTeleport;
    private float teleportLagDuration;
    private float maxShieldAmount;
    private float gravity;
    private float nextPhaseTimerMax;
    private int baseFontSize;
    private float nextPhaseTimerBase;
    private GameObject currentShootingEffect;
    private float shootingCooldownMax;
    public float shootingCooldown;

    private Vector3 addedVelocity;

    private Color[] colorArray = {Color.black, Color.blue, Color.green,
                                Color.red, new Color(1f,0f,.67f, 1f), new Color(.5f,0f,.5f, 1),
                                Color.white, Color.cyan, Color.yellow, Color.black};


    private void Start()
    {
        attackPower = StatsScript.ProjectileBaseDamage;
        playerSpeed = runningSpeed;
        attackPowerModifier = attackPowerModifierStages[0];
        controller = GetComponent<CharacterController>();
        anim = transform.GetComponentInChildren<Animator>();
        for (int i = 0; i < shootingEffectList.Length; i++)
        {
            shootingEffectList[i].SetActive(false);
        }
        teleportStartEffect.SetActive(false);
        teleportEndEffect.SetActive(false);

        projectileToShoot = projectileList[0];
        cState = ChannelingState.STATE_ZERO;
        availableDashes = 1;
        availableShields = 0;
        attackCooldown = attackCooldownMax;
        teleportLagDuration = teleportLagMaxDuration;
        maxShieldAmount = 3;
        gravity = -1f;
        attackPowerText.outlineColor = Color.blue;
        nextPhaseTimerBase = 2f;
        nextPhaseTimerMax = nextPhaseTimerBase;
        baseFontSize = 50;
        shootingCooldownMax = 2f;
        shieldChargesText.text = availableShields.ToString();
        dashChargesText.text = (availableDashes - 1).ToString();

    }

    public enum ChannelingState
    {
        STATE_ZERO = 0,
        STATE1 = 1,
        STATE2 = 2,
        STATE3 = 3,
        STATE4 = 4,
        STATE5 = 5,
        STATE6 = 6,
        STATE7 = 7,
        STATE8 = 8,
        STATE9 = 9,
    }

    public ChannelingState getPlayerChannelingState()
    {
        return this.cState;
    }

    /**
     * Method monitors which phase channeling is in, and 
     * sets a new attack multiplier and adds new effects for corresponding phase
     */

    private void nextChannelingPhase(ChannelingState state)
    {
        // Makes next phase 1 second longer
        nextPhaseTimerMax++;
        // Manages attack power text
        StartCoroutine(lerpTextSizeAndColor(colorArray[(int)cState]));       
        
        // state starts at 1, so to target the previous state effect, the correct check is -2
        if((int)state - 2 >= 0)
            shootingEffectList[(int)state-2].SetActive(false);

        // ChannelingState index has 0 occupied with no-shooting state, so (int)state-1 is to target the right index
        attackPowerModifier = attackPowerModifierStages[(int)state];
        projectileToShoot = projectileList[(int)state-1];
        if (!shootingEffectList[(int)state - 1].activeSelf)
        {
            shootingEffectList[(int)state - 1].SetActive(true);
            currentShootingEffect = shootingEffectList[(int)state - 1];
        }
             
    }

    /**
     * Method is called from Update()
     * Controlls everything from playermovement to skillset and abilities
     */
    private void playerMovement()
    {

        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(inputX, 0f, inputZ);
        addedVelocity.y += gravity * Time.deltaTime;
        moveDirection = moveDirection.normalized;

        // Stops player movement if in teleport
        if (!playerInTeleport)
        {
            // If player is in channeling mode, change movement to mouse-oriented
            if (playerIsShooting)
            {
                // Manage timer for when next channeling phase starts
                if (nextPhaseTimer <= 0)
                {
                    if(cState <= ChannelingState.STATE8)
                        nextChannelingPhase(++cState);
                    nextPhaseTimer = nextPhaseTimerMax;
                }
                else
                {
                    nextPhaseTimer -= Time.deltaTime;
                }

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

            controller.Move(addedVelocity * Time.deltaTime);


            // Calculate input magnitude
            animationSpeedMagnitude = new Vector2(inputX, inputZ).sqrMagnitude;
            anim.SetFloat("Blend", animationSpeedMagnitude, 0f, Time.deltaTime);
            anim.SetFloat("InputX", inputX);
            anim.SetFloat("InputZ", inputX);


            shootingCooldown -= Time.deltaTime;

            if (Input.GetMouseButtonDown(1))
            {
                if (shootingCooldown <= 0)
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

        // Code block listens for GetKeyDown and executes code to teleport player and manage dashes
        if (playerIsShooting)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !playerInTeleport && availableDashes >= 1 && dashCharges[0].fillAmount == 1)
            {
                dashCharges[availableDashes - 1].fillAmount = 0f;
                availableDashes--;
                TeleportMode();
                RaycastHit hitRay;
                RaycastHit hitLine;
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                
                if (Physics.Raycast(ray, out hitRay, 100f))
                {
                    if (Physics.Linecast(transform.position, new Vector3(hitRay.point.x, transform.position.y, hitRay.point.z), out hitLine))
                    {
                        Vector3 newPos = Vector3.Lerp(transform.position, new Vector3(hitLine.point.x, transform.position.y, hitLine.point.z), .75f);
                        StartCoroutine(TeleportLag(newPos, teleportLagDuration));
                    }
                    else
                    {
                        StartCoroutine(TeleportLag(new Vector3(hitRay.point.x, transform.position.y, hitRay.point.z), teleportLagDuration));
                    }
                }       
            }
        }

        // Channeling animation
        anim.SetBool("Shoot", playerIsShooting);

        // Fill up dash charges corresponding the channeling phase player is in
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
                shieldCharges[availableShields].fillAmount = 1;
                shieldFillTime = 0;
                if (availableShields <= maxShieldAmount)
                {
                    availableShields++;
                    calculateShields();                                 
                }
            }

            // Fill up shield charges visually
            if (availableShields <= maxShieldAmount)
                shieldCharges[availableShields].fillAmount = shieldFillTime / maxShieldFillTime;
                
            
        }

        // Fill up dash charges visually
        if(availableDashes >= 1)
            dashCharges[availableDashes-1].fillAmount = dashFillTime / maxDashFillTime;

        // Increase attack power while player is charging
        if(playerIsShooting)
            if (attackPower <= maxAttackPower)
                attackPower += attackPowerModifier * Time.deltaTime;

        // Display attack power on GUI text
        attackPowerText.text = attackPower.ToString("N0");

        // Update shield and dash charge texts
        shieldChargesText.text = availableShields.ToString();
        dashChargesText.text = (availableDashes - 1).ToString();
    }

    private IEnumerator lerpTextSizeAndColor(Color color)
    {
        if(cState == ChannelingState.STATE9)
        {
            attackPowerText.outlineColor = Color.Lerp(attackPowerText.outlineColor, Color.black, 1f);
            attackPowerText.faceColor = Color.Lerp(attackPowerText.faceColor, Color.white, 1f);
        }            
        else
        {
            attackPowerText.outlineColor = Color.Lerp(attackPowerText.outlineColor, color, 1f);
        }
        
        for (int i = 0; i < 20; i++)
        {
            attackPowerText.fontSize += 1f;
            yield return new WaitForSeconds(.0005f);
        }
        for (int i = 0; i < 15; i++)
        {
            attackPowerText.fontSize -= 1f;
            yield return new WaitForSeconds(.0001f);
        }
    }

    // Teleporting freezes player-movement for a bit and triggers animation
    private IEnumerator TeleportLag(Vector3 newPos, float lagDuration)
    {
        teleportEndEffect.SetActive(false);
        teleportStartEffect.SetActive(true);
        yield return new WaitForSeconds(lagDuration);
        teleportStartEffect.SetActive(false);
        controller.enabled = false;
        transform.position = newPos;
        controller.enabled = true;
        teleportEndEffect.SetActive(true);
        playerInTeleport = false;
    }

    private void playerChannelAttack()
    {
        playerIsShooting = true;
        shootingEffectList[0].SetActive(true);
        // Remove the current charging shield charge
        if (availableShields <= maxShieldAmount)
        {
            shieldCharges[availableShields].fillAmount = 0f;
            shieldFillTime = 0f;
        }
    }

    public void playerShieldDamage()
    {
        Destroy(Instantiate(playerHitEffect, shieldBreakPosition.position, Quaternion.identity), 5f);

        if(availableShields >= maxShieldAmount+1)
            shieldCharges[availableShields-1].fillAmount = 0f;
        else
            shieldCharges[availableShields].fillAmount = 0f;

        
        availableShields--;
        /*
        if (availableShields < 0)
            GameOver();  
        */
        calculateShields();
    }

    private void GameOver()
    {
      
        // Stop current running effect
        if (currentShootingEffect != null)
          currentShootingEffect.SetActive(false);

        // Stop animations
        anim.SetBool("GameOver", GameMasterScript.gameRunning);
        // Death animation
        // coroutine, og i slutten gameover
        GameMasterScript.gameMasterScript.GameOver();
    }

    private void calculateShields()
    {
        // Disable all shields
        for (int i = 0; i < visualShieldCharges.Length; i++)
            visualShieldCharges[i].SetActive(false);
        // Enable shields
        for (int i = 0; i < availableShields; i++)
            visualShieldCharges[i].SetActive(true);       
    }

    private void playerReleaseAttack()
    {
        // Reset dash displays
        for (int i = 0; i < dashCharges.Length; i++)
        {
            dashCharges[i].fillAmount = 0f;
        }
        // Reset everything to default or start value
        for (int i = 0; i < shootingEffectList.Length; i++)
            shootingEffectList[i].SetActive(false);

        availableDashes = 0;
        dashFillTime = maxDashFillTime;
        attackPowerModifier = attackPowerModifierStages[0];
        attackPowerText.fontSize = baseFontSize;
        attackPowerText.outlineColor = colorArray[1];
        attackPowerText.faceColor = Color.black;
        cState = ChannelingState.STATE_ZERO;
        nextPhaseTimer = 0f;
        nextPhaseTimerMax = nextPhaseTimerBase;
        shootingCooldown = shootingCooldownMax;

        // Creating a projectile, and setting the projectile damage in this current instance
        GameObject proj = Instantiate(projectileToShoot, projectileSpawnPoint.transform.position, transform.rotation);
        Destroy(proj, 10f);
        proj.GetComponent<Projectile>().setProjectileDamage(attackPower);
        attackPower = StatsScript.ProjectileBaseDamage;
        projectileToShoot = projectileList[0];

        // Exit shooting mode
        playerIsShooting = false;    
    }


    private void TeleportMode()
    {      
        playerInTeleport = true;
    }

    private void Update()
    {
        if(!GameMasterScript.gameIsPaused && GameMasterScript.gameRunning)
            playerMovement();

    }
}
