using System;

namespace Project.Core.Config
{
    [Serializable]
    public class CameraConfig
    {
        public float MouseSensitivity = 2f;
        public float MinLookAngle = -89f;
        public float MaxLookAngle = 89f;
        public float StandingCameraHeight = 0.6f;
        public float CrouchingCameraHeight = 0.1f;
    }
}

