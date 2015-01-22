namespace Manifracture
{
    // Actions that can be defined as on or off.
    public enum BinaryControlActions
    {
        MoveForward,
        MoveBackward,
        StrafeLeft,
        StrafeRight,
        TurnLeft,
        TurnRight,
        CameraYawDecrease,
        CameraYawIncrease,
        CameraPitchDecrease,
        CameraPitchIncrease,
        CameraDistanceDecrease,
        CameraDistanceIncrease,
        CameraReset,
        Jump,
        Crouch,
        Skill1,
        Skill2,
        Skill3,
        Skill4,
        Skill5,
        Skill6,
        VehicleRollRight,
        VehicleRollLeft,
        VehiclePitchUp,
        VehiclePitchDown,
        LevitateUp,
        LevitateDown,
        Aim,
        OpenInventory,
    }

    // Actions that can be defined in terms of a value in the range of 0 to 1.
    public enum HalfIntervalControlActions
    {
        BrakePower,
        Throttle,
    }

    // Actions that can be defined in terms of a value in the range of -1 to 1.
    public enum FullIntervalControlActions
    {
        MoveForwardBackwardRate,
        StrafeLeftRightRate,
        TurnLeftRightRate,
        CameraYawRate,
        CameraPitchRate,
        MoveCursorUpDownRate,
        MoveCursorLeftRightRate,
        VehiclePitchRate,
        VehicleRollRate,
        LevitateUpDownRate,
        MoveLeftRightRate,
        MoveDownUpRate,
    }

    // Actions that can be defined in terms of an arbitrary value.
    public enum FullAxisControlActions
    {
        MoveForwardBackwardDelta,
        StrafeLeftRightDelta,
        TurnLeftRightDelta,
        CameraPitchDelta,
        MoveCursorUpDownDelta,
        MoveCursorLeftRightDelta,
        CameraDistanceDelta,
        VehiclePitchDelta,
        VehicleRollDelta,
        CameraYawDelta,
    }
}
