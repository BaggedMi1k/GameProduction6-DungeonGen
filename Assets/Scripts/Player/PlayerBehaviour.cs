using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class PlayerBehaviour : NetworkBehaviour
{
    public CharacterController controller;
    public Camera playerCamera;
    public Camera pickUpamera;

    public float maxSpeed = 2.0f;
    public float gravity = -30.0f;
    public float jumpHeight = 3.0f;
    public Vector3 velocity;

    public Transform groundPoint;
    public float groundRadius = 0.5f;
    public LayerMask groundMask;
    public bool isGrounded;

    public CameraController cameraController;
    public GameObject pauseMenuUI;
    public bool isPaused = false;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (!IsOwner)
        {
            playerCamera.enabled = false;
            pickUpamera.enabled = false;
            return;
        }

        pauseMenuUI.SetActive(false);
        Cursor.visible = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner) return;

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
            return;
        }

        if (!isPaused)
        {
            Move();
        }
    }

    void Move()
    {
        isGrounded = Physics.CheckSphere(groundPoint.position, groundRadius, groundMask);

        if (isGrounded && velocity.y < 0.0f)
        {
            velocity.y = -2.0f;
        }

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * maxSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
        }

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(groundPoint.position, groundRadius);
    }

    void TogglePause()
    {
        isPaused = true;

        pauseMenuUI.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        cameraController.enabled = false;
    }
    public void ResumeGame()
    {
        isPaused = false;
        cameraController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
