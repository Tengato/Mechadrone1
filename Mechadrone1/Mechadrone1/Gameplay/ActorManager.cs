using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BEPUphysics;
using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Skelemator;
using SlagformCommon;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Threading;
using System.IO;
using BEPUphysics.CollisionRuleManagement;

namespace Mechadrone1
{
    /// <summary>
    /// Manages the actor collection and simulation.
    /// </summary>
    class ActorManager
    {
        private List<Cue> mCueSounds;            // List of currently playing 3D sounds
        private List<Cue> mCueSoundsDelete;      // List of 3D sounds that have finished and are ready to delete
        private ObservableCollection<PlayerView> mPlayers;
        private Dictionary<int, Actor> mActors;
        private ContentManager mContentLoader;
        public BoundingBox WorldBounds { get; private set; }
        public Space SimSpace { get; private set; }
        public Random Random { get; private set; }
        private Dictionary<string, ActorManifest> mTemplates;
        public HashSet<int> CameraClippingSimObjects { get; private set; }
        public CollisionGroup CharactersCollisionGroup { get; private set; }
        public CollisionGroup PickupsCollisionGroup { get; private set; }

        public event EventHandler LevelLoaded;
        public event AvatarSpawnedEventHandler AvatarSpawned;
        public event UpdateStepEventHandler PreAnimationUpdateStep;
        public event UpdateStepEventHandler AnimationUpdateStep;
        public event UpdateStepEventHandler PostAnimationUpdateStep;
        public event UpdateStepEventHandler PostPhysicsUpdateStep;
        private event UpdateStepEventHandler mDespawnStep;
        public event UpdateStepEventHandler DespawnStep
        {
            add
            {
                if (mDespawnStep == null || !mDespawnStep.GetInvocationList().Contains(value))
                    mDespawnStep += value;
            }
            remove
            {
                mDespawnStep -= value;
            }
        }
        public event UpdateStepEventHandler ProcessAIStep;
        public event UpdateStepEventHandler UpdateComplete;
        public event EventHandler LevelUnloading;

        public TimeSpan CurrentTime { get; private set; }

        public ActorManager(ObservableCollection<PlayerView> players)
        {
            mContentLoader = new ContentManager(SharedResources.Game.Services, "Content");
            mCueSounds = new List<Cue>();
            mCueSoundsDelete = new List<Cue>();
            mPlayers = players;
            mActors = new Dictionary<int, Actor>();
            WorldBounds = new BoundingBox(Vector3.Zero, Vector3.One);
            SimSpace = new Space();
            SimSpace.ForceUpdater.Gravity = new BEPUutilities.Vector3(0.0f, -89.2f, 0.0f);   // 9 units = 1 meter?
            CharactersCollisionGroup = new CollisionGroup();
            PickupsCollisionGroup = new CollisionGroup();
            CollisionGroup.DefineCollisionRule(CharactersCollisionGroup, PickupsCollisionGroup, CollisionRule.NoSolver);
            Random = new Random();
            mTemplates = new Dictionary<string, ActorManifest>();
            CameraClippingSimObjects = new HashSet<int>();
            CollisionComponent.CollisionComponentInitialized += CollisionComponentInitializedHandler;
            CurrentTime = TimeSpan.Zero;
        }

        public void Release()
        {
            OnLevelUnloading();
            mContentLoader.Unload();
            mContentLoader.Dispose();
            mPlayers.CollectionChanged -= AddRemovePlayerHandler;
            CollisionComponent.CollisionComponentInitialized -= CollisionComponentInitializedHandler;
        }

        private void AddActor(Actor actor)
        {
            mActors.Add(actor.Id, actor);
            actor.ActorDespawning += ActorDespawningHandler;
        }

        public void LoadContent(LevelManifest levelManifest)
        {
            foreach (ActorManifest actorManifest in levelManifest.Actors)
            {
                AddActor(Actor.CreateFromManifest(mContentLoader, actorManifest, Actor.INVALID_ACTOR_ID));
            }

            MethodInfo miLoad = (typeof(ContentManager)).GetMethod("Load");
            foreach (string templateManifestAssetName in levelManifest.Templates)
            {
                TemplateManifest templateManifest = mContentLoader.Load<TemplateManifest>(Path.Combine("templates", templateManifestAssetName));
                mTemplates.Add(templateManifest.Actor.Name, templateManifest.Actor);

                foreach (AssetInfo asset in templateManifest.AssetsToPreload)
                {
                    Type assetType = System.Type.GetType(asset.FullTypeName);
                    MethodInfo miLoadConstructed = miLoad.MakeGenericMethod(new Type[] { assetType });
                    miLoadConstructed.Invoke(mContentLoader, new object[] { asset.Name });
                }
            }

            // Step the simulation to let all 'settling' events take care of themselves.
            SimSpace.Update((float)(SharedResources.Game.TargetElapsedTime.TotalSeconds));
            OnLevelLoaded(EventArgs.Empty);

            foreach (PlayerView player in mPlayers)
            {
                if (player.ActorId == Actor.INVALID_ACTOR_ID)
                {
                    SpawnPlayerActor(player);
                }
            }

            mPlayers.CollectionChanged += AddRemovePlayerHandler;
        }

        private void SpawnPlayerActor(PlayerView player)
        {
            Actor newActor = SpawnTemplate(player.AvatarDesc.TemplateName);
            newActor.Customize(player.AvatarDesc, mContentLoader);
            player.AssignAvatar(newActor.Id);
            newActor.OnBecameAvatar();
            OnAvatarSpawnedEvent(newActor.Id);
        }

        private void OnLevelLoaded(EventArgs e)
        {
            EventHandler handler = LevelLoaded;

            if (handler != null)
                handler(this, e);
        }

        private void ActorDespawningHandler(object sender, EventArgs e)
        {
            // We need to ensure this is only called during the despawn step of the update loop. See Actor.Despawn().
            Actor despawningActor = (Actor)sender;
            mActors.Remove(despawningActor.Id);
            CollisionComponent cc = despawningActor.GetComponent<CollisionComponent>(ActorComponent.ComponentType.Physics);
            if (cc != null)
                SimSpace.Remove(cc.SimObject);
        }

        public Actor GetActorById(int actorId)
        {
            return mActors.ContainsKey(actorId) ? mActors[actorId] : null;
        }

        public Actor GetActorByName(string name)
        {
            return mActors.Values.First(a => a.Name == name);
        }

        public bool IsPlayer(int actorId)
        {
            return mPlayers.Any(p => p.ActorId == actorId);
        }

        public bool IsMob(int actorId)
        {
            return (mActors[actorId].GetBehaviorThatImplementsType<IAgentStateManager>() != null);
        }

        public PlayerView GetPlayerViewOfAvatar(int actorId)
        {
            return mPlayers.Single(p => p.ActorId == actorId);
        }

        public Actor SpawnTemplate(string templateName)
        {
            Actor newActor = Actor.CreateFromManifest(mContentLoader, mTemplates[templateName], Actor.INVALID_ACTOR_ID);
            AddActor(newActor);
            return newActor;
        }

        public void Update(GameTime gameTime)
        {
            CurrentTime += gameTime.ElapsedGameTime;
            float elapsedTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            // Update code is broken into several steps, because there are lots of dependencies between them.

            // At this point in the frame, the input processors have updated the desired state of all input-driven objects.

            // 1. Pre-animation update. Game-driven actors (not part of the simulation) should update their position now.
            // Also, actors should make sure their animation players have the correct animations selected. (Based on
            // game-driven or input stimuli). This usually requires stepping their state machines.
            OnPreAnimationUpdateStep(gameTime);

            // 2. Step the actor animation players here.
            OnUpdateAnimationStep(gameTime);

            // 3. Adjust poses as needed. Update physics models with new actor positions.
            OnPostAnimationUpdateStep(gameTime);

            // 4. Step the physics sim. This will trigger a large amount of the gameplay code.
            SimSpace.Update(elapsedTime);

            // 5. Ragdoll update.

            // 6. Post-physics update. Update Actor visual model positions with physics model positions:
            OnPostPhysicsUpdateStep(gameTime);

            // 7. Finalize animations here. Such as for rigs that were adjusted due to physics considerations?

            // 8. Misc updates:

            // TODO: P2: Can be parallel?
            // Delete any finished 3D sounds
            mCueSoundsDelete.Clear();

            foreach (Cue cue in mCueSounds)
            {
                if (cue.IsStopped)
                    mCueSoundsDelete.Add(cue);
            }

            foreach (Cue cue in mCueSoundsDelete)
            {
                mCueSounds.Remove(cue);
                cue.Dispose();
            }

            // Despawn all actors that are waiting to do so.
            OnDespawnStep(gameTime);

            // Process AIs that need to evaluate the new game state.
            OnProcessAIStep(gameTime);

            // Notice to allow bounding volumes to be recomputed or any other processing that needs to occur.
            OnUpdateComplete(gameTime);
        }

        private void OnPreAnimationUpdateStep(GameTime gameTime)
        {
            UpdateStepEventHandler handler = PreAnimationUpdateStep;

            if (handler != null)
                handler(this, new UpdateStepEventArgs(gameTime));
        }

        private void OnUpdateAnimationStep(GameTime gameTime)
        {
            UpdateStepEventHandler handler = AnimationUpdateStep;

            if (handler != null)
                handler(this, new UpdateStepEventArgs(gameTime));
        }

        private void OnPostAnimationUpdateStep(GameTime gameTime)
        {
            UpdateStepEventHandler handler = PostAnimationUpdateStep;

            if (handler != null)
                handler(this, new UpdateStepEventArgs(gameTime));
        }

        private void OnPostPhysicsUpdateStep(GameTime gameTime)
        {
            UpdateStepEventHandler handler = PostPhysicsUpdateStep;

            if (handler != null)
                handler(this, new UpdateStepEventArgs(gameTime));
        }

        private void OnAvatarSpawnedEvent(int actorId)
        {
            AvatarSpawnedEventHandler handler = AvatarSpawned;

            if (handler != null)
                handler(this, new AvatarSpawnedEventArgs(actorId, Random.Next()));
        }

        private void OnDespawnStep(GameTime gameTime)
        {
            UpdateStepEventHandler handler = mDespawnStep;

            if (handler != null)
                handler(this, new UpdateStepEventArgs(gameTime));
        }

        private void OnProcessAIStep(GameTime gameTime)
        {
            UpdateStepEventHandler handler = ProcessAIStep;

            if (handler != null)
                handler(this, new UpdateStepEventArgs(gameTime));
        }

        private void OnUpdateComplete(GameTime gameTime)
        {
            UpdateStepEventHandler handler = UpdateComplete;

            if (handler != null)
                handler(this, new UpdateStepEventArgs(gameTime));
        }

        private void OnLevelUnloading()
        {
            AvatarSpawnComponent.Reset();

            EventHandler handler = LevelUnloading;

            if (handler != null)
                handler(this, EventArgs.Empty);
        }

        private void AddRemovePlayerHandler(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (PlayerView player in e.NewItems)
                {
                    if (player.ActorId == Actor.INVALID_ACTOR_ID)
                        SpawnPlayerActor(player);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (PlayerView player in e.OldItems)
                {
                    mActors[player.ActorId].Despawn();
                }
            }
            else
            {
                throw new InvalidOperationException("Unsupported changes to players list.");
            }
        }

        private void CollisionComponentInitializedHandler(object sender, EventArgs e)
        {
            CollisionComponent cc = sender as CollisionComponent;
            SimSpace.Add(cc.SimObject);
        }

        // TODO: P2: Decide what to do with this:
        /* Old lighting code:
        public List<DirectLight> GetObjectLights(ISceneObject sceneObject, Vector3 eyePosition)
        {
            if (sceneObject is TerrainSector)
            {
                return GetTerrainLights();
            }
            else
            {
                return GetModelLights(sceneObject.Position, eyePosition);
            }
        }


        private List<DirectLight> GetModelLights(Vector3 position, Vector3 eyePosition)
        {
            List<DirectLight> lights = new List<DirectLight>();

            lights.Add(ShadowCastingLight);

            DirectLight fill = new DirectLight();
            fill.Ambient = Vector4.Zero;
            Matrix complementary = Matrix.CreateFromAxisAngle(Vector3.One * SlagformCommon.SlagMath.INV_SQRT_3, MathHelper.Pi);
            Vector3 diffuse = new Vector3(ShadowCastingLight.Diffuse.X, ShadowCastingLight.Diffuse.Y, ShadowCastingLight.Diffuse.Z);
            diffuse = Vector3.Transform(diffuse, complementary);
            fill.Diffuse.X = diffuse.X / 2.5f;
            fill.Diffuse.Y = diffuse.Y / 2.5f;
            fill.Diffuse.Z = diffuse.Z / 2.5f;
            fill.Diffuse.W = 1.0f;
            fill.Specular = Vector4.Zero;
            fill.Energy = ShadowCastingLight.Energy;
            Vector3 objToEye = eyePosition - position;
            Vector3 keyEyeCross = Vector3.Normalize(Vector3.Cross(ShadowCastingLight.Direction, objToEye));
            Matrix fillRot = Matrix.CreateFromAxisAngle(keyEyeCross, -MathHelper.Pi / 6.0f);
            fill.Direction = Vector3.Normalize(Vector3.Transform(-objToEye, fillRot));
            lights.Add(fill);

            DirectLight rim;
            rim = new DirectLight();
            rim.Ambient = Vector4.Zero;
            rim.Diffuse = Vector4.Zero;
            rim.Specular = ShadowCastingLight.Specular;
            rim.Energy = ShadowCastingLight.Energy * 1.2f;

            Matrix keyRot = Matrix.CreateFromAxisAngle(keyEyeCross, MathHelper.Pi / 6.0f);
            rim.Direction = Vector3.Normalize(Vector3.Transform(objToEye, keyRot));
            //lights.Add(rim);

            return lights;
        }


        private List<DirectLight> GetTerrainLights()
        {
                List<DirectLight> lights = new List<DirectLight>();

                lights.Add(ShadowCastingLight);

                DirectLight fill = new DirectLight();
                fill.Ambient = Vector4.Zero;
                Matrix complementary = Matrix.CreateFromAxisAngle(Vector3.One * SlagformCommon.SlagMath.INV_SQRT_3, MathHelper.Pi);
                Vector3 diffuse = new Vector3(ShadowCastingLight.Diffuse.X, ShadowCastingLight.Diffuse.Y, ShadowCastingLight.Diffuse.Z);
                diffuse = Vector3.Transform(diffuse, complementary);
                fill.Diffuse.X = diffuse.X / 5.0f;
                fill.Diffuse.Y = diffuse.Y / 5.0f;
                fill.Diffuse.Z = diffuse.Z / 5.0f;
                fill.Diffuse.W = 1.0f;
                fill.Specular = Vector4.Zero;
                fill.Energy = ShadowCastingLight.Energy;
                fill.Direction = Vector3.Down;
                lights.Add(fill);

                return lights;
        }
        ** End old lighting code */


        /* Old sound code
        /// <summary>
        /// Play a sound in 2D
        /// </summary>
        public void PlaySound(String soundName)
        {
            mSoundBank.PlayCue(soundName);
        }

        /// <summary>
        /// Play a sound in 3D at given position 
        /// (just fake 3D using distance attenuation but no stereo)
        /// </summary>
        public void PlaySound3D(String soundName, Vector3 position)
        {
            // get distance from sound to closest player
            float minimumDistance = float.MaxValue;

            foreach (KeyValuePair<PlayerIndex, GameObject> player in Avatars)
            {
                float dist = (position - player.Value.Position).LengthSquared();
                if (dist < minimumDistance)
                    minimumDistance = dist;
            }

            // create a new sound instance
            Cue cue = mSoundBank.GetCue(soundName);
            mCueSounds.Add(cue);

            // set volume based on distance from closest player
            cue.SetVariable("Distance", (float)Math.Sqrt(minimumDistance));

            // play sound 
            cue.Play();
        }
         * End old sound code */

        public PlayerView GetPlayer(int playerId)
        {
            return mPlayers.Single(p => p.PlayerId == playerId);
        }
    }
}
