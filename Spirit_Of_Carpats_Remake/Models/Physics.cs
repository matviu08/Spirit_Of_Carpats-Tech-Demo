using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.NetworkInformation;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static Spirit_Of_Carpats_Remake.Models.Physics;

namespace Spirit_Of_Carpats_Remake.Models
{
    public class Physics
    {

        public static Vector2 ApplyGravity(Vector2 velocity, float gravity, float deltaTime)
        {
            velocity.Y += gravity * deltaTime;
            return velocity;
        }

        //силла тертя закона Ньютона, F = μN, де F - сила тертя, μ - коефіцієнт тертя, N - нормальна сила (вага об'єкта). (Інерція)
        public static Vector2 ApplyFriction(Vector2 velocity, float friction, float deltaTime)
        {
            velocity.X *= (1 - friction * deltaTime);
            if (MathF.Abs(velocity.X) < 0.01f)
            {
                velocity.X = 0;
            }
            return velocity;
        }

        //Застосування імпульсу до об'єкта, що змінює його швидкість відповідно до формули: Δv = F * Δt / m, де Δv - зміна швидкості, F - сила імпульсу, Δt - час дії імпульсу, m - маса об'єкта.
        //Поштовх
        public static Vector2 ApplyImpulse(Vector2 velocity, Vector2 impulse, float mass)
        {
            velocity += impulse / mass;
            return velocity;
        }

        //Light physics

        public static class Light2D
        {
            public static float CalculateLight(
                Vector2 point,            // точка (піксель)
                Vector2 lightPos,         // позиція джерела світла
                float brightness = 1f,    // яскравість
                float kc = 1f,            // constant (постійна складова)
                float kl = 0f,            // linear (лінійне затухання)
                float kq = 0.1f,          // quadratic (квадратичне затухання)
                float ambient = 0.2f,     // фонове світло (мінімум)
                Vector2? lightDir = null, // напрям світла (наприклад, ліхтарик)
                float cutoff = -1f        // кут відсікання (0..1), -1 = вимкнено
            )
            {
                // напрям від світла до точки
                Vector2 dir = point - lightPos;

                float distanceSqr = dir.X * dir.X + dir.Y * dir.Y;
                float distance = MathF.Sqrt(distanceSqr);

                // нормалізація
                if (distance > 0)
                    dir /= distance;

                // затухання
                float attenuation = 1.0f / (kc + kl * distance + kq * distanceSqr);

                float intensity = brightness * attenuation;

                // направлене світло
                if (lightDir.HasValue && cutoff >= 0f)
                {
                    Vector2 L = Vector2.Normalize(lightDir.Value);
                    float dot = Vector2.Dot(L, dir);

                    if (dot < cutoff)
                        intensity = 0f;
                    else
                        intensity *= dot;
                }

                // мінімальне (ambient) світло
                intensity = MathF.Max(intensity, ambient);

                // обмеження до [0;1]
                return MathF.Min(intensity, 1f);
            }
        }

        //using example

        // звичайне освітлення
        //float light = Light2D.CalculateLight(
        //        pixelPos,
        //        lightPos,
        //        brightness: 1f,
        //        kq: 0.05f
        //);

        //ліхтар
        //float light = Light2D.CalculateLight(
        //        pixelPos,
        //        playerPos,
        //        brightness: 1.2f,
        //        kq: 0.1f,
        //        lightDir: playerDirection,
        //        cutoff: 0.7f
        //);


        // для кольорів
        //Color ApplyLight(Color color, float light)
        //{
        //    return new Color(
        //        (byte)(color.R * light),
        //        (byte)(color.G * light),
        //        (byte)(color.B * light)
        //    );
        //}
    }
}
