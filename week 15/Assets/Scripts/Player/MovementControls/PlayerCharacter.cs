// using System.Diagnostics;
using UnityEngine;
using Yarn.Unity;
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
            // Debug.Log("update movement called with MOVESPPED: " + moveSpeed);
            moveDirection =
                orientation.forward * verticalInput + orientation.right * horizontalInput;
            rb.AddForce(moveDirection.normalized * moveSpeed * 5f, ForceMode.Force);
            rb.linearDamping = groundDrag;
        }

        [YarnCommand("stop_player_movement")]
        public void StopPlayerMovement()
        {
            this.moveSpeed = 0;
        }

        [YarnCommand("resume_player_movement")]
        public void ResumePlayerMovement(float moveSpeed)
        {
            this.moveSpeed = moveSpeed;
        }
    }
}
