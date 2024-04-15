using Genesis.Core;
using Genesis.Core.GameElements;
using Genesis.Math;
using Summoning.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summoning.Behaviors
{
    /// <summary>
    /// Represents a tower that summons enemy minions.
    /// </summary>
    public class EnemyTower : Tower
    {
        /// <summary>
        /// Gets or sets the navigation mesh for pathfinding.
        /// </summary>
        public NavMesh NavMesh { get; set; }

        /// <summary>
        /// Gets or sets the number of minions spawned by this tower.
        /// </summary>
        public long SpawnedMinons { get; set; }

        /// <summary>
        /// Gets or sets the target tower for the spawned minions.
        /// </summary>
        public Tower Target { get; set; }

        /// <summary>
        /// Flag to track whether the tower has been removed.
        /// </summary>
        private bool m_removed = false;

        /// <summary>
        /// Constructor for EnemyTower.
        /// </summary>
        /// <param name="mesh">The navigation mesh for pathfinding.</param>
        /// <param name="target">The target tower for the spawned minions.</param>
        /// <param name="column">The column position of the tower.</param>
        /// <param name="row">The row position of the tower.</param>
        public EnemyTower(NavMesh mesh, Tower target, int column, int row)
        {
            this.NavMesh = mesh;
            Target = target;
            Column = column;
            Row = row;
        }

        /// <summary>
        /// Called on each frame update.
        /// </summary>
        public override void OnUpdate(Game game, GameElement parent)
        {
            // If the tower is still alive
            if (this.Health > 0)
            {
                // Gain power over time
                GainPower();

                // If enough resources accumulated, summon a minion
                if (this.Ressources >= 30)
                {
                    SummonMinion(game);
                    this.Ressources -= 30;
                }
            }
            else // If the tower is destroyed
            {
                // Remove the tower from the map
                if (!m_removed)
                {
                    var map = (Map)game.SelectedScene;
                    map.Destinations.Remove(this);
                    if(map.SelectedTower == this)
                    {
                        map.SelectedTower = map.GetNextTower();
                    }
                }
            }
        }

        /// <summary>
        /// Summon a minion.
        /// </summary>
        private void SummonMinion(Game game)
        {
            if(game.SelectedScene.GetType() == typeof(Map))
            {
                var map = (Map)game.SelectedScene;
                var startPos = NavMesh.GetGridCell(this.Column, this.Row);

                // Create a light for the minion
                Light2D light = new Light2D("MinionLight", startPos.ToVec3(), new Vec3(250, 250), game.AssetManager.GetTexture("LightShape_3.png"));
                light.Init(game, game.RenderDevice);
                light.LightColor = Color.FromArgb(183, 85, 255);
                map.Lights.Add(light);

                // Create the minion sprite
                var minion = new Sprite(this.Parent.Name + "_Minion_" + SpawnedMinons.ToString(), startPos.ToVec3(), new Vec3(28, 28), game.AssetManager.GetTexture("Minion_2.png"));

                // Add behavior for minion navigation and targeting
                var minionBehavior = minion.AddBehavior(new Minion(NavMesh, this, Target, light, MinionType.EnemyMinion));
                minionBehavior.Health = 100;
                minionBehavior.TowerDamage = 50; // was 25!
                game.Storage.ManageElement(minion);

                // add the game element to the scene
                map.AddGameElement("MinionLayer", minion);
            }
        }

        /// <summary>
        /// Method called when the tower is destroyed.
        /// </summary>
        public override void OnDestroy(Game game, GameElement parent)
        {

        }

        /// <summary>
        /// Method called when the tower is initialized.
        /// </summary>
        public override void OnInit(Game game, GameElement parent)
        {

        }

        /// <summary>
        /// Method called for rendering the tower.
        /// </summary>
        public override void OnRender(Game game, GameElement parent)
        {

        }
    }
}
