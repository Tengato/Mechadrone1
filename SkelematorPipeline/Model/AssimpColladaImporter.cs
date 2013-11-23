using System;
using System.Collections.Generic;
using System.Linq;
using Assimp;
using Assimp.Configs;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Text.RegularExpressions;

namespace SkelematorPipeline
{
    [ContentImporter(".dae", DisplayName = "Skelemator Collada Model Importer", DefaultProcessor = "ActorModelProcessor")]
    public class AssimpColladaImporter : ContentImporter<NodeContent>
    {
        public override NodeContent Import(string filename, ContentImporterContext context)
        {
            ContentIdentity identity = new ContentIdentity(filename, GetType().Name);

            const int MAX_BONE_WEIGHTS = 4;
            VertexBoneWeightLimitConfig boneConfig = new VertexBoneWeightLimitConfig(MAX_BONE_WEIGHTS);

            AssimpImporter importer = new AssimpImporter();
            importer.SetConfig(boneConfig);

            importer.AttachLogStream(new LogStream((msg, userData) => context.Logger.LogMessage(msg)));
            Scene scene = importer.ImportFile(filename,
                                            PostProcessSteps.FlipUVs |
                                            PostProcessSteps.JoinIdenticalVertices |
                                            PostProcessSteps.Triangulate |
                                            PostProcessSteps.SortByPrimitiveType |
                                            PostProcessSteps.FindInvalidData |
                                            PostProcessSteps.LimitBoneWeights |
                                            PostProcessSteps.FixInFacingNormals);


            // Root node
            NodeContent rootNode = new NodeContent
            {
                Name = scene.RootNode.Name,
                Identity = identity,
                Transform = Matrix.Transpose(ToXna(scene.RootNode.Transform))
            };


            // Materials
            MaterialContent[] materials = new MaterialContent[scene.MaterialCount];

            for (int m = 0; m < scene.MaterialCount; m++)
            {
                materials[m] = new BasicMaterialContent();

                materials[m].Identity = identity;
                // For some reason, there is all kinds of nasty junk in this string:
                materials[m].Name = CleanInput(scene.Materials[m].Name);

                for (int t = 0; t < scene.Materials[m].GetTextureCount(TextureType.Diffuse); t++)
                {
                    TextureSlot diffuseMap = scene.Materials[m].GetTexture(TextureType.Diffuse, t);
                    if (!String.IsNullOrEmpty(diffuseMap.FilePath))
                    {
                        materials[m].Textures.Add("Texture" + (t > 0 ? t.ToString() : ""),
                            new ExternalReference<TextureContent>(diffuseMap.FilePath, identity));
                    }
                }
            }


            // Bones

            // We find 'mesh container' nodes with the best names for those meshes while looking for the bones,
            // and will need them later when we create the MeshContents. I have a feeling that this won't work
            // in general, and may need to be made more robust.
            Dictionary<Mesh, string> meshNames = new Dictionary<Mesh, string>();
            Dictionary<Node, BoneContent> nodeToBoneMap = new Dictionary<Node, BoneContent>();
            BoneContent skeleton = null;    // The root bone for the model.

            List<Node> hierarchyNodes = scene.RootNode.Children.SelectDeep(n => n.Children).ToList();
            foreach (Node node in hierarchyNodes)
            {
                BoneContent bone = new BoneContent
                {
                    Name = node.Name,
                    Transform = Matrix.Transpose(ToXna(node.Transform))
                };


                if (node.MeshIndices != null)
                {
                    // This node is a 'mesh container' instead of a bone, so we only care about extracting the name of the mesh.
                    foreach (int meshIndex in node.MeshIndices)
                    {
                        if (!meshNames.ContainsKey(scene.Meshes[meshIndex]))
                            meshNames.Add(scene.Meshes[meshIndex], node.Name);
                    }
                }
                else if (node.Parent == scene.RootNode)
                {
                    if (skeleton == null)
                    {
                        // This will be our skeleton so put the animations here:
                        if (scene.HasAnimations)
                        {
                            foreach (Animation assimpAnim in scene.Animations)
                            {
                                if (assimpAnim.HasNodeAnimations)
                                {
                                    AnimationContent newAnim = new AnimationContent();
                                    newAnim.Identity = identity;
                                    newAnim.Duration = TimeSpan.FromSeconds(assimpAnim.DurationInTicks / assimpAnim.TicksPerSecond);
                                    newAnim.Name = assimpAnim.Name;

                                    foreach (NodeAnimationChannel nac in assimpAnim.NodeAnimationChannels)
                                    {
                                        Node animatedNode = hierarchyNodes.Find(n => n.Name == nac.NodeName);

                                        AnimationChannel newChan = BuildAnimtionChannel(animatedNode, nac);

                                        newAnim.Channels.Add(nac.NodeName, newChan);
                                    }

                                    if (String.IsNullOrEmpty(assimpAnim.Name))
                                    {
                                        bone.Animations.Add("SkelematorNoAnimationName", newAnim);
                                    }
                                    else
                                    {
                                        bone.Animations.Add(assimpAnim.Name, newAnim);
                                    }
                                }
                            }
                        }
                        rootNode.Children.Add(bone);
                        skeleton = bone;
                    }
                    else
                    {
                        context.Logger.LogWarning(null, identity, "Found multiple skeletons in the model, throwing extras away...");
                    }
                }
                else
                {
                    BoneContent parent = nodeToBoneMap[node.Parent];
                    parent.Children.Add(bone);
                }

                nodeToBoneMap.Add(node, bone);
            }


            // Meshes
            Dictionary<Mesh, MeshContent> meshes = new Dictionary<Mesh, MeshContent>();
            foreach (Mesh sceneMesh in scene.Meshes)
            {
                // See comment about meshNames at the beginning of the bone section.
                MeshBuilder mb = MeshBuilder.StartMesh(meshNames[sceneMesh]);

                mb.SwapWindingOrder = true; // Appears to require this...

                int positionIndex = -1;

                for (int v = 0; v < sceneMesh.VertexCount; v++)
                {
                    Vector3D vert = sceneMesh.Vertices[v];

                    // CreatePosition should just return a 0-based index of the newly added vertex.
                    positionIndex = mb.CreatePosition(new Vector3(vert.X, vert.Y, vert.Z));

                    if (positionIndex != v)
                        throw new InvalidContentException("Something unexpected happened while building a MeshContent from the Assimp scene mesh's vertices.  The scene mesh may contains duplicate vertices.");
                }

                if (positionIndex + 1 < 3)
                    throw new InvalidContentException("There were not enough vertices in the Assimp scene mesh.");



                // Create vertex channels
                int normalVertexChannelIndex = mb.CreateVertexChannel<Vector3>(VertexChannelNames.Normal());

                int[] texCoordVertexChannelIndex = new int[sceneMesh.TextureCoordsChannelCount];
                for (int x = 0; x < sceneMesh.TextureCoordsChannelCount; x++)
                {
                    texCoordVertexChannelIndex[x] = mb.CreateVertexChannel<Vector2>(VertexChannelNames.TextureCoordinate(x));
                }

                int boneWeightVertexChannelIndex = -1;

                if (sceneMesh.HasBones)
                    boneWeightVertexChannelIndex = mb.CreateVertexChannel<BoneWeightCollection>(VertexChannelNames.Weights());


                // Prepare vertex channel data
                BoneWeightCollection[] boneWeightData = null;
                if (sceneMesh.HasBones)
                {
                    boneWeightData = new BoneWeightCollection[sceneMesh.VertexCount];

                    for (int v = 0; v < sceneMesh.VertexCount; v++)
                    {
                        boneWeightData[v] = new BoneWeightCollection();
                    }

                    foreach (Bone sceneMeshBone in sceneMesh.Bones)
                    {
                        // We have to assume that the bone's name matches up with a node, and therefore one of our BoneContents.
                        foreach (VertexWeight sceneMeshBoneWeight in sceneMeshBone.VertexWeights)
                        {
                            boneWeightData[sceneMeshBoneWeight.VertexID].Add(new BoneWeight(sceneMeshBone.Name, sceneMeshBoneWeight.Weight));
                        }
                    }

                    for (int v = 0; v < sceneMesh.VertexCount; v++)
                    {
                        if (boneWeightData[v].Count <= 0)
                            throw new InvalidContentException("Encountered vertices without bone weights.");

                        boneWeightData[v].NormalizeWeights();
                    }

                }

                // Set the per-geometry data
                mb.SetMaterial(materials[sceneMesh.MaterialIndex]);
                mb.SetOpaqueData(new OpaqueDataDictionary());

                // Add each vertex
                for (int f = 0; f < sceneMesh.FaceCount; f++)
                {
                    if (sceneMesh.Faces[f].IndexCount != 3)
                        throw new InvalidContentException("Only triangular faces allowed.");

                    for (int t = 0; t < 3; t++)
                    {
                        mb.SetVertexChannelData(normalVertexChannelIndex, ToXna(sceneMesh.Normals[sceneMesh.Faces[f].Indices[t]]));

                        for (int x = 0; x < sceneMesh.TextureCoordsChannelCount; x++)
                        {
                            mb.SetVertexChannelData(texCoordVertexChannelIndex[x], ToXnaVector2((sceneMesh.GetTextureCoords(x))[sceneMesh.Faces[f].Indices[t]]));
                        }

                        if (sceneMesh.HasBones)
                            mb.SetVertexChannelData(boneWeightVertexChannelIndex, boneWeightData[sceneMesh.Faces[f].Indices[t]]);

                        mb.AddTriangleVertex((int)(sceneMesh.Faces[f].Indices[t]));
                    }
                }

                MeshContent mesh = mb.FinishMesh();
                rootNode.Children.Add(mesh);
                meshes.Add(sceneMesh, mesh);
            }

            return rootNode;
        }


        private static AnimationChannel BuildAnimtionChannel(Node animatedNode, NodeAnimationChannel nac)
        {

            AnimationChannel newChan = new AnimationChannel();

            Matrix animatedNodeTransform = Matrix.Transpose(ToXna(animatedNode.Transform));

            Vector3 xnaScale;
            Microsoft.Xna.Framework.Quaternion xnaRotation;
            Vector3 xnaPositon;

            animatedNodeTransform.Decompose(out xnaScale, out xnaRotation, out xnaPositon);

            Vector3D scale = new Vector3D(xnaScale.X, xnaScale.Y, xnaScale.Z);
            Assimp.Quaternion rotation;
            rotation.W = xnaRotation.W;
            rotation.X = xnaRotation.X;
            rotation.Y = xnaRotation.Y;
            rotation.Z = xnaRotation.Z;
            Vector3D position = new Vector3D(xnaPositon.X, xnaPositon.Y, xnaPositon.Z);

            int sKeyIndex = 0;
            int qKeyIndex = 0;
            int tKeyIndex = 0;

            double firstSKeyTime = nac.ScalingKeyCount > sKeyIndex ? nac.ScalingKeys[sKeyIndex].Time : Double.MaxValue;
            double firstQKeyTime = nac.RotationKeyCount > qKeyIndex ? nac.RotationKeys[qKeyIndex].Time : Double.MaxValue;
            double firstTKeyTime = nac.PositionKeyCount > tKeyIndex ? nac.PositionKeys[tKeyIndex].Time : Double.MaxValue;

            double currTime = Math.Min(Math.Min(firstSKeyTime, firstQKeyTime), firstTKeyTime);

            if (firstSKeyTime <= currTime)
                scale = nac.ScalingKeys[sKeyIndex].Value;

            if (firstQKeyTime <= currTime)
                rotation = nac.RotationKeys[qKeyIndex].Value;

            if (firstTKeyTime <= currTime)
                position = nac.PositionKeys[tKeyIndex].Value;

            while (currTime < double.MaxValue)
            {
                while (nac.ScalingKeyCount > sKeyIndex + 1 &&
                    nac.ScalingKeys[sKeyIndex + 1].Time <= currTime)
                {
                    sKeyIndex++;
                    scale = nac.ScalingKeys[sKeyIndex].Value;
                }

                while (nac.RotationKeyCount > qKeyIndex + 1 &&
                    nac.RotationKeys[qKeyIndex + 1].Time <= currTime)
                {
                    qKeyIndex++;
                    rotation = nac.RotationKeys[qKeyIndex].Value;
                }

                while (nac.PositionKeyCount > tKeyIndex + 1 &&
                    nac.PositionKeys[tKeyIndex + 1].Time <= currTime)
                {
                    tKeyIndex++;
                    position = nac.PositionKeys[tKeyIndex].Value;
                }

                xnaRotation.W = rotation.W;
                xnaRotation.X = rotation.X;
                xnaRotation.Y = rotation.Y;
                xnaRotation.Z = rotation.Z;

                Matrix transform =
                    Matrix.CreateScale(ToXna(scale)) *
                    Matrix.CreateFromQuaternion(xnaRotation) *
                    Matrix.CreateTranslation(ToXna(position));

                AnimationKeyframe newKeyframe = new AnimationKeyframe(TimeSpan.FromSeconds(currTime), transform);
                newChan.Add(newKeyframe);

                // Increment the time:

                double nextSKeyTime = nac.ScalingKeyCount > sKeyIndex + 1 ? nac.ScalingKeys[sKeyIndex + 1].Time : Double.MaxValue;
                double nextQKeyTime = nac.RotationKeyCount > qKeyIndex + 1 ? nac.RotationKeys[qKeyIndex + 1].Time : Double.MaxValue;
                double nextTKeyTime = nac.PositionKeyCount > tKeyIndex + 1 ? nac.PositionKeys[tKeyIndex + 1].Time : Double.MaxValue;

                currTime = Math.Min(Math.Min(nextSKeyTime, nextQKeyTime), nextTKeyTime);
            }

            return newChan;
        }


        public static Matrix ToXna(Matrix4x4 matrix)
        {
            var result = Matrix.Identity;

            result.M11 = matrix.A1;
            result.M12 = matrix.A2;
            result.M13 = matrix.A3;
            result.M14 = matrix.A4;

            result.M21 = matrix.B1;
            result.M22 = matrix.B2;
            result.M23 = matrix.B3;
            result.M24 = matrix.B4;

            result.M31 = matrix.C1;
            result.M32 = matrix.C2;
            result.M33 = matrix.C3;
            result.M34 = matrix.C4;

            result.M41 = matrix.D1;
            result.M42 = matrix.D2;
            result.M43 = matrix.D3;
            result.M44 = matrix.D4;

            return result;
        }


        static string CleanInput(string strIn)
        {
            // Replace invalid characters with empty strings.
            return Regex.Replace(strIn, @"[^\w\.@-]", String.Empty, RegexOptions.None);
        }

        public static Vector2[] ToXna(Vector2D[] vectors)
        {
            var result = new Vector2[vectors.Length];
            for (var i = 0; i < vectors.Length; i++)
                result[i] = new Vector2(vectors[i].X, vectors[i].Y);

            return result;
        }

        public static Vector2 ToXna(Vector2D vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector2[] ToXnaVector2(Vector3D[] vectors)
        {
            var result = new Vector2[vectors.Length];
            for (var i = 0; i < vectors.Length; i++)
                result[i] = new Vector2(vectors[i].X, 1 - vectors[i].Y);

            return result;
        }

        public static Vector2 ToXnaVector2(Vector3D vector)
        {
            return new Vector2(vector.X, vector.Y);
        }

        public static Vector3[] ToXna(Vector3D[] vectors)
        {
            var result = new Vector3[vectors.Length];
            for (var i = 0; i < vectors.Length; i++)
                result[i] = new Vector3(vectors[i].X, vectors[i].Y, vectors[i].Z);

            return result;
        }

        public static Vector3 ToXna(Vector3D vector)
        {
            return new Vector3(vector.X, vector.Y, vector.Z);
        }
    }


    public static class EnumerableExtensions
    {
        /// <summary>
        /// Returns each element of a tree structure in hierarchial order.
        /// </summary>
        /// <typeparam name="T">The enumerated type.</typeparam>
        /// <param name="source">The enumeration to traverse.</param>
        /// <param name="selector">A function which returns the children of the element.</param>
        /// <returns>An IEnumerable whose elements are in tree structure heriarchical order.</returns>
        public static IEnumerable<T> SelectDeep<T>(this IEnumerable<T> source, Func<T, IEnumerable<T>> selector)
        {
            var stack = new Stack<T>(source.Reverse());
            while (stack.Count > 0)
            {
                // Return the next item on the stack.
                var item = stack.Pop();
                yield return item;

                // Get the children from this item.
                var children = selector(item);

                // If we have no children then skip it.
                if (children == null)
                    continue;

                // We're using a stack, so we need to push the
                // children on in reverse to get the correct order.
                foreach (var child in children.Reverse())
                    stack.Push(child);
            }
        }


        /// <summary>
        /// Returns an enumerable from a single element.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static IEnumerable<T> AsEnumerable<T>(this T item)
        {
            yield return item;
        }
    }
}