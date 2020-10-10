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
            await TilesHolder.LoadTile(new Tile(23.32f, 42.69f, 23.33f, 42.7f)).ConfigureAwait(false);
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

        private string ChooseWithPriority(List<string> list, List<double> priorities)
        {
            if(list.Count == 0)
            {
                throw new Exception("List is empty");
            }

            if(list.Count != priorities.Count)
            {
                throw new Exception("List and probabilities list should be with same length.");
            }

            double maxPriority = 0;
            List<int> maxPrioritiesIndices = new List<int>();

            for(int i = 0; i < priorities.Count; i++)
            {
                if(priorities[i] > maxPriority)
                {
                    maxPriority = priorities[i];
                    maxPrioritiesIndices.Clear();
                    maxPrioritiesIndices.Add(i);
                }
                else if(priorities[i] == maxPriority)
                {
                    maxPrioritiesIndices.Add(i);
                }
            }

            int randomMaxPriorityIndex = ChooseRandomElement(maxPrioritiesIndices);
            return list[randomMaxPriorityIndex];
        }

        public string ChooseNeighbor(Walker walker, List<string> neighbors)
        {
            List<double> priorities = new List<double>(neighbors.Count);

            foreach(string neighbor in neighbors)
            {
                double neighborPathPos = walker.Path.FindLastIndex(x => x == neighbor);
                
                // Even works when the neighbor isn't in the path, because neighborPathPos
                // will be -1 and still work correctly.
                double probability = Walker.PATH_MAX_LENGTH / (neighborPathPos + 2);

                priorities.Add(probability);
            }

            string chosenNeighbor = ChooseWithPriority(neighbors, priorities);
            double chosenNeighborPriority = priorities[neighbors.FindIndex(x => x == chosenNeighbor)];

            Console.WriteLine("Next neighbor {0} chosen with priority {1}", chosenNeighbor, chosenNeighborPriority);

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
