using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Xna.Framework.Content;
using Manifracture;
using System.Threading;

namespace Mechadrone1
{
    sealed class Actor
    {
        private Dictionary<ActorComponent.ComponentType, ActorComponent> mComponents;
        public int Id { get; private set; }
        public string Name { get; private set; }

        public event EventHandler ComponentsCreated;
        public event EventHandler ActorInitialized;
        public event EventHandler ActorDespawning;
        public event EventHandler BecameAvatar;

        private static int sNextId;
        public const int INVALID_ACTOR_ID = Int32.MaxValue;

        static Actor()
        {
            sNextId = 0;
        }

        private Actor(int id)
        {
            Id = id;
            Name = String.Empty;
            mComponents = new Dictionary<ActorComponent.ComponentType, ActorComponent>();
        }

        public static Actor CreateFromManifest(ContentManager contentLoader, ActorManifest actorManifest, int actorId)
        {
            if (actorId == INVALID_ACTOR_ID)
                actorId = Interlocked.Increment(ref sNextId) - 1;

            Actor newActor = new Actor(actorId);
            newActor.Name = actorManifest.Name;

            foreach (ComponentManifest componentManifest in actorManifest.Components)
            {
                CreateAndAddComponent(contentLoader, componentManifest, newActor);
            }

            newActor.OnComponentsCreated(EventArgs.Empty);
            newActor.OnActorInitialized(EventArgs.Empty);

            return newActor;
        }

        private static void CreateAndAddComponent(ContentManager contentLoader, ComponentManifest manifest, Actor owner)
        {
            Type acLoadedType = Type.GetType(manifest.TypeFullName);
            object[] basicCtorParams = new object[] { owner };
            ActorComponent newComponent = Activator.CreateInstance(acLoadedType, basicCtorParams) as ActorComponent;
            owner.mComponents.Add(newComponent.Category, newComponent);
            newComponent.Initialize(contentLoader, manifest);
        }

        public T GetComponent<T>(ActorComponent.ComponentType componentType) where T : ActorComponent
        {
            return mComponents.ContainsKey(componentType) ? mComponents[componentType] as T : null;
        }

        public T GetBehavior<T>() where T : Behavior
        {
            if (mComponents.ContainsKey(ActorComponent.ComponentType.Behavior))
            {
                BehaviorComponent bc = mComponents[ActorComponent.ComponentType.Behavior] as BehaviorComponent;
                return bc.GetBehavior<T>();
            }
            return null;
        }

        public T GetBehaviorThatImplementsType<T>() where T : class
        {
            if (mComponents.ContainsKey(ActorComponent.ComponentType.Behavior))
            {
                BehaviorComponent bc = mComponents[ActorComponent.ComponentType.Behavior] as BehaviorComponent;
                return bc.GetBehaviorThatImplementsType<T>();
            }
            return null;
        }

        public void Despawn()
        {
            GameResources.ActorManager.DespawnStep += DespawnStepHandler;
        }

        private void DespawnStepHandler(object sender, UpdateStepEventArgs e)
        {
            OnActorDespawning(EventArgs.Empty);
            foreach (ActorComponent component in mComponents.Values)
            {
                component.Release();
            }

            GameResources.ActorManager.DespawnStep -= DespawnStepHandler;
        }

        private void OnComponentsCreated(EventArgs e)
        {
            EventHandler handler = ComponentsCreated;

            if (handler != null)
            {
                handler(this, e);
            }

            ComponentsCreated = null;   // We don't need to keep around a List with dozens of delegates...
        }

        private void OnActorInitialized(EventArgs e)
        {
            EventHandler handler = ActorInitialized;

            if (handler != null)
            {
                handler(this, e);
            }

            ActorInitialized = null;    // We don't need to keep around a List with dozens of delegates...
        }

        public void Customize(CharacterInfo customizations, ContentManager contentLoader)
        {
            foreach (KeyValuePair<ActorComponent.ComponentType, ActorComponent> kvp in mComponents)
            {
                ICustomizable candidate = kvp.Value as ICustomizable;
                if (candidate != null)
                    candidate.Customize(customizations, contentLoader);
            }
        }

        private void OnActorDespawning(EventArgs e)
        {
            EventHandler handler = ActorDespawning;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void OnBecameAvatar()
        {
            EventHandler handler = BecameAvatar;

            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }

            BecameAvatar = null;   // We don't need to keep around a List with dozens of delegates...
        }
    }
}
