using UnityEngine.XR;

namespace StupidTemplate.Classes
{
    public enum GunLineStyle
    {
        Straight,
        Curve,
        Wave,
    }

    public class GunSettings
    {
        public readonly XRNode hand = XRNode.RightHand;

        public GunLineStyle lineStyle = GunLineStyle.Straight;

        public float maxDistance = 512f;
        public float lineWidth   = 0.025f;
        public float pointerSize = 0.2f;

        public float curveAmount   = 0.15f;
        public float waveAmount    = 0.05f;
        public float waveFrequency = 3f;

        public int lineQuality = 24;

        public bool showPointer = true;

        public ExtGradient lineColor;
        public ExtGradient pointerIdleColor;
        public ExtGradient pointerActiveColor;
    }
}