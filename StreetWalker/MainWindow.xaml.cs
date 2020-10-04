using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using BruTile.Predefined;
using Mapsui.Layers;
using Newtonsoft.Json;

namespace StreetWalker
{
    public partial class MainWindow : Window
    {

        public Mapsui.INavigator Navigator;

        private MemoryLayer pinLayer;
        private Walker walker;
        private NetworkingManager networkingManager;
        private NodeChooser nodeChooser;
        private TilesHolder tilesHolder;

        public MainWindow()
        {
            InitializeComponent();

            MyMapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));
            networkingManager = new NetworkingManager();
            walker = new Walker();
            nodeChooser = new NodeChooser();

            _ = SetStartingNode().ConfigureAwait(false).GetAwaiter().GetResult();
            _ = Walk();
        }

        private async Task<string> SetStartingNode()
        {
            string startingNodeId = nodeChooser.GetStartingNode();
            SetCurrentNode(startingNodeId);
            Console.WriteLine("Starting node chosen: {0}", walker.CurrentNodeId);

            return startingNodeId;
        }

        private void UpdatePinLayer(Mapsui.Geometries.Point currentNodePoint)
        {
            LayerCollection layers = MyMapControl.Map.Layers;

            if (layers.Count > 1)
            {
                layers.Remove(pinLayer);
            }

            pinLayer = WalkerPin.CreateWalkerLayer(currentNodePoint);
            layers.Add(pinLayer);
        }

        private async Task<WalkerResponse> GetNodeWays()
        {
            string bodyFormat =
                "[out:json];" +
                "node({0});" +
                "way(bn);" +
                "(._; node(w););" +
                "out;";
            string body = string.Format(bodyFormat, walker.CurrentNodeId);
            return await networkingManager.MakeRequest(body);
        }

        private void SetCurrentNode(string nodeId)
        {
            Mapsui.Geometries.Point nodePoint = tilesHolder.GetNodePoint(nodeId);
            MyMapControl.Navigator.NavigateTo(nodePoint, 1.5);
            UpdatePinLayer(nodePoint);
            walker.CurrentNodeId = nodeId;

            Console.WriteLine("Point is now {0} at {1}", nodeId, nodePoint.ToString());
        }

        private async Task Walk()
        {
            while (true)
            {
                await WalkOnce();
                await Task.Delay(1000);
            }
        }

        private async Task WalkOnce()
        {
            DateTime start = DateTime.Now;

            WalkerResponse walkerResponse = await GetNodeWays();
            List<string> adjacentNodes = nodeChooser.FindAdjacentNodes(walkerResponse, walker.CurrentNodeId);
            string neighborId = nodeChooser.ChooseNeighbor(walker, adjacentNodes);
            SetCurrentNode(neighborId);

            Console.WriteLine("Request took {0}", (DateTime.Now - start).ToString());
        }
    }
}
