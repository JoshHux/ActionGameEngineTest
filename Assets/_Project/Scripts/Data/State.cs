using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using BEPUutilities;
using FixMath.NET;
using Spax.Input;

namespace Spax.StateMachine
{
    //reason for transitioning to state
    [Flags]
    public enum TransitionCondition : uint
    {
        HIT_CONFIRM = 1 << 11,
        CAUSE_FOR_BLOCK = 1 << 12,
        GET_HIT = 1 << 13,
        GROUNDED = 1 << 14,
        AERIAL = 1 << 15,
        ON_END = 1 << 16,
        QUART_METER = 1 << 17,
        HALF_METER = 1 << 18,
        FULL_METER = 1 << 19,
        CANJUMP = 1 << 20
    }

    [Flags]
    public enum EnterStateConditions : uint
    {
        //NOTHING = 0,  // 0
        KILL_X_MOMENTUM = 1 << 0, // 1
        KILL_Y_MOMENTUM = 1 << 1, // 1
        KILL_Z_MOMENTUM = 1 << 2, // 1
        CAUSE_FOR_BLOCK = 1 << 3 // 1
    }

    [Flags]
    public enum ExitStateConditions : uint
    {
        CLEAN_HITBOXES = 1 << 1,
        EXIT_STUN = 1 << 2,
        CAUSE_FOR_BLOCK = 1 << 3 // 1
    }

    [Flags]
    public enum StateConditions : uint
    {
        CAN_MOVE = 1 << 1, // 1
        APPLY_GRAV = 1 << 2, // 1
        NO_PARENT_TRANS = 1 << 3,
        CAN_TURN = 1 << 4,
        APPLY_FRICTION = 1 << 5,
        WALKING = 1 | (1 << 6),
        TRANSITION_TO_SELF = 1 << 7,
        NO_PARENT_COND = 1 << 8,
        GUARD_POINT = 1 << 9,
        STUN_STATE = 1 << 10
    }

    [System.Serializable]
    public struct CharacterStateTransition
    {
        public int TargetStateID;

        public uint TargetFrame;

        public uint MinFrame;

        public TransitionCondition[] Conditions;
    }

    [System.Serializable]
    public struct CharacterState
    {
        public string Name;

        public CharacterFrame[] Frames;

        public CharacterStateTransition[] Transitions;
    }

    [Flags]
    public enum FrameFlags : byte
    {
        INVULNERABLE = 1 << 0,
        AUTO_JUMP = 1 << 1,
        APPLY_VEL = 1 << 2,
        SET_VEL = 1 << 2 | 1 << 3,
        SET_ROT_AROUND = 1 << 4,
        PLAY_AUDIO = 1 << 5,
        SPAWN_PROJECTILE = 1 << 6
    }

    [System.Serializable]
    public struct CharacterFrame
    {
        //public const int kMaxPlayerHitboxCount = sizeof(HitboxBitfield) * 8;
        public int atFrame;

        public FrameFlags flags;

        public BepuVector3 velocity;

        public HitBoxData[] hitboxes;

        public HurtBoxData[] hurtboxes;

        public int AudioID;

        public StateConditions stateConditions;

        public CancelCondition cancelCondition;

        public bool HasHitboxes()
        {
            return (hitboxes != null) && (hitboxes.Length > 0);
        }

        public bool HasHurtboxes()
        {
            return (hurtboxes != null) && (hurtboxes.Length > 0);
        }

        public void Prepare()
        {
            if (hitboxes != null)
            {
                int len = hitboxes.Length;

                //assigns each hitbox priority
                for (int i = 0; i < len; i++)
                {
                    hitboxes[i].priority = i;
                }
            }
        }

        public CharacterFrame DeepCopy()
        {
            CharacterFrame ret = new CharacterFrame();
            ret.atFrame = this.atFrame;
            ret.flags = this.flags;
            ret.velocity =
                new BepuVector3(this.velocity.X,
                    this.velocity.Y,
                    this.velocity.Z);

            //note to self: create a better way of making deep copies
            int len = this.hitboxes.Length;
            HitBoxData[] newArrayHit = new HitBoxData[len];
            for (int i = 0; i < len; i++)
            {
                newArrayHit[i] = hitboxes[i].DeepCopy();
            }
            ret.hitboxes = newArrayHit;

            len = this.hurtboxes.Length;
            HurtBoxData[] newArrayHurt = new HurtBoxData[len];
            for (int i = 0; i < len; i++)
            {
                newArrayHurt[i] = hurtboxes[i].DeepCopy();
            }
            ret.hurtboxes = newArrayHurt;

            ret.AudioID = this.AudioID;

            return ret;
        }
    }

    [System.Serializable]
    public struct HitBoxData
    {
        public int priority;

        public int duration;

        public BepuVector3 offset;

        public BepuVector3 size;

        public Fix64 launchAngle;

        public Fix64 launchForce;

        public int damage;

        public int chipDamage;

        public int hitstop;

        public int hitstun;

        public int untechTime;

        public int blockstun;

        public HitboxType type;

        public RenderType renderType;

        public CancelCondition onHitCancel;

        public HitBoxData DeepCopy()
        {
            HitBoxData ret = new HitBoxData();
            ret.priority = this.priority;
            ret.duration = this.duration;
            ret.offset =
                new BepuVector3(this.offset.X, this.offset.Y, this.offset.Z);
            ret.size = new BepuVector3(this.size.X, this.size.Y, this.size.Z);
            ret.launchAngle = this.launchAngle;
            ret.launchForce = this.launchForce;
            ret.damage = this.damage;
            ret.chipDamage = this.chipDamage;
            ret.hitstop = this.hitstop;
            ret.hitstun = this.hitstun;
            ret.untechTime = this.untechTime;
            ret.blockstun = this.blockstun;
            ret.type = this.type;
            ret.renderType = this.renderType;
            ret.onHitCancel = this.onHitCancel;
            return ret;
        }
    }

    [System.Serializable]
    public struct HurtBoxData
    {
        public BepuVector3 offset;

        public BepuVector3 size;

        public HurtBoxData DeepCopy()
        {
            HurtBoxData ret = new HurtBoxData();
            ret.offset =
                new BepuVector3(this.offset.X, this.offset.Y, this.offset.Z);
            ret.size = new BepuVector3(this.size.X, this.size.Y, this.size.Z);
            return ret;
        }
    }

    public enum HitboxType
    {
        STRIKE = 1 << 0,
        GRAB = 1 << 1,
        LEFT = 1 << 2,
        RIGHT = 1 << 3,
        MIDDLE = 1 << 4,
        UNBLOCKABLE = 1 << 5,
        AIR_UNBLOCKABLE = 1 << 6
    }

    public enum RenderType
    {
        LIGHT = 1 << 0, //1
        MEDIUM = 1 << 1, //2
        HEAVY = 1 << 2, //4
        LEFT = 1 << 3, //8
        RIGHT = 1 << 4, //16
        MID = 1 << 5 //32
    }
}
