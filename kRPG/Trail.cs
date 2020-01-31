﻿using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;

namespace kRPG
{
    public class Trail
    {
        public Vector2[] displacement;
        private readonly Action<SpriteBatch, Player, Vector2, Vector2[], float> draw;
        private readonly Vector2 position;
        public float scale = 1f;
        private int timeleft;

        public Trail(Vector2 position, int timeleft, Action<SpriteBatch, Player, Vector2, Vector2[], float> draw)
        {
            this.position = position;
            this.timeleft = timeleft;
            this.draw = draw;
            scale = 1f;
        }

        public void Draw(SpriteBatch spritebatch, Player player)
        {
            timeleft -= 1;
            for (int i = 0; i < displacement.Length; i += 1)
                displacement[i] += new Vector2(0.6f, 0f).RotatedBy(displacement[i].ToRotation());
            draw(spritebatch, player, position, displacement, scale);
            scale -= 0.01f;
            if (timeleft <= 0)
                player.GetModPlayer<PlayerCharacter>().trails.Remove(this);
        }
    }
}