using Genesis.Core;
using Genesis.Math;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Summoning.Behaviors
{
    /// <summary>
    /// Abstract class representing a tower behavior in the game.
    /// </summary>
    public abstract class Tower : IGameBehavior
    {
        /// <summary>
        /// The column position of the tower.
        /// </summary>
        public int Column { get; set; }

        /// <summary>
        /// The row position of the tower.
        /// </summary>
        public int Row { get; set; }

        /// <summary>
        /// The maximum health of the tower.
        /// </summary>
        public int MaxHealth { get; set; } = 1000;

        /// <summary>
        /// The current health of the tower.
        /// </summary>
        public int Health { get; set; } = 1000;

        /// <summary>
        /// The last time the tower gained power.
        /// </summary>
        public long LastPowerGain { get; set; }

        /// <summary>
        /// The interval between power gains.
        /// </summary>
        public long PowerGainIntervall { get; set; } = 3000;

        /// <summary>
        /// The amount of power gained in each interval.
        /// </summary>
        public int PowerGain { get; set; } = 10;

        /// <summary>
        /// The resources possessed by the tower.
        /// </summary>
        public int Ressources { get; set; } = 0;

        private long m_lastPowerGainExt;
        private long m_lastPowerGainExtCoolDown = 1000;

        private float m_barWidth = 80;
        private float m_barHeight = 15;
        private Color m_backgroundColor = Color.FromArgb(26, 26, 26);
        private Color m_barColor = Color.Green;

        /// <summary>
        /// Receives damage and updates the health of the tower.
        /// </summary>
        /// <param name="value">The amount of damage received.</param>
        /// <returns>True if the tower is defeated, false otherwise.</returns>
        public bool ReciveDamage(int value)
        {
            Health -= value;
            if (Health <= 0)
            {
                Console.WriteLine("tower defeated!");
                Health = 0;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Renders the health bar of the tower.
        /// </summary>
        /// <param name="game">The current game.</param>
        /// <param name="parent">The parent game element.</param>
        /// <param name="selected">Flag indicating if the tower is selected.</param>
        public void RenderHealthbar(Game game, GameElement parent, bool selected = false)
        {
            float hpct = Health * 100 / MaxHealth;
            float _hbpct = m_barWidth * hpct / 100;

            float barX = parent.Location.X - ((m_barWidth / 2) - (_hbpct / 2));

            game.RenderDevice.FillRect(new Rect(parent.Location.X, parent.Location.Y + 50f, m_barWidth, m_barHeight), m_backgroundColor);
            game.RenderDevice.FillRect(new Rect(barX, parent.Location.Y + 50f, _hbpct, m_barHeight), m_barColor);
            if (selected)
            {
                game.RenderDevice.DrawRect(new Rect(parent.Location.X, parent.Location.Y + 50f, m_barWidth, m_barHeight), Color.Yellow, 1.5f);
            }
            game.RenderDevice.DrawString(Ressources.ToString(), new Vec3(parent.Location.X, parent.Location.Y + 70f), 12, game.AssetManager.GetFont("Formal_Future"), Color.White);
        }

        /// <summary>
        /// Gains power at regular intervals.
        /// </summary>
        public void GainPower()
        {
            var now = Utils.GetCurrentTimeMillis();
            if (now > this.LastPowerGain + this.PowerGainIntervall)
            {
                this.Ressources += this.PowerGain;
                this.LastPowerGain = now;
            }
        }

        /// <summary>
        /// Gains external power with an optional cooldown check.
        /// </summary>
        /// <param name="value">The amount of power to gain.</param>
        /// <param name="checkLastGain">Flag indicating whether to check the cooldown for the last power gain.</param>
        public void GainPowerExt(int value, bool checkLastGain = true)
        {
            if (checkLastGain)
            {
                var now = Utils.GetCurrentTimeMillis();
                if (now > m_lastPowerGainExt + m_lastPowerGainExtCoolDown)
                {
                    this.Ressources += value;
                    m_lastPowerGainExt = now;
                }
            }
            else
            {
                this.Ressources += value;
            }
        }

    }
}
