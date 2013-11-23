#region File Description
//-----------------------------------------------------------------------------
// AnimationClip.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Skelemator
{
    /// <summary>
    /// An animation clip is the runtime equivalent of the
    /// Microsoft.Xna.Framework.Content.Pipeline.Graphics.AnimationContent type.
    /// It holds all the keyframes needed to describe a single animation.
    /// </summary>
    public class Clip
    {
        /// <summary>
        /// Constructs a new animation clip object.
        /// </summary>
        public Clip(
            TimeSpan duration,
            List<Keyframe> keyframes,
            bool loopable,
            Dictionary<TimeSpan, AnimationControlEvents> controlEvents,
            TimeSpan syncTime)
        {
            Duration = duration;
            Keyframes = keyframes;
            Loopable = loopable;
            Events = controlEvents;
            SyncTime = syncTime;
        }


        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private Clip()
        {
        }


        /// <summary>
        /// Gets the total length of the animation.
        /// </summary>
        [ContentSerializer]
        public TimeSpan Duration { get; private set; }


        /// <summary>
        /// Gets a combined list containing all the keyframes for all bones,
        /// sorted by time.
        /// </summary>
        [ContentSerializer]
        public List<Keyframe> Keyframes { get; private set; }


        [ContentSerializer]
        public bool Loopable { get; private set; }


        [ContentSerializer]
        public Dictionary<TimeSpan, AnimationControlEvents> Events { get; set; }

        [ContentSerializer]
        public TimeSpan SyncTime { get; set; }
    }
}
