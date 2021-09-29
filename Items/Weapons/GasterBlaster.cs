﻿using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using GamerClass.Projectiles.GasterBlaster;
using Microsoft.Xna.Framework;

namespace GamerClass.Items.Weapons
{
    public class GasterBlaster : GamerWeapon
    {
        public override string Texture => "Terraria/Item_" + ItemID.Skull;
        public override int RamUsage => 0;

        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Funny Looking Skull");
            Tooltip.SetDefault("'It wants to tell you a pun about skeletons'");
        }

        public override void SafeSetDefaults()
        {
            item.noMelee = true;
            item.damage = 60;
            item.rare = ItemRarityID.Cyan;
            item.value = Item.sellPrice(gold: 10);
            item.useStyle = ItemUseStyleID.SwingThrow;
            item.useTime = item.useAnimation = 60;
            item.UseSound = SoundID.Item78;
            item.autoReuse = true;
            item.shoot = ModContent.ProjectileType<GasterSkull>();
        }

        public override bool Shoot(Player player, ref Vector2 position, ref float speedX, ref float speedY, ref int type, ref int damage, ref float knockBack)
        {
            float spawnRange = 600f;

            Vector2 spawnAt = new Vector2(Main.MouseWorld.X + Main.rand.NextFloat(-spawnRange, spawnRange), 580f);
            if (Main.rand.NextBool()) spawnAt.Y *= -1f;

            spawnAt.Y += player.Center.Y;

            Vector2 toMouse = Main.MouseWorld - spawnAt;
            Vector2 velocity = toMouse * Main.rand.NextFloat(0.03f, 0.04f);

            float rotation = Main.rand.NextFloat(MathHelper.TwoPi);

            Projectile.NewProjectile(spawnAt, velocity, type, damage, knockBack, player.whoAmI, 0f, rotation);

            return false;
        }
    }
}