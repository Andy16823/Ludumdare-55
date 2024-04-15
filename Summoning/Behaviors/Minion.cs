using Genesis.Core;
using Genesis.Core.GameElements;
using Genesis.Math;
using Summoning.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.TextBox;

namespace Summoning.Behaviors
{
    /// <summary>
    /// Enum for the minion type
    /// </summary>
    public enum MinionType
    {
        PlayerMinion,
        EnemyMinion
    }

    /// <summary>
    /// Controls the navigation and logic behind the minion
    /// </summary>
    public class Minion : IGameBehavior
    {
        /// <summary>
        /// Start waypoint
        /// </summary>
        public Waypoint Start { get; set; }

        /// <summary>
        /// Index for the next waypoint
        /// </summary>
        public int Next { get; set; }

        /// <summary>
        /// List of all waypoints
        /// </summary>
        public List<Waypoint> Waypoints { get; set; }

        /// <summary>
        /// Minion light
        /// </summary>
        public Light2D Light { get; set; }

        /// <summary>
        /// Last movement time
        /// </summary>
        private long lastMove = 0;

        /// <summary>
        /// Movement intervall in ms
        /// </summary>
        private long moveIntervall = 1;

        /// <summary>
        /// Health of the Minion
        /// </summary>
        public int Health { get; set; } = 100;

        /// <summary>
        /// Navigation mesh
        /// </summary>
        public NavMesh NavMesh { get; set; }

        /// <summary>
        /// Type of the minion.
        /// </summary>
        public MinionType MinionType { get; set; }

        /// <summary>
        /// Time when last damage was taken.
        /// </summary>
        public long LastDamageTaken { get; set; }

        /// <summary>
        /// Tower that summoned this minion.
        /// </summary>
        public Tower Summoner { get; set; }

        /// <summary>
        /// Destination tower.
        /// </summary>
        public Tower Destination { get; set; }

        /// <summary>
        /// Damage dealt by the minion to towers.
        /// </summary>
        public int TowerDamage { get; set; } = 25;

        /// <summary>
        /// Power gained by killing an enemy.
        /// </summary>
        public int KillPowerGain { get; set; } = 10;

        /// <summary>
        /// Power gained by hitting an enemy tower.
        /// </summary>
        public int TowerHitPowerGain { get; set; } = 25;

        /// <summary>
        /// Creates a new minion behavior.
        /// </summary>
        /// <param name="navMesh">Navigation mesh.</param>
        /// <param name="start">Starting tower.</param>
        /// <param name="destination">Destination tower.</param>
        /// <param name="lightSource">Light source for the minion.</param>
        public Minion(NavMesh navMesh, Tower start, Tower destination, Light2D lightSource)
        {
            this.CreateMinion(navMesh, start, destination, lightSource, MinionType.PlayerMinion);
        }

        /// <summary>
        /// Creates a new minion behavior.
        /// </summary>
        /// <param name="navMesh">Navigation mesh.</param>
        /// <param name="start">Starting tower.</param>
        /// <param name="destination">Destination tower.</param>
        /// <param name="lightSource">Light source for the minion.</param>
        /// <param name="type">Type of the minion.</param>
        public Minion(NavMesh navMesh, Tower start, Tower destination, Light2D lightSource, MinionType type)
        {
            this.CreateMinion(navMesh, start, destination, lightSource, type);
        }

        /// <summary>
        /// Initializes the minion with the provided parameters and starts finding the path asynchronously.
        /// </summary>
        /// <param name="navMesh">The navigation mesh for pathfinding.</param>
        /// <param name="start">The starting tower from which the minion will spawn.</param>
        /// <param name="destination">The destination tower to which the minion will navigate.</param>
        /// <param name="lightSource">The light source associated with the minion.</param>
        /// <param name="type">The type of the minion (player or enemy).</param>
        private void CreateMinion(NavMesh navMesh, Tower start, Tower destination, Light2D lightSource, MinionType type)
        {
            this.Summoner = start;
            this.Destination = destination;
            this.MinionType = type;
            this.NavMesh = navMesh;

            //find the path to the destination asynchron.
            NavMesh.FindPathAsync(start.Column, start.Row, destination.Column, destination.Row, (waypoint) =>
            {
                Waypoints = NavMesh.ToPath(waypoint);
                this.Start = waypoint;
            });

            this.Start = null;
            this.Next = 0;
            this.Light = lightSource;
        }

        /// <summary>
        /// Called when the minion is destroyed.
        /// </summary>
        public override void OnDestroy(Game game, GameElement parent)
        {
            
        }

        /// <summary>
        /// Called when the minion is initialized.
        /// </summary>
        public override void OnInit(Game game, GameElement parent)
        {
            
        }

        /// <summary>
        /// Called for rendering the minion.
        /// </summary>
        public override void OnRender(Game game, GameElement parent)
        {
            
        }

        /// <summary>
        /// Called on each frame update.
        /// </summary>
        public override void OnUpdate(Game game, GameElement parent)
        {
            // If a path is found, handle the minion navigation.
            if (this.Start != null)
            {
                // update the light source from the minion.
                Light.Location = this.Parent.Location;
                var now = Utils.GetCurrentTimeMillis();

                // Move the sprite.
                if (now > lastMove + moveIntervall)
                {
                    // Check if the sprite is not at the destination.
                    if (Waypoints.Count > this.Next)
                    {
                        var waypoint = this.Waypoints[this.Next];
                        if (waypoint != null)
                        {
                            var gridCell = NavMesh.GetGridCell(waypoint.column, waypoint.row);
                            //this.MoveTo(new Vec3(gridCell.x, gridCell.y), 1.2f);
                            this.MoveTo(new Vec3(gridCell.x, gridCell.y), 1.5f);
                            this.HandleMinionCollision(game);
                            this.lastMove = now;
                        }
                    }
                    else
                    {
                        this.AttackEnemy(game);
                        RemoveMinion(game);
                    }
                }
            } 
        }

        /// <summary>
        /// Moves the minion.
        /// </summary>
        /// <param name="target">Target position.</param>
        /// <param name="speed">Movement speed.</param>
        public void MoveTo(Vec3 target, float speed)
        {
            float dx = target.X - this.Parent.Location.X;
            float dy = target.Y - this.Parent.Location.Y;
            float distance = (float)Math.Sqrt(dx * dx + dy * dy);

            if (distance > 1f)
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

        /// <summary>
        /// Attacks the enemy tower and gains resources.
        /// </summary>
        private void AttackEnemy(Game game)
        {
            if(game.SelectedScene.GetType() == typeof(Map))
            {
                var map = (Map)game.SelectedScene;
                var playerEntity = map.GetElement("TowerLayer", "PlayerTower");
                if (playerEntity != null)
                {
                    var playerTowerBehavior = (PlayerTower) playerEntity.GetBehavior<PlayerTower>();
                    if (playerTowerBehavior != null)
                    {
                        switch (MinionType)
                        {
                            case MinionType.PlayerMinion:
                                Summoner.GainPowerExt(this.TowerHitPowerGain, false);
                                Destination.ReciveDamage(TowerDamage);
                                break;
                            case MinionType.EnemyMinion:
                                Summoner.GainPowerExt(this.TowerHitPowerGain, false);
                                Destination.ReciveDamage(TowerDamage);
                                break;
                            default:
                                break;
                        }
                        
                    }
                }
            }
        }

        /// <summary>
        /// Checks for collisions between the minions.
        /// </summary>
        /// <param name="game">The current game.</param>
        private void HandleMinionCollision(Game game)
        {
            var parent = (Sprite)this.Parent;

            if (game.SelectedScene.GetType() == typeof(Map))
            {
                var map = (Map)game.SelectedScene;
                var layer = map.GetLayer("MinionLayer");
                foreach (var item in layer.Elements)
                {
                    if (item != this.Parent)
                    {
                        var minion = (Sprite)item;
                        var minionBehavior = (Minion) item.GetBehavior<Minion>();
                        if(minionBehavior != null)
                        {
                            if (minionBehavior.MinionType != this.MinionType)
                            {
                                if (parent.GetBounds2D().Intersects(minion.GetBounds2D()))
                                {
                                    Console.WriteLine("Intersects");

                                    if (this.ReciveDamage(100, minionBehavior.KillPowerGain))
                                    {
                                        this.RemoveMinion(game);
                                    }
                                    if(minionBehavior.ReciveDamage(100, this.KillPowerGain))
                                    {
                                        minionBehavior.RemoveMinion(game);
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Deals damage to the minion.
        /// </summary>
        /// <param name="damage">Amount of damage to deal.</param>
        /// <param name="powerGain">Power gained on successful damage.</param>
        /// <returns>True if the minion is destroyed, false otherwise.</returns>
        public bool ReciveDamage(int damage, int powerGain)
        {
            var now = Utils.GetCurrentTimeMillis();
            if(now > LastDamageTaken + 1000)
            {
                this.Health -= damage;
                LastDamageTaken = now;  
                if (this.Health <= 0)
                {
                    this.Health = 0;
                    this.Destination.GainPowerExt(powerGain, false);
                    Console.WriteLine(this.Destination.Parent.Name + " gain " + powerGain + " power");
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Removes the minion from the game.
        /// </summary>
        /// <param name="game">The current game.</param>
        public void RemoveMinion(Game game)
        {
            var scene2D = (Scene2D)this.Parent.Scene;
            scene2D.RemoveLight(game, Light);
            this.Parent.Enabled = false;
        }
    }
}
