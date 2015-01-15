using System;
using System.Collections.Generic;
using BirdyNetwork.Classes.Graph;
using NeuralNetworkLibBase;

namespace BirdyNetwork.Classes.NeuralNetwork
{
    public class Layer : List<Node>, ILayer
    {
        public Layer()
            :base()
        {
        }

        public Layer(string weightConsequence)
            :this()
        {
            var nodes = weightConsequence.Split(' ');
            foreach (var node in nodes)
            {
                Add(new Node(double.Parse(node)));
            }
        }

        public Layer(int number)
            : this()
        {
            for (var i = 0; i < number; i++)
            {
                Add(new Node());
            }
        }

        #region Interface

        public IEnumerable<INode> Nodes
        {
            get { return this; }
        }

        #endregion

    }
}