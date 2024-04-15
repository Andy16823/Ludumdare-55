using Genesis.Core;
using Genesis.Core.GameElements;
using Genesis.Graphics;
using Genesis.Graphics.RenderDevice;
using Genesis.Graphics.Shaders.OpenGL;
using Genesis.Math;
using Genesis.Physics;
using Genesis.UI;
using Newtonsoft.Json.Linq;
using Summoning.Behaviors;
using Summoning.Navigation;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Summoning
{
    public class Map : Scene2D
    {
        /// <summary>
        /// The map data
        /// </summary>
        public Bitmap mapdata { get; set; }

        /// <summary>
        /// The navigation mesh for the map
        /// </summary>
        public NavMesh NavMesh { get; set; }

        /// <summary>
        /// The last time where a minion spawned
        /// </summary>
        public long LastMinionSpawn { get; set; } // Last Minion spawn time

        /// <summary>
        /// The intervall when new minion gets summoned
        /// </summary>
        public long MinionSummonIntervall { get; set; } = 10000;

        /// <summary>
        /// The start point for the minions
        /// </summary>
        private Tower m_playerTower;

        /// <summary>
        /// The possible targets for the minions
        /// </summary>
        public List<Tower> Destinations;

        /// <summary>
        /// The selected tower
        /// </summary>
        public Tower SelectedTower;

        /// <summary>
        /// The minion deffiniations json
        /// </summary>
        public JObject MinionDeffinitions;

        /// <summary>
        /// A value if the game is stared or not
        /// </summary>
        public bool Started { get; set; } = false;

        /// <summary>
        /// The time when the player starts the game in ms
        /// </summary>
        public long StartTime { get; set; }

        /// <summary>
        /// The game class
        /// </summary>
        private Game m_game;

        /// <summary>
        /// Loads a new map with the given map and the given minion deffinition
        /// </summary>
        /// <param name="path"></param>
        /// <param name="game"></param>
        /// <param name="minionDeffinitions"></param>
        public Map(String path, Game game, JObject minionDeffinitions, float lightmapIntensity) : base()
        {
            // Setup the level values
            m_game = game;
            this.LightmapIntensity = lightmapIntensity;
            this.Camera = new Camera(new Vec3(0, 0), new Vec3(game.Viewport.Width, game.Viewport.Height), -1.0f, 1.0f);
            this.PhysicHandler = new PhysicsHandler2D(0f, 0f);

            // Load the map data
            FileInfo fi = new FileInfo(path);
            Destinations = new List<Tower>();
            this.MinionDeffinitions = minionDeffinitions;
            this.Name = fi.Name;

            // Create the scene layers
            this.AddLayer("BaseLayer");
            this.AddLayer("TowerLayer");
            this.AddLayer("MinionLayer");
            this.LoadMap(path, game);
            this.CreateUI(game);

            // render the progress bars
            this.AfterLightmapRendering += (s, g, r) =>
            {
                m_playerTower.RenderHealthbar(g, m_playerTower.Parent);
                foreach (var enemy in this.Destinations)
                {
                    if(enemy == this.SelectedTower)
                    {
                        enemy.RenderHealthbar(g, enemy.Parent, true);
                    }
                    else
                    {
                        enemy.RenderHealthbar(g, enemy.Parent);
                    }
                    
                }
            };

        }

        /// <summary>
        /// Load the map from the file
        /// </summary>
        /// <param name="path"></param>
        /// <param name="game"></param>
        private void LoadMap(String path, Game game)
        {
            mapdata = (Bitmap)Bitmap.FromFile(path);
            
            // build the navigation mesh
            this.NavMesh = new NavMesh(Vec3.Zero(), mapdata.Width, mapdata.Height, 32);
            this.NavMesh.UseDiagonal = false;

            m_playerTower = new PlayerTower(NavMesh, MinionDeffinitions);

            // create the sprite buffers for the grass and the ground
            BufferedSprite grass = new BufferedSprite("Grass", new Vec3(0, 0), game.AssetManager.GetTexture("Grass.png"));
            BufferedSprite ground = new BufferedSprite("Ground", new Vec3(0, 0), game.AssetManager.GetTexture("Ground.png"));

            // parse the map
            for (int y = 0; y < mapdata.Height; y++)
            {
                for (int x = 0; x < mapdata.Width; x++)
                {
                    Color pixel = mapdata.GetPixel(x, y);

                    if (pixel == Color.FromArgb(0, 0, 0))
                    {
                        // add an obstacle to the navmesh
                        ground.AddShape(new Vec3(x * 32, y * 32), new Vec3(32, 32));
                        this.NavMesh.AddObstacle(x, y);
                    }
                    else if (pixel == Color.FromArgb(91, 110, 225))
                    {
                        // Create the enemy towers
                        grass.AddShape(new Vec3(x * 32, y * 32), new Vec3(32, 32));

                        var tower = new Sprite("Tower_" + x.ToString() + "_" + y.ToString(), new Vec3(x * 32, y * 32), new Vec3(64, 64), game.AssetManager.GetTexture("Tower.png"));
                        var towerBehavior = tower.AddBehavior(new EnemyTower(NavMesh, m_playerTower, x, y));
                        this.Destinations.Add(towerBehavior);
                        this.SelectedTower = towerBehavior;

                        this.AddGameElement("TowerLayer", tower);

                        Light2D light = new Light2D("Test", new Vec3(x * 32, y * 32), new Vec3(250, 250), game.AssetManager.GetTexture("LightShape_3.png"));
                        light.LightColor = Color.FromArgb(183, 85, 255);
                        this.Lights.Add(light);
                    }
                    else if (pixel == Color.FromArgb(251, 242, 54))
                    {
                        // Create the player tower
                        m_playerTower.Column = x;
                        m_playerTower.Row = y;

                        grass.AddShape(new Vec3(x * 32, y * 32), new Vec3(32, 32));
                        var tower = new Sprite("PlayerTower", new Vec3(x * 32, y * 32), new Vec3(64, 64), game.AssetManager.GetTexture("Tower_B.png"));
                        tower.AddBehavior(m_playerTower);
                        
                        this.AddGameElement("TowerLayer", tower);

                        Light2D light = new Light2D("Test", new Vec3(x * 32, y * 32), new Vec3(250, 250), game.AssetManager.GetTexture("LightShape_3.png"));
                        light.LightColor = Color.FromArgb(0, 63, 203);
                        this.Lights.Add(light);
                    }
                    else if(pixel == Color.FromArgb(255, 255, 255))
                    {
                        // create the grass
                        grass.AddShape(new Vec3(x * 32, y * 32), new Vec3(32, 32));
                    }
                }
            }

            // add the sprite buffers to the scene
            this.AddGameElement("BaseLayer", grass);
            this.AddGameElement("BaseLayer", ground);
        }

        /// <summary>
        /// Create the ui for the game
        /// </summary>
        /// <param name="game"></param>
        private void CreateUI(Game game)
        {
            var canvas = new Canvas("Overlay", new Vec3(0, 0), new Vec3(game.Viewport.Width, game.Viewport.Height));
            var label = new Genesis.UI.Label("ressources", new Vec3(15, 15), "Ressources", game.AssetManager.GetFont("Formal_Future"), Color.White);
            canvas.AddWidget(label);

            this.AddCanvas(canvas);
        }

        /// <summary>
        /// Handle the gameplay for the map
        /// </summary>
        /// <param name="game"></param>
        /// <param name="renderDevice"></param>
        public override void OnUpdate(Game game, IRenderDevice renderDevice)
        {
            base.OnUpdate(game, renderDevice);

            // set the start time on the first frame
            if(!this.Started)
            {
                this.StartTime = Utils.GetCurrentTimeMillis();
                this.Started = true;
            }

            // Check if no tower is allive
            if(this.Destinations.Count <= 0)
            {
                // End the game
                this.LevelComplete();
            }

            // Checks if the player tower is allive
            if(this.m_playerTower.Health <= 0)
            {
                // End the game
                this.GameOver();
            }

            // camera movement
            if(Input.IsKeyDown(System.Windows.Forms.Keys.W))
            {
                this.Camera.Location.Y += 5f;
            }
            else if(Input.IsKeyDown(System.Windows.Forms.Keys.S))
            {
                this.Camera.Location.Y -= 5f;
            }
            else if (Input.IsKeyDown(System.Windows.Forms.Keys.A))
            {
                this.Camera.Location.X -= 5f;
            }
            else if (Input.IsKeyDown(System.Windows.Forms.Keys.D))
            {
                this.Camera.Location.X += 5f;
            }
            this.Camera.Location = Vec3.Round(this.Camera.Location);

            // Selects another tower
            if(Input.IsKeyDown(Keys.LButton))
            {
                var mousePos = Input.GetRefMousePos(game);
                var pos = Camera.ProjectMouse2D(this.Camera, game.Viewport, (int) mousePos.X, (int) mousePos.Y);
                foreach(var item in this.GetLayer("TowerLayer").Elements) {
                    var tower = (Sprite)item;
                    if (tower.GetBounds2D().Contains(pos.X + (tower.Size.X / 2), pos.Y + (tower.Size.Y / 2)))
                    {
                        var towerBehavior = (EnemyTower) tower.GetBehavior<EnemyTower>();
                        if(towerBehavior != null)
                        {
                            this.SelectedTower = towerBehavior;
                            Console.WriteLine("New Target " + tower.Name);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Level complete function. Ends the level and loads the 
        /// level complete scene.
        /// </summary>
        public void LevelComplete()
        {
            var menu = (LevelCompleteMenu) m_game.FindScene("LevelCompleteMenu");
            if(menu != null)
            {
                var time = (Utils.GetCurrentTimeMillis() - this.StartTime) / 60000;
                menu.Time = time;
                menu.UpdateUI();
                m_game.LoadScene(menu);
            }
        }

        /// <summary>
        /// Displays the game over menu and updates it with the elapsed game time.
        /// </summary>
        public void GameOver()
        {
            var menu = (GameOverMenu)m_game.FindScene("GameOverMenu");
            if (menu != null)
            {
                var time = (Utils.GetCurrentTimeMillis() - this.StartTime) / 60000;
                menu.Time = time;
                menu.UpdateUI();
                m_game.LoadScene(menu);
            }
        }

        /// <summary>
        /// Gets the next tower
        /// </summary>
        /// <returns></returns>
        public Tower GetNextTower()
        {
            if(this.Destinations.Count > 0)
            {
                return Destinations[0];
            }

            return null;
        }
    }
}
