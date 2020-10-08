using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetWalker
{
    class Node {
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
}
