using UnityEngine;
using UnityEngine.UI;

public class PlayerMovement : MonoBehaviour
{
    public float inputX;
    public float inputZ;
    public float desiredRotationSpeed;
    public Animator anim;
    public float animationSpeedMagnitude;
    public Camera cam;
    public CharacterController controller;


    public Transform groundCheck;
    public float groundDistance = 0.4f;
    public LayerMask groundMask;


    public float playerSpeed;
    public float normalSpeed;
    public float rollSpeed;

    public bool playerIsShooting;
    public bool playerInAnimation;
    public float autoAttackTimer;
    public float autoAttackTimerMax;

    public float rollTimer;
    public float rollTimerMax;

    public bool rollCooldown;
    public bool rollAnimationActive;

    public GameObject bowActive;

    public GameObject arrow;
    public GameObject projectileSpawnPoint;

    public GameObject bowDrawEffect;

    public Image dodgeTimerDisplay;
    public Image shootTimerDisplay;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = transform.GetComponentInChildren<Animator>();
        rollTimer = rollTimerMax;
        bowDrawEffect.SetActive(false);
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

    private void playerMovement()
    {
        anim.ResetTrigger("NormalAttack");
        anim.ResetTrigger("PushbackTrigger");
        //anim.ResetTrigger("Roll");

        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(inputX, 0f, inputZ);
        moveDirection = moveDirection.normalized;

        if (playerIsShooting)
        {
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
        
        // autoattackTime er ikke nødvendig enda
        if (!playerInAnimation && !rollCooldown) 
        {
            if (Input.GetMouseButtonDown(1))
            {
                playerIsShooting = true;
                bowDrawEffect.SetActive(true);
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (playerIsShooting)
                {
                    bowDrawEffect.SetActive(false);
                    Destroy(Instantiate(arrow, projectileSpawnPoint.transform.position, transform.rotation), 10f);
                    anim.SetTrigger("PushbackTrigger");
                    playerIsShooting = false;               
                    autoAttackTimer = autoAttackTimerMax;               
                    playerInAnimation = true;  
                }                            
            }
        }

        if (moveDirection != Vector3.zero)
        {
            if (Input.GetKeyDown(KeyCode.LeftShift) && !rollCooldown) // &&!playerInAnimation
            {
                //playerIsShooting = false;               
                RollMode();
            }
        }

        if (rollCooldown)
        {
            rollTimer -= Time.deltaTime;
            dodgeTimerDisplay.fillAmount = rollTimer/1.1f;
            if (rollTimer <= 0)
            {
                rollTimer = rollTimerMax;
                dodgeTimerDisplay.fillAmount = 100;
                rollCooldown = false;
            }
        }

        if (rollAnimationActive)
            playerSpeed = rollSpeed;
        else
            playerSpeed = normalSpeed;
        

        anim.SetBool("Shoot", playerIsShooting);
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
