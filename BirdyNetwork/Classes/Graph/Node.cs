using System;
using System.Collections.Generic;
using System.Linq;
using NeuralNetworkLibBase;

namespace BirdyNetwork.Classes.Graph
{
    public class Node : ObjectWithId, INode
    {
        public double Value;

        public double ActivationValue;

        public double Weight;

        public List<Rib> Ribs;

        private List<Rib> _incomingRibs;

        public List<Rib> IncominRibs
        {
            get { return _incomingRibs; }
        }

        private List<Rib> _outcomingRibs;

        public List<Rib> OutcominRibs
        {
            get { return _outcomingRibs; }
        }

        public Node(double weight = 1)
        {
            Weight = weight;
            Ribs = new List<Rib>();
        }

        public override string ToString()
        {
            return "Node: " + Id + " Value: " + Value;
        }

        public List<Rib> GetIncomingRibs()
        {
            _incomingRibs = Ribs.Where(t => t.End == this).ToList();
            return _incomingRibs;
        }

        public List<Rib> GetOutcomingRibs()
        {
            _outcomingRibs = Ribs.Where(t => t.Start == this).ToList();
            return _outcomingRibs;
        }

        #region Interface

        public double Output
        {
            get { return ActivationValue; }
        }

        public IEnumerable<IConnection> Incoming
        {
            get { return IncominRibs; }
        }

        #endregion

    }
}