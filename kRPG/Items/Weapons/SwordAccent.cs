﻿using System;
using System.Collections.Generic;
using kRPG.Enums;
using kRPG.Items.Dusts;
using kRPG.Projectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace kRPG.Items.Weapons
{
    public class SwordAccent
    {
        public static Dictionary<int, SwordAccent> accents = new Dictionary<int, SwordAccent>();
        public static SwordAccent flame;
        public static SwordAccent gemBlue;
        public static SwordAccent gemGreen;
        public static SwordAccent gemPurple;
        public static SwordAccent gemRed;
        public static SwordAccent none;
        public int critBonus;
        public float dpsModifier = 1f;
        public Action<Rectangle, Player> effect;

        public Dictionary<ELEMENT, float> eleDamage = new Dictionary<ELEMENT, float>
        {
            {ELEMENT.FIRE, 0f}, {ELEMENT.COLD, 0f}, {ELEMENT.LIGHTNING, 0f}, {ELEMENT.SHADOW, 0f}
        };

        public int mana;

        public Action<Player, NPC, ProceduralSword, int, bool> onHit;
        public Vector2 origin;
        public string suffix = "";
        public Texture2D texture;

        public int type;

        public SwordAccent(string texture, string suffix, int origin_x, int origin_y, int mana = 0,
            Action<Player, NPC, ProceduralSword, int, bool> onHit = null, float dpsModifier = 1f, int critBonus = 0, Action<Rectangle, Player> effect = null)
        {
            type = accents.Count;
            if (Main.netMode != 2)
                if (texture != null)
                    this.texture = ModLoader.GetMod("kRPG").GetTexture("GFX/Items/Accents/" + texture);
            this.suffix = suffix;
            origin = new Vector2(origin_x, origin_y);
            this.onHit = onHit;
            this.dpsModifier = dpsModifier;
            this.critBonus = critBonus;
            this.effect = effect;
            this.mana = mana;

            if (!accents.ContainsKey(type))
                accents.Add(type, this);
        }

        public static void Initialize()
        {
            accents = new Dictionary<int, SwordAccent>();

            none = new SwordAccent(null, "", 0, 0);
            gemRed = new SwordAccent("GemRed", " of Rejuvenation", 2, 2, 2, delegate(Player player, NPC npc, ProceduralSword sword, int damage, bool crit)
            {
                if (Main.rand.Next(4) == 0)
                {
                    var character = player.GetModPlayer<PlayerCharacter>();
                    float distance = Vector2.Distance(npc.Center, character.player.Center);
                    int count = (int) (distance / 20);
                    var trail = new Trail(npc.Center, 60, delegate(SpriteBatch spriteBatch, Player p, Vector2 end, Vector2[] displacement, float scale)
                    {
                        for (int i = 0; i < count; i += 1)
                        {
                            var position = (npc.position - p.Center) * i / count + p.Center;
                            spriteBatch.Draw(GFX.heart, position - Main.screenPosition + displacement[i], null, Color.White, 0f, Vector2.Zero, scale,
                                SpriteEffects.None, 0f);
                        }
                    }) {displacement = new Vector2[count]};
                    for (int i = 0; i < count; i += 1)
                        trail.displacement[i] = new Vector2(Main.rand.NextFloat(-1f, 1f), Main.rand.NextFloat(-1f, 1f));
                    character.trails.Add(trail);
                    int healAmount = Main.rand.Next(5) + 3;
                    player.statLife += healAmount;
                    player.HealEffect(healAmount);
                }
            });
            flame = new SwordAccent("Flame", " of Ignition", 2, 2, 3, delegate(Player player, NPC npc, ProceduralSword sword, int damage, bool crit)
                {
                    Main.PlaySound(new LegacySoundStyle(2, 14).WithVolume(0.5f), player.Center);
                    var proj = Main.projectile[
                        Projectile.NewProjectile(npc.Center - new Vector2(16, 32), Vector2.Zero, ModContent.ProjectileType<Explosion>(),
                            Math.Max(1, (int) Math.Round(sword.eleDamage[ELEMENT.FIRE] * damage * 2)), 0f, player.whoAmI)];
                }, 1.05f).setEleDamage(new Dictionary<ELEMENT, float> {{ELEMENT.FIRE, 0.2f}, {ELEMENT.COLD, 0f}, {ELEMENT.LIGHTNING, 0f}, {ELEMENT.SHADOW, 0f}})
                .setEffect(delegate(Rectangle rect, Player player)
                {
                    if (Main.rand.Next(2) != 0)
                        return;
                    int dust = Dust.NewDust(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, DustID.Fire, player.velocity.X * 0.2f + player.direction * 3f,
                        player.velocity.Y * 0.2f, 63, new Color(), 1.5f);
                    Main.dust[dust].noGravity = true;
                });
            gemGreen = new SwordAccent("GemGreen", " of Thunder", 2, 2, 2, delegate(Player player, NPC npc, ProceduralSword sword, int damage, bool crit)
                {
                    Main.PlaySound(new LegacySoundStyle(2, 14).WithVolume(0.5f), player.Center);
                    var proj = Main.projectile[
                        Projectile.NewProjectile(npc.Center - new Vector2(24, 48), Vector2.Zero, ModContent.ProjectileType<SmokePellets>(),
                            Math.Max(1, damage / 6),
                            0f, player.whoAmI)];
                }, 1.1f, 4).setEleDamage(new Dictionary<ELEMENT, float>
                    {
                        {ELEMENT.FIRE, 0f}, {ELEMENT.COLD, 0f}, {ELEMENT.LIGHTNING, 0.2f}, {ELEMENT.SHADOW, 0f}
                    })
                .setEffect(delegate(Rectangle rect, Player player)
                {
                    if (Main.rand.Next(2) != 0)
                        return;
                    int dust = Dust.NewDust(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, DustID.Electric,
                        player.velocity.X * 0.2f + player.direction * 3f,
                        player.velocity.Y * 0.2f, 63, new Color(), 0.5f);
                    Main.dust[dust].noGravity = true;
                });
            gemBlue = new SwordAccent("GemBlue", " of Everest", 2, 2, 0, null, 1.15f, 0, delegate(Rectangle rect, Player player)
            {
                if (Main.rand.Next(3) == 0)
                {
                    int dust = Dust.NewDust(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, ModContent.GetInstance<Ice>().Type,
                        player.velocity.X * 0.2f + player.direction * 3f, player.velocity.Y * 0.2f, 100, new Color(), 1.5f);
                    Main.dust[dust].noGravity = true;
                }

                Lighting.AddLight(rect.Center(), 0f, 0.4f, 1f);
            }).setEleDamage(new Dictionary<ELEMENT, float> {{ELEMENT.FIRE, 0f}, {ELEMENT.COLD, 0.3f}, {ELEMENT.LIGHTNING, 0f}, {ELEMENT.SHADOW, 0f}});
            gemPurple = new SwordAccent("GemPurple", " of Starlight", 2, 1, 3, null, 1.1f).setEffect(delegate(Rectangle rect, Player player)
            {
                if (Main.rand.Next(2) != 0)
                    return;
                int dust = Dust.NewDust(new Vector2(rect.X, rect.Y), rect.Width, rect.Height, 27, player.velocity.X * 0.2f + player.direction * 3f,
                    player.velocity.Y * 0.2f, 63, new Color(), 1.5f);
                Main.dust[dust].noGravity = true;
            });
        }

        public static SwordAccent RandomAccent()
        {
            return accents.Random();
        }

        public SwordAccent setEffect(Action<Rectangle, Player> effect)
        {
            this.effect = effect;
            return this;
        }

        public SwordAccent setEleDamage(Dictionary<ELEMENT, float> eleDamage)
        {
            this.eleDamage = eleDamage;
            return this;
        }

        public static void Unload()
        {
            foreach (var accent in accents.Values)
                accent.texture = null;
        }
    }
}