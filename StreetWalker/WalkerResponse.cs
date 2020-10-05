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

        public static WalkerResponse FromJson(string json)
        {
            return JsonConvert.DeserializeObject<WalkerResponse>(json);
        }
    }
}
