using UnityEngine;

namespace HeroicEngine.Utils.Data
{
    public class GuidScriptable : ScriptableObject
    {
        [SerializeField] [HideInInspector] protected string guid;
        public string Guid { get { return guid; } set { guid = value; } }
    }
}