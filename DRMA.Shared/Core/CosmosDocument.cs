using Newtonsoft.Json;

namespace DRMA.Shared.Core;

public abstract class CosmosDocument
{
    private readonly bool _fixedId;

    protected CosmosDocument()
    {
        _fixedId = false;
    }

    protected CosmosDocument(string id)
    {
        Id = id;

        _fixedId = true;
    }

    [JsonProperty(PropertyName = "id")] public string Id { get; set; } = string.Empty;

    [JsonProperty(PropertyName = "_ts")] public long Timestamp { get; set; }

    [JsonIgnore] public DateTime DateTime => DateTimeOffset.FromUnixTimeSeconds(Timestamp).UtcDateTime;

    public void SetIds(string id)
    {
        if (_fixedId) throw new InvalidOperationException();

        Id = id;
    }
}
