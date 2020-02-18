using Kopernicus;


namespace SigmaHeatShifterPlugin
{
    internal static class Extensions
    {
        internal static float maxTempAngleOffset(this CelestialBody body)
        {
            if (body?.Has("maxTempAngleOffset") == true)
                return body.Get<float>("maxTempAngleOffset");

            return 45f;
        }
    }
}
