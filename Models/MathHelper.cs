namespace Spirit_Of_Carpats_Remake.Models
{
    internal static class MathHelper
    {
        public static float Lerp(float a, float b, float t)
            => a + (b - a) * Math.Clamp(t, 0f, 1f);
    }
}
