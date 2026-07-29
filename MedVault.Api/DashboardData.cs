namespace MedVault.Api;

public record VitalSummary(string Name, string Value, string Unit, string BadgeText, string BadgeColor, string LastTested);
public record TrendPoint(string DateLabel, double Value);
public record StoredReport(int Id, string Title, string LabName, string TestDate, string StatusTag);

public record PatientDashboardDto(
    string PatientName,
    List<VitalSummary> Vitals,
    List<TrendPoint> FastingSugarTrends,
    List<StoredReport> RecentReports
);
