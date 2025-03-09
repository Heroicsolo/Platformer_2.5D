using UnityEngine;

namespace HeroicEngine.Components
{
    public class Ragdoll : MonoBehaviour
    {
        [SerializeField] private Animator animator;
        [SerializeField] private Rigidbody hips;
        [SerializeField] private bool ragdollModeAtStart = false;

        private void Start()
        {
            SetRagdollMode(ragdollModeAtStart);
        }

        public void SetHipsAndAnimator(Rigidbody hips, Animator animator)
        {
            this.hips = hips;
            this.animator = animator;
        }

        /// <summary>
        /// This method sets ragdoll mode. If enabled, character Animator will be disabled and skeleton will become completely physical.
        /// Otherwise, Animator will be active and bones will be moved by animations.
        /// </summary>
        /// <param name="enable">Enable ragdoll?</param>
        public void SetRagdollMode(bool enable)
        {
            if (animator != null)
            {
                animator.enabled = !enable;
            }

            Rigidbody[] rbs = hips.transform.GetComponentsInChildren<Rigidbody>();

            foreach (Rigidbody rb in rbs)
            {
                rb.isKinematic = !enable;

                if (rb.TryGetComponent<Collider>(out var col))
                {
                    col.isTrigger = !enable;
                }
            }
        }

        /// <summary>
        /// Hehe, what can be more funny than pushing ragdoll?
        /// This method applies certain force to the character body with certain direction and automatically activates ragdoll mode on it.
        /// </summary>
        /// <param name="direction">Direction of pushing</param>
        /// <param name="force">Pushing force</param>
        /// <param name="forceMode">Force mode</param>
        public void Push(Vector3 direction, float force, ForceMode forceMode = ForceMode.Impulse)
        {
            SetRagdollMode(true);

            hips.AddForce(direction * force, forceMode);
        }

        /// <summary>
        /// This method applies certain rotation force to the character body and automatically activates its ragdoll mode.
        /// </summary>
        /// <param name="direction">Rotation direction</param>
        /// <param name="torque">Rotation force</param>
        /// <param name="forceMode">Force mode</param>
        public void AddTorque(Vector3 direction, float torque, ForceMode forceMode = ForceMode.Impulse)
        {
            SetRagdollMode(true);

            hips.AddTorque(direction * torque, forceMode);
        }
    }
}