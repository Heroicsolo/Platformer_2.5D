using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HeroicEngine.Utils.Data
{
    public class ConstructableSO : ScriptableObject
    {
        protected List<object> parameters = new List<object>();

        public void Construct(params object[] args)
        {
            parameters = new List<object>(args);
        }

        public virtual void Initialize()
        {
        }
    }
}
