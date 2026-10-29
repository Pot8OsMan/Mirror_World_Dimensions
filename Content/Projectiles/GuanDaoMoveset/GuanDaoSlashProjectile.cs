using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;

namespace ChinaManWuxiaNovel.Content.Projectiles.GuanDaoMoveset
{
    public class GuanDaoSlashProjectile : ModProjectile
    {
        private const int FRAME_COUNT = 6;
        private const int FRAME_SPEED = 2;

        private const float PROJECTILE_SPEED = 14f;
        private const int PROJECTILE_LIFETIME = 30;

        private int frameCounter;

        public override void SetDefaults()
        {
            Projectile.width = 320;
            Projectile.height = 40;

            Projectile.friendly = true;
            Projectile.hostile = false;

            Projectile.DamageType = DamageClass.Summon;

            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;

            Projectile.timeLeft = PROJECTILE_LIFETIME;

            // Prevent repeated hits on the same NPC.
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;

            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            // Advance independently of the player's weapon animation.
            Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero)
                * PROJECTILE_SPEED;

            Projectile.rotation = Projectile.velocity.ToRotation();

            // Animate the six-frame vertical spritesheet.
            frameCounter++;

            if (frameCounter >= FRAME_SPEED)
            {
                frameCounter = 0;

                Projectile.frame++;

                if (Projectile.frame >= FRAME_COUNT)
                    Projectile.frame = 0;
            }

            // Optional visual trail.
            if (Main.rand.NextBool(3))
            {
                Dust dust = Dust.NewDustPerfect(
                    Projectile.Center,
                    DustID.Torch,
                    -Projectile.velocity * 0.15f,
                    100,
                    default,
                    1.2f
                );

                dust.noGravity = true;
            }
        }

        public override bool PreDraw(
            ref Color lightColor)
        {
            Texture2D texture =
                TextureAssets.Projectile[Type].Value;

            int frameHeight = texture.Height / FRAME_COUNT;

            Rectangle sourceRectangle = new Rectangle(
                0,
                Projectile.frame * frameHeight,
                texture.Width,
                frameHeight
            );

            Vector2 origin = new Vector2(
                texture.Width / 2f,
                frameHeight / 2f
            );

            SpriteEffects effects = SpriteEffects.None;

            // Flip horizontally when traveling left.
            if (Projectile.velocity.X < 0f)
                effects = SpriteEffects.FlipVertically;

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                sourceRectangle,
                Color.White,
                Projectile.rotation,
                origin,
                Projectile.scale,
                effects,
                0
            );

            return false;
        }
    }
}
