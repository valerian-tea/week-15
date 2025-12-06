#nullable enable
// using System.Diagnostics;
using Yarn.Unity;
using Yarn.Unity.Samples;

namespace MyGame.Characters
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using UnityEngine;
    using UnityEngine.Events;
    using Yarn.Unity.Attributes;

    public class NPCMovement : BaseCharacter
    {
        #region Movement Variables

        [Group("Movement")]
        [SerializeField]
        float speed;

        [Group("Movement")]
        [SerializeField]
        float gravity = 10;

        [Group("Movement")]
        [SerializeField]
        float turnSpeed;

        [Group("Movement")]
        [SerializeField]
        float acceleration = 0.5f;

        [Group("Movement")]
        [SerializeField]
        float deceleration = 0.1f;

        [Group("Movement")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField]
        float outOfBoundsYPosition = -5;

        [HideIf(nameof(isPlayerControlled))]
        [SerializeField]
        float pathDestinationTolerance = 0.1f;

        [Group("Movement")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField]
        InputAxisVector2 movementInput = new();

        [HideIf(nameof(isPlayerControlled))]
        [SerializeField]
        protected SimplePath? followPath;
        public bool HasPath => followPath != null;
        private int currentDestinationPathIndex = -1;
        private float remainingPathWaitTime = 0f;
        private bool isMovingToPathPoint = false;
        private Quaternion targetRotation;

        public float CurrentSpeedFactor = 0f;

        private float lastFrameSpeed = 0f;
        private float lastFrameSpeedChange = 0f;
        private Vector3 lastFrameWorldPosition;

        private Vector3 lastGroundedPosition;

        #endregion

        #region Movement Logic

        private void SetupMovement()
        {
            // Remember our initial facing rotation
            targetRotation = transform.rotation;

            if (!isPlayerControlled && followPath != null && followPath.Count >= 2)
            {
                var startPoint = followPath.GetWorldPosition(0);
                var nextPoint = followPath.GetWorldPosition(1);

                transform.position = startPoint;
                targetRotation = Quaternion.LookRotation(nextPoint - startPoint);
                transform.rotation = GetCurrentLookDirection();

                currentDestinationPathIndex = 0;
                Mode = CharacterMode.PathMovement;
                Debug.Log(
                    "Starting NPC on path movement. currentDestinationPathIndex: "
                        + currentDestinationPathIndex
                        + " Mode: "
                        + Mode
                );
            }

            // Start facing our look target, if any
            if (lookTarget != null)
            {
                transform.rotation = GetCurrentLookDirection();
            }

            lastFrameWorldPosition = transform.position;
            lastGroundedPosition = transform.position;
        }

        public override void UpdateMovement()
        {
            if (Mode == CharacterMode.PathMovement && followPath != null && isMovingToPathPoint)
            {
                if (currentDestinationPathIndex == -1 || followPath.Count < 1)
                {
                    // No current path location.
                    CurrentSpeedFactor = 0;
                }
                else if (remainingPathWaitTime > 0)
                {
                    CurrentSpeedFactor = 0;
                    remainingPathWaitTime -= Time.deltaTime;
                }
                else
                {
                    // Move towards current path node

                    var worldOffset =
                        followPath.GetWorldPosition(currentDestinationPathIndex)
                        - transform.position;
                    var input = new Vector2(worldOffset.x, worldOffset.z).normalized;
                    ApplyMovement(input);

                    if (worldOffset.magnitude <= pathDestinationTolerance)
                    {
                        // We've reached the destination
                        currentDestinationPathIndex =
                            (currentDestinationPathIndex + 1) % followPath.Count;
                        remainingPathWaitTime = followPath.GetDelay(currentDestinationPathIndex);
                        isMovingToPathPoint = false;
                    }
                }
            }

            if (this.IsAlive)
            {
                // Rotate towards our current look direction if we're alive
                transform.rotation = Quaternion.RotateTowards(
                    Quaternion.LookRotation(transform.forward),
                    GetCurrentLookDirection(),
                    turnSpeed * Time.deltaTime
                );
            }

            lastFrameWorldPosition = transform.position;

            void ApplyMovement(Vector2 input)
            {
                float rawSpeed =
                    input.magnitude < 0.001 ? 0f : Mathf.Clamp01(input.magnitude) * speed;

                var dampingTime = (rawSpeed > lastFrameSpeed) ? acceleration : deceleration;

                var dampedSpeed = Mathf.SmoothDamp(
                    lastFrameSpeed,
                    rawSpeed,
                    ref lastFrameSpeedChange,
                    dampingTime
                );
                lastFrameSpeed = dampedSpeed;

                var movement = new Vector3(input.x, 0, input.y);

                if (movement.magnitude > 0)
                {
                    // If we're moving, update the direction we want to be looking
                    // at when we have no look target
                    targetRotation = Quaternion.LookRotation(movement.normalized);
                }

                movement = movement.normalized * dampedSpeed;
                movement.y = -gravity;

                if (characterController != null)
                {
                    characterController.Move(movement * Time.deltaTime);
                }

                CurrentSpeedFactor = Mathf.Clamp01(dampedSpeed / speed);
            }
        }

        private Quaternion GetCurrentLookDirection()
        {
            Quaternion direction = this.targetRotation;
            if (lookTarget != null)
            {
                var lookDirectionOnSameY = lookTarget.position - transform.position;
                lookDirectionOnSameY.y = 0;
                direction = Quaternion.LookRotation(lookDirectionOnSameY);
            }
            return direction;
        }

        [YarnCommand("move_to")]
        public YarnTask MoveToYarn(float x, float y, float z, bool wait = false)
        {
            var pos = new Vector3(x, y, z);
            var task = MoveTo(pos, CancellationToken.None);
            return wait ? task : YarnTask.CompletedTask;
        }

        public async YarnTask MoveTo(Vector3 position, CancellationToken cancellationToken)
        {
            if (Vector3.Distance(position, transform.position) <= 0.0001f)
            {
                // We're already at the position; nothing to do
                return;
            }
            // Look in the direction we're moving, not at any look target
            var lookDirection = position - transform.position;
            lookDirection.y = 0;
            targetRotation = Quaternion.LookRotation(lookDirection);

            var previousLookTarget = lookTarget;
            lookTarget = null;

            var previousMode = Mode;

            Mode = CharacterMode.ExternallyControlledMovement;

            do
            {
                transform.position = Vector3.MoveTowards(
                    transform.position,
                    position,
                    speed * Time.deltaTime
                );
                this.CurrentSpeedFactor = 1;

                await YarnTask.Yield();
            } while (
                Vector3.Distance(transform.position, position) > 0.05f
                && !cancellationToken.IsCancellationRequested
            );

            lookTarget = previousLookTarget;

            Mode = previousMode;

            this.CurrentSpeedFactor = 0;
        }

        public void SetLookDirection(Quaternion rotation, bool immediate = false)
        {
            targetRotation = rotation;

            if (immediate)
            {
                transform.rotation = rotation;
            }
        }
        #endregion

        #region Core Logic

        private void Start()
        {
            Mode = CharacterMode.ExternallyControlledMovement;

            SetupMovement();
            SetupAnimation();
            SetupInteraction();
        }

        private void Update()
        {
            UpdateMovement();
            UpdateAnimation();
            UpdateInteraction();
        }

        protected void OnDrawGizmosSelected()
        {
            if (isPlayerControlled)
            {
                // Show interaction volume
                Gizmos.color = Color.yellow;
                Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, Vector3.one);
                Gizmos.DrawWireSphere(offset, interactionRadius);
            }
        }
        #endregion
    }
}
