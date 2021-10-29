﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace GamerClass.Projectiles.DetachedGlove
{
    public class ChaserShot : ModProjectile
    {
        private readonly float RangeSQ = (float)Math.Pow(1000, 2);

        public override void SetStaticDefaults()
        {
            Main.projFrames[projectile.type] = 7;
            ProjectileID.Sets.Homing[projectile.type] = true;
        }

        public override void SetDefaults()
        {
            projectile.width = projectile.height = 12;
            projectile.friendly = true;
            projectile.timeLeft = 600;
            projectile.frame = Main.rand.Next(Main.projFrames[projectile.type]);
            projectile.alpha = 60;
            projectile.GamerProjectile().gamer = true;
        }

        public int CurrentTarget
        {
            get => (int)projectile.ai[0];
            set => projectile.ai[0] = value;
        }

        public override void AI()
        {
            if (CurrentTarget == -1) FindTarget();

            if (CurrentTarget > -1)
            {
                NPC target = Main.npc[CurrentTarget];


                if (target.active)
                {
                    float speed = 12f;
                    float inertia = 12f;
                    Vector2 direction = Vector2.Normalize(target.Center - projectile.Center);
                    projectile.velocity = (projectile.velocity * (inertia - 1) + direction * speed) / inertia;
                } else
                {
                    CurrentTarget = -1;
                }
            }

            RunAnimation();
            SpawnDusts();

            Lighting.AddLight(projectile.Center, Color.LightGreen.ToVector3() * 0.8f);
        }

        private void RunAnimation()
        {
            if (++projectile.frameCounter > 2)
            {
                projectile.frameCounter = 0;
                if (++projectile.frame >= Main.projFrames[projectile.type])
                {
                    projectile.frame = 0;
                }
            }

            projectile.rotation = projectile.velocity.ToRotation();
        }
        
        private void SpawnDusts()
        {
            float spread = MathHelper.PiOver4 * 0.8f;
            for (int d = 0; d < 3; d++)
            {
                Dust dust = Dust.NewDustDirect(projectile.position, projectile.width, projectile.height, DustID.GreenFairy);
                dust.noGravity = true;
                dust.velocity = Vector2.Normalize(projectile.velocity).RotatedBy(Main.rand.NextFloat(-spread, spread)) * dust.velocity.Length();
            }
        }

        public override void Kill(int timeLeft)
        {
            int lines = 4;
            for (int l = 0; l < lines; l++)
            {

                Vector2 direction = Main.rand.NextVector2CircularEdge(1f, 1f);
                float maxSpeed = Main.rand.NextFloat(4f, 5f);
                float dusts = Main.rand.Next(2, 8);
                float scale = Main.rand.NextFloat(1f, 1.6f);

                for (int d = 0; d < dusts; d++)
                {
                    float speed = maxSpeed - (d * 0.4f);

                    Dust dust = Dust.NewDustPerfect(projectile.Center, DustID.GreenFairy);
                    dust.noGravity = true;
                    dust.scale = scale;
                    dust.velocity = direction * speed;
                }
            }
        }

        private void FindTarget()
        {
            if (CurrentTarget > -1) return;

            int currentNPC = -1;
            float currentDistanceSQ = -1f;

            for (int i = 0; i < Main.maxNPCs; i++)
            {
                NPC npc = Main.npc[i];
                if (!npc.CanBeChasedBy()) continue;

                float distanceSQ
                    = Vector2.DistanceSquared(npc.Center, projectile.Center);

                bool inRange = distanceSQ <= RangeSQ;
                bool closest = currentDistanceSQ == -1f || distanceSQ < currentDistanceSQ;
                bool inSight = Collision.CanHitLine(projectile.position, projectile.width, projectile.height, npc.position, npc.width, npc.height);

                if (inRange && closest && inSight)
                {
                    currentNPC = i;
                    currentDistanceSQ = distanceSQ;
                }
            }

            CurrentTarget = currentNPC;
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Color lightColor)
        {
            Texture2D texture = Main.projectileTexture[projectile.type];
            int frameHeight = texture.Height / Main.projFrames[projectile.type];

            Rectangle sourceRectangle = new Rectangle(0, projectile.frame * frameHeight, texture.Width, frameHeight);
            Vector2 origin = new Vector2(texture.Width, frameHeight) / 2;
            Color color = GetAlpha(lightColor) ?? lightColor;

            // Afterimages
            if (projectile.alpha < 100)
            {
                int trails = 4;
                for (int i = 1; i <= trails; i++)
                {
                    int reverseIndex = trails - i + 1;
                    Vector2 position = projectile.Center - projectile.velocity * reverseIndex * 0.3f;

                    spriteBatch.Draw(
                        texture,
                        position - Main.screenPosition,
                        sourceRectangle,
                        color * (projectile.Opacity * i * 0.08f),
                        projectile.rotation,
                        origin,
                        projectile.scale * 0.9f,
                        SpriteEffects.None,
                        0f);
                }
            }

            spriteBatch.Draw(
                texture,
                projectile.Center - Main.screenPosition,
                sourceRectangle,
                color * projectile.Opacity,
                projectile.rotation,
                origin,
                projectile.scale,
                SpriteEffects.None,
                0f);

            return false;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
    }
}
