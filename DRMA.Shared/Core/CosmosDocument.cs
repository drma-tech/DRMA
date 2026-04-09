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

    [JsonProperty(PropertyName = "id")]
    public string Id { get; set; } = string.Empty;

    [System.Text.Json.Serialization.JsonPropertyName("_tsCreated")]
    [JsonProperty(PropertyName = "_tsCreated")]
    public long? TimestampCreated { get; set; } = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

    [JsonProperty(PropertyName = "_ts")]
    public long TimestampUpdated { get; set; }

    [JsonIgnore]
    public DateTime? DateTimeCreated => TimestampCreated.HasValue ? DateTimeOffset.FromUnixTimeSeconds(TimestampCreated.Value).UtcDateTime : null;

    [JsonIgnore]
    public DateTime DateTimeUpdated => DateTimeOffset.FromUnixTimeSeconds(TimestampUpdated).UtcDateTime;

    public void SetIds(string id)
    {
        if (_fixedId) throw new InvalidOperationException();

        Id = id;
    }
}
