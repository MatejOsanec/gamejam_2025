using UnityEngine;

// Similar to BGLib.DotnetExtensions RandomExtensions, but works with Unity types such as UnityEngine.Vector3
public static class UnitySpecificRandomExtensions {

    public static Vector3 InsideUnitSphere(this System.Random random) {

        float d, x, y, z;
        do {
            x = (float)random.NextDouble() * 2.0f - 1.0f;
            y = (float)random.NextDouble() * 2.0f - 1.0f;
            z = (float)random.NextDouble() * 2.0f - 1.0f;
            d = x*x + y*y + z*z;
        } while(d > 1.0f);
        return new Vector3(x, y, z);
    }

    public static Vector3 OnUnitSphere(this System.Random random) {

        Vector3 randomVector;
        do {
            randomVector = random.InsideUnitSphere();
        }
        while (randomVector == Vector3.zero);

        return randomVector.normalized;
    }
}
