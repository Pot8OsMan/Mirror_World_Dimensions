using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        private const int IdleFrames = 4;

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

            NPC.damage = 0;
            NPC.defense = 10;
            NPC.lifeMax = 500;

            NPC.npcSlots = 1;

            NPC.friendly = true;
            NPC.townNPC = false;

            NPC.noGravity = false;
            NPC.noTileCollide = false;

            NPC.knockBackResist = 0f;
        }

        public override void AI()
        {
            // Movement AI will be implemented here. When I'm less lazy
            if (Math.Abs(NPC.velocity.X) > 0.1f)
                animationState = AnimationState.Walking;
            else
                animationState = AnimationState.Idle;
        }

        public override void FindFrame(int frameHeight)
        {
            int startFrame;
            int frameCount;
            int frameSpeed;

            switch (animationState)
            {
                case AnimationState.Idle:
                    startFrame = IdleStart;
                    frameCount = IdleFrames;
                    frameSpeed = 6;
                    break;

                case AnimationState.Walking:
                    startFrame = WalkStart;
                    frameCount = WalkFrames;
                    frameSpeed = 5;
                    break;

                case AnimationState.Attack:
                    startFrame = AttackStart;
                    frameCount = AttackFrames;
                    frameSpeed = 4;
                    break;

                case AnimationState.Hurt:
                    startFrame = HurtStart;
                    frameCount = HurtFrames;
                    frameSpeed = 3;
                    break;

                default:
                    startFrame = IdleStart;
                    frameCount = IdleFrames;
                    frameSpeed = 6;
                    break;
            }

            NPC.frameCounter++;

            if (NPC.frameCounter >= frameSpeed)
            {
                NPC.frameCounter = 0;
                NPC.frame.Y += frameHeight;
            }

            int currentFrame = NPC.frame.Y / frameHeight;

            if (currentFrame < startFrame ||
                currentFrame >= startFrame + frameCount)
            {
                NPC.frame.Y = startFrame * frameHeight;
            }
        }
    }
}
