using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;

namespace TH
{
    public struct THMath
    {
        public static float square(float x)
        {
            return x * x;
        }

        public static float det(float2 vector1, float2 vector2)
        {
            return vector1.x * vector2.y - vector1.y * vector2.x;
        }

        public static float leftOf(float2 a, float2 b, float2 c)
        {
            return det(a - c, b - a);
        }

        public static int GetHighBit(uint value)
        {
            return 32 - math.lzcnt(value);
        }

        public static int GetLowBit(uint value)
        {
            return GetHighBit((value - 1) ^ value);
        }

        public static float GetNoiseValue(
            float2 xy,
            int octaves,
            float lacunarity,
            float persistence,
            float amplitude,
            float frequency)
        {
            float value = 0.0f;
            for (int i = 0; i < octaves; ++i)
            {
                value += amplitude * noise.snoise(frequency * xy);
                frequency *= lacunarity;
                amplitude *= persistence;
            }

            return value;
        }
    }
}
