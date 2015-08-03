using System;
using System.Collections.Generic;
using System.Linq;
using BirdyNetwork.Classes.Graph;
using BirdyNetwork.Classes.NeuralNetwork;
using NeuralNetworkLibBase;
using System.ComponentModel.Composition;
using System.IO;
using BirdyNetwork.Classes;

namespace BirdyNetwork
{
    [Export(typeof (INeuralNetwork))]
    [ExportMetadata("Type", typeof (NeuralNetwork))]
    public class NeuralNetwork : SimpleGraph, INeuralNetwork
    {
        public double LearningSpeedCoefficient = 1;

        public const double MaxSumValue = 200; // 100 + 100

        public int SamplesPassed = 0;

        private List<Layer> _layers;

        public Layer Inputs
        {
            get { return _layers.First(); }
        }

        public Layer Outputs
        {
            get { return _layers.Last(); }
        }

        public NeuralNetwork()
        {
            Empty();
        }

        public void Empty()
        {
            _layers = new List<Layer>();
            Ribs = new List<Rib>();
            Nodes = new List<Node>();
        }

        public void PrintAnswers()
        {
            var s = Outputs.Aggregate("", (current, output) => current + (output.ActivationValue*MaxSumValue + " "));
            Console.WriteLine(s);
        }

        public void PrintConfiguration(StreamWriter sw)
        {
            sw.WriteLine(Inputs.Count);
            var s = HiddenLayers.Aggregate("", (cur, next) => cur + ((next as Layer).Count + " ")).Trim();
            sw.WriteLine(s);
            sw.WriteLine(Outputs.Count);
        }

        public void PrintWeights(StreamWriter sw)
        {
            foreach (var layer in _layers)
            {
                if (layer == Inputs) continue;
                foreach (var node in layer.Nodes)
                {
                    var s = node.Incoming.Aggregate("", (cur, next) => cur + (next.Weight + " ")).Trim();
                    sw.WriteLine(s);
                }
            }
        }

        public void ReadSavedConfiguration(StreamReader sr)
        {
            var line = "";
            var i = 1;
            var j = 0;
            do
            {
                line = sr.ReadLine();
                var weights = line.Split(' ').Select(double.Parse).ToList();
                for (var k = 0; k < weights.Count; k++)
                {
                    ((_layers[i].Nodes as List<Node>)[j].Incoming as List<Rib>)[k].Weight = weights[k];
                }
                j++;
                if (j == _layers[i].Count)
                {
                    j = 0;
                    i++;
                }
            } while (!sr.EndOfStream);
        }

        public void TeachNetwork(List<double> inputSequence, List<double> answersSequence)
        {
            var precision = 0.000001;
            SamplesPassed++;
            var oldAnswers = answersSequence.Select(t => t/MaxSumValue).ToList();
            var rand = new Random();
            LearningSpeedCoefficient = Math.Max(0.1, 1 - SamplesPassed/10000);
            var iterations = 0;
            while (true)
            {
                iterations++;
                CountNetwork(inputSequence);
                var temp = 0d;
                for (var i = 0; i < oldAnswers.Count; i++)
                {
                    temp += (Outputs[i].ActivationValue - oldAnswers[i])*(Outputs[i].ActivationValue - oldAnswers[i]);
                }
                temp /= 2d;
                if (temp < precision || iterations > 10000)
                {
                    return;
                }
                //LearningSpeedCoefficient = 1/iterations;
                BackwardPropagation(answersSequence);
            }
        }

        public void BackwardPropagation(List<double> answersSequence)
        {
            var answers = answersSequence.Select(t => t/MaxSumValue).ToList();
            if (answers.Count() != Outputs.Count)
                throw new Exception("Количество ответов не совпадает с количеством выходных узлов");

            // считаем ошибки весов
            // для каждого выходного нейрона
            for (var i = 0; i < answers.Count(); i++)
            {
                var errorFunction = (answers[i] - Outputs[i].ActivationValue);
                for (var k = 0; k < Outputs[i].Ribs.Count; k++)
                {
                    Outputs[i].Ribs[k].WeightError = errorFunction;
                }
            }
            // для каждого "скрытого" нейрона
            for (var i = _layers.Count - 2; i > 0; i--)
            {
                var layer = _layers[i];
                for (var j = 0; j < layer.Count; j++)
                {
                    var errorFunction = layer[j].OutcominRibs.Sum(rib => rib.WeightError*rib.Weight);
                    for (var k = 0; k < layer[j].IncominRibs.Count; k++)
                    {
                        layer[j].IncominRibs[k].WeightError = errorFunction;
                    }
                }
            }

            // "применяем" ошибки к весам связей между нейронами
            foreach (var rib in Ribs)
            {
                rib.Weight += LearningSpeedCoefficient*rib.WeightError*rib.Start.ActivationValue*
                              ActivationFunctionDerivative(rib.End.Value);
            }
        }

        public void CountNetwork(List<double> inputsSequence)
        {
            var inputs = inputsSequence.Select(t => t/MaxSumValue).ToList();
            FillInputs(inputs);
            for (var i = 1; i < _layers.Count; i++)
            {
                CountLayer(_layers[i]);
            }
        }

        private void CountLayer(Layer layer)
        {
            foreach (var item in layer)
            {
                item.Value = item.IncominRibs.Sum(rib => rib.Start.ActivationValue*rib.Weight);
                item.ActivationValue = ActivationFunction(item.Value);
            }
        }

        private void FillInputs(IEnumerable<double> inputs)
        {
            if (Inputs.Count != inputs.Count())
                throw new Exception("Число переданных значений не совпадает с числом входов");
            var i = 0;
            foreach (var input in inputs)
            {
                Inputs[i].Value = input;
                Inputs[i].ActivationValue = input;
                i++;
            }
        }

        private double ActivationFunction(double x)
        {
            return 1/(1 + Math.Exp(-x));
        }

        private double ActivationFunctionDerivative(double x)
        {
            var afx = ActivationFunction(x);
            return afx*(1 - afx);
        }

        private void ConnectLayersFull()
        {
            var rand = new Random();
            for (var j = 0; j < _layers.Count - 1; j++)
            {
                foreach (var l1 in _layers[j])
                {
                    foreach (var l2 in _layers[j + 1])
                    {
                        ConnectNodes(l1, l2, rand.NextDouble()/2.0 + 0.25);
                    }
                }
            }
        }

        private void GetInOutRibsReferences()
        {
            foreach (var node in Nodes)
            {
                node.GetIncomingRibs();
                node.GetOutcomingRibs();
            }
        }

        #region Interface

        public void Create(object argsRaw)
        {
            var args = argsRaw as ConstructionArgs;
            if (args == null)
                throw new Exception("Arguments type mismatch") { Data = { { "Arguments", argsRaw } } };
            Empty();
            _layers.Add(new Layer(args.Inputs));
            Nodes.AddRange(Inputs);
            foreach (int layerCount in args.HiddenLayers)
            {
                var l = new Layer(layerCount);
                _layers.Add(l);
                Nodes.AddRange(l);
            }
            _layers.Add(new Layer(args.Outputs));
            Nodes.AddRange(Outputs);
            ConnectLayersFull();
            GetInOutRibsReferences();
        }

        public void Train(double[] inputs, double[] outputs)
        {
            TeachNetwork(inputs.ToList(), outputs.ToList());
        }

        public List<double> Calculate(double[] inputs)
        {
            CountNetwork(inputs.ToList());
            return Outputs.Select(t => t.ActivationValue*MaxSumValue).ToList();
        }

        public IEnumerable<IConnection> Connections
        {
            get { return Ribs; }
        }

        public ILayer InputLayer
        {
            get { return Inputs; }
        }

        public Type ArgsType
        {
            get { return typeof (ConstructionArgs); }
        }

        public IEnumerable<ILayer> HiddenLayers
        {
            get { return _layers.Where(t => t != Inputs && t != Outputs); }
        }

        public ILayer OutputLayer
        {
            get { return Outputs; }
        }

        public string Name
        {
            get { return "Birdy Neural Network"; }
        }

        public string Description
        {
            get { return "TADAM!"; }
        }

        public string Author
        {
            get { return "SHMILONELY"; }
        }

        #endregion
    }
}