using Genesis.Core;
using Genesis.Graphics;
using Genesis.Graphics.RenderDevice;
using Genesis.Math;
using Newtonsoft.Json.Linq;
using Summoning.Navigation;
using System;
using System.Windows.Forms;

namespace Summoning
{
    public partial class Form1 : Form
    {
        private Game m_game;
        private NavMesh m_navMesh;

        /// <summary>
        /// Initial the game
        /// </summary>
        public Form1(String mapname, float lightmapIntensity)
        {
            InitializeComponent();

            // Define the ressources folder
            String ressources = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory + "\\Resources";


            RenderSettings renderSettings = new RenderSettings();
            renderSettings.gamma = 0.5f;

            // Setup the game class and load the assets from the "Resources" folder.
            m_game = new Game(new Experimental(this.Handle, renderSettings), new Viewport(this.ClientSize.Width, this.ClientSize.Height));
            m_game.AssetManager.ImportAssetLibary(ressources + "\\Sprites.galib");
            m_game.AssetManager.LoadTextures();
            m_game.AssetManager.LoadFonts();
            m_game.TargetFPS = 90;

            var minionDeffinitions = JObject.Parse(System.IO.File.ReadAllText(ressources + "\\Minions.json"));

            // Create the level comple scene
            var levelCompleteMenu = new LevelCompleteMenu(m_game);
            m_game.AddScene(levelCompleteMenu);

            // Create the game over scene
            var gameOverMenu = new GameOverMenu(m_game);
            m_game.AddScene(gameOverMenu);

            // Create the level with the selected map
            var mapPath = ressources + "\\Maps";
            var scene = new Map(mapPath + "\\" + mapname, m_game, minionDeffinitions, lightmapIntensity);
            m_game.AddScene(scene);

            // Load the level and start the game
            m_game.LoadScene(scene);
            m_game.Start();
        }

        /// <summary>
        /// Resize the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_Resize(object sender, EventArgs e)
        {
            m_game.Viewport.SetNewViewport(ClientSize.Width, ClientSize.Height);
            if (m_game.SelectedScene != null)
            {
                m_game.SelectedScene.ResizeScene(m_game.Viewport);
                m_game.SelectedScene.Camera.Size = new Vec3(m_game.Viewport.Width, m_game.Viewport.Height);
            }
        }

        /// <summary>
        /// Stops the game
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_FormClosed(object sender, FormClosedEventArgs e)
        {
            m_game.Stop();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }
    }
}
