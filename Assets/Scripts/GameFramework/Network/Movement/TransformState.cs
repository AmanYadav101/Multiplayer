using System;
using Unity.Netcode;
using UnityEngine;

namespace GameFramework.Network.Movement
{
    public class TransformState : INetworkSerializable, IEquatable<TransformState>
    {
        public int Tick;
        public Vector3 Position;
        public Quaternion Rotation;
        public bool HasStartedMoving;
        

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            if (serializer.IsReader)
            {
                var reader = serializer.GetFastBufferReader();
                reader.ReadValueSafe(out Tick);
                reader.ReadValueSafe(out Position);
                reader.ReadValueSafe(out Rotation);
                reader.ReadValueSafe(out HasStartedMoving);
            }
            else
            {
                var writer = serializer.GetFastBufferWriter();

                // Ensure there's enough space in the buffer before writing
                if (!writer.TryBeginWrite(sizeof(int) + sizeof(float) * 3 + sizeof(float) * 4 + sizeof(bool)))
                {
                    Debug.LogError("Not enough space in buffer to serialize TransformState.");
                    return;
                }

                writer.WriteValueSafe(Tick);
                writer.WriteValueSafe(Position);
                writer.WriteValueSafe(Rotation);
                writer.WriteValueSafe(HasStartedMoving);
            }
        }
        public bool Equals(TransformState other)
        {
            if (other == null)
                return false;

            return Tick == other.Tick &&
                   Position == other.Position &&
                   Rotation == other.Rotation &&
                   HasStartedMoving == other.HasStartedMoving;
        }

        public override bool Equals(object obj)
        {
            if (obj is TransformState other)
                return Equals(other);

            return false;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 31 + Tick.GetHashCode();
            hash = hash * 31 + Position.GetHashCode();
            hash = hash * 31 + Rotation.GetHashCode();
            hash = hash * 31 + HasStartedMoving.GetHashCode();
            return hash;
        }
    }
}
