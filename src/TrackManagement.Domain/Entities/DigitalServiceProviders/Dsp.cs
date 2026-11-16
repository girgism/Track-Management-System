using TrackManagement.Domain.Common;

namespace TrackManagement.Domain.Entities.DigitalServiceProviders;

public sealed class Dsp
{
    public const int NameMaxLength = 100;

    private Dsp() { }

    public Guid Id { get; private set; }
    public string Name { get; private set; } = null!;

    public static Dsp Create(string? name) => new()
    {
        Id = Guid.CreateVersion7(),
        Name = Guard.NotEmpty(name, nameof(name), NameMaxLength)
    };
}
