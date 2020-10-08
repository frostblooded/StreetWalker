using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetWalker
{
    class Tile
    {
        public float Lon1 { get; set; }
        public float Lat1 { get; set; }
        public float Lon2 { get; set; }
        public float Lat2 { get; set; }

        public Tile(float lon1, float lat1, float lon2, float lat2)
        {
            Lon1 = lon1;
            Lat1 = lat1;
            Lon2 = lon2;
            Lat2 = lat2;
        }

        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", Lon1, Lat1, Lon2, Lat2);
        }

        public override bool Equals(object obj)
        {
            Tile other = obj as Tile;
            return Lon1 == other.Lon1
                && Lat1 == other.Lat1
                && Lon2 == other.Lon2
                && Lat2 == other.Lat2;
        }
    }
}
