using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace ChinaManWuxiaNovel.Content.HeishouSummons.NPCs.SiBranchGregor
{
    public class Gregor : ModNPC
    {
        private enum AnimationState
        {
            Idle,
            Walking,
            Attack,
            Hurt
        }

        private AnimationState animationState = AnimationState.Idle;

        private const int IdleStart = 0;
        private const int IdleFrames = 6;

        private const int WalkStart = 4;
        private const int WalkFrames = 6;

        private const int AttackStart = 10;
        private const int AttackFrames = 6;

        private const int HurtStart = 16;
        private const int HurtFrames = 2;

        public override void SetDefaults()
        {
            NPC.width = 20;
            NPC.height = 40;

            NPC.scale = 0.5f;

            NPC.damage = 20;
            NPC.defense = 10;
            NPC.lifeMax = 500;

            NPC.npcSlots = 1;

            NPC.friendly = true;
            NPC.townNPC = false;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.knockBackResist = 0f;
        }

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 4;
        }

        public override void AI()
        {
            FollowOwner();

            if (Math.Abs(NPC.velocity.X) > 0.1f)
                animationState = AnimationState.Walking;
            else
                animationState = AnimationState.Idle;
        }

        private int currentAnimationFrame;
        private int animationTimer;

        private const int FrameWidth = 256;
        private const int FrameHeight = 128;

        private const int IdleRow = 0;
        private const int WalkRow = 1;
        private const int AttackRow = 2;
        private const int HurtRow = 3;

        public override void FindFrame(int frameHeight)
        {
            int row;
            int frameCount;
            int frameSpeed;

            switch (animationState)
            {
                case AnimationState.Idle:
                    row = IdleRow;
                    frameCount = 4;
                    frameSpeed = 8;
                    break;

                case AnimationState.Walking:
                    row = WalkRow;
                    frameCount = 6;
                    frameSpeed = 6;
                    break;

                case AnimationState.Attack:
                    row = AttackRow;
                    frameCount = 6;
                    frameSpeed = 4;
                    break;

                case AnimationState.Hurt:
                    row = HurtRow;
                    frameCount = 2;
                    frameSpeed = 4;
                    break;

                default:
                    row = IdleRow;
                    frameCount = 4;
                    frameSpeed = 8;
                    break;
            }

            animationTimer++;

            if (animationTimer >= frameSpeed)
            {
                animationTimer = 0;
                currentAnimationFrame++;

                if (currentAnimationFrame >= frameCount)
                    currentAnimationFrame = 0;
            }

            // Explicitly select one 64x64 cell from the grid.
            NPC.frame = new Rectangle(
                currentAnimationFrame * FrameWidth,
                row * FrameHeight,
                FrameWidth,
                FrameHeight
            );
        }

        private void FollowOwner()
        {
            int ownerIndex = (int)NPC.ai[0];

            if (ownerIndex < 0 || ownerIndex >= Main.maxPlayers)
            {
                NPC.velocity.X *= 0.9f;
                return;
            }

            Player owner = Main.player[ownerIndex];

            if (!owner.active || owner.dead)
            {
                NPC.velocity.X *= 0.9f;
                return;
            }

            // Follow position slightly behind the owner.
            Vector2 targetPosition = owner.Center;

            targetPosition.X -= owner.direction * 80f;

            Vector2 distance = targetPosition - NPC.Center;

            // Teleport if the companion gets extremely far away.
            if (distance.Length() > 1200f)
            {
                NPC.Center = targetPosition;
                NPC.velocity = Vector2.Zero;
                NPC.netUpdate = true;
                return;
            }

            // Horizontal movement.
            if (Math.Abs(distance.X) > 35f)
            {
                NPC.direction = distance.X > 0 ? 1 : -1;

                NPC.velocity.X += NPC.direction * 0.2f;

                NPC.velocity.X = MathHelper.Clamp(
                    NPC.velocity.X,
                    -4f,
                    4f
                );
            }
            else
            {
                NPC.velocity.X *= 0.8f;

                if (Math.Abs(NPC.velocity.X) < 0.1f)
                    NPC.velocity.X = 0f;
            }
        }
    }
}
