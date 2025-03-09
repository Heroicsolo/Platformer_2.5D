using HeroicEngine.Components;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace HeroicEngine.Utils.Editor
{
    public class RagdollAdder : EditorWindow
    {
        private static readonly Dictionary<HumanBodyBones, float> bonesWeights = new Dictionary<HumanBodyBones, float>()
        {
            [HumanBodyBones.Hips] = 0.2f,
            [HumanBodyBones.Chest] = 0.25f,
            [HumanBodyBones.Head] = 0.15f,
            [HumanBodyBones.LeftUpperLeg] = 0.075f,
            [HumanBodyBones.RightUpperLeg] = 0.075f,
            [HumanBodyBones.LeftLowerLeg] = 0.075f,
            [HumanBodyBones.RightLowerLeg] = 0.075f,
            [HumanBodyBones.LeftUpperArm] = 0.05f,
            [HumanBodyBones.RightUpperArm] = 0.05f,
            [HumanBodyBones.LeftLowerArm] = 0.05f,
            [HumanBodyBones.RightLowerArm] = 0.05f,
        };

        private static readonly Dictionary<HumanBodyBones, HumanBodyBones> bonesConnections = new Dictionary<HumanBodyBones, HumanBodyBones>()
        {
            [HumanBodyBones.Chest] = HumanBodyBones.Hips,
            [HumanBodyBones.Head] = HumanBodyBones.Chest,
            [HumanBodyBones.LeftUpperLeg] = HumanBodyBones.Hips,
            [HumanBodyBones.RightUpperLeg] = HumanBodyBones.Hips,
            [HumanBodyBones.LeftLowerLeg] = HumanBodyBones.LeftUpperLeg,
            [HumanBodyBones.RightLowerLeg] = HumanBodyBones.RightUpperLeg,
            [HumanBodyBones.LeftUpperArm] = HumanBodyBones.Chest,
            [HumanBodyBones.RightUpperArm] = HumanBodyBones.Chest,
            [HumanBodyBones.LeftLowerArm] = HumanBodyBones.LeftUpperArm,
            [HumanBodyBones.RightLowerArm] = HumanBodyBones.RightUpperArm,
        };

        private static float totalWeight = 100f;

        [MenuItem("Tools/HeroicEngine/Components/Add Ragdoll")]
        public static void AddRagdoll()
        {
            GetWindow<RagdollAdder>("Add Ragdoll");
        }

        private void OnGUI()
        {
            GUILayout.Label("Add Ragdoll to GameObject", EditorStyles.boldLabel);

            EditorGUILayout.LabelField("Total weight of body:");

            totalWeight = EditorGUILayout.FloatField(totalWeight);

            totalWeight = Mathf.Max(0f, totalWeight);

            if (GUILayout.Button("Add Ragdoll"))
            {
                if (totalWeight == 0f)
                {
                    Debug.LogError("Weight is 0.");
                    return;
                }

                GameObject selectedObject = Selection.activeGameObject;

                if (selectedObject == null)
                {
                    Debug.LogError("No GameObject selected.");
                    return;
                }

                // Check if the selected GameObject has a Humanoid rig.
                Animator animator = selectedObject.GetComponent<Animator>();
                if (animator == null || animator.avatar == null || animator.avatar.isHuman == false)
                {
                    Debug.LogError("Selected GameObject does not have a humanoid rig.");
                    return;
                }

                // Proceed to configure the Ragdoll
                ConfigureRagdoll(selectedObject, animator);
            }
        }

        private static void ConfigureRagdoll(GameObject selectedObject, Animator animator)
        {
            // Check if a Ragdoll component already exists
            if (selectedObject.GetComponent<Ragdoll>() != null)
            {
                Debug.LogWarning("Ragdoll is already configured on this GameObject.");
                return;
            }

            // Create a Ragdoll component and configure body parts
            var ragdoll = selectedObject.AddComponent<Ragdoll>();
            SetupRagdoll(ragdoll, animator);
        }

        private static void SetupRagdoll(Ragdoll ragdoll, Animator animator)
        {
            // Try to find the humanoid body parts and configure them
            Transform hips = animator.GetBoneTransform(HumanBodyBones.Hips);

            Dictionary<HumanBodyBones, Transform> bonesList = new Dictionary<HumanBodyBones, Transform>
            {
                { HumanBodyBones.Hips, animator.GetBoneTransform(HumanBodyBones.Hips) },
                { HumanBodyBones.Head, animator.GetBoneTransform(HumanBodyBones.Head) },
                { HumanBodyBones.Chest, animator.GetBoneTransform(HumanBodyBones.Chest) },
                { HumanBodyBones.LeftUpperLeg, animator.GetBoneTransform(HumanBodyBones.LeftUpperLeg) },
                { HumanBodyBones.LeftLowerLeg, animator.GetBoneTransform(HumanBodyBones.LeftLowerLeg) },
                { HumanBodyBones.RightUpperLeg, animator.GetBoneTransform(HumanBodyBones.RightUpperLeg) },
                { HumanBodyBones.RightLowerLeg, animator.GetBoneTransform(HumanBodyBones.RightLowerLeg) },
                { HumanBodyBones.LeftUpperArm, animator.GetBoneTransform(HumanBodyBones.LeftUpperArm) },
                { HumanBodyBones.RightUpperArm, animator.GetBoneTransform(HumanBodyBones.RightUpperArm) },
                { HumanBodyBones.LeftLowerArm, animator.GetBoneTransform(HumanBodyBones.LeftLowerArm) },
                { HumanBodyBones.RightLowerArm, animator.GetBoneTransform(HumanBodyBones.RightLowerArm) }
            };

            // Setup hips bone and Ragdoll component
            if (hips != null)
            {
                var rb = CreateRagdollBone(hips, totalWeight * bonesWeights[HumanBodyBones.Hips]);
                ragdoll.SetHipsAndAnimator(rb, animator);
            }
            else
            {
                // Hips is not found, ragdoll cannot be configired
                Debug.LogWarning("Ragdoll configuration failed: hips is not found!");
                return;
            }

            // Initialize bones with proper weights
            foreach (var bone in bonesList)
            {
                if (bone.Value != null && bone.Key != HumanBodyBones.Hips)
                {
                    CreateRagdollBone(bone.Value, totalWeight * bonesWeights[bone.Key]);
                }
            }

            // Connect bones by joints
            foreach (var bone in bonesList)
            {
                if (bone.Value != null)
                {
                    if (bonesConnections.ContainsKey(bone.Key) && bonesList.ContainsKey(bonesConnections[bone.Key]))
                    {
                        ConnectWithJoint(bonesList[bonesConnections[bone.Key]].gameObject, bone.Value.gameObject);
                    }
                }
            }

            ragdoll.SetRagdollMode(false);

            // Log success message
            Debug.Log("Ragdoll configuration complete!");
        }

        private static Rigidbody CreateRagdollBone(Transform bone, float weight)
        {
            Rigidbody rb = bone.gameObject.GetComponent<Rigidbody>();

            // Add Rigidbody and Collider for the ragdoll part (simplified for basic setup)
            if (rb == null)
            {
                rb = bone.gameObject.AddComponent<Rigidbody>();
            }

            BoxCollider col = bone.gameObject.AddComponent<BoxCollider>(); // For simplicity, we use BoxCollider. You can adjust this as needed.

            MeshFilter meshFilter = bone.gameObject.GetComponent<MeshFilter>();

            // If mesh presented on this bone, adjust collider size to it
            if (meshFilter != null)
            {
                col.size = meshFilter.sharedMesh.bounds.size;
                col.center = meshFilter.sharedMesh.bounds.center;
            }
            else // If it's SkinnedMeshRenderer, we should just use bone weight then
            {
                col.size = Vector3.one;
                col.size *= weight;
                col.size /= totalWeight;
            }

            // Optional: Set Rigidbody properties (like mass, drag, etc.) for realism
            rb.mass = weight; // Adjust mass as needed
            rb.drag = 0.1f; // Adjust drag as needed

            // Optional: Adjust Collider properties
            col.isTrigger = false; // Can be set to true based on your needs

            return rb;
        }

        private static void ConnectWithJoint(GameObject parent, GameObject child)
        {
            // Add a ConfigurableJoint to the child (if it doesn't already have one)
            ConfigurableJoint joint = child.AddComponent<ConfigurableJoint>();

            // Set the connected body to the parent
            Rigidbody parentRb = parent.GetComponent<Rigidbody>();
            if (parentRb != null)
            {
                joint.connectedBody = parentRb;
            }

            // Configure joint settings
            joint.xMotion = ConfigurableJointMotion.Limited;  // Limit movement along the X axis
            joint.yMotion = ConfigurableJointMotion.Limited;  // Limit movement along the Y axis
            joint.zMotion = ConfigurableJointMotion.Limited;  // Limit movement along the Z axis

            // Angular motion can also be limited (rotation around each axis)
            joint.angularXMotion = ConfigurableJointMotion.Limited;
            joint.angularYMotion = ConfigurableJointMotion.Limited;
            joint.angularZMotion = ConfigurableJointMotion.Limited;

            // Set joint limits (adjust these values based on your requirements)
            SoftJointLimit angularLimit = new SoftJointLimit();
            angularLimit.limit = 60f;
            joint.lowAngularXLimit = angularLimit;
            joint.highAngularXLimit = angularLimit;

            SoftJointLimit linearLimit = new SoftJointLimit();
            linearLimit.limit = 0.05f;
            joint.linearLimit = linearLimit;

            SoftJointLimitSpring linearLimitSpring = new SoftJointLimitSpring();
            linearLimitSpring.spring = 4000f;
            linearLimitSpring.damper = 100f;
            joint.linearLimitSpring = linearLimitSpring;
            joint.angularXLimitSpring = linearLimitSpring;
            joint.angularYZLimitSpring = linearLimitSpring;

            joint.breakForce = 10000f;
            joint.breakTorque = 10000f;
        }
    }
}