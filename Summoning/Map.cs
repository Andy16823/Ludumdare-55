using Genesis.Core;
using Genesis.Core.GameElements;
using Genesis.Graphics;
using Genesis.Graphics.RenderDevice;
using Genesis.Graphics.Shaders.OpenGL;
using Genesis.Math;
using Summoning.Behaviors;
using Summoning.Navigation;
using System;
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
        public Bitmap mapdata { get; set; }
        public NavMesh NavMesh { get; set; }
        public long LastMinionSpawn { get; set; } // Last Minion spawn time
        public long MinionSummonIntervall { get; set; } = 30000;

        private Waypoint route;
        //private GridCell hoveredCell;
        private Waypoint start;
        private Waypoint end;

        public Map(String path, Game game) : base()
        {
            FileInfo fi = new FileInfo(path);
            this.Name = fi.Name;
            this.AddLayer("BaseLayer");
            this.AddLayer("TowerLayer");
            this.AddLayer("MinionLayer");
            this.AddLayer("Lights");

            //this.RenderLightmap = false;
            this.LightmapIntensity = 0.98f;

            Light2D light2D = new Light2D("Test", new Vec3(100, 100), new Vec3(250, 250), game.AssetManager.GetTexture("LightShape_3.png"));
            light2D.LightColor = Color.White; //Color.FromArgb(223, 113, 38);
            Light2D light2D_1 = new Light2D("Test", new Vec3(150, 150), new Vec3(250, 250), game.AssetManager.GetTexture("LightShape.png"));
            light2D_1.LightColor = Color.Red;
            this.Lights.Add(light2D);
            //this.Lights.Add(light2D_1);

            this.LoadMap(path, game);
        }

        private void LoadMap(String path, Game game)
        {
            mapdata = (Bitmap)Bitmap.FromFile(path);

            start = new Waypoint();
            end = new Waypoint();

            this.NavMesh = new NavMesh(Vec3.Zero(), mapdata.Width, mapdata.Height, 32);
            this.NavMesh.UseDiagonal = false;

            BufferedSprite grass = new BufferedSprite("Grass", new Vec3(0, 0), game.AssetManager.GetTexture("Grass.png"));
            BufferedSprite ground = new BufferedSprite("Ground", new Vec3(0, 0), game.AssetManager.GetTexture("Ground.png"));

            for (int y = 0; y < mapdata.Height; y++)
            {
                for (int x = 0; x < mapdata.Width; x++)
                {
                    Color pixel = mapdata.GetPixel(x, y);
                    if (pixel == Color.FromArgb(0, 0, 0))
                    {
                        ground.AddShape(new Vec3(x * 32, y * 32), new Vec3(32, 32));
                        this.NavMesh.AddObstacle(x, y);
                    }
                    else if (pixel == Color.FromArgb(91, 110, 225))
                    {
                        grass.AddShape(new Vec3(x * 32, y * 32), new Vec3(32, 32));
                        var tower = new Sprite("Tower_" + x.ToString() + "_" + y.ToString(), new Vec3(x * 32, y * 32), new Vec3(64, 64), game.AssetManager.GetTexture("Tower.png"));
                        this.AddGameElement("TowerLayer", tower);

                        Light2D light = new Light2D("Test", new Vec3(x * 32, y * 32), new Vec3(250, 250), game.AssetManager.GetTexture("LightShape_3.png"));
                        light.LightColor = Color.FromArgb(183, 85, 255);
                        this.Lights.Add(light);

                        if (end.column == 0)
                        {
                            end.column = x;
                            end.row = y;
                        }
                    }
                    else if (pixel == Color.FromArgb(251, 242, 54))
                    {
                        grass.AddShape(new Vec3(x * 32, y * 32), new Vec3(32, 32));
                        var tower = new Sprite("Tower_B_" + x.ToString() + "_" + y.ToString(), new Vec3(x * 32, y * 32), new Vec3(64, 64), game.AssetManager.GetTexture("Tower_B.png"));
                        this.AddGameElement("TowerLayer", tower);
                        start.column = x;
                        start.row = y;
                    }
                    else if(pixel == Color.FromArgb(255, 255, 255))
                    {
                        grass.AddShape(new Vec3(x * 32, y * 32), new Vec3(32, 32));
                    }
                }
            }

            this.AddGameElement("BaseLayer", grass);
            this.AddGameElement("BaseLayer", ground);
            NavMesh.FindPathAsync(start.column, start.row, end.column, end.row, (waypoint) =>
            {
                route = waypoint;
            });
        }

        public override void OnUpdate(Game game, IRenderDevice renderDevice)
        {
            base.OnUpdate(game, renderDevice);

            // Camera Movement
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

            // Get the hovered grid cell
            var mousePos = Input.GetRefMousePos(game);
            var pos = Camera.ProjectMouse2D(this.Camera, game.Viewport, (int) mousePos.X, (int) mousePos.Y);
            //hoveredCell = NavMesh.GetGridCellFromCoordinates((int)pos.X, (int)pos.Y);

            // Spawn the minions
            var now = Utils.GetCurrentTimeMillis();
            if(now > LastMinionSpawn + MinionSummonIntervall)
            {
                var waypoint = NavMesh.FindPath(start.column, start.row, end.column, end.row);
                var startPos = NavMesh.GetGridCell(start.column, start.row);
                SummonMinion(new Vec3(startPos.x, startPos.y), waypoint, game);
                route = waypoint;
                LastMinionSpawn = now;
            }
        }

        public void SummonMinion(Vec3 start, Waypoint target, Game game)
        {
            Sprite minion = new Sprite("Minion", start, new Vec3(32, 32), game.AssetManager.GetTexture("Minion.png"));
            Light2D light = new Light2D("Test", start, new Vec3(250, 250), game.AssetManager.GetTexture("LightShape_3.png"));
            light.Init(game, game.RenderDevice);
            light.LightColor = Color.FromArgb(0, 63, 203);
            this.Lights.Add(light);
            var minionBehavior = minion.AddBehavior(new Minion(NavMesh, target, light));
            game.Storage.ManageElement(minion);
            this.AddGameElement("MinionLayer", minion);
        }

        public override void OnRender(Game game, IRenderDevice renderDevice)
        {
            base.OnRender(game, renderDevice);
            //if (route != null)
            //{
            //    NavMesh.DrawPath(renderDevice, route);
            //}
            //NavMesh.DrawGrid(renderDevice);
            //if(hoveredCell.x != -1)
            //{
            //    renderDevice.FillRect(hoveredCell.ToRect(), Color.Purple);
            //}
            Console.WriteLine(renderDevice.GetError());
        }
    }
}
