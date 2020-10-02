using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BruTile.Predefined;
using Mapsui.Layers;
using Mapsui.Projection;
using Newtonsoft.Json;

namespace StreetWalker
{
    public partial class MainWindow : Window
    {
        public const int PATH_MAX_LENGTH = 100;
        public const string SERVER_URL = "https://overpass.kumi.systems/api/interpreter";

        public Mapsui.INavigator Navigator;

        private string currentNodeId;
        private Random random;
        private MemoryLayer pinLayer;
        private List<string> path;
        private HttpClient client;

        public MainWindow()
        {
            InitializeComponent();
            MyMapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));
            random = new Random();
            path = new List<string>();
            client = new HttpClient(new WinHttpHandler());

            SetStartingNode().GetAwaiter().GetResult();
            Walk();
        }

        class Element
        {
            public string type;
            public string id;
            public List<string> nodes;
            public float lat;
            public float lon;
        }

        class WalkerResponse
        {
            public float version;
            public List<Element> elements;
            public List<Element> nodes;
            public List<Element> ways;

            public Mapsui.Geometries.Point GetNodePoint(string nodeId)
            {
                Element node = nodes.Find(x => x.id == nodeId);
                return SphericalMercator.FromLonLat(node.lon, node.lat);
            }

            public static WalkerResponse FromJson(string json)
            {
                WalkerResponse walkerResponse = JsonConvert.DeserializeObject<WalkerResponse>(json);
                walkerResponse.ways = walkerResponse.elements.FindAll(x => x.type == "way");
                walkerResponse.nodes = walkerResponse.elements.FindAll(x => x.type == "node");
                return walkerResponse;
            }
        }

        private async Task<string> SetStartingNode()
        {           
            string bodyFormat =
                "[out:json];" +
                "node(42.69, 23.32, 42.7, 23.33);" +
                "out;";
            string body = String.Format(bodyFormat, currentNodeId);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(SERVER_URL),
                Content = new StringContent(body, Encoding.UTF8)
            };

            Console.WriteLine("Getting Sofia nodes...");
            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            var responseBody = await response.Content.ReadAsStringAsync();

            Console.WriteLine("Parsing Sofia nodes...");
            WalkerResponse walkerResponse = WalkerResponse.FromJson(responseBody);

            if(walkerResponse.elements.Count == 0)
            {
                Console.WriteLine("No elements retrieved");
                return "";
            }

            Element randomNode = ChooseRandomElement(walkerResponse.nodes);
            Console.WriteLine("Starting node chosen: {0}", currentNodeId);
            SetCurrentNode(walkerResponse, randomNode.id);

            return randomNode.id;
        }

        private List<string> FindAdjacentNodes(WalkerResponse walkerResponse, string currentNodeId)
        {
            List<string> res = new List<string>();

            foreach(Element way in walkerResponse.ways)
            {
                int currentNodeIndex = way.nodes.FindIndex(x => x == currentNodeId);
                
                if(currentNodeIndex > 0)
                {
                    res.Add(way.nodes[currentNodeIndex - 1]);
                }

                if(currentNodeIndex < way.nodes.Count - 1)
                {
                    res.Add(way.nodes[currentNodeIndex + 1]);
                }
            }

            return res;
        }

        private async Task<WalkerResponse> GetNodeWays()
        {
            string bodyFormat =
                "[out:json];" +
                "node({0});" +
                "way(bn);" +
                "(._; node(w););" +
                "out;";
            string body = String.Format(bodyFormat, currentNodeId);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(SERVER_URL),
                Content = new StringContent(body, Encoding.UTF8)
            };

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            WalkerResponse walkerResponse = JsonConvert.DeserializeObject<WalkerResponse>(responseBody);
            walkerResponse.ways = walkerResponse.elements.FindAll(x => x.type == "way");
            walkerResponse.nodes = walkerResponse.elements.FindAll(x => x.type == "node");
            return walkerResponse;
        }

        private T ChooseRandomElement<T>(List<T> list)
        {
            int randomIndex = random.Next(list.Count);
            return list[randomIndex];
        }

        private string ChooseNeighbor(List<string> neighbors)
        {
            List<string> usableNeighbors = new List<string>(neighbors);
            usableNeighbors.RemoveAll(x => path.Contains(x));

            if(usableNeighbors.Count == 0)
            {
                Console.WriteLine("No usable neighbors. Returning a random neighbor.");
                return ChooseRandomElement(neighbors);
            }

            return ChooseRandomElement(usableNeighbors);
        }

        private async Task Walk()
        {
            while(true)
            {
                await WalkOnce();
                await Task.Delay(1000);
            }
        }

        private void UpdatePinLayer(Mapsui.Geometries.Point currentNodePoint)
        {
            LayerCollection layers = MyMapControl.Map.Layers;

            if(layers.Count > 1)
            {
                layers.Remove(pinLayer);
            }

            pinLayer = WalkerPin.CreateWalkerLayer(currentNodePoint);
            layers.Add(pinLayer);
        }

        private void AddToPath(string nodeId)
        {
            if(path.Count >= PATH_MAX_LENGTH)
            {
                // If the path is getting too long, remove the oldest
                // visited node from the path, so that we can visit it again.
                path.RemoveAt(0);
            }

            path.Add(nodeId);
        }

        private void SetCurrentNode(WalkerResponse walkerResponse, string nodeId)
        {
            Mapsui.Geometries.Point nodePoint = walkerResponse.GetNodePoint(nodeId);
            MyMapControl.Navigator.NavigateTo(nodePoint, 1.0);
            UpdatePinLayer(nodePoint);
            AddToPath(nodeId);
            currentNodeId = nodeId;

            Console.WriteLine("Point is now {0} at {1}", nodeId, nodePoint.ToString());
        }

        private async Task WalkOnce()
        {
            DateTime start = DateTime.Now;

            WalkerResponse walkerResponse = await GetNodeWays();
            List<string> adjacentNodes = FindAdjacentNodes(walkerResponse, currentNodeId);
            string neighborId = ChooseNeighbor(adjacentNodes);
            SetCurrentNode(walkerResponse, neighborId);
            
            Console.WriteLine("Request took {0}", (DateTime.Now - start).ToString());
        }
    }
}
