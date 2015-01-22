using Microsoft.Xna.Framework.Content;
using Manifracture;
using Microsoft.Xna.Framework;
using SlagformCommon;
using System;
using System.Collections.Generic;
using BEPUphysics.CollisionShapes.ConvexShapes;
using RigidTransform = BEPUutilities.RigidTransform;
using BepuVec3 = BEPUutilities.Vector3;
using BepuRay = BEPUutilities.Ray;
using BepuQuaternion = BEPUutilities.Quaternion;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries.MobileCollidables;
using BEPUphysics.BroadPhaseEntries;

namespace Mechadrone1
{
    class Zombie : Behavior, IAgentStateManager
    {
        private SteeringBlender mSteering;
        private AgentState mCurrentState;
        public AgentState CurrentState
        {
            get
            {
                return mCurrentState;
            }
            set
            {
                if (value != mCurrentState)
                {
                    mCurrentState = value;
                    TimeInState = TimeSpan.Zero;
                    mCurrentState.Enter(mSteering, Owner, this);
                }
            }
        }
        public TimeSpan TimeInState { get; set; }
        private Dictionary<AgentPropertyName, object> mAgentProperties;
        private Memory mMemory;
        public float VisionDistance { get; set; }

        public Zombie(Actor owner)
            : base(owner)
        {
            mSteering = new SteeringBlender();
            CurrentState = null;
            TimeInState = TimeSpan.Zero;
            mAgentProperties = new Dictionary<AgentPropertyName, object>();
            mMemory = new Memory();
            VisionDistance = 340.0f;
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            Owner.ActorInitialized += ActorInitializedHandler;
            GameResources.ActorManager.ProcessAIStep += ProcessAIStepHandler;
        }

        private void ActorInitializedHandler(object sender, EventArgs e)
        {
            BipedControllerComponent bcc = Owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            mAgentProperties.Add(AgentPropertyName.HomePosition, BepuConverter.Convert(bcc.Controller.Body.Position));
            mAgentProperties.Add(AgentPropertyName.TerritoryRadius, 150.0f);
            CurrentState = new ZombieWaitState(6.0f);
        }

        private class ViewInterestFilter
        {
            private BroadPhaseEntry mSelf;

            public ViewInterestFilter(BroadPhaseEntry self)
            {
                mSelf = self;
            }

            public bool Test(BroadPhaseEntry test)
            {
                // This filter should return only actors with potential of causing an addition to Memory.
                bool isPlayer = false;
                bool isMob = false;

                EntityCollidable ec = test as EntityCollidable;
                if (ec != null &&
                    ec.Entity != null &&
                    ec.Entity.Tag != null)
                {
                    int viewedActorId = (int)(ec.Entity.Tag);
                    Actor viewedActor = GameResources.ActorManager.GetActorById(viewedActorId);
                    isPlayer = GameResources.ActorManager.IsPlayer(viewedActorId);
                    isMob = GameResources.ActorManager.IsMob(viewedActorId);
                }

                return test != mSelf && (isPlayer || isMob);
            }
        }

        private void ProcessAIStepHandler(object sender, UpdateStepEventArgs e)
        {
            // Check FOV, add any new foes to memory. And update existing ones. We may also have gained new memories by other means.

            // Get players and mobs in field of vision:
            List<RayCastResult> actorsInView = new List<RayCastResult>();

            ConeShape visionCone = new ConeShape(VisionDistance, VisionDistance);
            BipedControllerComponent bcc = Owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            RigidTransform tipOverCone = new RigidTransform(BepuVec3.Forward * VisionDistance * 0.75f,
                BepuQuaternion.CreateFromAxisAngle(BepuVec3.Right, MathHelper.PiOver2));
            RigidTransform eyeLevelAndFacing = new RigidTransform(bcc.Controller.Body.Position - bcc.Controller.Down * bcc.Controller.Body.Height * 0.45f,
                bcc.Controller.Body.Orientation);
            RigidTransform visionConeTransform;
            RigidTransform.Transform(ref tipOverCone, ref eyeLevelAndFacing, out visionConeTransform);
            BepuVec3 sweep = BepuVec3.Zero;
            ViewInterestFilter filter = new ViewInterestFilter(bcc.Controller.Body.CollisionInformation);
            GameResources.ActorManager.SimSpace.ConvexCast(visionCone, ref visionConeTransform, ref sweep, filter.Test, actorsInView);

            for (int a = 0; a < actorsInView.Count; ++a)
            {
                // Does this actor warrant an addition to be made to our memory?
                // If so, check for LOS and recheck range. If those tests pass, modify the memory.
                EntityCollidable otherEntityCollidable = actorsInView[a].HitObject as EntityCollidable;
                // We can jump to the Id in the Tag property because we know the filter has validated this.
                int actorId = (int)(otherEntityCollidable.Entity.Tag);
                Actor viewedActor = GameResources.ActorManager.GetActorById(actorId);
                BipedControllerComponent viewedActorBcc = viewedActor.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
                BepuVec3 toSubject = viewedActorBcc.Controller.Body.Position - eyeLevelAndFacing.Position;

                // Check range:
                if (toSubject.LengthSquared() <= VisionDistance * VisionDistance)
                {
                    BepuRay losRay = new BepuRay(eyeLevelAndFacing.Position, toSubject);

                    RayCastResult losResult;
                    LOSFilter losFilter = new LOSFilter(bcc.Controller.Body.CollisionInformation, otherEntityCollidable);
                    GameResources.ActorManager.SimSpace.RayCast(losRay, VisionDistance, losFilter.Test, out losResult);
                    EntityCollidable losEC = losResult.HitObject as EntityCollidable;

                    // Test for LOS:
                    if (losEC != null &&
                        losEC.Entity != null &&
                        losEC.Entity.Tag != null &&
                        (int)(losEC.Entity.Tag) == actorId)
                    {
                        // The viewed actor is either a player(foe) or a mob(ally).
                        if (GameResources.ActorManager.IsPlayer(actorId))
                        {
                            mMemory.SpotFoe(actorId);
                        }
                        else
                        {
                            IAgentStateManager agent = viewedActor.GetBehaviorThatImplementsType<IAgentStateManager>();
                            if (agent != null &&
                                agent.HasProperty(AgentPropertyName.ActiveOpponent))
                            {
                                int mobFoe = agent.GetProperty<int>(AgentPropertyName.ActiveOpponent);
                                mMemory.SenseFoe(mobFoe);
                            }
                        }
                    }
                }
            }

            // Evaluate current threats and select one to engage:
            int enemyId = mMemory.GetLargestThreat();
            if (enemyId != Actor.INVALID_ACTOR_ID)
            {
                if (mAgentProperties.ContainsKey(AgentPropertyName.ActiveOpponent))
                {
                    if ((int)(mAgentProperties[AgentPropertyName.ActiveOpponent]) != enemyId)
                        mAgentProperties[AgentPropertyName.ActiveOpponent] = enemyId;
                }
                else
                {
                    mAgentProperties.Add(AgentPropertyName.ActiveOpponent, enemyId);
                }
            }

            TimeInState += e.GameTime.ElapsedGameTime;
            CurrentState.Update(mSteering, Owner, this);
            Vector2 locomotion = mSteering.ComputeForce(Owner);

            if (locomotion.LengthSquared() == 0.0f)
            {
                bcc.OrientationChange = Quaternion.Identity;
                bcc.HorizontalMovement = Vector2.Zero;
            }
            else
            {
                bcc.OrientationChange = Quaternion.CreateFromAxisAngle(Vector3.Up, (float)(Math.Atan2(-locomotion.X, locomotion.Y)));
                bcc.HorizontalMovement = locomotion.Length() * Vector2.UnitY;
            }

            mMemory.Fade(e.GameTime);
        }

        public T GetProperty<T>(AgentPropertyName name)
        {
            return (T)(mAgentProperties[name]);
        }

        public bool HasProperty(AgentPropertyName name)
        {
            return mAgentProperties.ContainsKey(name);
        }

        public override void Release()
        {
            GameResources.ActorManager.ProcessAIStep -= ProcessAIStepHandler;
        }
    }

    class LOSFilter
    {
        // TODO: P2: We should have a mode where this can distinguish between LOS and LOE.
        private BroadPhaseEntry mSelf;
        private BroadPhaseEntry mOther;

        public LOSFilter(BroadPhaseEntry self, BroadPhaseEntry other)
        {
            mSelf = self;
            mOther = other;
        }

        public bool Test(BroadPhaseEntry test)
        {
            if (test == mSelf)
                return false;

            if (test == mOther)
                return true;

            // TODO: P2: We need a more robust way of determining what kinds of objects can block LOS.
            EntityCollidable ec = test as EntityCollidable;
            if (ec != null &&
                ec.Entity != null &&
                ec.Entity.Tag != null)
            {
                int actorId = (int)(ec.Entity.Tag);
                Actor testActor = GameResources.ActorManager.GetActorById(actorId);

                DynamicCollisionComponent dcc = testActor.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
                const float SMALL_OBJECT_THRESHOLD = 64.0f;
                if (dcc != null &&
                    dcc.Entity.LinearVelocity.LengthSquared() > 1.0f &&
                    dcc.Entity.Volume < SMALL_OBJECT_THRESHOLD)
                    return false;
            }

            return true;
        }
    }

    class ZombieWaitState : AgentState
    {
        public ZombieWaitState(float avgDurationSeconds)
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * (Math.Max(avgDurationSeconds + SlagMath.GenerateGaussianNoise(avgDurationSeconds * avgDurationSeconds * 0.25f,
                GameResources.ActorManager.Random), 0.25f)));
        }

        public override void Enter(SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            steering.Weights[(int)SteeringBlender.WeightType.Arrive] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Seek] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Wander] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Wait] = 1.0f;
            steering.Urgency = 1.0f;
            IBipedWeaponEquippable asw = owner.GetBehaviorThatImplementsType<IBipedWeaponEquippable>();
            asw.Weapon.CurrentOperation = WeaponFunctions.Neutral;
        }

        public override void Update(/* inout */ SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            if (agent.HasProperty(AgentPropertyName.ActiveOpponent))
            {
                ZombieCombatState zcs = new ZombieCombatState();
                agent.CurrentState = zcs;
            }
            else if (agent.TimeInState.Ticks >= mDurationTicks)
            {
                ZombieTurnState zts = new ZombieTurnState();
                agent.CurrentState = zts;
            }
        }
    }

    class ZombieTurnState : AgentState
    {
        public ZombieTurnState()
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * 0.1f);
        }

        public override void Enter(SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            float theta = SlagMath.Get0To1UpperFloat(GameResources.ActorManager.Random) * MathHelper.TwoPi;
            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);

            steering.Weights[(int)SteeringBlender.WeightType.Arrive] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Seek] = 1.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Wander] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Wait] = 0.0f;
            steering.Urgency = 1.0f;
            steering.Target = BepuConverter.Convert(bcc.Controller.Body.Position) + new Vector3((float)(Math.Sin(theta)), 0.0f, (float)(Math.Cos(theta)));
            steering.ForceScale = Single.Epsilon;
        }

        public override void Update(/* inout */ SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            if (agent.HasProperty(AgentPropertyName.ActiveOpponent))
            {
                ZombieCombatState zcs = new ZombieCombatState();
                agent.CurrentState = zcs;
            }
            else if (agent.TimeInState.Ticks >= mDurationTicks)
            {
                ZombieWanderState zws = new ZombieWanderState(8.0f);
                agent.CurrentState = zws;
            }

            steering.ForceScale = (float)(agent.TimeInState.Ticks) / (float)mDurationTicks;
        }
    }

    class ZombieWanderState : AgentState
    {
        public ZombieWanderState(float avgDurationSeconds)
        {
            mDurationTicks = (long)(TimeSpan.TicksPerSecond * (Math.Max(avgDurationSeconds + SlagMath.GenerateGaussianNoise(avgDurationSeconds * avgDurationSeconds * 0.25f,
                GameResources.ActorManager.Random), 0.25f)));
        }

        public override void Enter(SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            steering.Weights[(int)SteeringBlender.WeightType.Arrive] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Wait] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Seek] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Wander] = 1.0f;
            steering.Urgency = 1.0f;
            IAgentStateManager asm = owner.GetBehaviorThatImplementsType<IAgentStateManager>();
            steering.Target = asm.GetProperty<Vector3>(AgentPropertyName.HomePosition);
            steering.ForceScale = 0.5f;
        }

        public override void Update(/* inout */ SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            if (agent.HasProperty(AgentPropertyName.ActiveOpponent))
            {
                ZombieCombatState zcs = new ZombieCombatState();
                agent.CurrentState = zcs;
            }
            else if (agent.TimeInState.Ticks >= mDurationTicks)
            {
                ZombieWaitState zws = new ZombieWaitState(8.0f);
                agent.CurrentState = zws;
            }

            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            Vector3 displacementFromHome = BepuConverter.Convert(bcc.Controller.Body.Position) - steering.Target;
            displacementFromHome.Y = 0.0f;

            float tr = agent.GetProperty<float>(AgentPropertyName.TerritoryRadius);

            steering.Weights[(int)SteeringBlender.WeightType.Seek] = Math.Min(Math.Max(displacementFromHome.Length() - tr * 0.75f, 0.0f)
                / tr, 1.0f);
            steering.Weights[(int)SteeringBlender.WeightType.Wander] = 1.0f - steering.Weights[(int)SteeringBlender.WeightType.Seek];
        }
    }

    enum AgentPropertyName
    {
        HomePosition,
        TerritoryRadius,
        ActiveOpponent,
    }

    interface IAgentStateManager
    {
        AgentState CurrentState { get; set; }
        TimeSpan TimeInState { get; set; }
        // Be aware of which properties are required by the AgentStates you use.
        T GetProperty<T>(AgentPropertyName name);
        bool HasProperty(AgentPropertyName name);
    }

    class ZombieCombatState : AgentState
    {
        public ZombieCombatState()
        {
            mDurationTicks = TimeSpan.TicksPerSecond;   // Just a dummy value, it's not used.
        }

        public override void Enter(/* inout */ SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            // We just want to change our facing to track our foe.
            steering.Weights[(int)SteeringBlender.WeightType.Arrive] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Seek] = 1.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Wander] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Wait] = 0.0f;
            steering.Urgency = 1.0f;
            steering.ForceScale = 0.0001f;
            // Temporarily set the target directly ahead.
            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            steering.Target = BepuConverter.Convert(bcc.Controller.Body.Position + bcc.Controller.HorizontalViewDirection);
        }

        public override void Update(/* inout */ SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            if (!agent.HasProperty(AgentPropertyName.ActiveOpponent))
            {
                ZombieWaitState zws = new ZombieWaitState(6.0f);
                agent.CurrentState = zws;
                return;
            }

            Actor opponent = GameResources.ActorManager.GetActorById(agent.GetProperty<int>(AgentPropertyName.ActiveOpponent));
            BipedControllerComponent opponentBcc = opponent.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            steering.Target = BepuConverter.Convert(opponentBcc.Controller.Body.Position);

            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);

            IBipedWeaponEquippable attackBehavior = owner.GetBehaviorThatImplementsType<IBipedWeaponEquippable>();
            Matrix firePoint = attackBehavior.Weapon.FirePoint * Matrix.CreateWorld(BepuConverter.Convert(bcc.Controller.Body.Position),
                BepuConverter.Convert(bcc.Controller.ViewDirection), Vector3.Up);

            BepuVec3 bulletPath = opponentBcc.Controller.Body.Position - BepuConverter.Convert(firePoint.Translation);
            float distance = bulletPath.Length();

            // If we don't have a shot, we need to specify what kind of movement we need to remedy that.
            ZombieTacticalMovementState.MovementType movement = ZombieTacticalMovementState.MovementType.None;

            if (distance < attackBehavior.Weapon.EffectiveRangeMin)
            {
                movement = ZombieTacticalMovementState.MovementType.Retreat;
            }
            else if (distance > attackBehavior.Weapon.EffectiveRangeMax)
            {
                movement = ZombieTacticalMovementState.MovementType.Close;
            }
            else
            {
                BepuRay loeRay = new BepuRay(BepuConverter.Convert(firePoint.Translation), bulletPath);
                LOSFilter filter = new LOSFilter(bcc.Controller.Body.CollisionInformation, opponentBcc.Controller.Body.CollisionInformation);
                RayCastResult loeResult;
                GameResources.ActorManager.SimSpace.RayCast(loeRay, attackBehavior.Weapon.EffectiveRangeMax * 1.5f,
                    filter.Test, out loeResult);

                EntityCollidable otherEntityCollidable = loeResult.HitObject as EntityCollidable;
                if (otherEntityCollidable != null &&
                    otherEntityCollidable.Entity != null &&
                    otherEntityCollidable.Entity.Tag != null &&
                    (int)(otherEntityCollidable.Entity.Tag) == opponent.Id)
                {
                    // TODO: P1: Something wrong with this angle check calculation:
                    float aimTheta = (float)(Math.Acos(BepuVec3.Dot(bulletPath, bcc.Controller.ViewDirection) / distance));
                    const float AIM_CONE_RADIANS = MathHelper.Pi / 12.0f;
                    if (aimTheta <= AIM_CONE_RADIANS)
                    {
                        bcc.WorldAim = BepuConverter.Convert(bulletPath);
                        attackBehavior.Weapon.CurrentOperation = WeaponFunctions.TriggerPulled;
                        return;
                    }
                }
                else
                {
                    movement = ZombieTacticalMovementState.MovementType.Lateral;
                }
            }

            if (movement != ZombieTacticalMovementState.MovementType.None)
            {
                ZombieTacticalMovementState ztms = new ZombieTacticalMovementState(movement);
                agent.CurrentState = ztms;
            }
        }
    }

    class ZombieTacticalMovementState : AgentState
    {
        public enum MovementType
        {
            None,
            Lateral,
            Close,
            Retreat,
        }

        private MovementType mMovementType;

        public ZombieTacticalMovementState(MovementType movementType)
        {
            mMovementType = movementType;
            mDurationTicks = TimeSpan.TicksPerSecond; // Just initialize with a dummy value for now
        }

        public override void Enter(SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            IBipedWeaponEquippable attackBehavior = owner.GetBehaviorThatImplementsType<IBipedWeaponEquippable>();
            attackBehavior.Weapon.CurrentOperation = WeaponFunctions.Neutral;

            BipedControllerComponent bcc = owner.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            int opponentId = agent.GetProperty<int>(AgentPropertyName.ActiveOpponent);
            Actor opponent = GameResources.ActorManager.GetActorById(opponentId);
            BipedControllerComponent opponentBcc = opponent.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);

            BepuVec3 towardOpponent = opponentBcc.Controller.Body.Position - bcc.Controller.Body.Position;
            towardOpponent.Y = 0.0f;
            towardOpponent.Normalize();

            const float MOVE_SECS_MAX = 2.5f;
            const float MOVE_SECS_MIN = 0.5f;

            float moveRand = SlagMath.Get0To1UpperFloat(GameResources.ActorManager.Random);

            switch (mMovementType)
            {
                case MovementType.Lateral:
                    BepuVec3 right = BepuVec3.Cross(towardOpponent, BepuVec3.Up);
                    // Make the random symmetrical about 0.5f so that we can divide it into two equal segments for right and left.
                    moveRand = Math.Abs(moveRand - 0.5f);
                    mDurationTicks = (long)(TimeSpan.TicksPerSecond * (moveRand * 2.0f * (MOVE_SECS_MAX - MOVE_SECS_MIN) + MOVE_SECS_MIN));
                    steering.Target = BepuConverter.Convert(bcc.Controller.Body.Position + (moveRand > 0.5f ? right : -right) * 100.0f);
                    break;

                case MovementType.Close:
                    steering.Target = BepuConverter.Convert(towardOpponent * 100.0f);
                    mDurationTicks = (long)(TimeSpan.TicksPerSecond * (moveRand * (MOVE_SECS_MAX - MOVE_SECS_MIN) + MOVE_SECS_MIN));
                    break;

                case MovementType.Retreat:
                    steering.Target = BepuConverter.Convert(-towardOpponent * 100.0f);
                    mDurationTicks = (long)(TimeSpan.TicksPerSecond * (moveRand * (MOVE_SECS_MAX - MOVE_SECS_MIN) + MOVE_SECS_MIN));
                    break;

                default:
                    steering.Target = BepuConverter.Convert(towardOpponent * 100.0f);
                    mDurationTicks = 0;
                    break;
            }

            steering.Weights[(int)SteeringBlender.WeightType.Arrive] = 0.0f;
            steering.Weights[(int)SteeringBlender.WeightType.Seek] = 0.67f;
            steering.Weights[(int)SteeringBlender.WeightType.Wander] = 0.33f;
            steering.Weights[(int)SteeringBlender.WeightType.Wait] = 0.0f;
            steering.Urgency = 1.0f;
            steering.ForceScale = 1.0f;
        }

        public override void Update(SteeringBlender steering, Actor owner, IAgentStateManager agent)
        {
            if (agent.TimeInState.Ticks >= mDurationTicks)
            {
                ZombieCombatState zcs = new ZombieCombatState();
                agent.CurrentState = zcs;
            }
        }
    }
}
