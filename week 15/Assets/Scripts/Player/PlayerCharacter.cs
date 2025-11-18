// using System.Diagnostics;
using UnityEngine;
using Yarn.Unity.Samples;

namespace MyGame.Characters
{
    public class PlayerCharacter : BaseCharacter
    {
        Rigidbody rb;
        [Header("Movement")]
        public float moveSpeed;
        public Transform orientation;
        float horizontalInput;
        float verticalInput;
        Vector3 moveDirection;
        public float groundDrag;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Start()
        {
            rb = GetComponent<Rigidbody>();
            rb.freezeRotation = true;
            Mode = CharacterMode.PlayerControlledMovement;
            SetupAnimation();
            SetupInteraction();
        }

        // Update is called once per frame
        private void Update()
        {
            MyInput();
            UpdateMovement();
            SetupAnimation();
            UpdateInteraction();

        }

        private void MyInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");
            verticalInput = Input.GetAxisRaw("Vertical");
        }

        public override void UpdateMovement()

        {
            moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
            rb.AddForce(moveDirection.normalized * moveSpeed * 5f, ForceMode.Force);
            rb.linearDamping = groundDrag;
        }

    }
}