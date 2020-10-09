using Mapsui.Projection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StreetWalker
{
    class TilesHolder
    {
        public const float TILE_DIFFERENCE = 0.01f;

        private NetworkingManager networkingManager;
        private List<Tile> loadedTiles;

        public Dictionary<string, Node> nodes;
        public Dictionary<string, Way> ways;

        public List<Way> Highways
        {
            get
            {
                return ways.Where(x => x.Value.tags.highway != null)
                           .Select(x => x.Value)
                           .ToList();
            }
        }

        public TilesHolder()
        {
            networkingManager = new NetworkingManager();
            nodes = new Dictionary<string, Node>();
            ways = new Dictionary<string, Way>();
            loadedTiles = new List<Tile>();
        }

        public async Task LoadTile(Tile tile)
        {
            Console.WriteLine("Loading tile {0}", tile);

            string bodyFormat =
                "[out:json];" +
                "node({0}, {1}, {2}, {3});" +
                "way(bn);" +
                "(._; node(w););" +
                "out;";
            string body = string.Format(bodyFormat, tile.Lat1, tile.Lon1, tile.Lat2, tile.Lon2);
            WalkerResponse walkerResponse = await networkingManager.MakeRequest(body).ConfigureAwait(false);

            Console.WriteLine("Processing tile {0}", tile);

            ProcessTileResponse(walkerResponse);

            Console.WriteLine("Done loading tile {0}", tile);
            loadedTiles.Add(tile);
        }

        public async Task LoadAdjacentTiles(Tile tile)
        {
            List<Tile> tilesToLoad = new List<Tile>();

            tilesToLoad.Add(new Tile(tile.Lon1 - TILE_DIFFERENCE, tile.Lat1 - TILE_DIFFERENCE, tile.Lon2 - TILE_DIFFERENCE, tile.Lat2 - TILE_DIFFERENCE));
            tilesToLoad.Add(new Tile(tile.Lon1, tile.Lat1 - TILE_DIFFERENCE, tile.Lon2, tile.Lat2 - TILE_DIFFERENCE));
            tilesToLoad.Add(new Tile(tile.Lon1 + TILE_DIFFERENCE, tile.Lat1 - TILE_DIFFERENCE, tile.Lon2 + TILE_DIFFERENCE, tile.Lat2 - TILE_DIFFERENCE));
            tilesToLoad.Add(new Tile(tile.Lon1 - TILE_DIFFERENCE, tile.Lat1, tile.Lon2 - TILE_DIFFERENCE, tile.Lat2));
            tilesToLoad.Add(new Tile(tile.Lon1, tile.Lat1, tile.Lon2, tile.Lat2));
            tilesToLoad.Add(new Tile(tile.Lon1 + TILE_DIFFERENCE, tile.Lat1, tile.Lon2 + TILE_DIFFERENCE, tile.Lat2));
            tilesToLoad.Add(new Tile(tile.Lon1 - TILE_DIFFERENCE, tile.Lat1 + TILE_DIFFERENCE, tile.Lon2 - TILE_DIFFERENCE, tile.Lat2 + TILE_DIFFERENCE));
            tilesToLoad.Add(new Tile(tile.Lon1, tile.Lat1 + TILE_DIFFERENCE, tile.Lon2, tile.Lat2 + TILE_DIFFERENCE));
            tilesToLoad.Add(new Tile(tile.Lon1 + TILE_DIFFERENCE, tile.Lat1 + TILE_DIFFERENCE, tile.Lon2 + TILE_DIFFERENCE, tile.Lat2 + TILE_DIFFERENCE));

            List<Task> tasks = new List<Task>();

            foreach (Tile t in tilesToLoad)
            {
                if(loadedTiles.Contains(t))
                {
                    continue;
                }

                tasks.Add(LoadTile(t));
            }

            foreach(Task task in tasks)
            {
                await task.ConfigureAwait(false);
            }
        }

        private void ProcessTileResponse(WalkerResponse walkerResponse)
        {
            foreach (Element element in walkerResponse.elements)
            {
                if (element.type == "node")
                {
                    if(!nodes.ContainsKey(element.id))
                    {
                        nodes.Add(element.id, new Node(element));
                    }
                }
                else if (element.type == "way")
                {
                    if(!ways.ContainsKey(element.id))
                    {
                        ways.Add(element.id, new Way(element));
                    }
                }
            }
        }

        public Mapsui.Geometries.Point GetNodePoint(string nodeId)
        {
            Node node = nodes[nodeId];
            return SphericalMercator.FromLonLat(node.lon, node.lat);
        }

        public Tile GetNodeTile(string nodeId)
        {
            Node node = nodes[nodeId];

            float roundedLon1 = (float)Math.Round(node.lon, 2);
            float roundedLat1 = (float)Math.Round(node.lat, 2);
            float roundedLon2 = roundedLon1 + TILE_DIFFERENCE;
            float roundedLat2 = roundedLat1 + TILE_DIFFERENCE;

            return new Tile(roundedLon1, roundedLat1, roundedLon2, roundedLat2);
        }

        // Returns the distance in kilometers
        public double GetGeoDistance(Mapsui.Geometries.Point p1, Mapsui.Geometries.Point p2)
        {
            Mapsui.Geometries.Point lonLatPoint1 = SphericalMercator.ToLonLat(p1.X, p1.Y);
            Mapsui.Geometries.Point lonLatPoint2 = SphericalMercator.ToLonLat(p2.X, p2.Y);
            return GetGeoDistanceTo(lonLatPoint1.Y, lonLatPoint1.X, lonLatPoint2.Y, lonLatPoint2.X, 'K');
        }

        private double GetGeoDistanceTo(double lat1, double lon1, double lat2, double lon2, char unit = 'K')
        {
            double rlat1 = Math.PI * lat1 / 180;
            double rlat2 = Math.PI * lat2 / 180;
            double theta = lon1 - lon2;
            double rtheta = Math.PI * theta / 180;
            double dist =
                Math.Sin(rlat1) * Math.Sin(rlat2) + Math.Cos(rlat1) *
                Math.Cos(rlat2) * Math.Cos(rtheta);
            dist = Math.Acos(dist);
            dist = dist * 180 / Math.PI;
            dist = dist * 60 * 1.1515;

            // Convert to Kms
            return dist * 1.609344;
        }
    }
}
