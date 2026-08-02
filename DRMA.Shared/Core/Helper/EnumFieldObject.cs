namespace DRMA.Shared.Core.Helper
{
    public sealed class EnumFieldObject<T>(string name, T value) where T : Enum
    {
        public T Value { get; set; } = value;
        public string Name { get; set; } = name;
        public string? Group { get; set; }
        public string? Placeholder { get; set; }
        public string? Description { get; set; }
    }
}