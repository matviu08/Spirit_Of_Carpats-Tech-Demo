using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

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
            if(MathF.Abs(velocity.X) < 0.01f)
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

    }
}
