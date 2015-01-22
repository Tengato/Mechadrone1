using System;
using System.Collections.Generic;
using BEPUphysics;
using BEPUphysics.BroadPhaseEntries;
using BEPUphysics.Entities;
using Manifracture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SlagformCommon;
using BepuRay = BEPUutilities.Ray;
using System.Diagnostics;

namespace Mechadrone1
{
    // Human view for third-person action mode in Mechadrone1
    class ActionScreenHumanView : HumanView, ICameraProvider
    {
        public enum InputMode
        {
            Aloof,
            Aiming,
            COUNT,
        }

        private MmoCameraDesc mMmoCameraDesc;
        private StaticCamera mCamera;
        private bool mShadowViewMode;
        private float mBright;
        private float mContrast;
        private EffectParameter mBrightParam;
        private EffectParameter mContrastParam;
        private Entity mAvatarBepuEntity;
        private bool mCameraSmoothingEngaged;
        private Vector3 mAimingCameraOffset;
        private Dictionary<Type, InputProcessor>[] mInputProcs;
        private InputMode mInputMode;
        private CrosshairsWidget mCrosshairs;
        private InventoryPanel mInventoryPanel;


        protected override IEnumerable<KeyValuePair<Type, InputProcessor>> mInputProcessors
        {
            get { return mInputProcs[(int)mInputMode]; }
        }

        public ICamera Camera { get { return mCamera; } }

        public ActionScreenHumanView(PlayerInfo playerInfo, CharacterInfo selectedCharacter, ContentManager contentLoader)
            : base(playerInfo, selectedCharacter, contentLoader)
        {
            mMmoCameraDesc = new MmoCameraDesc();
            mCamera = new StaticCamera();
            mShadowViewMode = false;
            mBright = 0.1f;
            mContrast = 1.0f;
            mBrightParam = null;
            mContrastParam = null;
            mAvatarBepuEntity = null;
            mCameraSmoothingEngaged = false;
            mAimingCameraOffset = new Vector3(5.0f, 3.0f, 5.0f);
            mInputProcs = new Dictionary<Type, InputProcessor>[(int)(InputMode.COUNT)];
            mInputProcs[(int)(InputMode.Aloof)] = new Dictionary<Type, InputProcessor>();
            mInputProcs[(int)(InputMode.Aiming)] = new Dictionary<Type, InputProcessor>();
            mCrosshairs = null;
            mInventoryPanel = null;
        }

        public override void Load()
        {
            base.Load();

            mCrosshairs = new CrosshairsWidget(mContentLoader);
            //InventoryPanel.LoadContent(mContentLoader);
            mInventoryPanel = new InventoryPanel(AvatarDesc);

            if (DrawSegment.MainWindow.UIElements.Count == 0)
                DrawSegment.MainWindow.UIElements.Add(UIElementDepth.STANDARD_3D_PERSP, new Standard3dPerspective());

            DrawSegment.MainWindow.UIElements.Add(UIElementDepth.DEBUG_POSITION, new PositionWidget(ActorId));
            //Actor modelActor = GameResources.ActorManager.GetActorByName("Billboard");
            //BillboardRenderComponent rc = modelActor.GetComponent<BillboardRenderComponent>(ActorComponent.ComponentType.Render);
            //mBrightParam = rc.Effect.Parameters["gBright"];
            //mContrastParam = rc.Effect.Parameters["gContrast"];

            GameResources.ActorManager.PreAnimationUpdateStep += PreAnimationUpdateHandler;
        }

        public override void Unload()
        {
            mInventoryPanel.Release();
        }

        protected override void OnAssignInputMap()
        {
            for (int m = 0; m < (int)(InputMode.COUNT); ++m)
            {
                foreach (KeyValuePair<Type, InputProcessor> ip in mInputProcs[m])
                {
                    ip.Value.ActiveInputMap = mPlayerInputMap;
                }
            }
        }

        protected override void PrepareInputProcessors(InputManager input)
        {
            PlayerIndex inputIndex = GameResources.PlaySession.LocalPlayers[PlayerId];
            InputMode newMode = mInputMode;

            // TODO: P3: This needs a real state system to manage transitions:

            // Compute next mode:
            if (mInputMode == InputMode.Aiming &&
                !(input.CheckForBinaryInput(mPlayerInputMap, BinaryControlActions.Aim, inputIndex)))
            {
                newMode = InputMode.Aloof;
            }
            else if (mInputMode == InputMode.Aloof &&
                input.CheckForBinaryInput(mPlayerInputMap, BinaryControlActions.Aim, inputIndex))
            {
                newMode = InputMode.Aiming;
            }

            // Take some special actions if there is a mode change:
            if (newMode != mInputMode)
            {
                // Exiting mode tasks:
                switch (mInputMode)
                {
                    case InputMode.Aiming:
                        Actor avatar = GameResources.ActorManager.GetActorById(ActorId);
                        BipedControllerComponent bipedControl = avatar.GetComponent<BipedControllerComponent>(
                            ActorComponent.ComponentType.Control);
                        bipedControl.Controller.ViewDirection = bipedControl.Controller.HorizontalViewDirection;
                        mMmoCameraDesc.Pitch = -MathHelper.Pi / 12.0f;
                        mMmoCameraDesc.Yaw = (float)(Math.Atan2(-bipedControl.Controller.HorizontalViewDirection.X,
                            -bipedControl.Controller.HorizontalViewDirection.Z));

                        DrawSegment.MainWindow.UIElements.Remove(UIElementDepth.CROSSHAIRS);
                        break;
                    default:
                        break;
                }

                // Entering mode tasks:
                switch (newMode)
                {
                    case InputMode.Aiming:
                        DrawSegment.MainWindow.UIElements.Add(UIElementDepth.CROSSHAIRS, mCrosshairs);
                        break;
                    default:
                        break;
                }
            }

            mInputMode = newMode;

            // Handle some misc input functions:

            // Inventory window:
            if (input.CheckForNewBinaryInput(mPlayerInputMap, BinaryControlActions.OpenInventory, inputIndex))
            {
                DrawSegment.AddWindow(mInventoryPanel, inputIndex);
                mInventoryPanel.RefreshItems();
            }

            // TODO: P2: Remove nonessential/debug functionality:

            PlayerIndex dummyPlayerIndex;
            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.V, inputIndex, out dummyPlayerIndex))
            {
                mShadowViewMode = !mShadowViewMode;

                DrawSegment.MainWindow.UIElements.Clear();

                if (mShadowViewMode)
                {
                    DrawSegment.MainWindow.UIElements.Add(UIElementDepth.DEBUG_SHADOW_MAP, new ShadowMapVisual());
                }
                else
                {
                    DrawSegment.MainWindow.UIElements.Add(UIElementDepth.STANDARD_3D_PERSP, new Standard3dPerspective());
                }
            }

            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.P, inputIndex, out dummyPlayerIndex))
            {
                Debugger.Break();
            }

            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.U, inputIndex, out dummyPlayerIndex))
            {
                mBright += 0.01f;
            }

            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.J, inputIndex, out dummyPlayerIndex))
            {
                mBright -= 0.01f;
            }

            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.I, inputIndex, out dummyPlayerIndex))
            {
                mContrast += 0.1f;
            }

            if (input.IsNewKeyPress(Microsoft.Xna.Framework.Input.Keys.K, inputIndex, out dummyPlayerIndex))
            {
                mContrast -= 0.1f;
            }

            //mBrightParam.SetValue(mBright);
            //mContrastParam.SetValue(mContrast);
        }

        public override void AssignAvatar(int actorId)
        {
            base.AssignAvatar(actorId);

            PlayerIndex inputIndex = GameResources.PlaySession.LocalPlayers[PlayerId];

            Actor avatar = GameResources.ActorManager.GetActorById(ActorId);
            BipedControllerComponent bipedControl = avatar.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);
            bipedControl.AimCheck = delegate() { return mInputMode == InputMode.Aiming; };

            // Begin InputProcessors configuration:

            // Biped, aloof mode:
            TPBipedFixedControlsInputProcessor bipedProc;
            if (mInputProcs[(int)(InputMode.Aloof)].ContainsKey(typeof(TPBipedFixedControlsInputProcessor)))
            {
                bipedProc = (TPBipedFixedControlsInputProcessor)(mInputProcs[(int)(InputMode.Aloof)][typeof(TPBipedFixedControlsInputProcessor)]);
            }
            else
            {
                bipedProc = new TPBipedFixedControlsInputProcessor(inputIndex, mCamera);
                mInputProcs[(int)(InputMode.Aloof)].Add(typeof(TPBipedFixedControlsInputProcessor), bipedProc);
            }
            bipedProc.ActiveInputMap = mPlayerInputMap;
            bipedProc.SetControllerComponent(bipedControl);

            // Biped, aiming mode:
            TPAimingBipedMovementInputProcessor aimingBipedProc;
            if (mInputProcs[(int)(InputMode.Aiming)].ContainsKey(typeof(TPAimingBipedMovementInputProcessor)))
            {
                aimingBipedProc = (TPAimingBipedMovementInputProcessor)(mInputProcs[(int)(InputMode.Aiming)][typeof(TPAimingBipedMovementInputProcessor)]);
            }
            else
            {
                aimingBipedProc = new TPAimingBipedMovementInputProcessor(inputIndex);
                mInputProcs[(int)(InputMode.Aiming)].Add(typeof(TPAimingBipedMovementInputProcessor), aimingBipedProc);
            }
            aimingBipedProc.ActiveInputMap = mPlayerInputMap;
            aimingBipedProc.SetControllerComponent(bipedControl);

            // Skill palette, shared between aiming and aloof modes:
            TPBipedPaletteInputProcessor paletteProc;
            if (mInputProcs[(int)(InputMode.Aloof)].ContainsKey(typeof(TPBipedPaletteInputProcessor)))
            {
                paletteProc = (TPBipedPaletteInputProcessor)(mInputProcs[(int)(InputMode.Aloof)][typeof(TPBipedPaletteInputProcessor)]);
            }
            else
            {
                paletteProc = new TPBipedPaletteInputProcessor(inputIndex);
                mInputProcs[(int)(InputMode.Aloof)].Add(typeof(TPBipedPaletteInputProcessor), paletteProc);
            }
            if (!(mInputProcs[(int)(InputMode.Aiming)].ContainsKey(typeof(TPBipedPaletteInputProcessor))))
            {
                mInputProcs[(int)(InputMode.Aiming)].Add(typeof(TPBipedPaletteInputProcessor), paletteProc);
            }
            paletteProc.ActiveInputMap = mPlayerInputMap;
            paletteProc.SetControllerComponent(bipedControl);
            BipedSkillPalette skillPalette = avatar.GetBehaviorThatImplementsType<BipedSkillPalette>();
            paletteProc.SetSkillPalette(skillPalette);

            // Camera, aloof mode:
            TPCameraInputProcessor camera;
            if (mInputProcs[(int)(InputMode.Aloof)].ContainsKey(typeof(TPCameraInputProcessor)))
            {
                camera = (TPCameraInputProcessor)(mInputProcs[(int)(InputMode.Aloof)][typeof(TPCameraInputProcessor)]);
            }
            else
            {
                camera = new TPCameraInputProcessor(inputIndex, mMmoCameraDesc);
                mInputProcs[(int)(InputMode.Aloof)].Add(typeof(TPCameraInputProcessor), camera);
            }
            camera.ActiveInputMap = mPlayerInputMap;
            // Done with InputProcessor configuration.

            // Reset camera placement:
            mMmoCameraDesc.Distance = 38.0f;
            mMmoCameraDesc.Pitch = -MathHelper.Pi / 6.0f;   // Positive pitch will move the camera -Y since it's on the +Z side
            mMmoCameraDesc.Yaw = 0.0f;

            DynamicCollisionComponent dcc = avatar.GetComponent<DynamicCollisionComponent>(ActorComponent.ComponentType.Physics);
            mAvatarBepuEntity = dcc.Entity;
        }

        private void PreAnimationUpdateHandler(object sender, UpdateStepEventArgs e)
        {
            float elapsedTime = (float)(e.GameTime.ElapsedGameTime.TotalSeconds);

            Actor avatar = GameResources.ActorManager.GetActorById(ActorId);
            BipedControllerComponent bipedControl = avatar.GetComponent<BipedControllerComponent>(ActorComponent.ComponentType.Control);

            // Update the camera.
            Vector3 desiredCameraPosition;
            if (mInputMode == InputMode.Aloof)
            {
                Matrix cameraRotation = Matrix.CreateFromYawPitchRoll(mMmoCameraDesc.Yaw, mMmoCameraDesc.Pitch, 0.0f);
                BepuRay boomRay = new BepuRay(mAvatarBepuEntity.Position, BepuConverter.Convert(cameraRotation.Backward));
                RayCastResult result;

                GameResources.ActorManager.SimSpace.RayCast(boomRay, mMmoCameraDesc.Distance, CameraClipFilter, out result);

                desiredCameraPosition = result.HitObject != null ?
                    BepuConverter.Convert(BEPUutilities.Vector3.Lerp(result.HitData.Location, mAvatarBepuEntity.Position, 0.05f)) :
                    BepuConverter.Convert(mAvatarBepuEntity.Position) + mMmoCameraDesc.Distance * cameraRotation.Backward;
            }
            else if (mInputMode == InputMode.Aiming)
            {
                Matrix viewRotation = Matrix.CreateWorld(Vector3.Zero, BepuConverter.Convert(
                    bipedControl.Controller.ViewDirection), Vector3.Up);
                desiredCameraPosition = BepuConverter.Convert(mAvatarBepuEntity.Position) + Vector3.Transform(
                    mAimingCameraOffset, viewRotation);
            }
            else
            {
                desiredCameraPosition = mCamera.Transform.Translation;
            }

            Vector3 newCameraPosition = desiredCameraPosition;

            Vector3 desiredCameraDirection;
            if (mInputMode == InputMode.Aloof)
            {
                desiredCameraDirection = BepuConverter.Convert(mAvatarBepuEntity.Position) - newCameraPosition;
            }
            else if (mInputMode == InputMode.Aiming)
            {
                desiredCameraDirection = BepuConverter.Convert(bipedControl.Controller.ViewDirection);
            }
            else
            {
                desiredCameraDirection = mCamera.Transform.Forward;
            }
            desiredCameraDirection.Normalize();

            Vector3 newCameraDirection = desiredCameraDirection;

            if (mCameraSmoothingEngaged)
            {
                Vector3 positionDelta = desiredCameraPosition - mCamera.Transform.Translation;
                Quaternion directionDelta = SpaceUtils.GetSweptQuaternion(mCamera.Transform.Forward, desiredCameraDirection);

                const float POSITION_DELTA_THRESHHOLD = 4.0f;
                const float DIRECTION_DELTA_THRESHHOLD = MathHelper.Pi / 16.0f;

                float positionDeltaLength = positionDelta.Length();
                float directionDeltaAngle = (float)(SpaceUtils.GetQuaternionAngle(directionDelta));

                float fractionComplete = Math.Min(POSITION_DELTA_THRESHHOLD / positionDeltaLength,
                    DIRECTION_DELTA_THRESHHOLD / directionDeltaAngle);

                if (fractionComplete < 1.0f)
                {
                    newCameraPosition = Vector3.Lerp(mCamera.Transform.Translation, desiredCameraPosition, fractionComplete);
                    Quaternion smoothedCamRotation = Quaternion.Slerp(Quaternion.Identity, directionDelta, fractionComplete);
                    newCameraDirection = Vector3.Transform(mCamera.Transform.Forward, smoothedCamRotation);
                }
            }
            else
            {
                mCameraSmoothingEngaged = true;
            }

            mCamera.Transform = Matrix.CreateWorld(newCameraPosition, newCameraDirection, Vector3.Up);
        }

        private static bool CameraClipFilter(BroadPhaseEntry test)
        {
            return (GameResources.ActorManager.CameraClippingSimObjects.Contains(test.GetHashCode()));
        }


    }
}
