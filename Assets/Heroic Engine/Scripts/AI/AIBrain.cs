using HeroicEngine.Utils.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace HeroicEngine.AI
{
    public class AIBrain : MonoBehaviour
    {
        [SerializeField] private List<PerceptronBinding> perceptronBindings;

        private Action onBrainInitialized;

        public List<PerceptronBinding> PerceptronBindings => perceptronBindings;

        public Perceptron GetPerceptronByTask(string taskName)
        {
            int idx = perceptronBindings.FindIndex(pb => pb.taskName == taskName);
            return idx >= 0 ? perceptronBindings[idx].perceptron : null;
        }

        private void Awake()
        {
            perceptronBindings.ForEach(pb => pb.perceptron.Init());
            onBrainInitialized?.Invoke();
        }

        public void SubscribeToBrainInit(Action callback)
        {
            onBrainInitialized += callback;
        }

        // Call this method to force AI bot learn some successful solution
        public void SaveSolution(string taskName, List<float> inputParams, float solution, bool saveToKnowledgeBase = true)
        {
            Perceptron perceptron = GetPerceptronByTask(taskName);

            if (perceptron == null)
            {
                return;
            }

            // Train neural network
            perceptron.TrainSingle(inputParams, solution, saveToKnowledgeBase);
        }

        // Call this method to force AI bot forget about some bad solution
        public void ForgetSolution(string taskName, List<float> inputParams, float solution)
        {
            Perceptron perceptron = GetPerceptronByTask(taskName);

            if (perceptron == null)
            {
                return;
            }

            perceptron.ForgetSolution(inputParams, solution);
        }

        // This method returns solution with known input parameters
        public bool FindSolution(string taskName, List<float> inputParams, out float solution)
        {
            Perceptron perceptron = GetPerceptronByTask(taskName);

            if (perceptron == null)
            {
                solution = 0f;
                return false;
            }

            solution = perceptron.GetSolution(inputParams);
            return true;
        }
    }

    [Serializable]
    public struct PerceptronBinding
    {
        public string taskName;
        public Perceptron perceptron;
    }
}