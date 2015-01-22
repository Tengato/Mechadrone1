using Microsoft.Xna.Framework;
using Manifracture;
namespace Mechadrone1
{
    class TPCameraInputProcessor : InputProcessor
    {
        private MmoCameraDesc mCameraDesc;

        public TPCameraInputProcessor(PlayerIndex inputIndex, MmoCameraDesc cameraDesc)
            : base(inputIndex)
        {
            mCameraDesc = cameraDesc;
        }

        public override void HandleInput(GameTime gameTime, InputManager input)
        {
            float dTimeMs = (float)(gameTime.ElapsedGameTime.TotalMilliseconds);

            // Camera yaw
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraYawDecrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraYawDecrease])
                {
                    mCameraDesc.Yaw += (input.IsBinaryControlDown(control, InputIndex) ? -INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraYawIncrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraYawIncrease])
                {
                    mCameraDesc.Yaw += (input.IsBinaryControlDown(control, InputIndex) ? INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.CameraYawRate))
            {
                mCameraDesc.Yaw -= input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.CameraYawRate],
                    InputIndex) * INPUT_PAD_LOOK_FACTOR * dTimeMs;
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.CameraYawDelta))
            {
                mCameraDesc.Yaw += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.CameraYawDelta],
                    InputIndex) * INPUT_MOUSE_LOOK_FACTOR;
            }

            mCameraDesc.Yaw = mCameraDesc.Yaw % MathHelper.TwoPi;

            // Camera pitch
            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraPitchDecrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraPitchDecrease])
                {
                    mCameraDesc.Pitch += (input.IsBinaryControlDown(control, InputIndex) ? -INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.BinaryMap.ContainsKey(BinaryControlActions.CameraPitchIncrease))
            {
                foreach (BinaryControls control in ActiveInputMap.BinaryMap[BinaryControlActions.CameraPitchIncrease])
                {
                    mCameraDesc.Pitch += (input.IsBinaryControlDown(control, InputIndex) ? INPUT_PAD_LOOK_FACTOR * dTimeMs : 0.0f);
                }
            }

            if (ActiveInputMap.FullIntervalMap.ContainsKey(FullIntervalControlActions.CameraPitchRate))
            {
                mCameraDesc.Pitch += input.GetFullIntervalControlValue(ActiveInputMap.FullIntervalMap[FullIntervalControlActions.CameraPitchRate],
                    InputIndex) * INPUT_PAD_LOOK_FACTOR * dTimeMs;
            }

            if (ActiveInputMap.FullAxisMap.ContainsKey(FullAxisControlActions.CameraPitchDelta))
            {
                mCameraDesc.Pitch += input.GetFullAxisControlValue(ActiveInputMap.FullAxisMap[FullAxisControlActions.CameraPitchDelta],
                    InputIndex) * INPUT_MOUSE_LOOK_FACTOR;
            }

            mCameraDesc.Pitch = MathHelper.Clamp(mCameraDesc.Pitch, -2.0f * MathHelper.Pi / 5.0f, 2.0f * MathHelper.Pi / 5.0f);
        }
    }
}
