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
        private TilesHolder tilesHolder;

        public NodeChooser()
        {
            random = new Random();
            networkingManager = new NetworkingManager();

            tilesHolder = new TilesHolder();
            tilesHolder.LoadTile(42.69f, 23.32f, 42.7f, 23.33f).ConfigureAwait(false).GetAwaiter().GetResult();
        }

        public  List<string> FindAdjacentNodes(WalkerResponse walkerResponse, string currentNodeId)
        {
            List<string> res = new List<string>();

            foreach (Element way in walkerResponse.ways)
            {
                int currentNodeIndex = way.nodes.FindIndex(x => x == currentNodeId);

                if (currentNodeIndex > 0)
                {
                    res.Add(way.nodes[currentNodeIndex - 1]);
                }

                if (currentNodeIndex < way.nodes.Count - 1)
                {
                    res.Add(way.nodes[currentNodeIndex + 1]);
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
            return ChooseRandomElement(tilesHolder.nodes.Keys.ToList());
        }
    }
}
