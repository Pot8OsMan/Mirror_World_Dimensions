using ChinaManWuxiaNovel.Content.Projectiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ChinaManWuxiaNovel.Content.Items.Weapons
{
    // This is a basic item template.
    // Please see tModLoader's ExampleMod for every other example:
    // https://github.com/tModLoader/tModLoader/tree/stable/ExampleMod
    public class LandOfIllusionGuanDao : ModItem
    {

        public int attackType = 0;	// keeps track of which attack it's on
        public int comboExpireTimer = 1; //reset combo after idle

        // The Display Name and Tooltip of this item can be edited in the 'Localization/en-US_Mods.ChinaMAnFantasy.hjson' file.
        public override void SetDefaults()
        {
            Item.width = 80;
            Item.height = 80;
            Item.rare = ItemRarityID.LightRed;
            Item.value = Item.sellPrice(platinum: 2, gold: 9, silver: 15, copper: 35);

            Item.useTime = 40;
            Item.useAnimation = 40;
            Item.useStyle = ItemUseStyleID.Shoot;

            // Weapon Properties
            Item.knockBack = 4f;
            Item.autoReuse = true;
            Item.damage = 40;
            Item.DamageType = DamageClass.Summon;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.shoot = ModContent.ProjectileType<GuanDaoCustomSwingProjectile>();

            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }

        public override void AddRecipes()
        {
            Recipe recipe = CreateRecipe();
            recipe.AddRecipeGroup("IronBar", 4);
            recipe.AddRecipeGroup("Wood", 6);
            recipe.AddIngredient(ItemID.Silk, 2);
            //recipe.AddIngredient(ModContent.ItemType<JadeEye>(), 1);
            recipe.AddTile(TileID.Anvils);
            recipe.Register();
        }

        public override bool AltFunctionUse(Player player)
        {
            return true;
        }

    }
}
