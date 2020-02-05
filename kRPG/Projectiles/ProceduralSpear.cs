﻿// Kalciphoz's RPG Mod
//  Copyright (c) 2016, Kalciphoz's RPG Mod
// 
// 
// THIS SOFTWARE IS PROVIDED BY Kalciphoz's ''AS IS'' AND ANY EXPRESS OR IMPLIED WARRANTIES,
// INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR
// PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL FAIRFIELDTEK LLC BE LIABLE FOR ANY DIRECT, INDIRECT,
// INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR
// OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH
// DAMAGE.
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to
// deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using kRPG.Items;
using kRPG.Items.Weapons;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace kRPG.Projectiles
{
    public class ProceduralSpear : ProceduralProjectile
    {
        public SwordAccent Accent { get; set; }
        public SwordBlade Blade { get; set; }
        public SwordHilt Hilt { get; set; }

        public float MovementFactor // Change this value to alter how fast the spear moves
        {
            get => projectile.ai[0];
            set => projectile.ai[0] = value;
        }
        // It appears that for this AI, only the ai0 field is used!

        public override void AI()
        {
            try
            {
                // Since we access the owner player instance so much, it's useful to create a helper local variable for this
                // Sadly, Projectile/ModProjectile does not have its own
                Player projOwner = Main.player[projectile.owner];
                // Here we set some of the projectile's owner properties, such as held item and itemtime, along with projectile directio and playerPosition based on the player
                //Vector2 ownerMountedCenter = projOwner.RotatedRelativePoint(projOwner.MountedCenter, true);

                if (projectile.velocity.X > 0)
                    projOwner.direction = 1;
                else
                    projOwner.direction = -1;

                projectile.direction = projOwner.direction;
                projectile.spriteDirection = projectile.direction;
                projOwner.heldProj = projectile.whoAmI;
                projOwner.itemTime = projOwner.itemAnimation;
                projectile.position.X = projOwner.Center.X - LocalTexture.Width / 2f /* + 2f*projOwner.direction*/;
                projectile.position.Y = projOwner.Center.Y - LocalTexture.Height / 2f /* + 4f*/;
                // As long as the player isn't frozen, the spear can move
                if (!projOwner.frozen)
                {
                    if (Math.Abs(MovementFactor) < .01) // When initially thrown out, the ai0 will be 0f
                    {
                        MovementFactor = 3f;
                        projectile.netUpdate = true;
                    }

                    if (projOwner.itemAnimation < projOwner.itemAnimationMax / 3)
                        MovementFactor -= 2.4f;
                    else
                        MovementFactor += 2.1f;
                }

                projectile.position += projectile.velocity * MovementFactor;
                Vector2 unitVelocity = projectile.velocity;
                unitVelocity.Normalize();
                projectile.position += unitVelocity * (Blade.Origin.Y * 2.8f + 8f);

                if (projOwner.itemAnimation == 1)
                    projectile.Kill();
                // Apply proper rotation, with an offset of 135 degrees due to the sprite's rotation, notice the usage of MathHelper, use this class!
                // MathHelper.ToRadians(xx degrees here)
                projectile.rotation = (float) Math.Atan2(projectile.velocity.Y, projectile.velocity.X) + MathHelper.ToRadians(45f);
                // Offset by 90 degrees here
                if (projectile.spriteDirection == -1)
                    projectile.rotation += MathHelper.ToRadians(90f);

                Rectangle rect = new Rectangle((int) projectile.position.X, (int) projectile.position.Y, LocalTexture.Width, LocalTexture.Height);
                Blade.Effect?.Invoke(rect, projOwner);
                Accent.Effect?.Invoke(rect, projOwner);
            }
            catch (SystemException e)
            {
                Main.NewText(e.ToString());
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            Player owner = Main.player[projectile.owner];
            return (target.position.X - owner.position.X) * owner.direction > -1f ? base.CanHitNPC(target) : false;
        }

        public Point CombinedTextureSize()
        {
            return new Point(Blade.Texture.Width - (int) Blade.Origin.X + (int) Hilt.SpearOrigin.X,
                (int) Blade.Origin.Y + Hilt.SpearTexture.Height - (int) Hilt.SpearOrigin.Y);
        }

        //public override void SendExtraAI(BinaryWriter writer)
        //{
        //    writer.Write(blade.type);
        //    writer.Write(hilt.type);
        //    writer.Write(accent.type);
        //}

        //public override void ReceiveExtraAI(BinaryReader reader)
        //{
        //    blade = SwordBlade.blades[reader.ReadInt32()];
        //    hilt = SwordHilt.hilts[reader.ReadInt32()];
        //    accent = SwordAccent.accents[reader.ReadInt32()];
        //    if (Main.netMode == 1) Initialize();
        //}

        public override void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, float scale)
        {
            if (LocalTexture == null && Main.netMode == 0)
            {
                Initialize();
                return;
            }

            if (LocalTexture == null)
            {
                Initialize();
                return;
            }

            switch (Main.netMode)
            {
                case 0:
                    spriteBatch.Draw(LocalTexture, position + LocalTexture.Size() / 2f, null, Blade.Lighted ? Color.White : color, rotation,
                        projectile.spriteDirection > 0 ? LocalTexture.Bounds.TopRight() : Vector2.Zero, scale,
                        projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                    break;
                case 1:
                {
                    Texture2D t2d = Main.projectileTexture[projectile.type];
                    spriteBatch.Draw(t2d, position + t2d.Size() / 2f, null, color, rotation,
                        projectile.spriteDirection > 0 ? t2d.Bounds.TopRight() : Vector2.Zero, scale,
                        projectile.spriteDirection > 0 ? SpriteEffects.None : SpriteEffects.FlipHorizontally, 0f);
                    break;
                }
            }
        }

        public override void Initialize()
        {
            LocalTexture = GFX.CombineTextures(new List<Texture2D> {Blade.Texture, Hilt.SpearTexture, Accent.Texture},
                new List<Point>
                {
                    new Point(CombinedTextureSize().X - Blade.Texture.Width, 0),
                    new Point(0, CombinedTextureSize().Y - Hilt.SpearTexture.Height),
                    new Point((int) Hilt.SpearOrigin.X - (int) Accent.Origin.X,
                        CombinedTextureSize().Y - Hilt.SpearTexture.Height + (int) Hilt.SpearOrigin.Y - (int) Accent.Origin.Y)
                }, CombinedTextureSize());
            projectile.width = LocalTexture.Width;
            projectile.height = LocalTexture.Height;
        }

        // ReSharper disable once IdentifierTypo
        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            Player owner = Main.player[projectile.owner];
            hitbox = new Rectangle((int) projectile.position.X - 2, (int) projectile.position.Y - 2, (int) (projectile.Right.X - projectile.Left.X) + 2,
                (int) (projectile.Bottom.Y - projectile.Top.Y + 2));
            if (owner.direction < 0) hitbox.X += hitbox.Width / 2;
            else hitbox.X -= hitbox.Width / 2;
        }

        public override void OnHitNPC(NPC target, int damage, float knockback, bool crit)
        {
            try
            {
                Player owner = Main.player[projectile.owner];
                Accent.OnHit?.Invoke(owner, target, (ProceduralSword) owner.inventory[owner.selectedItem].modItem, damage, crit);
            }
            catch (SystemException e)
            {
                Main.NewText(e.ToString());
                ModLoader.GetMod("kRPG").Logger.InfoFormat(e.ToString());
            }
        }

        public override void SetDefaults()
        {
            try
            {
                projectile.width = 40;
                projectile.height = 40;
                projectile.scale = 1f;
                projectile.friendly = true;
                projectile.hostile = false;
                projectile.melee = true;
                projectile.penetrate = -1;
                projectile.timeLeft = 600;
                projectile.tileCollide = false;
            }
            catch (SystemException e)
            {
                Main.NewText(e.ToString());
                ModLoader.GetMod("kRPG").Logger.InfoFormat(e.ToString());
            }
        }

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Procedurally Generated Spear; Please Ignore");
        }
    }
}