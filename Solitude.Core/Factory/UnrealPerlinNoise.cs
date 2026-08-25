using System;

/// <summary>
/// The fixed 2D improved-Perlin implementation used by Unreal's FMath.
/// Domain sampled this function directly, without a seed.
/// </summary>
public static class PerlinNoise
{
    private static readonly int[] Permutation =
    {
        63, 9, 212, 205, 31, 128, 72, 59, 137, 203, 195, 170, 181, 115, 165, 40,
        116, 139, 175, 225, 132, 99, 222, 2, 41, 15, 197, 93, 169, 90, 228, 43,
        221, 38, 206, 204, 73, 17, 97, 10, 96, 47, 32, 138, 136, 30, 219, 78,
        224, 13, 193, 88, 134, 211, 7, 112, 176, 19, 106, 83, 75, 217, 85, 0,
        98, 140, 229, 80, 118, 151, 117, 251, 103, 242, 81, 238, 172, 82, 110, 4,
        227, 77, 243, 46, 12, 189, 34, 188, 200, 161, 68, 76, 171, 194, 57, 48,
        247, 233, 51, 105, 5, 23, 42, 50, 216, 45, 239, 148, 249, 84, 70, 125,
        108, 241, 62, 66, 64, 240, 173, 185, 250, 49, 6, 37, 26, 21, 244, 60,
        223, 255, 16, 145, 27, 109, 58, 102, 142, 253, 120, 149, 160, 124, 156, 79,
        186, 135, 127, 14, 121, 22, 65, 54, 153, 91, 213, 174, 24, 252, 131,
        192, 190, 202, 208, 35, 94, 231, 56, 95, 183, 163, 111, 147, 25, 67, 36,
        92, 236, 71, 166, 1, 187, 100, 130, 143, 237, 178, 158, 104, 184, 159, 177,
        52, 214, 230, 119, 87, 114, 201, 179, 198, 3, 248, 182, 39, 11, 152, 196,
        113, 20, 232, 69, 141, 207, 234, 53, 86, 180, 226, 74, 150, 218, 29, 133,
        8, 44, 123, 28, 146, 89, 101, 154, 220, 126, 155, 122, 210, 168, 254, 162,
        129, 33, 18, 209, 61, 191, 199, 157, 245, 55, 164, 167, 215, 246, 144, 107,
        235
    };

    public static float Noise2D(float x, float y)
    {
        var floorX = MathF.Floor(x);
        var floorY = MathF.Floor(y);
        var latticeX = (int)floorX & 255;
        var latticeY = (int)floorY & 255;

        x -= floorX;
        y -= floorY;
        var xMinusOne = x - 1f;
        var yMinusOne = y - 1f;

        var aa = Perm(latticeX) + latticeY;
        var ab = aa + 1;
        var ba = Perm(latticeX + 1) + latticeY;
        var bb = ba + 1;
        var smoothX = SmoothCurve(x);
        var smoothY = SmoothCurve(y);

        return Lerp(
            Lerp(Gradient(Perm(aa), x, y), Gradient(Perm(ba), xMinusOne, y), smoothX),
            Lerp(
                Gradient(Perm(ab), x, yMinusOne),
                Gradient(Perm(bb), xMinusOne, yMinusOne),
                smoothX),
            smoothY);
    }

    private static int Perm(int index) => Permutation[index & 255];

    private static float Gradient(int hash, float x, float y) => (hash & 7) switch
    {
        0 => x,
        1 => x + y,
        2 => y,
        3 => -x + y,
        4 => -x,
        5 => -x - y,
        6 => -y,
        7 => x - y,
        _ => 0f
    };

    private static float SmoothCurve(float value) =>
        value * value * value * (value * (value * 6f - 15f) + 10f);

    private static float Lerp(float from, float to, float alpha) =>
        from + alpha * (to - from);
}
