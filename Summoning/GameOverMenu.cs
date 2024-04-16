using Genesis.Core;
using Genesis.Graphics;
using Genesis.Math;
using Genesis.Physics;
using Genesis.UI;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summoning
{
    /// <summary>
    /// Represents the game over menu scene.
    /// </summary>
    public class GameOverMenu : Scene
    {
        /// <summary>
        /// The time survived in the game.
        /// </summary>
        public long Time { get; set; }

        private Game m_game;

        private ProgressBar m_progressBar;

        /// <summary>
        /// Initializes the game over menu scene.
        /// </summary>
        /// <param name="game">The game instance.</param>
        public GameOverMenu(Game game)
        {
            // Initialize the scene
            m_game = game;
            this.Name = "GameOverMenu";
            this.Camera = new Camera(new Vec3(0, 0), new Vec3(game.Viewport.Width, game.Viewport.Height), -1.0f, 1.0f);

            // Load the font for the widgets
            var font = game.AssetManager.GetFont("Formal_Future");

            // Create a canvas for the widgets
            var canvas = new Canvas("Canvas", new Genesis.Math.Vec3(0, 0), new Vec3(game.Viewport.Width, game.Viewport.Height));

            // Calculate the label width and center it in the middle of the screen
            var text = "Game Over";

            var levelCompleteLabel = new Label("LevelCompleteLabel", new Vec3((game.Viewport.Width / 2), game.Viewport.Height / 2), text, font, Color.White, WidgetAnchor.MID_MID);
            levelCompleteLabel.FontSize = 42;
            canvas.AddWidget(levelCompleteLabel);

            // Create a label for the play time
            var timeLabel = new Label("Time", new Vec3((game.Viewport.Width / 2), (game.Viewport.Height / 2) - 30), "15 Minutes", font, Color.White, WidgetAnchor.MID_MID);
            timeLabel.FontSize = 30;
            canvas.AddWidget(timeLabel);

            // Create a restart button
            var button = new Button("button", new Vec3((game.Viewport.Width / 2), (game.Viewport.Height / 2) - 70), new Vec3(200, 30), "Restart", font, WidgetAnchor.MID_MID);
            button.Click += (widget, wGame, wScene, wCanvas) =>
            {
                var map = game.GetScene<Map>();
                map.Restart();
                game.LoadScene(map);
            };
            canvas.AddWidget(button);

            

            // Add the UI canvas to the scene
            this.AddCanvas(canvas);
        }

        private void Button_Click(Widget entity, Game game, Scene scene, Canvas canvas)
        {
            
        }

        /// <summary>
        /// Updates the UI with the latest game over information.
        /// </summary>
        public void UpdateUI()
        {
            // Calculate the text width
            var text = "You survived " + Time.ToString() + " Minutes";

            // Get the canvas and the widget from the scene
            var canvas = this.GetCanvas("Canvas");
            var label = (Label)canvas.GetWidget("Time");

            // Update the widget text and position
            label.Text = text;
        }
    }
}
