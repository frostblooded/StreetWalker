using Mapsui.Projection;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace StreetWalker
{
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

        public static WalkerResponse FromJson(string json)
        {
            WalkerResponse walkerResponse = JsonConvert.DeserializeObject<WalkerResponse>(json);
            walkerResponse.ways = walkerResponse.elements.FindAll(x => x.type == "way");
            walkerResponse.nodes = walkerResponse.elements.FindAll(x => x.type == "node");
            return walkerResponse;
        }
    }
}
