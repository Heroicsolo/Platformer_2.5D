using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Utils.HidingObjects
{
    //Add this script to object which must be hidden if will appear near the camera (that object has to contain MeshRenderer component)
    //If that object doesn't have MeshRenderer component, this script will search all MeshRenderer components in his children
    //You can also switch "proccessChildren" flag to "true", to proccess all children MeshRenderer components by default
    //Place needed shader which contains "_Color" property and Fade/Transparency mode into the "fadeShader" field!
    //For example, "Legacy Shaders/Transparent/Diffuse" will be fine
    public class AutoHidingObject : MonoBehaviour
    {
        [SerializeField] Shader m_fadeShader;
        [SerializeField] bool m_proccessChildren = false;

        private List<MeshRenderer> m_renderers;
        private bool m_hidden = false;
        private Dictionary<Material, Shader> m_initShaders;

        public bool IsHidden { get { return m_hidden; } }

        private void Awake()
        {
            m_renderers = new List<MeshRenderer>();

            MeshRenderer rootRenderer = GetComponent<MeshRenderer>();

            if (!rootRenderer) m_proccessChildren = true;

            if (m_proccessChildren)
                m_renderers.AddRange(GetComponentsInChildren<MeshRenderer>());
            else
                m_renderers.Add(rootRenderer);

            m_initShaders = new Dictionary<Material, Shader>();

            foreach (var rend in m_renderers)
            {
                foreach (var mat in rend.materials)
                {
                    m_initShaders.Add(mat, rend.material.shader);
                }
            }

            m_hidden = false;
        }

        public void Hide()
        {
            if (m_hidden)
            {
                return;
            }

            m_hidden = true;

            foreach (var rend in m_renderers)
            {
                foreach (var mat in rend.materials)
                {
                    mat.shader = m_fadeShader;
                    mat.ToFadeMode();
                    Color col = mat.color;
                    col.a = 0.15f;
                    mat.color = col;
                }
            }
        }

        public void ShowChild(Transform child)
        {
            for (int i = 0; i < m_renderers.Count; i++)
            {
                if (m_renderers[i].transform == child)
                {
                    foreach (var mat in m_renderers[i].materials)
                    {
                        mat.shader = m_initShaders[mat];

                        if (mat.HasProperty("_Color"))
                        {
                            Color col = mat.color;
                            col.a = 1f;
                            mat.color = col;
                        }
                    }
                }
            }
        }

        public void Show()
        {
            if (!m_hidden)
            {
                return;
            }

            m_hidden = false;

            if (m_renderers == null)
            {
                return;
            }

            for (int i = 0; i < m_renderers.Count; i++)
            {
                foreach (var mat in m_renderers[i].materials)
                {
                    mat.shader = m_initShaders[mat];
                    mat.ToOpaqueMode();

                    if (mat.HasProperty("_Color"))
                    {
                        Color col = mat.color;
                        col.a = 1f;
                        mat.color = col;
                    }
                }
            }
        }
    }
}