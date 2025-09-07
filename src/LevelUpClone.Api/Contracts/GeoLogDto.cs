namespace LevelUpClone.Api.Contracts
{
    public sealed class GeoLogDto
    {
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public double? AccuracyMeters { get; set; }
        public double? AltitudeMeters { get; set; }
        public double? AltitudeAccuracyMeters { get; set; }
        public double? SpeedMetersPerSecond { get; set; }
        public double? HeadingDegrees { get; set; }
        public long? TimestampEpochMs { get; set; }
        public string? City { get; set; }
        public string? EnvBrowser { get; set; }
        public string? EnvBrowserVersion { get; set; }
        public string? EnvOperatingSystem { get; set; }
        public string? EnvOSVersion { get; set; }
        public string? EnvArchitecture { get; set; }
        public string? EnvDeviceType { get; set; }
        public string? EnvDeviceModel { get; set; }
        public int? EnvTouchPoints { get; set; }
        public bool? EnvIsBot { get; set; }
        public string? EnvBotName { get; set; }
        public string? EnvLanguage { get; set; }
        public object? EnvLanguagesJson { get; set; } 
        public string? EnvPlatform { get; set; }
        public bool? EnvIsOnline { get; set; }
        public string? EnvTimeZone { get; set; }
        public int? EnvScreenWidth { get; set; }
        public int? EnvScreenHeight { get; set; }
        public double? EnvDevicePixelRatio { get; set; }
        public string? EnvReferrer { get; set; }
        public string? EnvPageUrl { get; set; }
        public double? NetDownlink { get; set; }
        public string? NetEffectiveType { get; set; }
        public int? NetRtt { get; set; }
        public bool? NetSaveData { get; set; }
        public string? Error { get; set; }
        public string? SessionId { get; set; }
    }
}
