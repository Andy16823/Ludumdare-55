using Genesis.Core;
using Genesis.Math;
using Summoning.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summoning.Behaviors
{
    public class Minion : IGameBehavior
    {
        public Waypoint Start { get; set; }
        public int Next { get; set; }
        public List<Waypoint> Waypoints { get; set; }
        public Light2D Light { get; set; }

        private long lastMove = 0;

        private long moveIntervall = 1;

        public NavMesh NavMesh { get; set; }

        public Minion(NavMesh navMesh, Waypoint waypoint, Light2D lightSource)
        {
            this.NavMesh = navMesh;
            this.Start = waypoint;
            this.Waypoints = NavMesh.ToPath(waypoint);
            this.Next = 0;
            this.Light = lightSource;
        }

        public override void OnDestroy(Game game, GameElement parent)
        {
            
        }

        public override void OnInit(Game game, GameElement parent)
        {
            
        }

        public override void OnRender(Game game, GameElement parent)
        {
            
        }

        public override void OnUpdate(Game game, GameElement parent)
        {
            Light.Location = this.Parent.Location;
            var now = Utils.GetCurrentTimeMillis();
            if(now > lastMove + moveIntervall)
            {
                if(Waypoints.Count > this.Next)
                {
                    var waypoint = this.Waypoints[this.Next];
                    if (waypoint != null)
                    {
                        var gridCell = NavMesh.GetGridCell(waypoint.column, waypoint.row);
                        this.MoveTo(new Vec3(gridCell.x, gridCell.y), 1.2f);
                        this.lastMove = now;
                    }
                }
                else
                {
                    this.Parent.Enabled = false;
                }
            }
        }

        public void MoveTo(Vec3 target, float speed)
        {
            float dx = target.X - this.Parent.Location.X;
            float dy = target.Y - this.Parent.Location.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 0.5f)
            {
                float ratio = speed / distance;
                this.Parent.Location.X += dx * ratio;
                this.Parent.Location.Y += dy * ratio;
            }
            else
            {
                this.Next++;
            }
        }
    }
}
