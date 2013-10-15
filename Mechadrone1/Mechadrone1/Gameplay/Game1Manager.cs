using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BEPUphysics;
using Manifracture;
using Mechadrone1.Rendering;
using Mechadrone1.StateManagement;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Skelemator;
using Mechadrone1.Gameplay.Prefabs;
using SlagformCommon;

namespace Mechadrone1.Gameplay
{
    /// <summary>
    /// Manages the gameplay object model and simulation. It generally doesn't care about
    /// rendering problems but will store information that is necessary for dealing with them,
    /// like a quadtree.
    /// </summary>
    class Game1Manager : IRenderableScene, IGameManager
    {
        SoundBank soundBank;
        List<Cue> cueSounds;            // list of currently playing 3D sounds
        List<Cue> cueSoundsDelete;      // 3D sounds finished and ready to delete

        float[] vibrationTime;          // pad vibration times for each player (zero for no vibration)

        public Skydome Sky { get; set; }

        public List<TerrainChunk> Substrate { get; private set; }
        public List<GameObject> GameObjects { get; private set; }
        public BoundingBox WorldBounds { get; private set; }

        public QuadTree QuadTree { get; private set; }

        public FogDesc Fog { get; set; }

        public Dictionary<PlayerIndex, GameObject> Avatars { get; set; }


        public GameObject GetGameObject(string name)
        {
            for (int i = 0; i < GameObjects.Count; i++)
            {
                if (GameObjects[i].Name == name)
                    return GameObjects[i];
            }

            return null;
        }

        public DirectLight ShadowCastingLight { get; private set; }

        public Space SimSpace { get; private set; }

        public event UpdateStepEventHandler BotControlUpdateStep;
        public event UpdateStepEventHandler PreAnimationUpdateStep;
        public event UpdateStepEventHandler PostPhysicsUpdateStep;

        public PowerupManager powerup;
        public ProjectileManager projectile;
        public AnimSpriteManager animatedSprite;
        public ParticleManager particle;

        // particle texture files (matches ParticleSystemType)
        string[] particleFiles;
        Texture2D[] particleTextures;

        // animated sprite texture files (matches AnimSpriteType)
        string[] animatedSpriteFiles;
        Texture2D[] animatedSpriteTextures;

        // projectile modell files (matches ProjectileType)
        string[] projectileFiles;
        Model[] projectileModels;

        // powerup model files (matches PowerupType)
        string[] powerupFiles;
        Model[] powerupModels;

        public Game1Manager(SoundBank soundBank)
        {
            this.soundBank = soundBank;

            GameObjects = new List<GameObject>();

            Avatars = new Dictionary<PlayerIndex, GameObject>();

            cueSounds = new List<Cue>();
            cueSoundsDelete = new List<Cue>();

            vibrationTime = new float[GameOptions.MaxPlayers];

            particleFiles = new string[] { "Spark1", "Point1", "Spark2", "Point1", "Point2" };
            animatedSpriteFiles = new string[] { "BlasterGrid_16", "MissileGrid_16", "ShipGrid_32", "SpawnGrid_16", "ShieldGrid_32" };
            projectileFiles = new string[0];
            powerupFiles = new string[0];

            powerup = new PowerupManager(this);
            projectile = new ProjectileManager();
            animatedSprite = new AnimSpriteManager();
            particle = new ParticleManager();

            SimSpace = new Space();
            SimSpace.ForceUpdater.Gravity = new BEPUutilities.Vector3(0.0f, -89.2f, 0.0f);   // 9 units = 1 meter?

            Substrate = new List<TerrainChunk>();
        }


        public void LoadContent(GraphicsDevice gd, ContentManager contentMan, Game1Manifest manifest)
        {
            WorldBounds = new BoundingBox(Vector3.Zero, Vector3.One);

            Sky = new Skydome(contentMan.Load<TextureCube>(manifest.SkydomeTextureName),
                contentMan.Load<Model>("models\\system\\SkyEllipsoid"),
                contentMan.Load<Effect>("shaders\\Skymap"));

            // Load terrain assests from level manifest:
            foreach (TerrainChunkLoadInfo tcli in manifest.TerrainChunks)
            {
                TerrainChunk chunk = new TerrainChunk(
                    contentMan.Load<Skelemator.Terrain>(tcli.AssetName),
                    tcli.Position,
                    tcli.CastsShadow);
                Substrate.Add(chunk);
                SimSpace.Add(chunk.SimulationObject);

                BoundingBox chunkExtents = BepuConverter.Convert(chunk.SimulationObject.BoundingBox);
                chunkExtents.Max.Y += (chunkExtents.Max - chunkExtents.Min).Length() * SlagformCommon.SlagMath.INV_SQRT_3;   // Add some head room.

                WorldBounds = SlagformCommon.SpaceUtils.CombineBBoxes(WorldBounds, chunkExtents);
            }

            QuadTree = new QuadTree(WorldBounds);

            foreach (TerrainChunk tc in Substrate)
            {
                foreach (TerrainSector ts in tc.Sectors)
                {
                    QuadTree.AddOrUpdateSceneObject(ts);
                }
            }

            // Load game object assets from level manifest:
            // The manifest may direct us to instantiate any kind of class that inherits from GameObject.
            // So we must use reflection to construct the object and initialize its properties.
            GameObject goLoaded;
            Type goLoadedType;
            PropertyInfo[] goLoadedProperties;
            PropertyInfo goLoadedProperty;
            // If the property requires an asset from the content manager, it will have a special attribute.
            object[] goLoadedPropertyAttributes;
            // We'll have to construct the ContentManager.Load generic method because we don't know the type
            // at runtime.
            MethodInfo miLoad = (typeof(ContentManager)).GetMethod("Load");
            MethodInfo miLoadConstructed;

            object[] basicCtorParams = new object[] { this };

            foreach (GameObjectLoadInfo goli in manifest.GameObjects)
            {
                goLoadedType = Type.GetType(goli.TypeFullName);
                goLoadedProperties = goLoadedType.GetProperties();
                goLoaded = Activator.CreateInstance(goLoadedType, basicCtorParams) as GameObject;

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

                goLoaded.Initialize();

                GameObjects.Add(goLoaded);

                if (goLoaded.Visible)
                    QuadTree.AddOrUpdateSceneObject(goLoaded);
            }

            DirectLight dirLight = manifest.KeyLight;
            dirLight.Direction = Vector3.Normalize(dirLight.Direction);
            ShadowCastingLight = dirLight;

            Fog = manifest.Fog;

            // load particle textures
            if (particleTextures == null)
            {
                int i, j = particleFiles.GetLength(0);
                particleTextures = new Texture2D[j];
                for (i = 0; i < j; i++)
                    particleTextures[i] = contentMan.Load<Texture2D>("particles/" + particleFiles[i]);
            }

            // load animated sprite textures
            if (animatedSpriteTextures == null)
            {
                int i, j = animatedSpriteFiles.GetLength(0);
                animatedSpriteTextures = new Texture2D[j];
                for (i = 0; i < j; i++)
                    animatedSpriteTextures[i] = contentMan.Load<Texture2D>("explosions/" + animatedSpriteFiles[i]);
            }

            // load projectile models
            if (projectileModels == null)
            {
                int i, j = projectileFiles.GetLength(0);
                projectileModels = new Model[j];
                for (i = 0; i < j; i++)
                    projectileModels[i] = contentMan.Load<Model>("projectiles/" + projectileFiles[i]);
            }

            // load powerup models
            if (powerupModels == null)
            {
                int i, j = powerupFiles.GetLength(0);
                powerupModels = new Model[j];
                for (i = 0; i < j; i++)
                    powerupModels[i] = contentMan.Load<Model>("powerups/" + powerupFiles[i]);
            }

            // load content for animated sprite manager
            animatedSprite.LoadContent(gd, contentMan);

            // load content for particle system manager
            particle.LoadContent(gd, contentMan);
        }


        public void HandleInput(GameTime gameTime, InputManager input)
        {
            // Player-controlled objects need to update animation state machines,
            // perform shape cast queries for movement, and such.
            foreach (KeyValuePair<PlayerIndex, GameObject> kvp in Avatars)
            {
                kvp.Value.HandleInput(gameTime, input, kvp.Key);
            }

            // TODO: remove temp code:
            //GetGameObject("Suzanne").HandleInput(gameTime, input, PlayerIndex.One);
        }


        public void Update(GameTime gameTime)
        {
            float elapsedTime = (float)(gameTime.ElapsedGameTime.TotalSeconds);

            // These seven update steps must be carefully organized, because they are highly
            // interdependent.

            // 1. Pre-animation update. Game-driven objects should update their position now.
            OnPreAnimationUpdateStep(gameTime);

            // 2. Update object animations here.

            // 3. Adjust poses as needed. Update physics models with game object positions.

            // 4. Step the physics sim:
            SimSpace.Update(elapsedTime);

            // 5. Ragdoll update.

            // 6. Post-physics update. Update game objects with physics model positions:
            OnPostPhysicsUpdateStep(gameTime);

            // 7. Finalize animations here.

            // 8. Misc updates:
            foreach (GameObject go in Avatars.Values)
            {
                go.UpdateCamera(elapsedTime);
            }

            // update animated projectiles
            projectile.Update(elapsedTime);

            // update powerups
            powerup.Update(elapsedTime);

            // update animated sprites
            animatedSprite.Update(elapsedTime);

            // update particle systems
            particle.Update(elapsedTime);


            // if gamepad vibreate enabled
            if (GameOptions.UseGamepadVibrate)
            {
                // check vibration for each player
                for (int i = 0; i < GameOptions.MaxPlayers; i++)
                {
                    float leftMotorAmount = 0;
                    float rightMotorAmount = 0;

                    // if left vibration
                    if (vibrationTime[i] > 0)
                    {
                        leftMotorAmount = GameOptions.VibrationIntensity *
                            Math.Min(1.0f,
                            vibrationTime[i] / GameOptions.VibrationFadeout);
                        vibrationTime[i] = Math.Max(0.0f,
                            vibrationTime[i] - elapsedTime);
                    }
                    else
                        // if right vibration
                        if (vibrationTime[i] < 0)
                        {
                            rightMotorAmount = GameOptions.VibrationIntensity *
                                Math.Min(1.0f,
                                -vibrationTime[i] / GameOptions.VibrationFadeout);
                            vibrationTime[i] = Math.Min(0.0f,
                                    vibrationTime[i] + elapsedTime);
                        }

                    // set vibration values
                    GamePad.SetVibration((PlayerIndex)i, leftMotorAmount,
                        rightMotorAmount);
                }
            }

            // delete any finished 3D sounds
            cueSoundsDelete.Clear();

            foreach (Cue cue in cueSounds)
            {
                if (cue.IsStopped)
                    cueSoundsDelete.Add(cue);
            }

            foreach (Cue cue in cueSoundsDelete)
            {
                cueSounds.Remove(cue);
                cue.Dispose();
            }

        }


        private void OnPreAnimationUpdateStep(GameTime gameTime)
        {
            PreAnimationUpdateStep(this, new UpdateStepEventArgs(gameTime));

            // This event is a temporary stand-in for a real AI system.
            BotControlUpdateStep(this, new UpdateStepEventArgs(gameTime));
        }


        private void OnPostPhysicsUpdateStep(GameTime gameTime)
        {
            PostPhysicsUpdateStep(this, new UpdateStepEventArgs(gameTime));
        }


        public void AddPlayer(PlayerIndex player, string avatarName)
        {
            GameObject avatar = GetGameObject(avatarName);
            Avatars.Add(player, avatar);
            avatar.CreateCamera();
        }


        public void RemovePlayer(PlayerIndex player)
        {
            throw new NotImplementedException();
        }


        public ICamera GetCamera(PlayerIndex player)
        {
            return Avatars[player].Camera;
        }


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
            fill.Diffuse.X = diffuse.X / 7.0f;
            fill.Diffuse.Y = diffuse.Y / 7.0f;
            fill.Diffuse.Z = diffuse.Z / 7.0f;
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
            lights.Add(rim);

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


        /// <summary>
        /// Play a sound in 2D
        /// </summary>
        public void PlaySound(String soundName)
        {
            soundBank.PlayCue(soundName);
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
            Cue cue = soundBank.GetCue(soundName);
            cueSounds.Add(cue);

            // set volume based on distance from closest player
            cue.SetVariable("Distance", (float)Math.Sqrt(minimumDistance));

            // play sound 
            cue.Play();
        }


        /// <summary>
        /// Create a new particle system and add it to the particle system manager
        /// </summary>
        public ParticleSystem AddParticleSystem(ParticleSystemType type, Matrix transform)
        {
            ParticleSystem ps = null;

            switch (type)
            {
                case ParticleSystemType.ShipExplode:
                    ps = new ParticleSystem(
                        ParticleSystemType.ShipExplode,
                        200,                    // num particles
                        0.0f,                   // emission angle (0 for omni)
                        0.8f, 0.8f,             // particle and total time
                        20.0f, 50.0f,           // min and max size
                        600.0f, 1000.0f,        // min and max vel
                        new Vector4(1.0f, 1.0f, 1.0f, 1.6f),    // start color
                        new Vector4(1.0f, 1.0f, 1.0f, 0.0f),    // end color
                        particleTextures[(int)type],          // texture
                        DrawMode.Additive,       // draw mode
                        transform);              // transform
                    break;
                case ParticleSystemType.ShipTrail:
                    ps = new ParticleSystem(
                        ParticleSystemType.ShipTrail,
                        100,                    // num particles
                        5.0f,                   // emission angle (0 for omni)
                        0.5f, 2.0f,             // particle time and total time
                        50.0f, 100.0f,          // min and max size
                        1000.0f, 1500.0f,       // min and max vel
                        new Vector4(0.5f, 0.2f, 0.0f, 1.0f),    // start color
                        new Vector4(1.0f, 0.0f, 0.0f, 0.0f),    // end color
                        particleTextures[(int)type],            // texture
                        DrawMode.AdditiveAndGlow,  // draw mode
                        transform);                // transform
                    break;
                case ParticleSystemType.MissileExplode:
                    ps = new ParticleSystem(
                        ParticleSystemType.MissileExplode,
                        200,                    // num particles
                        0.0f,                   // emission angle (0 for omni)
                        0.5f, 0.5f,             // particle and total time
                        20.0f, 60.0f,           // min and max size
                        800.0f, 1200.0f,        // min and max vel
                        new Vector4(1.0f, 1.0f, 1.0f, 1.5f),    // start color
                        new Vector4(1.0f, 1.0f, 1.0f, -0.5f),   // end color
                        particleTextures[(int)type],          // texture
                        DrawMode.AdditiveAndGlow,      // draw mode
                        transform);              // transform
                    break;
                case ParticleSystemType.MissileTrail:
                    ps = new ParticleSystem(
                        ParticleSystemType.MissileTrail,
                        100,                    // num particles
                        10.0f,                  // emission angle (0 for omni)
                        0.5f, 1.0f,             // particle time and total time
                        15.0f, 30.0f,           // min and max size
                        1000.0f, 1500.0f,       // min and max vel
                        new Vector4(0.5f, 0.2f, 0.0f, 1.0f),    // start color
                        new Vector4(1.0f, 0.0f, 0.0f, 0.0f),    // end color
                        particleTextures[(int)type],          // texture
                        DrawMode.AdditiveAndGlow,      // draw mode
                        transform);              // transform
                    break;
                case ParticleSystemType.BlasterExplode:
                    ps = new ParticleSystem(
                        ParticleSystemType.BlasterExplode,
                        40,                      // num particles
                        2,                       // emission angle (0 for omni)
                        0.25f, 0.25f,            // particle time and total time
                        30.0f, 40.0f,            // min and max size
                        200.0f, 800.0f,          // min and max vel
                        new Vector4(1.0f, 1.0f, 1.0f, 1.5f),    // start color
                        new Vector4(1.0f, 1.0f, 1.0f, -0.2f),   // end color
                        particleTextures[(int)type],          // texture
                        DrawMode.AdditiveAndGlow,      // draw mode
                        transform);               // transform
                    break;
            }

            if (ps != null)
            {
                particle.Add(ps);
            }

            return ps;
        }


        /// <summary>
        /// Create a new animated sprite and add it to the animated sprite manager
        /// </summary>
        public AnimSprite AddAnimSprite(
            AnimSpriteType type,
            Vector3 position,
            float radius,
            float viewOffset,
            float frameRate,
            DrawMode mode,
            int player)
        {
            // create animated sprite
            AnimSprite a = new AnimSprite(type, position, radius, viewOffset,
                animatedSpriteTextures[(int)type], 256, 256,
                frameRate, mode, player);

            // add it to the animated sprite manager
            animatedSprite.Add(a);

            return a;
        }


        /// <summary>
        /// Add vibration to the gamepad of the given player
        /// </summary>
        public void SetVibration(int player, float duration)
        {
            vibrationTime[player] = duration;
        }


        internal int GetPlayerAtPosition(Vector3 position)
        {
            throw new NotImplementedException();
        }


        internal void AddDamageSplash(int player, float explosionDamage, Vector3 explosionPosition, float explosionDamageRadius)
        {
            throw new NotImplementedException();
        }


        internal GameObject GetPlayer(int playerHit)
        {
            throw new NotImplementedException();
        }


        internal void AddDamage(int player, int playerHit, float contactDamage, Vector3 vector3)
        {
            throw new NotImplementedException();
        }

    }

}
