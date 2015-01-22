using System;
using Microsoft.Xna.Framework;

namespace Skelemator
{
    /// <summary>
    /// The animation player is in charge of decoding bone position
    /// matrices from an animation clip.
    /// </summary>
    public class SkinClipPlayer : ClipPlayer, ISkinnedSkeletonPoser
    {
        Matrix[] worldTransforms;
        Matrix[] skinTransforms;

        /// <summary>
        /// Constructs a new animation player.
        /// </summary>
        public SkinClipPlayer(SkinningData skinningData)
            : base (skinningData)
        {
            worldTransforms = new Matrix[skinningData.BindPose.Count];
            skinTransforms = new Matrix[skinningData.BindPose.Count];
        }

        /// <summary>
        /// Advances the current animation position.
        /// </summary>
        public override void Update(TimeSpan time, bool relativeToCurrentTime)
        {
            base.Update(time, relativeToCurrentTime);
            UpdateWorldTransforms();
            UpdateSkinTransforms();
        }

        /// <summary>
        /// Helper used by the Update method to refresh the WorldTransforms data.
        /// </summary>
        public void UpdateWorldTransforms()
        {
            // Root bone.
            worldTransforms[0] = boneTransforms[0];

            // Child bones.
            for (int bone = 1; bone < worldTransforms.Length; bone++)
            {
                int parentBone = skinningDataValue.SkeletonHierarchy[bone];

                worldTransforms[bone] = boneTransforms[bone] *
                                             worldTransforms[parentBone];
            }
        }

        /// <summary>
        /// Helper used by the Update method to refresh the SkinTransforms data.
        /// </summary>
        public void UpdateSkinTransforms()
        {
            for (int bone = 0; bone < skinTransforms.Length; bone++)
            {
                skinTransforms[bone] = skinningDataValue.InverseBindPose[bone] *
                                            worldTransforms[bone];
            }
        }

        /// <summary>
        /// Gets the current bone transform matrices, in absolute format.
        /// </summary>
        public Matrix[] GetWorldTransforms()
        {
            return worldTransforms;
        }

        /// <summary>
        /// Gets the current bone transform matrices,
        /// relative to the skinning bind pose.
        /// </summary>
        public Matrix[] GetSkinTransforms()
        {
            return skinTransforms;
        }
    }
}
