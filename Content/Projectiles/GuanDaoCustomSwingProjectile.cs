using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria;
using Terraria.ModLoader;

namespace ChinaManWuxiaNovel.Content.Projectiles
{
    public class GuanDaoCustomSwingProjectile : ModProjectile
    {
        private const float SWINGRANGE = 1.67f * (float)Math.PI; // The angle a swing attack covers (300 deg)
        private const float FIRSTHALFSWING = 0.45f; // How much of the swing happens before it reaches the target angle (in relation to swingRange)
        private const float SPINRANGE = 3.5f * (float)Math.PI; // The angle a spin attack covers (630 degrees)
        private const float WINDUP = 0.15f; // How far back the player's hand goes when winding their attack (in relation to swingRange)
        private const float UNWIND = 0.4f; // When should the sword start disappearing
        private const float SPINTIME = 2.5f; // How much longer a spin is than a swing


        // =========================================
        // SLASH ANIMATION SETTINGS
        // =========================================

        private const float SLASH_SCALE = 1f;
        private const float WEAPON_REACH = 100f;

        private const int SLASH_FRAME_COUNT = 6;
        private const int SLASH_FRAME_SPEED = 2;

        // =========================================
        // UPPER SLASH ANIMATION SETTINGS
        // =========================================

        private const float UPPER_SLASH_SCALE = 1f;
        private const float UPPERSLASH_WEAPON_REACH = 100f;

        private const int UPPER_SLASH_FRAME_COUNT = 6;
        private const int UPPER_SLASH_FRAME_SPEED = 2;


        // =========================================
        // STAB ANIMATION SETTINGS
        // =========================================

        private const float STAB_DISTANCE = 38f;
        private const float STAB_EXECUTION_MULTIPLIER = 0.8f;
        private const float STAB_RETRACT_MULTIPLIER = 0.6f;

        private const int STAB_FRAME_COUNT = 4;
        private const int STAB_FRAME_SPEED = 1;

        // =========================================
        // SPIN ANIMATION SETTINGS
        // =========================================

        private const float SPIN_REPOSITION_TIME = 5f;
        private const float STOMP_WINDUP_TIME = 5f;
        private const float STOMP_IMPACT_TIME = 3f;
        private const float STOMP_RECOVERY_TIME = 10f;

        private const float STOMP_FORWARD_OFFSET = 5f;
        private const float STOMP_RADIUS = 150f;

        private const float STOMP_RAISE_HEIGHT = 55f;
        private const float STOMP_FORWARD_DISTANCE = 35f;

        // Weapon sprite rotation offsets relative to its spin pose.
        private const float STOMP_RAISED_ANGLE = -0.65f;
        private const float STOMP_DOWN_ANGLE = 0.9f;


        // Path to the vertical sprite sheet.
        private Texture2D SlashTexture =>
            ModContent.Request<Texture2D>(
                "ChinaManWuxiaNovel/Content/Projectiles/GuanDaoMoveset/SlashEffect"
            ).Value;

        // Select which frame to show based on the attack timer.
        private int GetSlashFrame()
        {
            int frame = (int)(Timer / SLASH_FRAME_SPEED);

            return Math.Clamp(
                frame,
                0,
                SLASH_FRAME_COUNT - 1
            );
        }
        
        private enum AttackType // Which attack is being performed
        {
            Swing = 0, // Phase 1: Slash
            ReverseSwing = 1,  // Phase 2: Upper Slash
            Stab = 2,  // Phase 3: Thrust
            SpinStomp = 3    // Phase 4: Spin-stomp
        }

        // Draw the smear separately from the weapon.
        private void DrawSlashEffect(Color lightColor)
        {
            // Only display the smear during a slash.
            if (CurrentAttack != AttackType.Swing ||
                CurrentStage != AttackStage.Execute)
            {
                return;
            }

            Texture2D texture = SlashTexture;

            // The sprite sheet contains 6 vertically stacked frames.
            int frameHeight = texture.Height / SLASH_FRAME_COUNT;
            int frame = GetSlashFrame();

            Rectangle source = new Rectangle(
                0,
                frame * frameHeight,
                texture.Width,
                frameHeight
            );

            // Pivot: bottom-left when facing right,
            // bottom-right when facing left.
            Vector2 origin = Projectile.spriteDirection > 0
                ? new Vector2(0f, frameHeight)
                : new Vector2(texture.Width, frameHeight);

            SpriteEffects effects = Projectile.spriteDirection > 0
                ? SpriteEffects.None
                : SpriteEffects.FlipHorizontally;

            // Match the weapon's existing sprite orientation.
            float rotationOffset = Projectile.spriteDirection > 0
                ? MathHelper.ToRadians(45f)
                : MathHelper.ToRadians(135f);

            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                source,
                lightColor * Projectile.Opacity,
                Projectile.rotation + rotationOffset,
                origin,
                Projectile.scale * SLASH_SCALE,
                effects,
                0f
            );
        }

        private enum AttackStage // What stage of the attack is being executed, see functions found in AI for description
        {
            Prepare,
            Execute,
            Unwind,

            // Flashy weapon handle for one singular move in the weapon's moveset
            StompWindup,
            StompImpact,
            StompRecovery
        }

        // These properties wrap the usual ai and localAI arrays for cleaner and easier to understand code.
        private AttackType CurrentAttack
        {
            get => (AttackType)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        private AttackStage CurrentStage
        {
            get => (AttackStage)Projectile.localAI[0];
            set
            {
                Projectile.localAI[0] = (float)value;
                Timer = 0; // reset the timer when the projectile switches states
            }
        }

        private enum ComboAttack
        {
            Slash,
            ReverseSlash,
            Stab,
            SpinStomp,
        }

        private ComboAttack CurrentCombo
        {
            get => (ComboAttack)Projectile.ai[0];
            set => Projectile.ai[0] = (float)value;
        }

        // Variables to keep track of during runtime
        private ref float InitialAngle => ref Projectile.ai[1]; // Angle aimed in (with constraints)
        private ref float Timer => ref Projectile.ai[2]; // Timer to keep track of progression of each stage
        private ref float Progress => ref Projectile.localAI[1]; // Position of projectile relative to initial angle
        private ref float Size => ref Projectile.localAI[2]; // Size of weapon

        // We define timing functions for each stage, taking into account melee attack speed
        // Note that you can change this to suit the need of your projectile
        private float prepTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

        private float execTime => 6f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

        private float hideTime => 12f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

        //Reverse properties
        private float reverseSwingexecTime => 10f / Owner.GetTotalAttackSpeed(Projectile.DamageType);

        //Stab properties
        private float stabExecTime => execTime * STAB_EXECUTION_MULTIPLIER;

        private float stabHideTime => hideTime * STAB_RETRACT_MULTIPLIER;

        public override string Texture => "ChinaManWuxiaNovel/Content/Items/Weapons/LandOfIllusionGuanDao"; // Projectile texture
        private Player Owner => Main.player[Projectile.owner]; //Player position

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.HeldProjDoesNotUsePlayerGfxOffY[Type] = true;
            ProjectileID.Sets.AllowsContactDamageFromJellyfish[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 46; // Hitbox width of projectile
            Projectile.height = 48; // Hitbox height of projectile
            Projectile.friendly = true; // Projectile hits enemies
            Projectile.timeLeft = 10000; // Time it takes for projectile to expire
            Projectile.penetrate = -1; // Projectile pierces infinitely
            Projectile.tileCollide = false; // Projectile does not collide with tiles
            Projectile.usesLocalNPCImmunity = true; // Uses local immunity frames
            Projectile.localNPCHitCooldown = -1; // We set this to -1 to make sure the projectile doesn't hit twice
            Projectile.ownerHitCheck = false; // Make sure the owner of the projectile has line of sight to the target (aka can't hit things through tile). true BORING!!! / false HELL YEAH!!!
            Projectile.DamageType = DamageClass.Summon; // Projectile is a summoner projectile
        }

        public override void OnSpawn(IEntitySource source)
        {
            CurrentStage = AttackStage.Prepare;

            Projectile.spriteDirection = Main.MouseWorld.X > Owner.MountedCenter.X ? 1 : -1;

            float targetAngle = (Main.MouseWorld - Owner.MountedCenter).ToRotation();

            if (CurrentAttack == AttackType.SpinStomp)
            {
                InitialAngle = (float)(-Math.PI / 2 - Math.PI / 3 * Projectile.spriteDirection); // For the spin, starting angle is designated based on direction of hit
            }
            else if (CurrentAttack == AttackType.ReverseSwing)
            {
                InitialAngle = (float)(Math.PI / 2 *Projectile.spriteDirection);
            }
            else if (CurrentAttack == AttackType.Stab)
            {
                // A stab aims directly at the cursor.
                InitialAngle = targetAngle;
            }
            else
            {
                if (Projectile.spriteDirection == 1)
                {
                    // However, we limit the angle of possible directions so it does not look too ridiculous
                    targetAngle = MathHelper.Clamp(targetAngle, (float)-Math.PI / 3, (float)Math.PI / 6);
                }
                else
                {
                    if (targetAngle < 0)
                    {
                        targetAngle += 2 * (float)Math.PI; // This makes the range continuous for easier operations
                    }

                    targetAngle = MathHelper.Clamp(targetAngle, (float)Math.PI * 5 / 6, (float)Math.PI * 4 / 3);
                }

                InitialAngle = targetAngle - FIRSTHALFSWING * SWINGRANGE * Projectile.spriteDirection; // Otherwise, we calculate the angle
            }
        }

        public override void AI()
        {
            // Extend use animation until projectile is killed
            Owner.itemAnimation = 2;
            Owner.itemTime = 2;

            // Kill the projectile if the player dies or gets crowd controlled
            if (!Owner.active || Owner.dead || Owner.noItems || Owner.CCed)
            {
                Projectile.Kill();
                return;
            }

            // AI depends on stage and attack
            // Note that these stages are to facilitate the scaling effect at the beginning and end
            // If this is not desirable for you, feel free to simplify
            switch (CurrentStage)
            {
                case AttackStage.Prepare:
                    PrepareStrike();
                    break;
                case AttackStage.Execute:
                    ExecuteStrike();
                    break;
                case AttackStage.Unwind:
                    UnwindStrike();
                    break;

                case AttackStage.StompWindup:
                    StompWindup();
                    break;
                case AttackStage.StompImpact:
                    StompImpact();
                    break;
                case AttackStage.StompRecovery:
                    StompRecovery();
                    break;
            }

            SetGuanDaoPosition();
            Timer++;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            // Projectile.spriteDirection for this projectile is derived from the mouse position of the owner in OnSpawn, as such it needs to be synced.
            // spriteDirection is not one of the fields automatically synced over the network. All Projectile.ai slots are used already, so we will sync it manually.
            writer.Write((sbyte)Projectile.spriteDirection);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.spriteDirection = reader.ReadSByte();
        }

        private void CreateSlashDust()
        {
            // Dust is cosmetic; skip spawning on a dedicated server.
            if (Main.netMode == NetmodeID.Server)
                return;

            // Control how frequently the spray appears.
            if (!Main.rand.NextBool(2))
                return;

            Vector2 direction = Projectile.rotation.ToRotationVector2();

            // Perpendicular direction for the sideways spray.
            Vector2 sideways = new Vector2(-direction.Y, direction.X);

            // Spawn several particles per emission.
            int particleCount = Main.rand.Next(2, 5);

            for (int i = 0; i < particleCount; i++)
            {
                // Spawn toward the forward half of the weapon.
                float distance = Main.rand.NextFloat(35f, 100f);

                Vector2 position =
                    Owner.MountedCenter + direction * distance;

                // Add a little sideways variation to the spawn position.
                position += sideways * Main.rand.NextFloat(-8f, 8f);

                int dustType = Main.rand.NextBool(4)
                    ? DustID.BloodWater
                    : DustID.Blood;

                // Spray mostly forward, with random sideways movement.
                Vector2 velocity =
                    direction * Main.rand.NextFloat(1.5f, 5f)
                    + sideways * Main.rand.NextFloat(-2.5f, 2.5f);

                Dust dust = Dust.NewDustPerfect(
                    position,
                    dustType,
                    velocity,
                    80,
                    dustType == DustID.Blood
                        ? Color.OrangeRed
                        : new Color(180, 15, 25),
                    Main.rand.NextFloat(0.9f, 1.6f)
                );

                dust.noGravity = true;
                dust.fadeIn = 0.5f;
            }
        }

        private void CreateStabDust()
        {
            if (Main.netMode == NetmodeID.Server)
                return;

            // Don't spawn particles until the thrust has started.
            if (Progress < 0.15f)
                return;

            // Keep the effect reasonably lightweight.
            if (!Main.rand.NextBool(2))
                return;

            Vector2 direction = InitialAngle.ToRotationVector2();
            Vector2 sideways = new Vector2(-direction.Y, direction.X);

            // Follow the weapon's forward movement.
            Vector2 position =
                Owner.GetFrontHandPosition(
                    Player.CompositeArmStretchAmount.Full,
                    InitialAngle - MathHelper.PiOver2
                );

            position.Y += Owner.gfxOffY;

            position += direction * (STAB_DISTANCE * Progress);

            int count = Main.rand.Next(1, 4);

            for (int i = 0; i < count; i++)
            {
                Vector2 spawnPosition =
                    position + sideways * Main.rand.NextFloat(-8f, 8f);

                Vector2 velocity =
                    direction * Main.rand.NextFloat(2f, 5f)
                    + sideways * Main.rand.NextFloat(-1.5f, 1.5f);

                Dust dust = Dust.NewDustPerfect(
                    spawnPosition,
                    DustID.Blood,
                    velocity,
                    100,
                    Color.OrangeRed,
                    Main.rand.NextFloat(0.8f, 1.3f)
                );

                dust.noGravity = true;
                dust.fadeIn = 0.3f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;

            Vector2 origin;
            float rotationOffset;
            SpriteEffects effects;

            if (Projectile.spriteDirection > 0)
            {
                if (CurrentAttack == AttackType.ReverseSwing)
                {
                    origin = new Vector2(
                        texture.Width,
                        texture.Height
                    );

                    rotationOffset = MathHelper.ToRadians(135f);
                    effects = SpriteEffects.FlipHorizontally;
                }
                else
                {
                    if (CurrentAttack == AttackType.SpinStomp)
                    { 
                        origin = new Vector2(40f, texture.Height - 40f);
                    }
                    else
                    { 
                        origin = new Vector2(0f, texture.Height);
                    }
                    rotationOffset = MathHelper.ToRadians(45f);
                    effects = SpriteEffects.None;
                }
            }
            else
            {
                if (CurrentAttack == AttackType.ReverseSwing)
                {
                    origin = new Vector2(0f, texture.Height);
                    rotationOffset = MathHelper.ToRadians(45f);
                    effects = SpriteEffects.None;
                }
                else {
                    if (CurrentAttack == AttackType.SpinStomp)
                    {
                        origin = new Vector2(
                            texture.Width - 40f,
                            texture.Height - 40f
                        );
                    }
                    else
                    {
                        origin = new Vector2(
                            texture.Width,
                            texture.Height
                        );
                    }
                    rotationOffset = MathHelper.ToRadians(135f);
                    effects = SpriteEffects.FlipHorizontally;
                }
            }

            // Draw the Guan Dao behind the vfx.
            Main.spriteBatch.Draw(
                texture,
                Projectile.Center - Main.screenPosition,
                null,
                lightColor * Projectile.Opacity,
                Projectile.rotation + rotationOffset,
                origin,
                Projectile.scale,
                effects,
                0f
            );

            // Vfx infront the weapon.
            DrawSlashEffect(lightColor);

            // We handled drawing manually.
            return false;
        }

        // Stomp attacks AoE hitbox position
        private Vector2 GetStompCenter()
        {
            float facing = Projectile.spriteDirection;

            return new Vector2(
                Owner.MountedCenter.X + facing * STOMP_FORWARD_OFFSET,
                Owner.Bottom.Y - 8f
            );
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            // The stomp collider uses a circular area rather than drawing a weapon line.
            if (CurrentAttack == AttackType.SpinStomp &&
                CurrentStage == AttackStage.StompImpact)
            {
                Vector2 center = GetStompCenter();

                // Find the closest point on the enemy hitbox.
                float closestX = MathHelper.Clamp(
                    center.X,
                    targetHitbox.Left,
                    targetHitbox.Right
                );

                float closestY = MathHelper.Clamp(
                    center.Y,
                    targetHitbox.Top,
                    targetHitbox.Bottom
                );

                float dx = center.X - closestX;
                float dy = center.Y - closestY;

                return dx * dx + dy * dy
                    <= STOMP_RADIUS * STOMP_RADIUS;
            }

            // Start at the weapon's actual drawing pivot.
            Vector2 start = Projectile.Center;

            // The existing drawing rotation offset compensates for
            // the diagonal weapon sprite, so rotation gives its blade direction.
            Vector2 direction = Projectile.rotation.ToRotationVector2();

            // Match the collision length to the visible weapon.
            Vector2 end = start
                + direction * WEAPON_REACH * Projectile.scale;

            float collisionPoint = 0f;

            return Collision.CheckAABBvLineCollision(
                targetHitbox.TopLeft(),
                targetHitbox.Size(),
                start,
                end,
                8f * Projectile.scale,
                ref collisionPoint
            );
        }

        public override void CutTiles()
        {
            Vector2 start = Projectile.Center;

            Vector2 end = start
                + Projectile.rotation.ToRotationVector2()
                * WEAPON_REACH
                * Projectile.scale;

            Utils.PlotTileLine(
                start,
                end,
                8f * Projectile.scale,
                DelegateMethods.CutTiles
            );
        }

        // We make it so that the projectile can only do damage in its release and unwind phases
        public override bool? CanDamage()
        {
            // Wind-up phase does no dmg
            if (CurrentStage == AttackStage.Prepare)
                return false;

            // Spin is harmless until its impact stage.
            if (CurrentAttack == AttackType.SpinStomp)
            {
                return CurrentStage == AttackStage.StompImpact
                    ? null
                    : false;
            }
            return base.CanDamage();
        }

        private float GetComboDamageMultiplier()
        {
            switch (CurrentAttack)
            {
                case AttackType.Swing:
                    return 1.00f;

                case AttackType.ReverseSwing:
                    return 1.2f;

                case AttackType.Stab:
                    return 1.5f;

                case AttackType.SpinStomp:
                    return 1.8f;

                default:
                    return 1.00f;
            }
        }

        private float GetFinalDamageMultiplier(NPC target)
        {
            float multiplier = GetComboDamageMultiplier();

            // Future conditional modifiers go here.
            // Example: bonus damage against a particular enemy type,
            // a target with your custom status, or a late-game boss.

            return multiplier;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // Make knockback go away from player
            modifiers.HitDirectionOverride = target.position.X > Owner.MountedCenter.X ? 1 : -1;

            modifiers.SourceDamage *= GetFinalDamageMultiplier(target);

            // Stab's peel knockback.
            if (CurrentAttack == AttackType.Stab)
            {
                modifiers.Knockback += 2f;
            }

            // Preserve the spin's extra knockback.
            if (CurrentAttack == AttackType.SpinStomp)
            {
                modifiers.Knockback += 1f;
            }
        }

        // Function to easily set projectile and arm position
        public void SetGuanDaoPosition()
        {
            // Set projectile rotation based on attck types
            if (CurrentAttack == AttackType.Stab)
            {
                // Keep the blade pointed in one direction.
                Projectile.rotation = InitialAngle;
            }
            else
            {
                // Preserve the curved movement of the slash and spin.
                Projectile.rotation =
                    InitialAngle + Projectile.spriteDirection * Progress;
            }

            // Set composite arm allows you to set the rotation of the arm and stretch of the front and back arms independently
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, Projectile.rotation - MathHelper.ToRadians(90f)); // set arm position (90 degree offset since arm starts lowered)
            Vector2 armPosition = Owner.GetFrontHandPosition(Player.CompositeArmStretchAmount.Full, Projectile.rotation - (float)Math.PI / 2); // get position of hand

            // Adjust the position for reversed gravity.
            if (Owner.gravDir == -1f)
            {
                Projectile.rotation = 0f - Projectile.rotation;
                armPosition.Y = Owner.Bottom.Y + (Owner.position.Y - armPosition.Y);
            }

            armPosition.Y += Owner.gfxOffY;

            // Stab motion
            if (CurrentAttack == AttackType.Stab)
            {
                // During a stab, move the entire weapon forward
                // instead of rotating it through a swing arc.
                Vector2 thrustDirection =
                    InitialAngle.ToRotationVector2();

                armPosition += thrustDirection * STAB_DISTANCE * Progress;
            }

            Projectile.Center = armPosition; // Set projectile to arm position
            Projectile.scale = Size * 1.2f * Owner.GetAdjustedItemScale(Owner.HeldItem); // Slightly scale up the projectile and also take into account melee size modifiers

            Owner.heldProj = Projectile.whoAmI; // set held projectile to this projectile
        }

        // Function facilitating the taking out of the sword
        private void PrepareStrike()
        {
            Progress = WINDUP * SWINGRANGE * (1f - Timer / prepTime); // Calculates rotation from initial angle
            Size = MathHelper.SmoothStep(0, 1, Timer / prepTime); // Make sword slowly increase in size as we prepare to strike until it reaches max

            if (Timer >= prepTime)
            {
                SoundEngine.PlaySound(SoundID.Item1); // Play sword sound here since playing it on spawn is too early
                CurrentStage = AttackStage.Execute; // If attack is over prep time, we go to next stage
            }
        }

        // Function facilitating the first half of the swing
        private void ExecuteStrike()
        {
            // PHASE 1: SLASH
            if (CurrentAttack == AttackType.Swing)
            {
                // Move the weapon along its curved swing trajectory.
                Progress = MathHelper.SmoothStep(
                    0f,
                    SWINGRANGE,
                    (1f - UNWIND) * Timer / execTime
                );

                // Emit dust during the slash.
                CreateSlashDust();

                // Finish the attack when its execution time expires.
                if (Timer >= execTime)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }

            // PHASE 2: UPPER SLASH
            else if (CurrentAttack == AttackType.ReverseSwing)
            {
                // Move the weapon along its curved swing trajectory.
                Progress = MathHelper.SmoothStep(
                    0f,
                    -SWINGRANGE,
                    (1f - UNWIND) * Timer / reverseSwingexecTime
                );

                // Emit dust during the slash.
                CreateSlashDust();

                // Finish the attack when its execution time expires.
                if (Timer >= execTime)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }

            // PHASE 3: STAB
            else if (CurrentAttack == AttackType.Stab)
            {
                // Thrust from the hand toward the target.
                Progress = MathHelper.SmoothStep(
                    0f,
                    1f,
                    MathHelper.Clamp(Timer / stabExecTime, 0f, 1f)
                );

                CreateStabDust();

                if (Timer >= stabExecTime)
                {
                    CurrentStage = AttackStage.Unwind;
                }
            }

            // Existing spin attack.
            else if (CurrentAttack == AttackType.SpinStomp)
            {
                Progress = MathHelper.SmoothStep(
                    0f,
                    SPINRANGE,
                    //(1f - UNWIND / 2f) * Timer / (execTime * SPINTIME)
                    MathHelper.Clamp( Timer / SPIN_REPOSITION_TIME, 0f, 1f )
                );

                if (Timer >= SPIN_REPOSITION_TIME)
                {
                    CurrentStage = AttackStage.StompWindup;
                }
            }

        }

        // Function facilitating the latter half of the swing where the sword disappears
        private void UnwindStrike()
        {
            if (CurrentAttack == AttackType.Swing)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
                //Size = 1f - MathHelper.SmoothStep(0.5f, 1, Timer / hideTime); // Make weapon slowly decrease in size as we end the swing to make a smooth hiding animation

                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }

            if (CurrentAttack == AttackType.ReverseSwing)
            {
                Progress = MathHelper.SmoothStep(0, SWINGRANGE, (1f - UNWIND) + UNWIND * Timer / hideTime);
                //Size = 1f - MathHelper.SmoothStep(0.5f, 1, Timer / hideTime); // Make weapon slowly decrease in size as we end the swing to make a smooth hiding animation

                if (Timer >= hideTime)
                {
                    Projectile.Kill();
                }
            }

            // STAB RECOVERY
            else if (CurrentAttack == AttackType.Stab)
            {
                // Retract from the fully extended position back to the hand.
                Progress = 1f - MathHelper.SmoothStep(
                    0f,
                    1f,
                    MathHelper.Clamp(Timer / stabHideTime, 0f, 1f)
                );

                if (Timer >= stabHideTime)
                {
                    Projectile.Kill();
                }
            }

            // SPIN RECOVERY
            else
            {
                Progress = MathHelper.SmoothStep(0, SPINRANGE, (1f - UNWIND / 2) + UNWIND / 2 * Timer / (hideTime * SPINTIME / 2));
                Size = 1f - MathHelper.SmoothStep(0, 1, Timer / (hideTime * SPINTIME / 2));

                if (Timer >= hideTime * SPINTIME / 2)
                {
                    Projectile.Kill();
                }
            }
        }

        // Whole ass moveset for one flashy Stomp attack
        private void StompWindup()
        {
            // Hold the weapon in its repositioned pose.
            // Animate the weapon moving upward/downward in front of player model
            // in SetGuanDaoPosition() as the next visual refinement.
            Progress = SPINRANGE;

            if (Timer >= STOMP_WINDUP_TIME)
            {
                CurrentStage = AttackStage.StompImpact;
            }
        }

        private void StompImpact()
        {
            // Keep the weapon at the impact pose.
            Progress = SPINRANGE;

            if (Timer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            }

            // Damage is enabled only during this stage.
            if (Timer >= STOMP_IMPACT_TIME)
            {
                CurrentStage = AttackStage.StompRecovery;
            }
        }

        private void StompRecovery()
        {
            // Brief recovery before the combo projectile disappears.
            Progress = SPINRANGE;

            if (Timer >= STOMP_RECOVERY_TIME)
            {
                Projectile.Kill();
            }
        }

    }
}
