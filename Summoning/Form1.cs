using Genesis.Core;
using Genesis.Core.GameElements;
using Genesis.Graphics;
using Genesis.Graphics.RenderDevice;
using Genesis.Math;
using Genesis.Physics;
using Summoning.Navigation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public Form1()
        {
            InitializeComponent();
            // Setup the game class and load the assets from the "Resources" folder. Make sure you copy them into the output directory!
            m_game = new Game(new Experimental(this.Handle), new Viewport(this.ClientSize.Width, this.ClientSize.Height));
            m_game.AssetManager.LoadTextures();

            String mapPath = new System.IO.FileInfo(System.Reflection.Assembly.GetEntryAssembly().Location).Directory + "\\Resources\\Maps";

            // Setup a demo scene with an camera and an physics handler
            var scene = new Map(mapPath + "\\map-002.png", m_game);
            
            scene.Camera = new Camera(new Vec3(0, 0), new Vec3(m_game.Viewport.Width, m_game.Viewport.Height), -1.0f, 1.0f);
            scene.PhysicHandler = new PhysicsHandler2D(0f, 0f);

            // Hook into the update event. You can use this lambda or create an own function for it. 
            m_game.OnUpdate += (game, renderer) =>
            {
                
            };

            // Add the scene to the game and run it
            m_game.AddScene(scene);
            m_game.LoadScene("map-002.png");
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
