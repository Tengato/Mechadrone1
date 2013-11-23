#region File Description
//-----------------------------------------------------------------------------
// SkinningData.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
#endregion

namespace Skelemator
{
    /// <summary>
    /// Combines all the data (i.e. the skeleton and clips) needed to render and
    /// animate a skinned object. This is typically stored in the Tag property of
    /// the Model being animated.
    /// </summary>
    public class SkinningData
    {
        /// <summary>
        /// Constructs a new skinning data object.
        /// </summary>
        public SkinningData(Dictionary<string, Clip> animationClips,
                            List<Matrix> bindPose, List<Matrix> inverseBindPose,
                            List<int> skeletonHierarchy, int weightsPerVert)
        {
            AnimationClips = animationClips;
            BindPose = bindPose;
            InverseBindPose = inverseBindPose;
            SkeletonHierarchy = skeletonHierarchy;
            WeightsPerVert = weightsPerVert;
        }


        /// <summary>
        /// Private constructor for use by the XNB deserializer.
        /// </summary>
        private SkinningData()
        {
        }


        /// <summary>
        /// Gets a collection of animation clips. These are stored by name in a
        /// dictionary, so there could for instance be clips for "Walk", "Run",
        /// "JumpReallyHigh", etc.
        /// </summary>
        [ContentSerializer]
        public Dictionary<string, Clip> AnimationClips { get; private set; }


        /// <summary>
        /// Bindpose matrices for each bone in the skeleton,
        /// relative to the parent bone.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> BindPose { get; private set; }


        /// <summary>
        /// Vertex to bonespace transforms for each bone in the skeleton.
        /// </summary>
        [ContentSerializer]
        public List<Matrix> InverseBindPose { get; private set; }


        /// <summary>
        /// For each bone in the skeleton, stores the index of the parent bone.
        /// </summary>
        [ContentSerializer]
        public List<int> SkeletonHierarchy { get; private set; }


        /// <summary>
        /// The maximum number of bones that a single vertex may possess a weight value for.
        /// </summary>
        [ContentSerializer]
        public int WeightsPerVert { get; private set; }
    }
}
