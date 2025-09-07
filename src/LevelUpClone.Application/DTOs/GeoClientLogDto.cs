namespace LevelUpClone.Application.Abstractions.Geo;

public sealed class GeoClientLogDto
{
    public int? UserAccountId { get; init; }
    public string? RemoteIp { get; init; }
    public string? ForwardedFor { get; init; }
    public string? UserAgent { get; init; }
    public double? Latitude { get; init; }
    public double? Longitude { get; init; }
    public double? AccuracyMeters { get; init; }
    public double? AltitudeMeters { get; init; }
    public double? AltitudeAccuracyMeters { get; init; }
    public double? SpeedMetersPerSecond { get; init; }
    public double? HeadingDegrees { get; init; }
    public long? TimestampEpochMs { get; init; }
    public string? City { get; init; }
    public string? EnrichGranularity { get; init; }
    public string? EnrichCountry { get; init; }
    public string? EnrichUF { get; init; }
    public string? EnrichMunicipio { get; init; }
    public string? EnrichCodigoMunicipioIBGE { get; init; }
    public string? EnrichBairro { get; init; }
    public string? EnrichLogradouro { get; init; }
    public string? EnrichNumero { get; init; }
    public string? EnrichCEP { get; init; }
    public string? EnrichTimezoneId { get; init; }
    public double? EnrichConfidence { get; init; }
    public object? EnrichSourcesJson { get; init; }
    public string? EnrichAttribution { get; init; }
    public string? EnvBrowser { get; init; }
    public string? EnvBrowserVersion { get; init; }
    public string? EnvOperatingSystem { get; init; }
    public string? EnvOSVersion { get; init; }
    public string? EnvArchitecture { get; init; }
    public string? EnvDeviceType { get; init; }
    public string? EnvDeviceModel { get; init; }
    public int? EnvTouchPoints { get; init; }
    public bool? EnvIsBot { get; init; }
    public string? EnvBotName { get; init; }
    public string? EnvLanguage { get; init; }
    public object? EnvLanguagesJson { get; init; }
    public string? EnvPlatform { get; init; }
    public bool? EnvIsOnline { get; init; }
    public string? EnvTimeZone { get; init; }
    public int? EnvScreenWidth { get; init; }
    public int? EnvScreenHeight { get; init; }
    public double? EnvDevicePixelRatio { get; init; }
    public string? EnvReferrer { get; init; }
    public string? EnvPageUrl { get; init; }
    public double? NetDownlink { get; init; }
    public string? NetEffectiveType { get; init; }
    public int? NetRtt { get; init; }
    public bool? NetSaveData { get; init; }
    public string? Error { get; init; }
    public string? CorrelationId { get; init; }
    public string? SessionId { get; init; }
}
