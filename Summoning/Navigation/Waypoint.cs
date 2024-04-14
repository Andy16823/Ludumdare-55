using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summoning.Navigation
{
    public class Waypoint
    {
        public int column;
        public int row;
        public Waypoint parent;
        public int hCost;
        public int gCost;
        public int fCost;

        public void CopyValues(Waypoint waypoint)
        {
            //this.parent = waypoint.parent;
            this.hCost = waypoint.hCost;
            this.gCost = waypoint.gCost;
            this.fCost = waypoint.fCost;
        }
    }
}
