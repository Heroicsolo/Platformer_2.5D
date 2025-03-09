using System;
using System.Collections.Generic;
using UnityEngine;
using HeroicEngine.Utils.Data;
using static HeroicEngine.AI.Perceptron;
using System.Linq;

namespace HeroicEngine.AI
{
    [CreateAssetMenu(fileName = "NewPerceptron", menuName = "Tools/HeroicEngine/AI/New Perceptron")]
    public class Perceptron : GuidScriptable
    {
        [Header("Perceptron structure")]
        [Min(1)][Tooltip("Inputs number")] public int inputsNumber = 1;            // Inputs number
        [Min(1)][Tooltip("Hidden layer size")] public int neuronsNumber = 1;       // Neurons number

        [Header("Optimization")]
        [Tooltip("If true, perceptron will firstly look at knowledge database and get solution from there, if available. Otherwise, it will calculate solution by himself")]
        public bool useKnowledgeBase = true;

        [Header("Training Settings")]
        [Min(0f)] public float learningRate = 0.1f;  // Learning rate for gradient descent
        [Min(1)] public int trainingEpochs = 100;    // Training epochs count

        [Header("Training Data")]
        [Tooltip("Here you can add some training data, which will be used during the first perceptron launch")]
        public List<PerceptronTrainingData> perceptronTrainingDatas = new List<PerceptronTrainingData>();

        private List<List<float>> inputWeights;  // List to store weights for each input of each neuron
        private List<float> outputWeights;  // List to store output weights for each neuron
        private List<float> inputs;         // List to store input values
        private List<float> hiddenLayer;    // List to store hidden layer values
        private List<float> hiddenBiases;   // Bias values for hidden layer neurons
        private float bias;                 // Bias value for the perceptron

        private AIKnowledgeBase knowledges;

        public List<List<float>> Weights => inputWeights;
        public int NeuronsNumber => neuronsNumber;
        public int InputsNumber => inputsNumber;

        public static List<float> ConvertStringToFloats(string str)
        {
            List<float> result = new List<float>();
            foreach (var symbol in str)
            {
                result.Add(symbol);
            }
            return result;
        }

        public string Init()
        {
            LoadKnowledges();

            inputs = new List<float>(inputsNumber);
            hiddenLayer = new List<float>(neuronsNumber);

            if (DataSaver.LoadDataSecurely(guid, out ModelData modelData))
            {
                inputWeights = modelData.GetWeights();
                hiddenBiases = modelData.hiddenBiases;
                outputWeights = modelData.hiddenWeights;
                bias = modelData.bias;
            }
            else
            {
                inputWeights = new List<List<float>>();
                outputWeights = new List<float>();
                hiddenBiases = new List<float>();
                // Initialize weights randomly

                System.Random rand = new System.Random();
                for (int i = 0; i < neuronsNumber; i++)
                {
                    inputWeights.Add(new List<float>());

                    for (int j = 0; j < inputsNumber; j++)
                    {
                        inputWeights[i].Add((float)rand.NextDouble() * 2 - 1); // Random values between -1 and 1
                    }

                    outputWeights.Add((float)(rand.NextDouble() * 2 - 1)); // Random values between -1 and 1
                    hiddenBiases.Add((float)(rand.NextDouble() * 2 - 1));  // Bias for hidden neurons
                }

                // Initialize bias with a random value
                bias = (float)rand.NextDouble() * 2 - 1; // Random value between -1 and 1

                Train(perceptronTrainingDatas.ConvertAll(td => td.Inputs), perceptronTrainingDatas.ConvertAll(td => td.TargetOutput));
            }

            return guid;
        }

        public void SetInputs(List<float> inputValues)
        {
            inputs.Clear();

            foreach (var value in inputValues)
            {
                inputs.Add(value);
            }
        }

        private void LoadKnowledges()
        {
            if (!DataSaver.LoadData(Guid + "_knowledges", out knowledges))
            {
                knowledges = new AIKnowledgeBase();
                knowledges.solutions = new List<AIKnownSolution>();
            }
        }

        private void SaveKnowledges()
        {
            DataSaver.SaveData(knowledges, Guid + "_knowledges");
        }

        // Sigmoid activation function
        private float Sigmoid(float x)
        {
            return 1f / (1f + Mathf.Exp(-x));  // Standard sigmoid function
        }

        // Sigmoid derivative for backpropagation
        private float SigmoidDerivative(float x)
        {
            return x * (1 - x);
        }

        public float GetSolution(List<float> inputs)
        {
            int solutionIdx = knowledges.solutions.FindIndex(s => s.inputValues.SequenceEqual(inputs));

            // If we have known solution in knowledge base, return it
            if (solutionIdx >= 0 && useKnowledgeBase)
            {
                return knowledges.solutions[solutionIdx].solution;
            }
            else // Otherwise, calculate solution by neural network
            {
                SetInputs(inputs);

                return Process();
            }
        }

        private float Process()
        {
            float output = 0f;

            hiddenLayer.Clear();

            // Calculate hidden layer outputs
            for (int j = 0; j < neuronsNumber; j++)
            {
                float sum = hiddenBiases[j];
                for (int i = 0; i < inputsNumber; i++)
                {
                    sum += inputs[i] * inputWeights[j][i];
                }
                hiddenLayer.Add(Sigmoid(sum));
            }

            // Calculate final output
            float outputSum = bias;
            for (int i = 0; i < neuronsNumber; i++)
            {
                outputSum += hiddenLayer[i] * outputWeights[i];
            }
            output = Sigmoid(outputSum);

            return output;
        }

        // Training method (Gradient Descent)
        public void Train(List<List<float>> inputSet, List<float> targetOutputs, bool saveToKnowledgeBase = true)
        {
            for (int i = 0; i < inputSet.Count; i++)
            {
                TrainSingle(inputSet[i], targetOutputs[i], saveToKnowledgeBase);
            }
        }

        public void TrainSingle(List<float> inputs, float targetOutput, bool saveToKnowledgeBase = true)
        {
            for (int epoch = 0; epoch < trainingEpochs; epoch++)
            {
                Backpropagate(inputs, targetOutput);
            }

            SaveModel();

            if (!saveToKnowledgeBase)
            {
                return;
            }

            int solutionIdx = knowledges.solutions.FindIndex(s => s.inputValues.SequenceEqual(inputs));

            if (solutionIdx < 0)
            {
                knowledges.solutions.Add(new AIKnownSolution
                {
                    inputValues = inputs,
                    solution = targetOutput
                });

                SaveKnowledges();
            }
        }

        public void LearnSolution(List<float> inputs, float targetOutput, bool saveToKnowledgeBase = true)
        {
            TrainSingle(inputs, targetOutput, saveToKnowledgeBase);
        }

        public void ForgetSolution(List<float> inputs, float solution)
        {
            int solutionIdx = knowledges.solutions.FindIndex(s => s.inputValues.SequenceEqual(inputs) && s.solution == solution);

            if (solutionIdx >= 0)
            {
                knowledges.solutions.RemoveAt(solutionIdx);
                SaveKnowledges();
            }
        }

        // Backpropagation to update the weights
        private void Backpropagate(List<float> inputValues, float target)
        {
            SetInputs(inputValues);

            // Feedforward to get current output
            float prediction = Process();

            // Calculate error
            float error = target - prediction;

            // Calculate the gradient for output layer
            float outputGradient = error * SigmoidDerivative(prediction);

            // Update weights from hidden to output layer
            for (int i = 0; i < neuronsNumber; i++)
            {
                outputWeights[i] += learningRate * outputGradient * hiddenLayer[i];
            }

            // Update the output bias
            bias += learningRate * outputGradient;

            // Backpropagate error to hidden layer
            float[] hiddenGradients = new float[neuronsNumber];
            for (int j = 0; j < neuronsNumber; j++)
            {
                float hiddenError = outputGradient * outputWeights[j];
                hiddenGradients[j] = hiddenError * SigmoidDerivative(hiddenLayer[j]);

                // Update weights from input to hidden layer
                for (int i = 0; i < inputsNumber; i++)
                {
                    inputWeights[j][i] += learningRate * hiddenGradients[j] * inputValues[i];
                }

                // Update hidden bias
                hiddenBiases[j] += learningRate * hiddenGradients[j];
            }
        }

        // Save percetron weights and bias to file
        private void SaveModel()
        {
            ModelData modelData = new ModelData(inputWeights, outputWeights, hiddenBiases, bias);
            DataSaver.SaveDataSecurely(modelData, guid);
        }

        // Helper class for serializing the model data (weights and bias)
        [Serializable]
        private class ModelData
        {
            public List<WeightsData> weights;
            public List<float> hiddenBiases;
            public List<float> hiddenWeights;
            public float bias;

            public ModelData(List<List<float>> inputWeights, List<float> hiddenWeights, List<float> hiddenBiases, float bias)
            {
                this.hiddenBiases = hiddenBiases;
                weights = new List<WeightsData>();
                this.hiddenWeights = hiddenWeights;
                inputWeights.ForEach(w =>
                {
                    weights.Add(new WeightsData
                    {
                        weights = w,
                    });
                });
                this.bias = bias;
            }

            public List<List<float>> GetWeights()
            {
                return weights.ConvertAll(w => new List<float>(w.weights));
            }
        }

        [Serializable]
        private struct WeightsData
        {
            public List<float> weights;
        }

        [Serializable]
        public struct AIKnownSolution
        {
            public List<float> inputValues;
            public float solution;
        }

        [Serializable]
        public struct AIKnowledgeBase
        {
            public List<AIKnownSolution> solutions;
        }

        [Serializable]
        public struct PerceptronTrainingData
        {
            public List<float> Inputs;
            public float TargetOutput;
        }
    }
}