using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using System.Xml;

class Player
{
    [SuppressMessage("ReSharper", "ConvertIfStatementToConditionalTernaryExpression")]
    static void Main(string[] args)
    {
        var engine = new Engine();

        while (true)
        {
            engine.Update();

            var velocityDirection = Vector2.Normalize(engine.Velocity);
            var velToG1 = Vector2.Dot(velocityDirection, Vector2.Normalize(engine.ToG1));
            var velToG2 = Vector2.Dot(velocityDirection, Vector2.Normalize(engine.ToG2));
            var velToCheckpoint = Vector2.Dot(velocityDirection, Vector2.Normalize(engine.ToCheckpoint));
            
            Vector2 target;
            Vector2 toTarget;
            
            if (Math.Abs(engine.Velocity.Length()) < float.Epsilon)
            {
                target = engine.CheckpointPosition;
                toTarget = engine.ToCheckpoint;
            }
            else
            {
                if (velToG1 > velToG2)
                {
                    if (velToCheckpoint > velToG1)
                    {
                        target = engine.CheckpointPosition;
                        toTarget = engine.ToCheckpoint;
                    }
                    else
                    {
                        target = engine.G2;
                        toTarget = engine.ToG2;
                    }
                }
                else
                {
                    if (velToCheckpoint > velToG2)
                    {
                        target = engine.CheckpointPosition;
                        toTarget = engine.ToCheckpoint;
                    }
                    else
                    {
                        target = engine.G1; 
                        toTarget = engine.ToG1;
                    }
                }
            }

            var thrust = 100;
            var boost = false;

            var dotProduct = Vector2.Dot(engine.Orientation, Vector2.Normalize(toTarget));

            var distanceToTarget = toTarget.Length();

            if (dotProduct < 0)
            {
                thrust = 0;
            }
            else if (dotProduct < 0.2)
            {
                thrust = 5;
            }
            else
            {
                if (distanceToTarget >= 6000 && dotProduct > 0.9)
                {
                    boost = true;
                }
                else if (distanceToTarget >= 3000)
                {
                    thrust = 100;
                }
                else
                {
                    if (engine.Velocity.Length() < 250)
                    {
                        thrust = 80;
                    }
                    else
                    {
                        thrust = 50;
                    }
                }
            }

            Console.Error.WriteLine(distanceToTarget);

            if (boost)
            {
                Engine.Output(target.X, target.Y);
            }
            else
            {
                Engine.Output(target.X, target.Y, thrust);
            }
        }
    }

    public class Engine
    {
        public Vector2 Position { get; private set; }
        public Vector2 Orientation { get; private set; }
        public Vector2 CheckpointPosition { get; private set; }
        public Vector2 EnemyPosition { get; private set; }

        private Vector2 LastPosition { get; set; }
        public Vector2 Velocity { get; private set; }

        public Vector2 ToCheckpoint { get; private set; }

        public Vector2 G1 { get; private set; }
        public Vector2 ToG1 { get; private set; }
        public Vector2 G2 { get; private set; }
        public Vector2 ToG2 { get; private set; }

        public Engine()
        {
            Position = Vector2.Zero;
            CheckpointPosition = Vector2.Zero;
            EnemyPosition = Vector2.Zero; 
            Velocity = Vector2.Zero;
        }

        [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
        public void Update()
        {
            var inputs = Console.ReadLine().Split(' ');
            var x = int.Parse(inputs[0]);
            var y = int.Parse(inputs[1]);

            LastPosition = Position;
            
            Position = new Vector2(x, y);
            var nextCheckpointX = int.Parse(inputs[2]);
            var nextCheckpointY = int.Parse(inputs[3]);

            if (LastPosition == Vector2.Zero)
            {
                LastPosition = Position;
            }

            Velocity = Position - LastPosition;

            CheckpointPosition = new Vector2(nextCheckpointX, nextCheckpointY);
            var nextCheckpointAngle = int.Parse(inputs[5]);

            ToCheckpoint = CheckpointPosition - Position;
            var radians = DegreeToRadian(nextCheckpointAngle);

            Orientation = Vector2.Normalize(Rotate(ToCheckpoint, -radians));

            inputs = Console.ReadLine().Split(' ');
            var opponentX = int.Parse(inputs[0]);
            var opponentY = int.Parse(inputs[1]);

            EnemyPosition = new Vector2(opponentX, opponentY);

            var toCheckpointDirection = Vector2.Normalize(ToCheckpoint);
            var g1 = Rotate(toCheckpointDirection, -Math.PI / 2) * 150;
            var g2 = g1 * -1;
            G1 = CheckpointPosition + g1;
            ToG1 = G1 - Position;

            G2 = CheckpointPosition + g2;
            ToG2 = G2 - Position;
        }

        public static void Output(float targetX, float targetY, OutputType outputType = OutputType.Boost)
        {
            var x = (int)Math.Round(targetX);
            var y = (int)Math.Round(targetY);

            switch (outputType)
            {
                case OutputType.Boost:
                    Console.WriteLine($"{x} {y} BOOST");
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(outputType), outputType, null);
            }
        }

        public static void Output(float targetX, float targetY, int thrust)
        {
            var x = (int) Math.Round(targetX);
            var y = (int) Math.Round(targetY);

            Console.WriteLine($"{x} {y} {thrust}");
        }

        private static double DegreeToRadian(double angle)
        {
            return Math.PI * angle / 180.0;
        }

        private static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }

        private static Vector2 Rotate(Vector2 vector, double angle)
        {
            var sin = (float)Math.Sin(angle);
            var cos = (float)Math.Cos(angle);
            return new Vector2(
                vector.X * cos - vector.Y * sin,
                vector.X * sin + vector.Y * cos
            );
        }
    }

    public enum OutputType
    {
        Boost
    }
}