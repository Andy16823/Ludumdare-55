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
    public class LevelCompleteMenu : Scene
    {
        public long Time { get; set; }

        private Game m_game;

        public LevelCompleteMenu(Game game)
        {
            m_game = game;
            this.Name = "LevelCompleteMenu";
            this.Camera = new Camera(new Vec3(0, 0), new Vec3(game.Viewport.Width, game.Viewport.Height), -1.0f, 1.0f);

            var font = game.AssetManager.GetFont("Formal_Future");

            var canvas = new Canvas("Canvas", new Genesis.Math.Vec3(0, 0), new Vec3(game.Viewport.Width, game.Viewport.Height));

            var stringWidth = Utils.GetStringWidth("Level Complete", 42, 0.5f);
            var levelCompleteLabel = new Label("LevelCompleteLabel", new Vec3((game.Viewport.Width / 2) - (stringWidth / 2), game.Viewport.Height / 2), "Level Complete", game.AssetManager.GetFont("Formal_Future"), Color.White);
            levelCompleteLabel.FontSize = 42;
            //newGameButton.Click += (widget, wGame, wScene, wCanvas) =>
            //{
            //    wGame.LoadScene("map-002.png");
            //};
            canvas.AddWidget(levelCompleteLabel);

            var timeLabel = new Label("Time", new Vec3((game.Viewport.Width / 2), (game.Viewport.Height / 2) - 40), "15 Minutes", font, Color.White);
            timeLabel.FontSize = 30;
            canvas.AddWidget(timeLabel);

            this.AddCanvas(canvas);
        }

        public void UpdateUI()
        {
            var text = Time.ToString() + " Minutes";
            text = text.Replace(" ", "");
            var stringWidth = Utils.GetStringWidth(text, 30, 0.5f);

            var canvas = this.GetCanvas("Canvas");
            var label = (Label)canvas.GetWidget("Time");

            label.Text = text;
            label.Location.X = (m_game.Viewport.Width / 2) - (stringWidth / 2);
        }
    }
}
