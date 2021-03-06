﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using SlagformCommon;

namespace Skelemator
{
    public class AnimationStateMachine : ISkinnedSkeletonPoser
    {
        public AnimationState CurrentState { get; set; }
        public bool IsActive { get { return CurrentState != null; } }
        private TimeSpan transitionTime;
        // The transition info describes how to blend the current state and the next state.
        public TransitionInfo ActiveTransition;
        protected bool blendFromCancelledTransitionPose;
        protected Matrix[] cancelledPose;
        public AnimationControlEvents ActiveControlEvents;
        private SkinningData mSkinningData;

        private Dictionary<AnimationState, Dictionary<AnimationState, TransitionInfo>> transitions;

        // The next state is the state that the current will transition into (may or may not be
        // the desired state, but it will be one step closer to the desired state.
        protected AnimationState nextState;
        protected Dictionary<string, AnimationState> states;

        protected Vector2 horizMovement;
        public Vector2 HorizontalMovement
        {
            get
            {
                return horizMovement;
            }

            set
            {
                horizMovement = value;
                float angle = (float)(Math.Atan2(horizMovement.Y, horizMovement.X));
                if (angle < 0.0f)
                    angle += MathHelper.TwoPi;

                // TODO: P2: some of these details are specific to bipeds and ought to be refactored out...
                CurrentState.AdjustBlendParam("2DWalk", angle);
                CurrentState.AdjustBlendParam("Fast2DWalk", angle);
                CurrentState.AdjustBlendParam("SpeedBlendedMove", horizMovement.Length());

                if (nextState != null)
                {
                    nextState.AdjustBlendParam("2DWalk", angle);
                    nextState.AdjustBlendParam("Fast2DWalk", angle);
                    nextState.AdjustBlendParam("SpeedBlendedMove", horizMovement.Length());
                }
            }
        }


        protected AnimationState desiredState;
        public string DesiredStateName
        {
            get
            {
                if (desiredState != null)
                    return desiredState.Name;

                return String.Empty;
            }

            set
            {
                if (value != desiredState.Name)
                {
                    // The desired state is our long-term goal state.
                    desiredState = states[value];

                    bool beginNewTransition = false;

                    // We will begin transitioning to the new state under the following conditions:
                    if (ActiveTransition == null)  // if no transitions are active...
                    {
                        beginNewTransition = true;
                    }
                    else if (transitionTime == TimeSpan.Zero) // if we have not started the active transition yet...
                    {
                        beginNewTransition = true;
                    }
                    else if (ActiveTransition != null && nextState.CanTransitionOut()) // if the next state will let us switch out already...
                    {
                        beginNewTransition = true;
                        blendFromCancelledTransitionPose = true;
                    }
                    // Otherwise, we need to wait for the current transition to finish.

                    if (beginNewTransition)
                    {
                        desiredState.Synchronize();
                        nextState = desiredState;
                        UpdateTransition();
                    }
                }
            }
        }

        public AnimationStateMachine(AnimationPackage package)
        {
            horizMovement = Vector2.Zero;
            mSkinningData = package.SkinningData;

            // Use the data in the AnimationPackage structure to create states to fill our list and build
            // a transition matrix.

            states = new Dictionary<string, AnimationState>();
            foreach (AnimationStateDescription stateDesc in package.StateDescriptions)
            {
                AnimationState newState = new AnimationState(stateDesc, package);
                states.Add(newState.Name, newState);
            }

            transitions = new Dictionary<AnimationState, Dictionary<AnimationState, TransitionInfo>>();

            foreach (KeyValuePair<string, AnimationState> currentStateKvp in states)
            {
                transitions.Add(currentStateKvp.Value, new Dictionary<AnimationState, TransitionInfo>());

                foreach (KeyValuePair<string, AnimationState> nextStateKvp in states)
                {
                    TransitionInfo bestFitTransition = null;

                    foreach (TransitionInfo ti in package.Transitions)
                    {
                        // The items lower in the list are higher priority and will override previous matches.
                        if (ti.IsMatch(currentStateKvp.Key, nextStateKvp.Key))
                            bestFitTransition = ti;
                    }

                    transitions[currentStateKvp.Value].Add(nextStateKvp.Value, bestFitTransition);
                }
            }

            CurrentState = states[package.InitialStateName];
            desiredState = CurrentState;
            ActiveTransition = null;
            nextState = null;
            blendFromCancelledTransitionPose = false;
            ActiveControlEvents = AnimationControlEvents.None;
        }


        /// <summary>
        /// Advance the currently playing clips, transitions, etc, and return any metachannel events that
        /// have occurred during the elapsed time.
        /// </summary>
        /// <param name="gameTime"></param>
        /// <returns></returns>
        public void Update(GameTime gameTime)
        {
            ActiveControlEvents = AnimationControlEvents.None;

            if (ActiveTransition == null ||
                (transitionTime == TimeSpan.Zero && !CurrentState.CanTransitionOut()))
            {
                // We're either completely in a given state, or we can't begin the transition just yet.
                CurrentState.AdvanceTime(gameTime.ElapsedGameTime);
                ActiveControlEvents |= CurrentState.GetActiveControlEvents();
            }
            else
            {
                // A transition is in progress.
                if (ActiveTransition.Type != TransitionType.Frozen)
                {
                    CurrentState.AdvanceTime(gameTime.ElapsedGameTime);
                    ActiveControlEvents |= CurrentState.GetActiveControlEvents();
                }

                nextState.AdvanceTime(gameTime.ElapsedGameTime);
                ActiveControlEvents |= nextState.GetActiveControlEvents();

                transitionTime += gameTime.ElapsedGameTime;

                // Has the transition completed?
                if (transitionTime.TotalSeconds >= ActiveTransition.DurationInSeconds)
                {
                    CurrentState = nextState;
                    blendFromCancelledTransitionPose = false;
                    if (nextState != desiredState)
                    {
                        nextState = desiredState;
                    }
                    else
                    {
                        nextState = null;
                    }
                    UpdateTransition();
                }
            }
        }


        private void UpdateTransition()
        {
            // Find the appropriate transition info and assign it here.
            if (nextState != null)
            {
                if (blendFromCancelledTransitionPose)
                {
                    TransitionInfo cancelTransition = new TransitionInfo();
                    cancelTransition.DurationInSeconds = 0.2f;
                    cancelTransition.Type = TransitionType.Frozen;
                    cancelTransition.UseSmoothStep = true;
                    ActiveTransition = cancelTransition;
                }
                else
                {
                    ActiveTransition = transitions[CurrentState][nextState];
                }
                transitionTime = TimeSpan.Zero;
            }
            else
            {
                ActiveTransition = null;
            }
        }


        public Matrix[] GetSkinTransforms()
        {
            Matrix[] blendStartPose;

            if (blendFromCancelledTransitionPose)
            {
                blendStartPose = cancelledPose;
            }
            else
            {
                blendStartPose = CurrentState.GetBoneTransforms();
            }

            if (ActiveTransition == null)
            {
                return CreateSkinTransforms(blendStartPose);
            }
            else
            {
                Matrix[] nextStatePose = nextState.GetBoneTransforms();

                float pctComplete = (float)(transitionTime.TotalSeconds / ActiveTransition.DurationInSeconds);
                float blendFactor = ActiveTransition.UseSmoothStep ? MathHelper.SmoothStep(0.0f, 1.0f, pctComplete) : pctComplete;

                Matrix[] blendedPose = SpaceUtils.LerpSkeletalPose(blendStartPose, nextStatePose, blendFactor);

                // Store the blended pose in case we have to cancel the transition next frame.
                if (!blendFromCancelledTransitionPose)
                    cancelledPose = blendedPose;

                return CreateSkinTransforms(blendedPose);
            }
        }

        private Matrix[] CreateSkinTransforms(Matrix[] boneTransforms)
        {
            Matrix[] skinTransforms = new Matrix[boneTransforms.Length];

            // First compute the local-to-model transforms:
            skinTransforms[0] = boneTransforms[0];  // Root bone.

            // Child bones.
            for (int bone = 1; bone < skinTransforms.Length; bone++)
            {
                int parentBone = mSkinningData.SkeletonHierarchy[bone];
                skinTransforms[bone] = boneTransforms[bone] * skinTransforms[parentBone];
            }

            // Apply the inverse bind pose to get the skinning palette:
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = mSkinningData.InverseBindPose[bone] * skinTransforms[bone];
            }

            return skinTransforms;
        }

    }

    [Flags]
    public enum AnimationControlEvents
    {
        None = 0,
        Jump = 1,
        Fire = 2,
        Reload = 4,
        Boost,
    }
}
