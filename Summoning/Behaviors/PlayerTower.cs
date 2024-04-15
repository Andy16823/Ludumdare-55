using Genesis.Core;
using Genesis.Core.GameElements;
using Genesis.Graphics;
using Genesis.Math;
using Genesis.UI;
using Newtonsoft.Json.Linq;
using Summoning.Navigation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Summoning.Behaviors
{
    /// <summary>
    /// Represents a tower controlled by the player.
    /// </summary>
    public class PlayerTower : Tower
    {
        /// <summary>
        /// Navigation mesh for pathfinding.
        /// </summary>
        public NavMesh NavMesh { get; set; }

        /// <summary>
        /// Time when the last minion was spawned.
        /// </summary>
        public long LastSpawn { get; set; }

        /// <summary>
        /// Interval between minion spawns.
        /// </summary>
        public long SpawnIntervall { get; set; } = 10000;

        private JObject m_minionDeffinitions;

        /// <summary>
        /// Constructs a player tower with the specified parameters.
        /// </summary>
        /// <param name="navMesh">Navigation mesh for pathfinding.</param>
        /// <param name="column">Column index of the tower's position.</param>
        /// <param name="row">Row index of the tower's position.</param>
        /// <param name="minionDefinitions">Definitions of available minions.</param>
        public PlayerTower(NavMesh navMesh, int column, int row, JObject minionDeffinitions)
        {
            NavMesh = navMesh;
            this.Column = column;
            this.Row = row;
            m_minionDeffinitions = minionDeffinitions;
        }

        /// <summary>
        /// Constructs a player tower with the specified parameters.
        /// </summary>
        /// <param name="navMesh">Navigation mesh for pathfinding.</param>
        /// <param name="minionDefinitions">Definitions of available minions.</param>
        public PlayerTower(NavMesh navMesh, JObject minionDeffinitions)
        {
            this.NavMesh = navMesh;
            m_minionDeffinitions = minionDeffinitions;
        }

        /// <summary>
        /// Called when the tower is destroyed.
        /// </summary>
        public override void OnDestroy(Game game, GameElement parent)
        {
            
        }

        /// <summary>
        /// Called when the tower is initialized.
        /// </summary>
        public override void OnInit(Game game, GameElement parent)
        {
            
        }

        /// <summary>
        /// Called for rendering the tower.
        /// </summary>
        public override void OnRender(Game game, GameElement parent)
        {

        }

        /// <summary>
        /// Called on each frame update.
        /// </summary>
        public override void OnUpdate(Game game, GameElement parent)
        {
            UpdateUI(game);
            this.GainPower();

            if (Input.IsKeyDown(System.Windows.Forms.Keys.D1))
            {
                SummonMinion(game, "Minion");
            }
            else if(Input.IsKeyDown(System.Windows.Forms.Keys.D2))
            {
                SummonMinion(game, "Towerbuster");
            }
            else if (Input.IsKeyDown(System.Windows.Forms.Keys.D3))
            {
                SummonMinion(game, "Tank");
            }
        }

        /// <summary>
        /// Summons a minion of the specified type if enough resources are available.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="name">The name of the minion to summon.</param>
        private void SummonMinion(Game game, String name)
        {
            var minion = m_minionDeffinitions["minions"][name];

            if (this.Ressources >= (int)minion["cost"])
            {
                if (game.SelectedScene.GetType() == typeof(Map))
                {
                    if (SummonMinion(game, (JObject)minion, 1000))
                    {
                        this.Ressources -= (int)minion["cost"];
                    }
                }
            }
        }

        /// <summary>
        /// Summons a minion with the specified parameters.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="minion">The JSON object containing minion parameters.</param>
        /// <param name="cooldown">The cooldown time between summonings.</param>
        /// <returns>True if the minion is successfully summoned, false otherwise.</returns>
        private bool SummonMinion(Game game, JObject minion, int cooldown)
        {
            return this.SummonMinion(game, Color.FromArgb((int)minion["lightColor"][0], (int)minion["lightColor"][1], (int)minion["lightColor"][2]), (string) minion["name"], (int) minion["health"], (int) minion["towerDamage"], (int) minion["killPowerGain"], (int) minion["towerHitPowerGain"], (string) minion["texture"], cooldown);
        }

        /// <summary>
        /// Summons a minion with the specified parameters.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="lightColor">The color of the minion's light.</param>
        /// <param name="name">The name of the minion.</param>
        /// <param name="health">The health of the minion.</param>
        /// <param name="towerDamage">The damage the minion deals to towers.</param>
        /// <param name="killPowerGain">The power gain on killing an enemy.</param>
        /// <param name="towerHitPowerGain">The power gain on hitting a tower.</param>
        /// <param name="texture">The texture of the minion.</param>
        /// <param name="cooldown">The cooldown time between summonings.</param>
        /// <returns>True if the minion is successfully summoned, false otherwise.</returns>
        private bool SummonMinion(Game game, Color lightColor, String name, int health, int towerDamage, int killPowerGain, int towerHitPowerGain, String texture, long cooldown)
        {
            if (game.SelectedScene.GetType() == typeof(Map))
            {
                var map = (Map)game.SelectedScene;
                var now = Utils.GetCurrentTimeMillis();
                if (now > LastSpawn + cooldown)
                {
                    var destination = map.SelectedTower;
                    if (destination == null)
                    {
                        return false;
                    }
                    var startPos = NavMesh.GetGridCell(this.Column, this.Row);

                    Light2D light = new Light2D("MinionLight", startPos.ToVec3(), new Vec3(250, 250), game.AssetManager.GetTexture("LightShape_3.png"));
                    light.Init(game, game.RenderDevice);
                    light.LightColor = lightColor;
                    map.Lights.Add(light);

                    // Create the minion behavior for pathfinding and navigation.
                    var minion = new Sprite(name, startPos.ToVec3(), new Vec3(28, 28), game.AssetManager.GetTexture(texture));
                    var minionBehavior = minion.AddBehavior(new Minion(NavMesh, this, destination, light, MinionType.PlayerMinion));
                    minionBehavior.Health = health;
                    minionBehavior.TowerDamage = towerDamage;
                    minionBehavior.KillPowerGain = killPowerGain;
                    minionBehavior.TowerHitPowerGain = towerHitPowerGain;
                    game.Storage.ManageElement(minion);

                    // Add the game element to the scene.
                    map.AddGameElement("MinionLayer", minion);

                    LastSpawn = now;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Updates the UI elements to display current resource information.
        /// </summary>
        /// <param name="game">The current game.</param>
        private void UpdateUI(Game game)
        {
            if (game.SelectedScene.GetType() == typeof(Map))
            {
                var map = (Map)game.SelectedScene;
                var ressourcesLabel = (Label)map.GetWidget("Overlay", "ressources");
                if (ressourcesLabel != null)
                {
                    ressourcesLabel.Text = "Ressources: " + Ressources.ToString();
                }
            }
        }
    }
}
