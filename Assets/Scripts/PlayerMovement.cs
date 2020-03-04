using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public bool playerIsShooting;
    public bool playerInAnimation;
    public float autoAttackTimer;
    public float autoAttackTimerMax;

    public GameObject bowInactive;
    public GameObject bowActive;

    public GameObject arrow;
    public GameObject projectileSpawnPoint;

    private void Start()
    {
        bowInactive.SetActive(false);
        controller = GetComponent<CharacterController>();
        anim = transform.GetComponentInChildren<Animator>();
    }

    /* Method gets called by animation event on "Pushback" animation on  Player*/
    public void shootingModeExit()
    {
        playerInAnimation = false;
    }

    private void playerMovement()
    {
        anim.ResetTrigger("NormalAttack");
        anim.ResetTrigger("PushbackTrigger");

        inputX = Input.GetAxisRaw("Horizontal");
        inputZ = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = new Vector3(inputX, 0f, inputZ);
        moveDirection = moveDirection.normalized;

        if (playerIsShooting)
        {

            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out hit, 100))
            {
                transform.LookAt(new Vector3(hit.point.x, transform.position.y, hit.point.z));
            }
            float zVel = transform.InverseTransformDirection(moveDirection).z;
            float xVel = transform.InverseTransformDirection(moveDirection).x;
            anim.SetFloat("zVel", zVel, .1f, Time.deltaTime);
            anim.SetFloat("xVel", xVel, .1f, Time.deltaTime);
            controller.Move(moveDirection * playerSpeed / 3 * Time.deltaTime);

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
        if (autoAttackTimer <= 0 && !playerInAnimation)
        {
            if (Input.GetMouseButtonDown(1))
            {
                playerIsShooting = true;
                
            }
            if (Input.GetMouseButtonUp(1))
            {
                if (playerIsShooting)
                {
                    Destroy(Instantiate(arrow, projectileSpawnPoint.transform.position, transform.rotation), 10f);
                    anim.SetTrigger("PushbackTrigger");
                    playerIsShooting = false;               
                    autoAttackTimer = autoAttackTimerMax;               
                    playerInAnimation = true;  
                }                            
            }
            anim.SetBool("Shoot", playerIsShooting);
        }
        else
        {
            autoAttackTimer -= Time.deltaTime;
        }

        


    }

    private void Update()
    {
        playerMovement();
    }
}
