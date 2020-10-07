using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreetWalker
{
    class NodeChooser
    {
        private Random random;

        public TilesHolder TilesHolder { get; set; }

        public NodeChooser()
        {
            random = new Random();
            TilesHolder = new TilesHolder();
        }

        public async Task LoadTiles()
        {
            await TilesHolder.LoadTile(42.69f, 23.32f, 42.7f, 23.33f).ConfigureAwait(false);
        }

        public  List<string> FindAdjacentNodes(string currentNodeId)
        {
            List<string> res = new List<string>();

            foreach (KeyValuePair<string, Way> way in TilesHolder.ways)
            {
                List<string> nodes = way.Value.nodes;
                int currentNodeIndex = nodes.FindIndex(x => x == currentNodeId);

                // If node doesn't appear in this way, skip it
                if(currentNodeIndex < 0)
                {
                    continue;
                }

                if (currentNodeIndex > 0)
                {
                    res.Add(nodes[currentNodeIndex - 1]);
                }

                if (currentNodeIndex < nodes.Count - 1)
                {
                    res.Add(nodes[currentNodeIndex + 1]);
                }
            }

            return res;
        }

        private T ChooseRandomElement<T>(IList<T> list)
        {
            int randomIndex = random.Next(list.Count);
            return list[randomIndex];
        }

        private bool ProbabilitiesListIsValid(List<double> probabilities)
        {
            double sum = probabilities.Aggregate(0.0, (s, x) => s + x);

            // Make sure probabilities sum up to 1.
            return Math.Abs(1 - sum) < double.Epsilon;
        }

        private string ChooseWithProbabilities(List<string> list, List<double> probabilities)
        {
            bool validProbabilities = ProbabilitiesListIsValid(probabilities);

            if(list.Count == 0)
            {
                throw new Exception("List is empty");
            }

            if(!validProbabilities)
            {
                Console.Error.WriteLine("Probabilities {0} don't sum up to 1.", string.Join(",", probabilities));
            }

            if(list.Count != probabilities.Count)
            {
                throw new Exception("List and probabilities list should be with same length.");
            }

            double randValue = random.NextDouble();

            for (int i = 0; i < list.Count; i++)
            {
                if(randValue <= probabilities[i])
                {
                    return list[i];
                }

                randValue -= probabilities[i];
            }

            return list[list.Count - 1];
        }

        public string ChooseNeighbor(Walker walker, List<string> neighbors)
        {
            List<double> probabilities = new List<double>(neighbors.Count);
            double probabilitiesSum = 0;

            foreach(string neighbor in neighbors)
            {
                double neighborPathPos = walker.Path.FindIndex(x => x == neighbor);
                double probability;

                // If isn't in path
                if(neighborPathPos == -1)
                {
                    probability = Walker.PATH_MAX_LENGTH;
                }
                // If it is in path
                else
                {
                    // probability = Walker.PATH_MAX_LENGTH / (Walker.PATH_MAX_LENGTH * 2 - neighborPathPos);
                    probability = (neighborPathPos + 1) / Walker.PATH_MAX_LENGTH;
                }

                probabilities.Add(probability);
                probabilitiesSum += probability;
            }

            for(int i = 0; i < probabilities.Count; i++)
            {
                probabilities[i] /= probabilitiesSum;

                if(double.IsNaN(probabilities[i]))
                {
                    throw new Exception("Probability is NaN");
                }
            }

            string chosenNeighbor = ChooseWithProbabilities(neighbors, probabilities);
            double chosenNeighborProbability = probabilities[neighbors.FindIndex(x => x == chosenNeighbor)];

            Console.WriteLine("Next neighbor {0} chosen with probability {1}", chosenNeighbor, chosenNeighborProbability);

            return chosenNeighbor;
        }

        public string GetStartingNode()
        {
            List<Way> highways = TilesHolder.Highways;
            List<string> highwayNodes = highways.SelectMany(x => x.nodes).ToList();
            return ChooseRandomElement(highwayNodes);
        }

        public string GetNextNode(Walker walker)
        {
            List<string> adjacentNodes = FindAdjacentNodes(walker.CurrentNodeId);
            return ChooseNeighbor(walker, adjacentNodes);
        }
    }
}
