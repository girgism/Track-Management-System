using TrackManagement.Domain.Common;

namespace TrackManagement.Domain.Entities.DigitalServiceProviders;

public sealed class DSP
{
    public const int NameMaxLength = 100;

    private DSP() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    public static DSP Create(string? name) => new()
    {
        Id = Guid.CreateVersion7(),
        Name = Guard.NotEmpty(name, nameof(name), NameMaxLength)
    };
}
