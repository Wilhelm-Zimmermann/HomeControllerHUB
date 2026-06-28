using System.Text.Json;
using HomeControllerHUB.Application.Sensors.Commands.UpdateSensor;
using HomeControllerHUB.Infra.Services;
using FluentAssertions;

namespace HomeControllerHUB.Application.Tests.AuditLogs.Services;

public class AuditMetadataSanitizerTests
{
    [Theory]
    [InlineData("password", "plain-password")]
    [InlineData("ConfirmPassword", "plain-confirm-password")]
    [InlineData("TOKEN", "plain-token")]
    [InlineData("accessToken", "plain-access-token")]
    [InlineData("refreshToken", "plain-refresh-token")]
    [InlineData("apiKey", "plain-api-key")]
    [InlineData("Authorization", "plain-authorization")]
    [InlineData("SECRET", "plain-secret")]
    public void Sanitize_RemovesSensitiveFields(string fieldName, string fieldValue)
    {
        var sanitizer = new AuditMetadataSanitizer();
        var metadata = new Dictionary<string, object?>
        {
            [fieldName] = fieldValue,
            ["safeField"] = "visible"
        };

        var sanitized = sanitizer.Sanitize(metadata);
        var json = JsonSerializer.Serialize(sanitized);

        json.Should().NotContain(fieldName);
        json.Should().NotContain(fieldValue);
        json.Should().Contain("safeField");
        json.Should().Contain("visible");
    }

    [Fact]
    public void Sanitize_RemovesSensorApiKeyFromMetadataJson()
    {
        var sanitizer = new AuditMetadataSanitizer();
        var metadata = new
        {
            request = new UpdateSensorCommand
            {
                Id = Guid.NewGuid(),
                Name = "Temperature",
                ApiKey = "sensor-api-key"
            }
        };

        var sanitized = sanitizer.Sanitize(metadata);
        var json = JsonSerializer.Serialize(sanitized, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

        json.Should().NotContain("apiKey");
        json.Should().NotContain("sensor-api-key");
        json.Should().Contain("Temperature");
    }
}
