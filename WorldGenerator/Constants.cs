namespace WorldGenerator
{
    public static class Constants
    {
        public static float EarthRadiusMm => 6.37f;
        public static float EarthSurfaceAreaMm2 => 4.0f * (float)Math.PI * EarthRadiusMm * EarthRadiusMm;
        public static float CrustThicknessMm => 0.005f;

        public static float EarthMassGT => 5.972e12f;
        public static float GravitationalConstantTNMm2PerGT2 => 6.674e4f;

        public static float MantleDensityGTPerMm3 => 3.3e3f;

        public static float CrustDensityGTPerMm3 => 3.3e3f;

        public static float CellVolumeMm3(int cellCount) => 
            EarthSurfaceAreaMm2 * CrustThicknessMm / cellCount;
    }
}
