namespace OpenLobby.Utility.Utils;

/// <summary>
/// Wraps the serialization and deserialization of a byte into an arr
/// </summary>
public class ByteMember
{
    public byte Value { get; init; }
    public ByteMember(in ArraySegment<byte> body, int index, byte value) => Value = body[index] = value;
    public ByteMember(in ArraySegment<byte> body, int index) => Value = body[index];
}