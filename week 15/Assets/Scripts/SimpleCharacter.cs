#nullable enable
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

    public abstract class SimpleCharacter : MonoBehaviour
    {
        [Group("Movement")]
        public Transform? lookTarget;
        public bool IsAlive { get; protected set; } = true;

        public abstract void UpdateMovement();
    }
}
