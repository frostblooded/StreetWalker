using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetWalker
{
    class NodeChooser
    {
        private Random random;
        private NetworkingManager networkingManager;

        public TilesHolder TilesHolder { get; set; }

        public NodeChooser()
        {
            random = new Random();
            networkingManager = new NetworkingManager();
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

        public string ChooseNeighbor(Walker walker, List<string> neighbors)
        {
            List<string> usableNeighbors = new List<string>(neighbors);
            usableNeighbors.RemoveAll(x => walker.Path.Contains(x));

            if (usableNeighbors.Count == 0)
            {
                Console.WriteLine("No usable neighbors. Returning a random neighbor.");
                return ChooseRandomElement(neighbors);
            }

            return ChooseRandomElement(usableNeighbors);
        }

        public string GetStartingNode()
        {
            return ChooseRandomElement(TilesHolder.nodes.Keys.ToList());
        }

        public string GetNextNode(Walker walker)
        {
            List<string> adjacentNodes = FindAdjacentNodes(walker.CurrentNodeId);
            return ChooseNeighbor(walker, adjacentNodes);
        }
    }
}
