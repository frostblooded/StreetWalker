using System.Collections.Generic;
using System.IO;
using Mapsui.Layers;
using Mapsui.Providers;
using Mapsui.Styles;

public class WalkerPin
{
    public string Name => "1 Points";
    public string Category => "Geometries";

    public static MemoryLayer CreateWalkerLayer(Mapsui.Geometries.Point currentNodePosition)
    {
        return new MemoryLayer
        {
            Name = "Walker",
            IsMapInfoLayer = true,
            DataSource = new MemoryProvider(CreateFeature(currentNodePosition)),
            Style = CreateBitmapStyle()
        };
    }

    private static IEnumerable<IFeature> CreateFeature(Mapsui.Geometries.Point currentNodePosition)
    {
        List<Feature> res = new List<Feature>();
        Feature feature = new Feature();
        feature.Geometry = currentNodePosition;
        feature["name"] = "Current walker position";
        res.Add(feature);
        return res;
    }
    private static SymbolStyle CreateBitmapStyle()
    {
        var path = "../../../images/pin.png";
        var bitmapId = GetBitmapIdForEmbeddedResource(path);
        var bitmapHeight = 50; // To set the offset correct we need to know the bitmap height
        var scale = 0.5;
        return new SymbolStyle { BitmapId = bitmapId, SymbolScale = scale, SymbolOffset = new Offset(0, bitmapHeight * scale * 0.5) };
    }

    private static int GetBitmapIdForEmbeddedResource(string imagePath)
    {
        FileStream fileStream = File.OpenRead(imagePath);
        return BitmapRegistry.Instance.Register(fileStream);
    }
}
