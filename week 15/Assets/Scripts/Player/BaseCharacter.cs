#nullable enable
using System;
using Yarn.Unity;
using Yarn.Unity.Samples;
namespace MyGame.Characters
{
    using UnityEngine;
    using System.Threading;
    using Yarn.Unity.Attributes;
    using System.Collections.Generic;
    using UnityEngine.Events;
    using System.Threading.Tasks;

    public abstract class BaseCharacter : MonoBehaviour
    {
        [SerializeField] protected bool isPlayerControlled;
        public enum CharacterMode
        {
            PlayerControlledMovement,
            ExternallyControlledMovement,
            PathMovement,
            Interact,
        }

        public CharacterMode Mode { get; protected set; }

        protected CharacterController? characterController;

        [HideIf(nameof(isPlayerControlled))]
        [SerializeField] protected SimplePath? followPath;
        
        public float CurrentSpeedFactor { get; private set; } = 0f;

        public bool CanInteract => Mode == CharacterMode.PlayerControlledMovement;
        public bool HasPath => followPath != null;

        [Group("Movement")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] protected InputAxisButton interactInput = new();

        [Group("Movement")]
        public Transform? lookTarget;
        public bool IsAlive { get; protected set; } = true;

        #region Animation Variables     
        [Group("Animation")]
        [SerializeField] private Animator? animator;
        [Group("Animation")]
        [SerializeField] SerializableDictionary<string, string> facialExpressions = new();
        [Group("Animation")]
        [SerializeField] string facialExpressionsLayer = "Face";
        private int facialExpressionsLayerID = 0;

        [SerializeField] Texture2D? deathMouthTexture;

        [Group("Animation")]
        [Header("Blinking")]
        [SerializeField] float meanBlinkTime = 2f;
        [Group("Animation")]
        [SerializeField] float blinkTimeVariance = 0.5f;

        [Group("Animation Parameters", true)]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Float)]
        [SerializeField] private string speedParameter = "Speed";
        [Group("Animation Parameters", true)]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Float)]
        [SerializeField] private string sideTiltParameter = "Side Tilt";
        [Group("Animation Parameters", true)]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Float)]
        [SerializeField] private string forwardTiltParameter = "Forward Tilt";
        [Group("Animation Parameters", true)]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Float)]
        [SerializeField] private string turnParameter = "Turn";
        [Group("Animation Parameters", true)]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Trigger)]
        [SerializeField] string blinkTriggerName = "Blink";
        [Group("Animation Parameters", true)]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Float)]
        [SerializeField] string cycleOffsetParameter = "Cycle Offset";
        [Group("Animation Parameters", true)]
        [AnimationParameter(nameof(animator), AnimatorControllerParameterType.Bool)]
        [SerializeField] string aliveParameter = "Alive";

        [Group("Animation Parameters")]
        [AnimationLayer(nameof(animator))]

        private float timeUntilNextBlink = 0f;
        private Dictionary<int, CancellationTokenSource> activeAnimationLerps = new();

        #endregion
        #region Interaction Variables
        [Group("Interaction")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] protected float interactionRadius = 1f;
        [Group("Interaction")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] protected Vector3 offset = Vector3.zero;
        [Group("Interaction")]
        [ShowIf(nameof(isPlayerControlled))]
        [SerializeField] UnityEvent<Interactable>? onInteracting;

        private List<Interactable> interactables = new();

        private Interactable? currentInteractable = null;

        #endregion

        #region Movement Logic
        public abstract void UpdateMovement();
        #endregion

        #region Interaction Logic

        public void SetupInteraction()
        {
            interactables.Clear();

            interactables.AddRange(FindObjectsByType<Interactable>(FindObjectsInactive.Include, FindObjectsSortMode.None));

        }

        protected void UpdateInteraction()
        {
            // Debug.Log("UpdateInteraction called with isPlayerControlled " + isPlayerControlled);
            // Debug.Log("UpdateInteraction called with CanInteract " + CanInteract);
            // Console.WriteLine("Update interaction hERE");
            if (isPlayerControlled == false)
            {
                // Only player-controlled characters can interact
                // Debug.Log("Here1");

                return;
            }

            if (!CanInteract)
            {
                // Debug.Log("Here2");
                // We can only interact if we're allowed to move around.
                return;
            }
            Debug.Log("Here3");

            var previousInteractable = currentInteractable;

            (float Distance, Interactable? Interactable) nearest = (float.PositiveInfinity, null);
            Debug.Log("nearest" + interactables.Count);
            for (int i = 0; i < interactables.Count; i++)
            {
                var interactable = interactables[i];

                if (!interactable.isActiveAndEnabled)
                {
                    Debug.Log("Here4");
                    // We can't interact if the component or its gameobject
                    // isn't enabled
                    continue;
                }

                if (interactable.gameObject == gameObject)
                {
                    Debug.Log("Here5");
                    // We can't interact with ourselves
                    continue;
                }

                if (interactable.gameObject.TryGetComponent<SimpleCharacter>(out var character) && !character.IsAlive)
                {
                    Debug.Log("Here6");
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
        #region Animation Logic

        protected void SetupAnimation()
        {
            characterController = GetComponent<CharacterController>();

            if (animator != null)
            {
                facialExpressionsLayerID = animator.GetLayerIndex(facialExpressionsLayer);

                // Randomly offset the cycle for the base pose so that
                // characters don't sync up
                animator.SetFloat(cycleOffsetParameter, Random.value);
            }

            timeUntilNextBlink = GetNextBlinkTime();
        }

        private float GetNextBlinkTime()
        {
            return meanBlinkTime + Mathf.Lerp(-blinkTimeVariance, blinkTimeVariance, UnityEngine.Random.value);
        }

        public void UpdateAnimation()
        {

            if (animator == null)
            {
                return;
            }

            timeUntilNextBlink -= Time.deltaTime;

            if (timeUntilNextBlink <= 0 && !string.IsNullOrEmpty(blinkTriggerName))
            {
                animator.SetTrigger(blinkTriggerName);
                timeUntilNextBlink = GetNextBlinkTime();
            }

            animator.SetFloat(speedParameter, CurrentSpeedFactor);
        }

        protected async YarnTask TweenAnimationParameter(string animationParameter, float to, float duration, System.Func<float, float> easingFunction, CancellationToken cancellationToken)
        {
            if (animator == null)
            {
                return;
            }

            var hash = Animator.StringToHash(animationParameter);
            var currentValue = animator.GetFloat(hash);

            // If a tween was already running for this parameter, cancel it now
            if (activeAnimationLerps.TryGetValue(hash, out var cancellationTokenSource))
            {
                cancellationTokenSource.Cancel();
            }

            // Create and store a cancellation token source for this animation
            var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            activeAnimationLerps[hash] = cts;

            // Run the tween
            await Tweening.TweenValue(currentValue, to, duration, easingFunction, value => animator.SetFloat(hash, value), cts.Token);

            // Clean up
            activeAnimationLerps.Remove(hash);
        }
        #endregion

        #region Core logic
        // protected void Awake()
        // {
        //     Mode = CharacterMode.PlayerControlledMovement;

        //     // SetupMovement();
        //     // SetupAnimation();
        //     SetupInteraction();
        // }

        // protected virtual void Update() {
        //     // UpdateMovement();
        //     // UpdateAnimation();
        //     UpdateInteraction();
        // }

        #endregion

    }
}
