using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Utils.HidingObjects
{
    //This is hiding objects manager - script that checks if some AutoHidingObject is situated in front of camera
    //Add AutoHidingObject script to all objects which you need to hide (that objects have to contain MeshRenderer component)
    //Add this component to your camera and set needed objects check distance
    public class HidingObjectsManager : MonoBehaviour
    {
        private const float TickPeriod = 0.5f;
        private const int MaxCollidersToFind = 50;
        private const int SearchRadius = 8;

        [SerializeField] float m_distance = 25f;
        [SerializeField] private Camera cam;

        private List<AutoHidingObject> m_ahoList = new List<AutoHidingObject>();
        private List<AutoHidingObject> hiddenObjects = new List<AutoHidingObject>();

        private Transform m_playerTransform;
        private float tickDelay = TickPeriod;
        private RaycastHit[] raycastHits = new RaycastHit[5];

        public void SetPlayerTransform(Transform playerTransform)
        {
            m_playerTransform = playerTransform;
        }

        private void Start()
        {
            m_ahoList.Clear();
        }

        private bool InFrontOfPlayer(Transform t)
        {
            if (Physics.RaycastNonAlloc(transform.position, m_playerTransform.position - transform.position, raycastHits, m_distance) > 0)
            {
                for (int i = 0; i < raycastHits.Length; i++)
                {
                    if (raycastHits[i].transform == t)
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private bool InScreenArea(Transform t)
        {
            Vector3 pos = cam.WorldToViewportPoint(t.position);
            return (pos.x > 0 && pos.x < 1 &&
                    pos.y > 0 && pos.y < 1 && pos.z > 0);
        }

        void Update()
        {
            if (tickDelay > 0)
            {
                tickDelay -= Time.deltaTime;
                return;
            }

            tickDelay = TickPeriod;

            ProcessPreviouslyHidden();

            RefreshNearObjects();

            foreach (AutoHidingObject aho in m_ahoList)
            {
                if (aho == null) continue;

                if (InScreenArea(aho.transform) && InFrontOfPlayer(aho.transform))
                {
                    aho.Hide();
                    hiddenObjects.Add(aho);
                }
                else
                {
                    aho.Show();
                }
            }
        }

        private void ProcessPreviouslyHidden()
        {
            for (int i = hiddenObjects.Count - 1; i >= 0; i--)
            {
                if (hiddenObjects[i] == null)
                {
                    hiddenObjects.Remove(hiddenObjects[i]);
                    continue;
                }

                if (InFrontOfPlayer(hiddenObjects[i].transform) == false ||
                    InScreenArea(hiddenObjects[i].transform) == false)
                {
                    hiddenObjects[i].Show();
                    hiddenObjects.Remove(hiddenObjects[i]);
                }
            }
        }

        private void RefreshNearObjects()
        {
            m_ahoList.Clear();

            int maxColliders = MaxCollidersToFind;
            Collider[] hitColliders = new Collider[maxColliders];
            int numColliders = Physics.OverlapSphereNonAlloc(m_playerTransform.position, SearchRadius, hitColliders);
            for (int i = 0; i < numColliders; i++)
            {
                if (hitColliders[i].TryGetComponent(out AutoHidingObject objetToHide))
                {
                    m_ahoList.Add(objetToHide);
                }
            }
        }
    }
}