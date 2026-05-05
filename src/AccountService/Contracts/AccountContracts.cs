namespace AccountService.Contracts;

using System.Text.Json.Serialization;

public record FavoriteProductResponse(string ProductId);

public record FavoriteProductRequest(string ProductId);

public sealed record PushTokenUpsertRequest(
    [property: JsonPropertyName("fcm_token")] string FcmToken,
    [property: JsonPropertyName("platform")] string Platform,
    [property: JsonPropertyName("device_id")] string? DeviceId
);
