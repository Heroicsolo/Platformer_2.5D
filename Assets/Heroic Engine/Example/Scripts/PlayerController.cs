using HeroicEngine.Components;
using HeroicEngine.Systems;
using HeroicEngine.Systems.Inputs;
using HeroicEngine.Systems.DI;
using UnityEngine;

namespace HeroicEngine.Examples
{
    public class PlayerController : MonoBehaviour
    {
        [SerializeField][Min(0f)] private float moveSpeed = 5f;

        [Inject] private CameraController cameraController;
        [Inject] private IInputManager inputManager;

        private CharacterController characterController;

        void Start()
        {
            InjectionManager.InjectTo(this);

            cameraController.SetPlayerTransform(transform);

            characterController = GetComponent<CharacterController>();
        }

        void Update()
        {
            characterController.Move(moveSpeed * Time.deltaTime * cameraController.GetWorldDirection(inputManager.GetMovementDirection()));

            // Push manequinn on example scene
            if (Input.GetKeyDown(KeyCode.F))
            {
                Ragdoll ragdoll = FindObjectOfType<Ragdoll>();
                if (ragdoll != null)
                {
                    ragdoll.Push(ragdoll.transform.position - transform.position, 150f);
                }
            }
        }
    }
}