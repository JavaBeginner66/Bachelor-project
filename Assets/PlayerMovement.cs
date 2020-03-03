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
    public bool pushbackTimerActive;
    public float pushbackLength;
    public float pushbackTimer;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        anim = transform.GetComponentInChildren<Animator>();
        pushbackTimer = pushbackLength;
    }

    private void playerMovement()
    {
        anim.ResetTrigger("NormalAttack");

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


        if (Input.GetMouseButtonDown(1))
        {
            playerIsShooting = true;

        }
        else if (Input.GetMouseButtonUp(1))
        {
            playerIsShooting = false;
            pushbackTimerActive = true;
        }
        anim.SetBool("Shoot", playerIsShooting);

       

    }

    private void Update()
    {
        playerMovement();
    }
}
