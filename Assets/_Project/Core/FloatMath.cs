using System;

public static class FloatMath
{
    public static float Clamp(float value, float minimum, float maximum)
    {
        if (minimum > maximum)
            throw new ArgumentOutOfRangeException(nameof(minimum));

        if (value < minimum)
            return minimum;

        return value > maximum ? maximum : value;
    }

    public static float Clamp01(float value)
    {
        return Clamp(value, 0f, 1f);
    }

    public static float Lerp(float from, float to, float time)
    {
        return from + (to - from) * Clamp01(time);
    }

    public static float InverseLerp(float from, float to, float value)
    {
        return from == to
            ? 0f
            : Clamp01((value - from) / (to - from));
    }
}
