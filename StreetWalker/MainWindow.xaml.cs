using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
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

        private Mapsui.Geometries.Point NodeToPoint(string nodeId)
        {
            string url = "https://overpass.kumi.systems/api/interpreter";
            string bodyFormat =
                "[out:json];" +
                "node({0});" +
                "out;";
            string body = String.Format(bodyFormat, nodeId);

            var handler = new WinHttpHandler();
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Content = new StringContent(body, Encoding.UTF8)
            };

            var responseTask = client.SendAsync(request).ConfigureAwait(false);
            var response = responseTask.GetAwaiter().GetResult().EnsureSuccessStatusCode();
            response.EnsureSuccessStatusCode();

            var responseBodyTask = response.Content.ReadAsStringAsync().ConfigureAwait(false);
            var responseBody = responseBodyTask.GetAwaiter().GetResult();
            NodeResponse responseJson = JsonConvert.DeserializeObject<NodeResponse>(responseBody);
            Node node = responseJson.elements[0];

            return SphericalMercator.FromLonLat(node.lon, node.lat);
        }

        private string GetRandomElement(List<string> list)
        {
            int randomIndex = random.Next(list.Count);
            return list[randomIndex];
        }

        private async Task Walk()
        {
            while(true)
            {
                await WalkOnce();
                await Task.Delay(1000);
            }
        }

        private async Task WalkOnce()
        {
            string url = "https://overpass.kumi.systems/api/interpreter";
            string bodyFormat =
                "[out:json];" +
                "node({0});" +
                "way(bn);" +
                "out;";
            string body = String.Format(bodyFormat, currentNodeId);

            var handler = new WinHttpHandler();
            var client = new HttpClient(handler);

            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri(url),
                Content = new StringContent(body, Encoding.UTF8)
            };

            var response = await client.SendAsync(request).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync();

            WalkerResponse responseJson = JsonConvert.DeserializeObject<WalkerResponse>(responseBody);
            List<string> adjacentNodes = FindAdjacentNodes(responseJson, currentNodeId);
            currentNodeId = GetRandomElement(adjacentNodes);
            Mapsui.Geometries.Point currentNodePoint = NodeToPoint(currentNodeId);

            // Mapsui.Geometries.Point point = SphericalMercator.FromLonLat(23.3155870, 42.6987510);
            MyMapControl.Navigator.NavigateTo(currentNodePoint, 1.0);
        }
    }
}
