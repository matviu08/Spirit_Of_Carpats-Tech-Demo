using System.Numerics;
using Raylib_cs;
using static Raylib_cs.Raylib;

namespace Spirit_Of_Carpats_Remake.Models
{
    public enum TileType
    {
        Air      = 0,
        Solid    = 1,
        Platform = 2,
        Slope45L = 3,
        Slope45R = 4,
    }

    public struct CollisionResult
    {
        public bool  HitGround;
        public bool  HitCeiling;
        public bool  HitWallLeft;
        public bool  HitWallRight;
        public bool  OnPlatform;
        public float GroundY;
    }

    public class PhysicsBody
    {
        public Vector2 Position;
        public Vector2 Velocity;
        public float   Width;
        public float   Height;

        public bool IsGrounded      { get; internal set; }
        public bool WasGrounded     { get; internal set; }
        public bool HitCeiling      { get; internal set; }
        public bool HitWallLeft     { get; internal set; }
        public bool HitWallRight    { get; internal set; }
        public bool OnPlatform      { get; internal set; }

        public float GravityScale   = 1f;
        public float MaxFallSpeed   = 900f;
        public bool  IgnorePlatforms = false;

        public Rectangle AABB => new Rectangle(
            Position.X - Width  / 2f,
            Position.Y - Height,
            Width,
            Height
        );

        public PhysicsBody(Vector2 startPos, float width, float height)
        {
            Position = startPos;
            Width    = width;
            Height   = height;
        }
    }

    public class PlayerPhysicsController
    {
        public PhysicsBody Body { get; }

        public float MoveSpeed       = 290f;
        public float JumpForce       = -460f;
        public float Gravity         = 820f;
        public float GroundFriction  = 18f;
        public float AirFriction     = 5f;
        public float Acceleration    = 22f;

        public float CoyoteTime      = 0.12f;
        private float _coyoteTimer   = 0f;

        public float JumpBuffer      = 0.14f;
        private float _jumpBufferTimer = 0f;

        public float JumpCutMultiplier = 0.45f;
        private bool _jumpHeld = false;

        public PlayerPhysicsController(Vector2 startPos, float width = 28f, float height = 62f)
        {
            Body = new PhysicsBody(startPos, width, height);
        }

        public void Update(float dt, int moveInput, bool jumpPressed, bool jumpHeld,
                           PhysicsSystem physics, Rectangle[] solids, Rectangle[] platforms)
        {
            Body.WasGrounded = Body.IsGrounded;

            if (Body.WasGrounded && !Body.IsGrounded)
                _coyoteTimer = CoyoteTime;
            else if (Body.IsGrounded)
                _coyoteTimer = CoyoteTime;
            else
                _coyoteTimer -= dt;

            bool canJump = _coyoteTimer > 0f;

            if (jumpPressed)
                _jumpBufferTimer = JumpBuffer;
            else
                _jumpBufferTimer -= dt;

            if (_jumpBufferTimer > 0f && canJump)
            {
                Body.Velocity.Y  = JumpForce;
                _jumpBufferTimer = 0f;
                _coyoteTimer     = 0f;
                _jumpHeld        = true;
            }

            if (!jumpHeld && _jumpHeld && Body.Velocity.Y < 0f)
            {
                Body.Velocity.Y *= JumpCutMultiplier;
                _jumpHeld = false;
            }
            if (!jumpHeld) _jumpHeld = false;

            float targetVX = moveInput * MoveSpeed;
            float friction  = Body.IsGrounded ? GroundFriction : AirFriction;

            if (moveInput != 0)
                Body.Velocity.X = MathHelper.Lerp(Body.Velocity.X, targetVX, dt * Acceleration);
            else
            {
                Body.Velocity.X = MathHelper.Lerp(Body.Velocity.X, 0f, dt * friction);
                if (MathF.Abs(Body.Velocity.X) < 0.5f) Body.Velocity.X = 0f;
            }

            float gScale = (Body.Velocity.Y < 0f) ? 1f : 1.35f;
            Body.Velocity.Y += Gravity * gScale * Body.GravityScale * dt;
            Body.Velocity.Y  = MathF.Min(Body.Velocity.Y, Body.MaxFallSpeed);

            physics.MoveAndCollide(Body, dt, solids, platforms);
        }
    }

    public class PhysicsSystem
    {
        private const float Skin = 0.5f;

        public void MoveAndCollide(
            PhysicsBody   body,
            float         dt,
            Rectangle[]   solids,
            Rectangle[]   platforms)
        {
            body.HitCeiling   = false;
            body.HitWallLeft  = false;
            body.HitWallRight = false;
            body.IsGrounded   = false;
            body.OnPlatform   = false;

            Vector2 delta = body.Velocity * dt;

            body.Position.X += delta.X;
            ResolveX(body, solids);

            body.Position.Y += delta.Y;
            ResolveY(body, solids);

            if (!body.IgnorePlatforms && body.Velocity.Y >= 0f)
                ResolvePlatforms(body, platforms, delta.Y);
        }

        private void ResolveX(PhysicsBody body, Rectangle[] solids)
        {
            Rectangle aabb = body.AABB;

            foreach (var solid in solids)
            {
                if (!CheckCollisionRecs(aabb, solid)) continue;

                float overlapRight = (aabb.X + aabb.Width) - solid.X;
                float overlapLeft  = (solid.X + solid.Width) - aabb.X;

                if (overlapRight < overlapLeft)
                {
                    body.Position.X  -= overlapRight + Skin;
                    body.Velocity.X   = 0f;
                    body.HitWallRight = true;
                }
                else
                {
                    body.Position.X += overlapLeft + Skin;
                    body.Velocity.X  = 0f;
                    body.HitWallLeft = true;
                }

                aabb = body.AABB;
            }
        }

        private void ResolveY(PhysicsBody body, Rectangle[] solids)
        {
            Rectangle aabb = body.AABB;

            foreach (var solid in solids)
            {
                if (!CheckCollisionRecs(aabb, solid)) continue;

                float overlapBottom = (aabb.Y + aabb.Height) - solid.Y;
                float overlapTop    = (solid.Y + solid.Height) - aabb.Y;

                if (overlapBottom < overlapTop)
                {
                    // FIX: було "overlapBottom - Skin" — при малому overlap давало від'ємне зміщення
                    body.Position.Y -= overlapBottom;
                    body.Position.Y -= Skin;
                    body.Velocity.Y  = 0f;
                    body.IsGrounded  = true;
                }
                else
                {
                    body.Position.Y += overlapTop + Skin;
                    if (body.Velocity.Y < 0f) body.Velocity.Y = 0f;
                    body.HitCeiling = true;
                }

                aabb = body.AABB;
            }
        }

        private void ResolvePlatforms(PhysicsBody body, Rectangle[] platforms, float deltaY)
        {
            Rectangle aabb = body.AABB;
            float prevFeetY = body.Position.Y - deltaY;

            foreach (var plat in platforms)
            {
                if (!CheckCollisionRecs(aabb, plat)) continue;

                float feetY  = body.Position.Y;
                float platTop = plat.Y;

                if (prevFeetY <= platTop + 2f && feetY >= platTop)
                {
                    float overlap = feetY - platTop;
                    body.Position.Y -= overlap;
                    body.Velocity.Y  = 0f;
                    body.IsGrounded  = true;
                    body.OnPlatform  = true;
                }
            }
        }

        public static Vector2 ApplyGravity(Vector2 velocity, float gravity, float deltaTime)
        {
            velocity.Y += gravity * deltaTime;
            return velocity;
        }

        public static Vector2 ApplyFriction(Vector2 velocity, float friction, float deltaTime)
        {
            velocity.X *= (1 - friction * deltaTime);
            if (MathF.Abs(velocity.X) < 0.01f) velocity.X = 0;
            return velocity;
        }

        public static Vector2 ApplyImpulse(Vector2 velocity, Vector2 impulse, float mass)
        {
            velocity += impulse / mass;
            return velocity;
        }

        public static class Light2D
        {
            public static float CalculateLight(
                Vector2 point,
                Vector2 lightPos,
                float brightness  = 1f,
                float kc          = 1f,
                float kl          = 0f,
                float kq          = 0.1f,
                float ambient     = 0.2f,
                Vector2? lightDir = null,
                float cutoff      = -1f)
            {
                Vector2 dir       = point - lightPos;
                float distSqr     = dir.X * dir.X + dir.Y * dir.Y;
                float distance    = MathF.Sqrt(distSqr);
                if (distance > 0) dir /= distance;

                float attenuation = 1.0f / (kc + kl * distance + kq * distSqr);
                float intensity   = brightness * attenuation;

                if (lightDir.HasValue && cutoff >= 0f)
                {
                    Vector2 L   = Vector2.Normalize(lightDir.Value);
                    float   dot = Vector2.Dot(L, dir);
                    if (dot < cutoff) intensity = 0f;
                    else intensity *= dot;
                }

                intensity = MathF.Max(intensity, ambient);
                return MathF.Min(intensity, 1f);
            }
        }
    }
}
