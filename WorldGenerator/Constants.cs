namespace WorldGenerator
{
    public static class Constants
    {
        public static float EarthRadiusMm => 6.37f;
        public static float EarthSurfaceAreaMm2 => 4.0f * (float)Math.PI * EarthRadiusMm * EarthRadiusMm;
        public static float CrustThicknessMm => 0.005f;

        public static float OceanCrustDensityGTPerKm3 => 3.0f;

        public static float MantleDensityGTPerKm3 => 3.6f;
        public static int OceanLithosphereMimimumLifespanMY => 200;

        public static int OceanLithosphereMimimumLifespanKY =>
            OceanLithosphereMimimumLifespanMY * 1000;
        public static float LithospereCriticalTemperatureC => 130;

        public static float AesthenosphereTemperatureC => 1300;
        public static float SurfaceTemperatureC => 0;

        public static float PlumeRadiusKm => 300;

        public static float CellVolumeMm3(int cellCount) => 
            EarthSurfaceAreaMm2 * CrustThicknessMm / cellCount;
    }
}
