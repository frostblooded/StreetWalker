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
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public Mapsui.INavigator Navigator;

        private string currentNodeId;
        private Random random;
        private MemoryLayer pinLayer;

        public MainWindow()
        {
            InitializeComponent();
            MyMapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));
            random = new Random();
            currentNodeId = "6260778311";
            Walk();
        }

        class Way
        {
            public string id;
            public List<string> nodes;
        }

        class WalkerResponse
        {
            public float version;
            public List<Way> elements;
        }

        class Node
        {
            public float lon;
            public float lat;
        }

        class NodeResponse
        {
            public List<Node> elements;
        }

        private List<string> FindAdjacentNodes(WalkerResponse walkerResponse, string currentNodeId)
        {
            List<string> res = new List<string>();

            foreach(Way way in walkerResponse.elements)
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

        private async Task<Mapsui.Geometries.Point> NodeToPoint(string nodeId, HttpClient client, string url)
        {
            string bodyFormat =
                "[out:json];" +
                "node({0});" +
                "out;";
            string body = String.Format(bodyFormat, nodeId);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Content = new StringContent(body, Encoding.UTF8)
            };

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            NodeResponse responseJson = JsonConvert.DeserializeObject<NodeResponse>(responseBody);
            Node node = responseJson.elements[0];

            return SphericalMercator.FromLonLat(node.lon, node.lat);
        }
        private async Task<WalkerResponse> GetNodeWays(HttpClient client, string url)
        {
            string bodyFormat =
                "[out:json];" +
                "node({0});" +
                "way(bn);" +
                "out;";
            string body = String.Format(bodyFormat, currentNodeId);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Content = new StringContent(body, Encoding.UTF8)
            };

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<WalkerResponse>(responseBody);
        }

        private string GetRandomElement(List<string> list)
        {
            int randomIndex = random.Next(list.Count);
            return list[randomIndex];
        }

        private async Task Walk()
        {
            var handler = new WinHttpHandler();
            var client = new HttpClient(handler);
            string url = "https://lz4.overpass-api.de/api/interpreter";

            while(true)
            {
                await WalkOnce(client, url);
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

        private async Task WalkOnce(HttpClient client, string url)
        {
            DateTime start = DateTime.Now;

            WalkerResponse walkerResponse = await GetNodeWays(client, url);
            List<string> adjacentNodes = FindAdjacentNodes(walkerResponse, currentNodeId);
            currentNodeId = GetRandomElement(adjacentNodes);
            Mapsui.Geometries.Point currentNodePoint = await NodeToPoint(currentNodeId, client, url);
            MyMapControl.Navigator.NavigateTo(currentNodePoint, 1.0);
            UpdatePinLayer(currentNodePoint);
            
            Console.WriteLine("Request took {0}", (DateTime.Now - start).ToString());
            Console.WriteLine("Point is now {0}", currentNodeId);
        }
    }
}
