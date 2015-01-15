using System;
using System.Collections.Generic;
using System.Linq;

namespace BirdyNetwork.Classes.Graph
{
    public class SimpleGraph
    {
        public List<Rib> Ribs { get; set; }

        public List<Node> Nodes { get; set; }

        public SimpleGraph()
        {
            Ribs = new List<Rib>();
            Nodes = new List<Node>();
        }

        public void ConnectNodes(Node start, Node end, double weight = 1)
        {
            var rib = new Rib(start, end, weight);
            start.Ribs.Add(rib);
            end.Ribs.Add(rib);
            Ribs.Add(rib);
        }

        public void ConnectNodes(int startId, int endId, double weight = 1)
        {
            var start = Nodes.FirstOrDefault(t => t.Id == startId);
            var end = Nodes.FirstOrDefault(t => t.Id == endId);
            ConnectNodes(start, end, weight);
        }
    }
}
