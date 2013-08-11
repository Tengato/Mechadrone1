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
        public virtual string ClipDataFilePath { get; set; }

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
                XmlSerializer serializer = new XmlSerializer(typeof(List<MaterialData>));
                incomingMaterials = (List<MaterialData>)serializer.Deserialize(reader);
            }

            //TraverseGeometryContents(input);

            List<ClipData> incomingClips;

            using (XmlReader reader = XmlReader.Create(ClipDataFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<ClipData>));

                // Use the Deserialize method to restore the object's state.
                incomingClips = (List<ClipData>)serializer.Deserialize(reader);
            }

            // Convert animation data to our runtime format.
            Dictionary<string, AnimationClip> animationClips;
            animationClips = ProcessAnimations(skeleton.Animations, bones, incomingClips);

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
            model.Tag = new SkinningData(animationClips, bindPose, inverseBindPose, skeletonHierarchy, modelMaxWeightsPerVert);

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
        static Dictionary<string, AnimationClip> ProcessAnimations(
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
            Dictionary<string, AnimationClip> animationClips = new Dictionary<string, AnimationClip>();

            foreach (ClipData clip in clipInfo)
            {
                AnimationClip processed = ProcessAnimation(animations[clip.SourceTake], boneMap, clip.FirstFrame, clip.LastFrame);

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
        static AnimationClip ProcessAnimation(
            AnimationContent animation,
            Dictionary<string, int> boneMap,
            int firstFrame,
            int lastFrame)
        {
            List<Keyframe> keyframes = new List<Keyframe>();

            // For each input animation channel.
            foreach (KeyValuePair<string, AnimationChannel> channel in animation.Channels)
            {
                // Look up what bone this channel is controlling.
                int boneIndex;

                if (!boneMap.TryGetValue(channel.Key, out boneIndex))
                {
                    throw new InvalidContentException(string.Format(
                        "Found animation for bone '{0}', which is not part of the skeleton.", channel.Key));
                }

                TimeSpan startTime = channel.Value[firstFrame].Time;

                // Convert the keyframe data.
                for (int i = firstFrame; i <= lastFrame; i++)
                {
                    keyframes.Add(new Keyframe(boneIndex, channel.Value[i].Time - startTime, channel.Value[i].Transform));
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

            return new AnimationClip(duration, keyframes);
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

            foreach (KeyValuePair<String, ExternalReference<TextureContent>> texture in material.Textures)
            {
                emc.Textures.Add(texture.Key, texture.Value);
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
