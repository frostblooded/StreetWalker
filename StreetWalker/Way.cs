using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetWalker
{
    class Way
    {
        public string id;
        public List<string> nodes;
        public Tags tags;

        public Way(Element element)
        {
            id = element.id;
            nodes = new List<string>(element.nodes);
            tags = element.tags;
        }
    }
}
