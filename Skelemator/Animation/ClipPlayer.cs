using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Skelemator
{
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class ClipPlayer
    {
        protected Clip currentClipValue;
        protected TimeSpan currentTimeValue;
        protected int currentKeyframe;
        protected Matrix[] boneTransforms;
        protected SkinningData skinningDataValue;
        public AnimationControlEvents ActiveControlEvents { get; private set; }

        public bool IsActive { get { return CurrentClip != null; } }

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public ClipPlayer(SkinningData skinningData)
        {
            if (skinningData == null)
                throw new ArgumentNullException("skinningData");

            skinningDataValue = skinningData;

            boneTransforms = new Matrix[skinningData.BindPose.Count];
            ActiveControlEvents = AnimationControlEvents.None;
        }

        /// <summary>
        /// Starts decoding the specified animation clip.
        /// </summary>
        public void StartClip(Clip clip)
        {
            if (clip == null)
                throw new ArgumentNullException("clip");

            currentClipValue = clip;
            currentTimeValue = TimeSpan.Zero;
            currentKeyframe = 0;

            // Initialize bone transforms to the bind pose.
            skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
        }

        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public virtual void Update(TimeSpan time, bool relativeToCurrentTime)
        {
            UpdateBoneTransforms(time, relativeToCurrentTime);
        }

        /// <summary>
        /// Helper used by the Update method to refresh the BoneTransforms data.
        /// </summary>
        public void UpdateBoneTransforms(TimeSpan time, bool relativeToCurrentTime)
        {
            if (currentClipValue == null)
                throw new InvalidOperationException("AnimationPlayer.Update was called before StartClip");

            // Update the animation position.
            if (relativeToCurrentTime)
            {
                time += currentTimeValue;
            }

            // If we reached either end, and the clip is loopable, wrap around to the other side.
            if (currentClipValue.Loopable)
            {
                while (time >= currentClipValue.Duration)
                    time -= currentClipValue.Duration;

                while (time < TimeSpan.Zero)
                    time += currentClipValue.Duration;
            }

            // Clamp the time if we've gone out of bounds.
            if (time < TimeSpan.Zero)
            {
                time = TimeSpan.Zero;
            }
            else if (time >= currentClipValue.Duration)
            {
                time = currentClipValue.Duration;
            }

            // If the position moved backwards, reset the keyframe index.
            if (time < currentTimeValue)
            {
                currentKeyframe = 0;
                skinningDataValue.BindPose.CopyTo(boneTransforms, 0);
            }

            // Grab the control events that have occurred
            ActiveControlEvents = AnimationControlEvents.None;
            foreach (KeyValuePair<TimeSpan, AnimationControlEvents> ace in currentClipValue.Events)
            {
                if (ace.Key > currentTimeValue && ace.Key <= time)
                    ActiveControlEvents |= ace.Value;
            }

            currentTimeValue = time;

            // Read keyframe matrices.
            IList<Keyframe> keyframes = currentClipValue.Keyframes;

            while (currentKeyframe < keyframes.Count)
            {
                Keyframe keyframe = keyframes[currentKeyframe];

                // Stop when we've read up to the current time position.
                if (keyframe.Time > currentTimeValue)
                    break;

                // Use this keyframe.
                boneTransforms[keyframe.Bone] = keyframe.Transform;

                currentKeyframe++;
            }
        }

        /// <summary>
        /// Gets the current bone transform matrices, relative to their parent bones.
        /// </summary>
        public Matrix[] GetBoneTransforms()
        {
            return boneTransforms;
        }

        /// <summary>
        /// Gets the clip currently being decoded.
        /// </summary>
        public Clip CurrentClip
        {
            get { return currentClipValue; }
        }

        /// <summary>
        /// Gets the current play position.
        /// </summary>
        public TimeSpan CurrentTime
        {
            get { return currentTimeValue; }
        }
    }
}
