using UnityEngine;
using System.Threading;
using Yarn.Unity.Attributes;
using System.Collections.Generic;
using UnityEngine.Events;
using System.Threading.Tasks;
using Yarn.Unity;
using Yarn.Unity.Samples;

public class PlayerMovement : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed;

    public float groundDrag;

    [Header("Ground Check")]
    public float playerHeight;
    public LayerMask whatIsGround;
    bool grounded;

    #region Interaction Variables

    public enum CharacterMode
    {
        PlayerControlledMovement,
        ExternallyControlledMovement,
        PathMovement,
        Interact,
    }
    public CharacterMode Mode { get; private set; }
    [SerializeField] bool isPlayerControlled;

    [Group("Interaction")]
    [ShowIf(nameof(isPlayerControlled))]
    [SerializeField] float interactionRadius = 1f;

    [Group("Interaction")]
    [ShowIf(nameof(isPlayerControlled))]
    [SerializeField] Vector3 offset = Vector3.zero;

    [Group("Interaction")]
    [ShowIf(nameof(isPlayerControlled))]
    [SerializeField] UnityEvent<Interactable>? onInteracting;
    private List<Interactable> interactables = new();

    private Interactable? currentInteractable = null;
    public bool CanInteract => Mode == CharacterMode.PlayerControlledMovement;
    public bool HasPath => followPath != null;

    [HideIf(nameof(isPlayerControlled))]
    [SerializeField] SimplePath? followPath;
    [SerializeField] InputAxisButton interactInput = new();
    [Group("Movement")]
    public Transform? lookTarget;

    #endregion

    public Transform orientation;
    float horizontalInput;
    float verticalInput;
    Vector3 moveDirection;
    Rigidbody rb;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;
    }

    // Update is called once per frame
    void Update()
    {
        MyInput();
        SpeedControl();

    }

    protected void Awake()
    {
        Mode = CharacterMode.PlayerControlledMovement;

        SetupInteraction();
    }
    void FixedUpdate()
    {
        MovePlayer();
        rb.linearDamping = groundDrag;
        UpdateInteraction();

    }

    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
    }

    private void MovePlayer()

    {

        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;
        rb.AddForce(moveDirection.normalized * moveSpeed * 5f, ForceMode.Force);
    }

    private void SpeedControl()
    {
        Vector3 flatVel = new Vector3(rb.linearVelocity.x, 0f, rb.linearVelocity.z);

        if (flatVel.magnitude > moveSpeed)
        {
            Vector3 limitedVel = flatVel.normalized * moveSpeed;
            rb.linearVelocity = new Vector3(limitedVel.x, rb.linearVelocity.y, limitedVel.z);
        }

    }

    #region Interaction Logic

    public void SetupInteraction()
    {
        interactables.Clear();

        interactables.AddRange(FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None));
    }

    protected void UpdateInteraction()
    {
        if (isPlayerControlled == false)
        {
            // Only player-controlled characters can interact
            return;
        }

        if (!CanInteract)
        {
            // We can only interact if we're allowed to move around.
            return;
        }

        var previousInteractable = currentInteractable;

        (float Distance, Interactable? Interactable) nearest = (float.PositiveInfinity, null);

        for (int i = 0; i < interactables.Count; i++)
        {
            var interactable = interactables[i];

            if (!interactable.isActiveAndEnabled)
            {
                // We can't interact if the component or its gameobject
                // isn't enabled
                continue;
            }

            if (interactable.gameObject == gameObject)
            {
                // We can't interact with ourselves
                continue;
            }

            if (interactable.gameObject.TryGetComponent<SimpleCharacter>(out var character) && !character.IsAlive)
            {
                // We can't interact with characters that aren't alive
                continue;
            }

            var distance = Vector3.Distance(transform.TransformPoint(offset), interactable.transform.position);
            if (distance > interactionRadius)
            {
                continue;
            }
            if (distance < nearest.Distance)
            {
                nearest = (distance, interactable);
            }
        }

        if (previousInteractable != nearest.Interactable)
        {
            if (previousInteractable != null) { previousInteractable.IsCurrent = false; }
            if (nearest.Interactable != null) { nearest.Interactable.IsCurrent = true; }
            currentInteractable = nearest.Interactable;
        }

        if (interactInput.WasPressedThisFrame && currentInteractable != null)
        {
            async YarnTask RunInteraction(Interactable interactable, CancellationToken cancellationToken)
            {
                var previousMode = Mode;
                Mode = CharacterMode.Interact;

                if (interactable.InteractorShouldTurnToFaceWhenInteracted)
                {
                    lookTarget = interactable.transform;
                }

                interactable.IsCurrent = false;
                currentInteractable = null;

                onInteracting?.Invoke(interactable);
                await interactable.Interact(gameObject);

                // Wait a frame so that if 'advance dialogue' is the same
                // button as 'interact', we don't accidentally trigger a new
                // dialogue with the same input as leaving the previous
                // dialogue (i.e. we'd never leave dialogue)
                await YarnTask.Yield();

                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                if (interactable.InteractorShouldTurnToFaceWhenInteracted)
                {
                    lookTarget = null;
                }

                Mode = previousMode;
            }

            RunInteraction(currentInteractable, this.destroyCancellationToken).Forget();
        }
    }

    #endregion
}