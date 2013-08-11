#region File Description
//-----------------------------------------------------------------------------
// StaticModelProcessor.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
#endregion

namespace SkelematorPipeline
{
    /// <summary>
    /// Custom processor extends the builtin framework ModelProcessor class,
    /// adding custom material support.
    /// </summary>
    [ContentProcessor(DisplayName = "Skelemator Static Model Processor")]
    public class StaticModelProcessor : ModelProcessor
    {
        #region Properties & Fields

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
            contentPath = Environment.CurrentDirectory;

            using (XmlReader reader = XmlReader.Create(MaterialDataFilePath))
            {
                XmlSerializer serializer = new XmlSerializer(typeof(List<MaterialData>));
                incomingMaterials = (List<MaterialData>)serializer.Deserialize(reader);
            }

            // Chain to the base ModelProcessor class so it can convert the model data.
            ModelContent model = base.Process(input, context);

            // Put the material's flags into the ModelMeshPartContent's Tag property.
            foreach (ModelMeshContent mmc in model.Meshes)
            {
                foreach (ModelMeshPartContent mmpc in mmc.MeshParts)
                {
                    MaterialData mat = incomingMaterials.Single(m => m.Name == mmpc.Material.Name);
                    mmpc.Tag = mat.HandlingFlags;
                }
            }

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
            // TODO: The textures are getting double compressed or something!
            return base.ConvertMaterial(emc, context);
        }

    }

}
