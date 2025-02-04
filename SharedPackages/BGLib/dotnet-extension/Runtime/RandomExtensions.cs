public static class RandomExtensions {

    public static float GetRange(this System.Random random, float min, float max) {

        return (float)(random.NextDouble() * (max - min) + min);
    }

    public static bool GetBool(this System.Random random) {

        return random.Next(2) == 0;
    }
}
