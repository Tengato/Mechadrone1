using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Manifracture;
using System.Reflection;
using Microsoft.Xna.Framework.Content;

namespace Mechadrone1.Gameplay
{
    class GameObjectLoader
    {
        Dictionary<string, GameObject> templates;
        IGameManager owner;
        ContentManager contentMan;


        public GameObjectLoader(IGameManager owner, ContentManager contentManager)
        {
            this.owner = owner;
            contentMan = contentManager;
        }


        public GameObject LoadObject(GameObjectLoadInfo goli)
        {
            // The GameObjectLoadInfo may direct us to instantiate any kind of class that inherits from
            // GameObject. So we must use reflection to construct the object and initialize its properties.
            Type goLoadedType = Type.GetType(goli.TypeFullName);
            PropertyInfo[] goLoadedProperties = goLoadedType.GetProperties();
            object[] basicCtorParams = new object[] { owner };
            GameObject goLoaded = Activator.CreateInstance(goLoadedType, basicCtorParams) as GameObject;

            // Iterator variables for the object's properties:
            PropertyInfo goLoadedProperty;
            // If the property requires an asset from the content manager, it will have a special attribute.
            object[] goLoadedPropertyAttributes;
            // We'll have to construct the ContentManager.Load generic method because we don't know the asset
            // type until runtime.
            MethodInfo miLoad = (typeof(ContentManager)).GetMethod("Load");
            MethodInfo miLoadConstructed;

            foreach (KeyValuePair<string, object> kvp in goli.Properties)
            {
                // Find the matching runtime property:
                goLoadedProperty = goLoadedProperties.Single(pi => pi.Name == kvp.Key);
                goLoadedPropertyAttributes = goLoadedProperty.GetCustomAttributes(false);

                if (goLoadedPropertyAttributes.OfType<LoadedAssetAttribute>().Count() > 0)
                {
                    // The presence of that attribute indicates that the property value in the manifest is
                    // the name of the asset to be loaded.
                    miLoadConstructed = miLoad.MakeGenericMethod(new Type[] { goLoadedProperty.PropertyType });
                    // TODO: catch TargetInvocationException, log an error message, and load some kind of obvious placeholder content so missing content won't break game:
                    goLoadedProperty.SetValue(goLoaded, miLoadConstructed.Invoke(contentMan, new object[] { kvp.Value }), null);
                }
                else
                {
                    goLoadedProperty.SetValue(goLoaded, kvp.Value, null);
                }
            }

            return goLoaded;
        }
    }
}
