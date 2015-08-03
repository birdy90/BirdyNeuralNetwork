using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BirdyNetwork.Classes
{
    public class ConstructionArgs
    {
        [Category("Basic")]
        [Description("Number of inputs of network")]
        public int Inputs { get; set; }

        [Category("Basic")]
        [Description("Number of outputs of network")]
        public int Outputs { get; set; }

        [Category("Basic")]
        [Description("Hidden layers defenition, each number represents number of neurons per layer")]
        public ObservableCollection<int> HiddenLayers { get; set; }

        public ConstructionArgs()
        {
            Inputs = 2;
            Outputs = 3;
            HiddenLayers = new ObservableCollection<int>() { 10, 10, 10 };
        }
    }
}
