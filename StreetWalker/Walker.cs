using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StreetWalker
{
    class Walker
    {
        private string currentNodeId;

        public const int PATH_MAX_LENGTH = 100;

        // Average human walking speed
        public const double WALK_SPEED_KM_PER_HOUR = 5;
        public const double WALK_SPEED_KM_PER_SECOND = WALK_SPEED_KM_PER_HOUR / 3600;

        public string CurrentNodeId {
            get {
                return currentNodeId;
            }
            set {
                currentNodeId = value;
                AddToPath(value);
            }
        }

        public List<string> Path { get; set; }

        public Walker()
        {
            Path = new List<string>();
        }

        private void AddToPath(string nodeId)
        {
            if(Path.Count >= PATH_MAX_LENGTH)
            {
                // If the path is getting too long, remove the oldest
                // visited node from the path, so that we can visit it again.
                Path.RemoveAt(0);
            }

            Path.Add(nodeId);
        }
    }
}
