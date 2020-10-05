using Mapsui.Projection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StreetWalker
{
    class Node
    {
        public string id;
        public float lat;
        public float lon;

        public Node(Element element)
        {
            id = element.id;
            lat = element.lat;
            lon = element.lon;
        }
    }

    class Way
    {
        public string id;
        public List<string> nodes;

        public Way(Element element)
        {
            id = element.id;
            nodes = new List<string>(element.nodes);
        }
    }

    class TilesHolder
    {
        public const float TILE_DIFFERENCE = 0.01f;

        private NetworkingManager networkingManager;
        public Dictionary<string, Node> nodes;
        public Dictionary<string, Way> ways;

        public TilesHolder()
        {
            networkingManager = new NetworkingManager();
            nodes = new Dictionary<string, Node>();
            ways = new Dictionary<string, Way>();
        }

        public async Task LoadTile(float lon1, float lat1, float lon2, float lat2)
        {
            Console.WriteLine("Loading tile {0}, {1}, {2}, {3}", lon1, lat1, lon2, lat2);

            string bodyFormat =
                "[out:json];" +
                "node({0}, {1}, {2}, {3});" +
                "way(bn);" +
                "(._; node(w););" +
                "out;";
            string body = string.Format(bodyFormat, lon1, lat1, lon2, lat2);
            WalkerResponse walkerResponse = await networkingManager.MakeRequest(body).ConfigureAwait(false);

            Console.WriteLine("Processing tile {0}, {1}, {2}, {3}", lon1, lat1, lon2, lat2);

            ProcessTileResponse(walkerResponse);

            Console.WriteLine("Done loading tile {0}, {1}, {2}, {3}", lon1, lat1, lon2, lat2);
        }

        public async Task LoadAdjacentTiles(float lon1, float lat1, float lon2, float lat2, bool includingCurrentTile = false)
        {
            List<Task> tasks = new List<Task>();

            tasks.Add(LoadTile(lon1 - TILE_DIFFERENCE, lat1 - TILE_DIFFERENCE, lon2 - TILE_DIFFERENCE, lat2 - TILE_DIFFERENCE));
            tasks.Add(LoadTile(lon1, lat1 - TILE_DIFFERENCE, lon2, lat2 - TILE_DIFFERENCE));
            tasks.Add(LoadTile(lon1 + TILE_DIFFERENCE, lat1 - TILE_DIFFERENCE, lon2 + TILE_DIFFERENCE, lat2 - TILE_DIFFERENCE));
            tasks.Add(LoadTile(lon1 - TILE_DIFFERENCE, lat1, lon2 - TILE_DIFFERENCE, lat2));

            if(includingCurrentTile)
            {
                tasks.Add(LoadTile(lon1, lat1, lon2, lat2));
            }

            tasks.Add(LoadTile(lon1 + TILE_DIFFERENCE, lat1, lon2 + TILE_DIFFERENCE, lat2));
            tasks.Add(LoadTile(lon1 - TILE_DIFFERENCE, lat1 + TILE_DIFFERENCE, lon2 - TILE_DIFFERENCE, lat2 + TILE_DIFFERENCE));
            tasks.Add(LoadTile(lon1, lat1 + TILE_DIFFERENCE, lon2, lat2 + TILE_DIFFERENCE));
            tasks.Add(LoadTile(lon1 + TILE_DIFFERENCE, lat1 + TILE_DIFFERENCE, lon2 + TILE_DIFFERENCE, lat2 + TILE_DIFFERENCE));

            Console.WriteLine("Loading {0} tiles...", tasks.Count);

            foreach(Task t in tasks)
            {
                await t.ConfigureAwait(false);
            }
        }

        private void ProcessTileResponse(WalkerResponse walkerResponse)
        {
            foreach (Element element in walkerResponse.elements)
            {
                if (element.type == "node")
                {
                    nodes.Add(element.id, new Node(element));
                }
                else if (element.type == "way")
                {
                    ways.Add(element.id, new Way(element));
                }
            }
        }

        public Mapsui.Geometries.Point GetNodePoint(string nodeId)
        {
            Node node = nodes[nodeId];
            return SphericalMercator.FromLonLat(node.lon, node.lat);
        }
    }
}
