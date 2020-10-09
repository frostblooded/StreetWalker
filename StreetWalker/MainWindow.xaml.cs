using System;
using System.Threading.Tasks;
using System.Windows;
using BruTile.Predefined;
using Mapsui.Layers;

namespace StreetWalker
{
    public partial class MainWindow : Window
    {
        private MemoryLayer pinLayer;
        private Walker walker;
        private NodeChooser nodeChooser;

        public MainWindow()
        {
            InitializeComponent();

            MyMapControl.Map.Layers.Add(new TileLayer(KnownTileSources.Create()));
            walker = new Walker();
            nodeChooser = new NodeChooser();

            _ = InitMap().ConfigureAwait(false);
        }

        private async Task InitMap()
        {
            await nodeChooser.LoadTiles().ConfigureAwait(false);
            SetStartingNode();
            _ = Walk().ConfigureAwait(false);
        }

        private void SetStartingNode()
        {
            string startingNodeId = nodeChooser.GetStartingNode();
            SetCurrentNode(startingNodeId);
            Console.WriteLine("Starting node chosen: {0}", walker.CurrentNodeId);
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

        private async Task SetCurrentNodeDelayed(string nodeId)
        {
            Mapsui.Geometries.Point nodePoint = nodeChooser.TilesHolder.GetNodePoint(nodeId);
            Mapsui.Geometries.Point currentNodePoint = nodeChooser.TilesHolder.GetNodePoint(walker.CurrentNodeId);

            double distanceKms = nodeChooser.TilesHolder.GetGeoDistance(currentNodePoint, nodePoint);
            double walkTimeSeconds = distanceKms / Walker.WALK_SPEED_KM_PER_SECOND;
            int walkTimeMilliseconds = (int)(walkTimeSeconds * 1000);

            Console.WriteLine("Walking to new point {0} in {1} seconds", nodePoint, walkTimeSeconds);
            await Task.Delay(walkTimeMilliseconds);

            SetCurrentNode(nodeId);
        }

        private void SetCurrentNode(string nodeId)
        {
            Mapsui.Geometries.Point nodePoint = nodeChooser.TilesHolder.GetNodePoint(nodeId);
            MyMapControl.Navigator.NavigateTo(nodePoint, 1);
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
            string neighborId = nodeChooser.GetNextNode(walker);
            Mapsui.Geometries.Point nodePoint = nodeChooser.TilesHolder.GetNodePoint(neighborId);
            await SetCurrentNodeDelayed(neighborId);
            Tile neighborTile = nodeChooser.TilesHolder.GetNodeTile(neighborId);
            await nodeChooser.TilesHolder.LoadAdjacentTiles(neighborTile);
        }
    }
}
