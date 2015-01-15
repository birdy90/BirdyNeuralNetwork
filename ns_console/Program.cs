using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using BirdyNetwork;
using NeuralNetworkLibBase;

namespace ns_console
{
    internal class Program
    {
        private static NeuralNetwork _neuralNetwork;

        private static ConstructionParameters _networkParameters;

        private static ConsoleKey Menu()
        {
            Console.WriteLine("==== Menu: ====");
            Console.WriteLine("P. Open predefined configuration");
            Console.WriteLine("O. Open from file and teach");
            Console.WriteLine("I. Import saved network");
            if (_neuralNetwork != null)
            {
                Console.WriteLine("T. Teach");
                Console.WriteLine("C. Try to calculate");
                Console.WriteLine("S. Save network");
                Console.WriteLine("R. Reinitialize and teach");
            }
            Console.WriteLine("Q. Exit");
            var key = Console.ReadKey().Key;
            Console.Clear();
            return key;
        }

        private static void Main(string[] args)
        {
            while (true)
            {
                var key = Menu();

                if (_neuralNetwork != null)
                {
                    switch (key)
                    {
                        case ConsoleKey.T:
                            TeachNetwork(_neuralNetwork);
                            Console.WriteLine("Teaching done");
                            break;
                        case ConsoleKey.C:
                            var done = false;
                            while (!done)
                            {
                                Console.Write("Write {0} numbers divided with whitespaces: ",
                                    _neuralNetwork.Inputs.Count);
                                var s = Console.ReadLine();
                                try
                                {
                                    done = true;
                                    var l = s.Split(' ').Select(t => double.Parse(t)).ToList();
                                    Console.Write("a: {0}, b: {1} - required: {2} - we get: ", l[0], l[1], l[0] + l[1]);
                                    _neuralNetwork.CountNetwork(l);
                                    _neuralNetwork.PrintAnswers();
                                }
                                catch (Exception ex)
                                {
                                    Console.Clear();
                                    Console.WriteLine("Some error occured");
                                    done = false;
                                }
                            }
                            break;
                        case ConsoleKey.S:
                            var file = File.Create("nn_result_weights.txt");
                            var writer = new StreamWriter(file);
                            writer.WriteLine("# network configuration");
                            _neuralNetwork.PrintConfiguration(writer);
                            writer.WriteLine("# network weights");
                            _neuralNetwork.PrintWeights(writer);
                            Console.WriteLine("Saving done");
                            writer.Close();
                            break;
                        case ConsoleKey.R:
                            _neuralNetwork = new NeuralNetwork();
                            _neuralNetwork.Create(_networkParameters);
                            TeachNetwork(_neuralNetwork);
                            break;
                    }
                }

                var filename = "";
                switch (key)
                {
                    case ConsoleKey.P:
                        ConfigureFromFile("nn_configuration.txt");
                        break;
                    case ConsoleKey.O:
                        Console.Clear();
                        Console.WriteLine(
                            "File must be near to executable and contain 3 lines of integers divided with whitespace (number of inputs, number of nodes for each hidden layer and number of outputs)");
                        Console.Write("Write the file name with configuration: ");
                        filename = Console.ReadLine();
                        ConfigureFromFile(filename);
                        break;
                    case ConsoleKey.I:
                        Console.Clear();
                        Console.WriteLine(
                            "File must be a previously saved configuration");
                        Console.Write("Write the file name with configuration: ");
                        filename = Console.ReadLine();
                        ReadSavedConfiguration(filename);
                        break;
                    case ConsoleKey.Q:
                        return;
                }
            }
        }

        private static void ConfigureFromFile(string filename)
        {
            try
            {
                var file = File.ReadLines(filename).ToList();
                _networkParameters = new ConstructionParameters
                {
                    HiddenLayers = file[1].Split(' ').Select(t => int.Parse(t)).ToList(),
                    Inputs = int.Parse(file[0]),
                    Outputs = int.Parse(file[2])
                };
                _neuralNetwork = new NeuralNetwork();
                _neuralNetwork.Create(_networkParameters);
                Console.WriteLine("File read");
            }
            catch (Exception)
            {
                Console.Clear();
                Console.WriteLine("Error reading file (or file not exist)");
            }
        }

        private static void ReadSavedConfiguration(string filename)
        {
            if (File.Exists(filename))
            {
                FileStream file = File.Open(filename, FileMode.Open);
                StreamReader sr = new StreamReader(file);

                try
                {
                    var line = sr.ReadLine();
                    line = sr.ReadLine();
                    var lines = new List<string>();
                    do
                    {
                        lines.Add(line);
                        line = sr.ReadLine();
                    } while (line[0] != '#');

                    _networkParameters = new ConstructionParameters
                    {
                        HiddenLayers = lines[1].Split(' ').Select(int.Parse).ToList(),
                        Inputs = int.Parse(lines[0]),
                        Outputs = int.Parse(lines[2])
                    };
                    _neuralNetwork = new NeuralNetwork();
                    _neuralNetwork.Create(_networkParameters);
                    _neuralNetwork.ReadSavedConfiguration(sr);
                    Console.WriteLine("File read");
                }
                catch (Exception)
                {
                    Console.Clear();
                    Console.WriteLine("Error reading file");
                }

                if (sr != null) sr.Close();
            }
            else
            {
                Console.WriteLine("File not exists");
            }
        }

        private static void TeachNetwork(NeuralNetwork neuralNetwork)
        {
            var highLimit = 10;
            var repeatsNumber = 1000;
            var rand = new Random();
            for (var repeats = 0; repeats < repeatsNumber; repeats++)
            {
                Console.Clear();
                Console.WriteLine("Teaching... {0,4:F1}% passed iterations: {1}", repeats*100f/ repeatsNumber, neuralNetwork.SamplesPassed);
                var randI = rand.Next(1, highLimit);
                var randJ = rand.Next(1, highLimit);
                neuralNetwork.TeachNetwork(
                    new List<double> {randI, randJ}, 
                    new List<double> {Operation(randI, randJ)});
            }
            TryTestSamples(neuralNetwork);
        }

        private static void TryTestSamples(NeuralNetwork neuralNetwork)
        {
            var rand = new Random();
            for (var i = 0; i < 10; i++)
            {
                var l = new List<double>
                {
                    rand.Next(1, 10),
                    rand.Next(1, 10)
                };
                neuralNetwork.CountNetwork(l);
                Console.Write("a: {0}, b: {1} - required: {2} - we get: ", l[0], l[1], Operation(l[0], l[1]));
                neuralNetwork.PrintAnswers();
            }
        }

        private static double Operation(double a, double b)
        {
            return a + b;
        }
    }
}
