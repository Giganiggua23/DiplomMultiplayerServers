using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController controller;

    [SerializeField] private float speed = 8f;
    private float _gravity = -19.62f;
    [SerializeField] private float jumpHeight = 2f;

    [SerializeField] private Transform groundCheck;
    private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    Vector3 velocity;
    bool isGrounded;

    void Update()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position,groundDistance, groundMask);

        if (isGrounded && velocity.y < 0) 
        {
            velocity.y = -2f;
        }
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        if ( x == 1 || z == 1 && Input.GetKey(KeyCode.LeftShift))
        {
            speed = 16f;
        }
        else
        {
            speed = 8f;
        }

        Vector3 move = transform.right * x + transform.forward * z;

        controller.Move(move * speed * Time.deltaTime);

        if(Input.GetButtonDown("Jump") && isGrounded)
        {
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * _gravity);
        }

        velocity.y += _gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }
}
