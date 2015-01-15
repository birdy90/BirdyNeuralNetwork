using System;
using NeuralNetworkLibBase;

namespace BirdyNetwork.Classes.Graph
{
    public class Rib : ObjectWithId, IConnection
    {
        public double Weight { get; set; }

        public double WeightError;

        public Node Start;

        public Node End;

        public Rib(Node start, Node end, double weight = 1)
        {
            Weight = weight;
            Start = start;
            End = end;
        }

        #region Interface

        public override string ToString()
        {
            return string.Format("rib: {0} (ns: {1}, ne: {2}) w: {3}", Id, Start.Id, End.Id, Weight);
        }

        public INode StartNode
        {
            get { return Start; }
        }

        #endregion

    }
}
