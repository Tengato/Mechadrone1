using System.Collections.Generic;
using System;
using Microsoft.Xna.Framework.Content;
using Manifracture;

namespace Mechadrone1
{
    class BehaviorComponent : ActorComponent, ICustomizable
    {
        public Dictionary<Type, Behavior> Behaviors { get; private set; }

        public override ComponentType Category { get { return ComponentType.Behavior; } }

        public BehaviorComponent(Actor owner)
            : base(owner)
        {
            Behaviors = new Dictionary<Type, Behavior>();
        }

        public override void Initialize(ContentManager contentLoader, ComponentManifest manifest)
        {
            List<ComponentManifest> behaviorManifests = manifest.Properties[ManifestKeys.BEHAVIORS] as List<ComponentManifest>;
            foreach (ComponentManifest bm in behaviorManifests)
            {
                CreateAndAddBehavior(contentLoader, bm);
            }
        }

        private void CreateAndAddBehavior(ContentManager contentLoader, ComponentManifest manifest)
        {
            Type behaviorLoadedType = Type.GetType(manifest.TypeFullName);
            object[] basicCtorParams = new object[] { Owner };
            Behavior newBehavior = Activator.CreateInstance(behaviorLoadedType, basicCtorParams) as Behavior;
            Behaviors.Add(behaviorLoadedType, newBehavior);
            newBehavior.Initialize(contentLoader, manifest);
        }

        public T GetBehavior<T>() where T : Behavior
        {
            return Behaviors.ContainsKey(typeof(T)) ? Behaviors[typeof(T)] as T : null;
        }

        public T GetBehaviorThatImplementsType<T>() where T : class
        {
            foreach (KeyValuePair<Type, Behavior> behavior in Behaviors)
            {
                T candidate = behavior.Value as T;
                if (candidate != null)
                    return candidate;
            }
            return null;
        }

        public void Customize(CharacterInfo customizations, ContentManager contentLoader)
        {
            foreach (KeyValuePair<Type, Behavior> behavior in Behaviors)
            {
                ICustomizable candidate = behavior.Value as ICustomizable;
                if (candidate != null)
                    candidate.Customize(customizations, contentLoader);
            }
        }

        public override void Release()
        {
            foreach (KeyValuePair<Type, Behavior> behavior in Behaviors)
            {
                behavior.Value.Release();
            }
        }
    }
}
