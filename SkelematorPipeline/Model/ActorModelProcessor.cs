#region File Description
//-----------------------------------------------------------------------------
// ActorModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Skelemator;
using System.Xml;
using System.Xml.Serialization;
using System.Linq;
using Microsoft.Xna.Framework.Content.Pipeline.Serialization.Intermediate;
#endregion

namespace SkelematorPipeline
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding animation, normal map support.
    /// </summary>
    [ContentProcessor(DisplayName = "Skelemator Actor Model Processor")]
    public class ActorModelProcessor : ModelProcessor
    {
        #region Properties & Fields

        // Location of an XML file that describes how to import animation clips.
        public virtual string AnimationPackageDataFilePath { get; set; }

        // Location of an XML file that describes which materials to use.
        public virtual string MaterialDataFilePath { get; set; }

        private string contentPath;
        private List<MaterialData> incomingMaterials;

        #endregion // Properties & Fields


        /// <summary>
        /// The main Process method converts an intermediate format content pipeline
        /// NodeContent tree to a ModelContent object with embedded animation data.
        /// </summary>
        public override ModelContent Process(NodeContent input, ContentProcessorContext context)
        {
            ValidateMesh(input, context, null);

            // Find the skeleton.
            BoneContent skeleton = MeshHelper.FindSkeleton(input);

            // TODO : Skeleton should be optional.
            if (skeleton == null)
                throw new InvalidContentException("Input skeleton not found.");

            // We don't want to have to worry about different parts of the model being
            // in different local coordinate systems, so let's just bake everything.
            FlattenTransforms(input, skeleton);

            // Read the bind pose and skeleton hierarchy data.
            IList<BoneContent> bones = MeshHelper.FlattenSkeleton(skeleton);

            // TODO: Get this value from the Constants.fxh file
            if (bones.Count > 72)
            {
                throw new InvalidContentException(string.Format(
                    "Skeleton has {0} bones, but the maximum supported is {1}.",
                    bones.Count, 72));
            }

            List<Matrix> bindPose = new List<Matrix>();
            List<Matrix> inverseBindPose = new List<Matrix>();
            List<int> skeletonHierarchy = new List<int>();

            foreach (BoneContent bone in bones)
            {
                bindPose.Add(bone.Transform);
                inverseBindPose.Add(Matrix.Invert(bone.AbsoluteTransform));
                skeletonHierarchy.Add(bones.IndexOf(bone.Parent as BoneContent));
            }

            contentPath = Environment.CurrentDirectory;

            using (XmlReader reader = XmlReader.Create(MaterialDataFilePath))
            {
                incomingMaterials = IntermediateSerializer.Deserialize<List<MaterialData>>(reader, null);
            }
            context.AddDependency(Path.Combine(Environment.CurrentDirectory, MaterialDataFilePath));

            // Placeholder for when you could perform other ModelMeshPart/GeometryContent processing:
            //TraverseGeometryContents(input);

            AnimationPackageData incomingAnimation;

            using (XmlReader reader = XmlReader.Create(AnimationPackageDataFilePath))
            {
                incomingAnimation = IntermediateSerializer.Deserialize<AnimationPackageData>(reader, null);
            }
            context.AddDependency(Path.Combine(Environment.CurrentDirectory, AnimationPackageDataFilePath));

            // Convert animation data to our runtime format.
            Dictionary<string, Clip> animationClips;
            animationClips = ProcessAnimations(skeleton.Animations, bones, incomingAnimation.Clips);

            // Chain to the base ModelProcessor class so it can convert the model data.
            ModelContent model = base.Process(input, context);

            int modelMaxWeightsPerVert = 0;

            const string WEIGHTSPERVERT_PARAM_NAME = "WeightsPerVert";

            // Put the material's flags into the ModelMeshPartContent's Tag property.
            // Also, note the largest value of "WeightsPerVert" used in any material.
            foreach (ModelMeshContent mmc in model.Meshes)
            {
                foreach (ModelMeshPartContent mmpc in mmc.MeshParts)
                {
                    MaterialData mat = incomingMaterials.Single(m => m.Name == mmpc.Material.Name);
                    mmpc.Tag = mat.HandlingFlags;

                    EffectParam wpvEp = mat.EffectParams.Find(wpv => wpv.Name == WEIGHTSPERVERT_PARAM_NAME);
                    if (wpvEp != null)
                    {
                        modelMaxWeightsPerVert = Math.Max(modelMaxWeightsPerVert, (int)(wpvEp.Value));
                    }
                }
            }

            // Store our custom animation data in the Tag property of the model.
            SkinningData skinningData = new SkinningData(animationClips, bindPose, inverseBindPose, skeletonHierarchy, modelMaxWeightsPerVert);

            model.Tag = new AnimationPackage(
                skinningData,
                incomingAnimation.AnimationStateDescriptions,
                incomingAnimation.AnimationNodeDescriptions,
                incomingAnimation.InitialStateName,
                incomingAnimation.Transitions);

            return model;
        }


        private void TraverseGeometryContents(NodeContent node)
        {
            MeshContent mesh = node as MeshContent;
            if (mesh != null)
            {
                foreach (GeometryContent geometry in mesh.Geometry)
                {
                    // In case we want to do some processing here.
                }
            }

            foreach (NodeContent child in node.Children)
            {
                TraverseGeometryContents(child);
            }
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContentDictionary
        /// object to our runtime AnimationClip format.
        /// </summary>
        static Dictionary<string, Clip> ProcessAnimations(
            AnimationContentDictionary animations,
            IList<BoneContent> bones,
            List<ClipData> clipInfo)
        {
            // Build up a table mapping bone names to indices.
            Dictionary<string, int> boneMap = new Dictionary<string, int>();

            for (int i = 0; i < bones.Count; i++)
            {
                string boneName = bones[i].Name;

                if (!string.IsNullOrEmpty(boneName))
                    boneMap.Add(boneName, i);
            }

            // Convert each animation in turn.
            Dictionary<string, Clip> animationClips = new Dictionary<string, Clip>();
            foreach (ClipData clip in clipInfo)
            {
                Clip processed = ProcessAnimation(animations[clip.SourceTake], boneMap, clip);

                animationClips.Add(clip.Alias, processed);
            }

            if (animationClips.Count == 0)
            {
                throw new InvalidContentException("Input file does not contain any animations.");
            }

            return animationClips;
        }


        /// <summary>
        /// Converts an intermediate format content pipeline AnimationContent
        /// object to our runtime AnimationClip format.
        /// </summary>
        static Clip ProcessAnimation(
            AnimationContent animation,
            Dictionary<string, int> boneMap,
            ClipData clipData)
        {
            List<Keyframe> keyframes = new List<Keyframe>();
            Dictionary<TimeSpan, AnimationControlEvents> controlEvents = new Dictionary<TimeSpan,AnimationControlEvents>();

            bool frameTimesFilled = false;
            TimeSpan[] frameTimes = new TimeSpan[clipData.LastFrame - clipData.FirstFrame + 1];
            TimeSpan syncTime = TimeSpan.Zero;

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    // string.Format("Found animation for bone '{0}', which is not part of the skeleton.", channel.Key));

                    // We can just ignore these channels.
                    continue;
                }

                if (!frameTimesFilled)
                {
                    // I guess the frames in the ClipData are specified with a 0-based index?
                    TimeSpan startTime = channel.Value[clipData.FirstFrame].Time;

                    for (int i = clipData.FirstFrame; i <= clipData.LastFrame; i++)
                    {
                        frameTimes[i - clipData.FirstFrame] = channel.Value[i].Time - startTime;
                        // Also, let's get the times for the events:
                        foreach (KeyValuePair<int, AnimationControlEvents> ace in clipData.Events)
                        {
                            if (ace.Key == i)
                            {
                                controlEvents.Add(frameTimes[i - clipData.FirstFrame], ace.Value);
                            }
                        }

                        // Don't forget to convert the sync frame offset into a time:
                        if (clipData.SyncFrameOffset == i - clipData.FirstFrame)
                            syncTime = frameTimes[i - clipData.FirstFrame];
                    }

                    if (controlEvents.Count != clipData.Events.Count)
                        throw new InvalidContentException("The time index of some control events could not be determined.");

                    frameTimesFilled = true;
                }

                // Convert the keyframe data.
                for (int i = clipData.FirstFrame; i <= clipData.LastFrame; i++)
                {
                    Matrix boneTransform;
                    if (channel.Value.Count <= i)
                    {
                        boneTransform = channel.Value[channel.Value.Count - 1].Transform;
                    }
                    else
                    {
                        boneTransform = channel.Value[i].Transform;
                    }

                    keyframes.Add(new Keyframe(boneIndex, frameTimes[i - clipData.FirstFrame], boneTransform));
                }

            }

            if (keyframes.Count == 0)
                throw new InvalidContentException("Animation has no keyframes.");

            TimeSpan framePeriod = TimeSpan.Zero;
            if (keyframes.Count > 1)
                 framePeriod = keyframes[1].Time - keyframes[0].Time;

            // Sort the merged keyframes by time.
            keyframes.Sort(CompareKeyframeTimes);

            TimeSpan duration = keyframes[keyframes.Count - 1].Time + framePeriod;

            if (duration <= TimeSpan.Zero)
                throw new InvalidContentException("Animation has a zero duration.");

            return new Clip(duration, keyframes, clipData.Loopable, controlEvents, syncTime);
        }


        /// <summary>
        /// Comparison function for sorting keyframes into ascending time order.
        /// </summary>
        static int CompareKeyframeTimes(Keyframe a, Keyframe b)
        {
            return a.Time.CompareTo(b.Time);
        }


        /// <summary>
        /// Makes sure this mesh contains the kind of data we know how to animate.
        /// </summary>
        static void ValidateMesh(
            NodeContent node,
            ContentProcessorContext context,
            string parentBoneName)
        {
            MeshContent mesh = node as MeshContent;

            if (mesh != null)
            {
                // Validate the mesh.
                if (parentBoneName != null)
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} is a child of bone {1}. SkinnedModelProcessor " +
                        "does not correctly handle meshes that are children of bones.",
                        mesh.Name, parentBoneName);
                }

                if (!MeshHasSkinning(mesh))
                {
                    context.Logger.LogWarning(null, null,
                        "Mesh {0} has no skinning information, so it has been deleted.",
                        mesh.Name);

                    mesh.Parent.Children.Remove(mesh);
                    return;
                }
            }
            else if (node is BoneContent)
            {
                // If this is a bone, remember that we are now looking inside it.
                parentBoneName = node.Name;
            }

            // Recurse (iterating over a copy of the child collection,
            // because validating children may delete some of them).
            foreach (NodeContent child in new List<NodeContent>(node.Children))
                ValidateMesh(child, context, parentBoneName);
        }


        /// <summary>
        /// Checks whether a mesh contains skininng information.
        /// </summary>
        static bool MeshHasSkinning(MeshContent mesh)
        {
            foreach (GeometryContent geometry in mesh.Geometry)
            {
                if (!geometry.Vertices.Channels.Contains(VertexChannelNames.Weights()))
                    return false;
            }

            return true;
        }


        /// <summary>
        /// Bakes unwanted transforms into the model geometry,
        /// so everything ends up in the same coordinate system.
        /// </summary>
        static void FlattenTransforms(NodeContent node, BoneContent skeleton)
        {
            foreach (NodeContent child in node.Children)
            {
                // Don't process the skeleton, because that is special.
                if (child == skeleton)
                    continue;

                // Bake the local transform into the actual geometry.
                MeshHelper.TransformScene(child, child.Transform);

                // Having baked it, we can now set the local
                // coordinate system back to identity.
                child.Transform = Matrix.Identity;

                // Recurse.
                FlattenTransforms(child, skeleton);
            }
        }


        protected override MaterialContent ConvertMaterial(MaterialContent material, ContentProcessorContext context)
        {
            MaterialData mat = incomingMaterials.Single(m => m.Name == material.Name);

            EffectMaterialContent emc = new EffectMaterialContent();
            emc.Effect = new ExternalReference<EffectContent>(Path.Combine(contentPath, mat.CustomEffect));
            emc.Name = material.Name;
            emc.Identity = material.Identity;

            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture in material.Textures)
            {
                if (texture.Key == "Texture")
                {
                    emc.Textures.Add(texture.Key, texture.Value);
                }
                else
                {
                    context.Logger.LogWarning(null, material.Identity, "There were some other textures referenced by the model, but we can't properly assign them to the correct effect parameter.");
                }
            }

            foreach (EffectParam ep in mat.EffectParams)
            {
                if (ep.Category == EffectParamCategory.OpaqueData)
                {
                    emc.OpaqueData.Add(ep.Name, ep.Value);
                }
                else if (ep.Category == EffectParamCategory.Texture)
                {
                    emc.Textures.Add(ep.Name, new ExternalReference<TextureContent>((string)(ep.Value)));
                }
            }

            return base.ConvertMaterial(emc, context);
        }

    }

}
