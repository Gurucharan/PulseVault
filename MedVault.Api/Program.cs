var builder = WebApplication.CreateBuilder(args);

// Configure CORS for local development and cloud hosting (Vercel/Render)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod());
});

var app = builder.Build();

app.UseCors("AllowAll");

// In-Memory Data Store
var vitalsStore = new List<object>
{
    new { name = "Fasting Blood Sugar", value = "110", unit = "mg/dL", badgeText = "Borderline", badgeColor = "#FEF08A", lastTested = "Today" },
    new { name = "Blood Pressure", value = "120 / 80", unit = "mmHg", badgeText = "Normal", badgeColor = "#DCFCE7", lastTested = "Yesterday" },
    new { name = "HbA1c (3-Month Avg)", value = "5.8", unit = "%", badgeText = "Pre-Diabetic", badgeColor = "#FEF08A", lastTested = "10 days ago" }
};

var trendsStore = new List<object>
{
    new { dateLabel = "Jan 2026", value = 95 },
    new { dateLabel = "Mar 2026", value = 115 },
    new { dateLabel = "May 2026", value = 92 },
    new { dateLabel = "Jul 2026", value = 125 },
    new { dateLabel = "Today", value = 110 }
};

var reportsStore = new List<object>
{
    new { id = 1, title = "Complete Blood Count (CBC)", labName = "Apollo Diagnostic Center", testDate = "12-Jul-2026", statusTag = "Self-Uploaded" },
    new { id = 2, title = "Lipid Profile Panel", labName = "Dr. Lal PathLabs", testDate = "05-May-2026", statusTag = "Self-Uploaded" },
    new { id = 3, title = "Thyroid Profile (T3, T4, TSH)", labName = "Metropolis Healthcare", testDate = "10-Jan-2026", statusTag = "Self-Uploaded" }
};

// Root Health Check Endpoint
app.MapGet("/", () => "MedVault API is running successfully!");

// Screen 1: Dashboard Overview Endpoint
app.MapGet("/api/dashboard", () =>
{
    return Results.Ok(new
    {
        patientName = "Gurucharan",
        vitals = vitalsStore,
        fastingSugarTrends = trendsStore,
        recentReports = reportsStore
    });
});

// Screen 2: POST Endpoint for New Report Log
app.MapPost("/api/reports/upload", (NewReportRequest req) =>
{
    int newId = reportsStore.Count + 1;

    // Add to reports table
    reportsStore.Insert(0, new { id = newId, title = req.TestName, labName = req.LabName, testDate = req.TestDate, statusTag = "Self-Uploaded" });

    // If Fasting Blood Sugar was logged, update trend chart & card
    if (!string.IsNullOrEmpty(req.SugarValue) && double.TryParse(req.SugarValue, out double val))
    {
        trendsStore.Add(new { dateLabel = "Latest", value = val });
        vitalsStore[0] = new { name = "Fasting Blood Sugar", value = req.SugarValue, unit = "mg/dL", badgeText = val > 120 ? "High" : "Borderline", badgeColor = val > 120 ? "#FEE2E2" : "#FEF08A", lastTested = "Just now" };
    }

    return Results.Ok(new { message = "Report successfully logged!" });
});

// Screen 3: Report Details Viewer by ID
app.MapGet("/api/reports/{id:int}", (int id) =>
{
    var reportDetail = new
    {
        id = id,
        title = "Complete Blood Count (CBC)",
        labName = "Apollo Diagnostic Center",
        testDate = "12-Jul-2026",
        isUnverified = true,
        parameters = new[]
        {
            new { name = "Hemoglobin (Hb)", result = "14.2", unit = "g/dL", refRange = "13.5 - 18.0", status = "Normal", badgeColor = "#DCFCE7", fontColor = "#166534" },
            new { name = "White Blood Cells (WBC)", result = "12,500", unit = "/mcL", refRange = "4,500 - 11,000", status = "High", badgeColor = "#FEE2E2", fontColor = "#991B1B" },
            new { name = "Platelet Count", result = "250,000", unit = "/mcL", refRange = "150,000 - 450,000", status = "Normal", badgeColor = "#DCFCE7", fontColor = "#166534" }
        }
    };

    return Results.Ok(reportDetail);
});

// Screen 5: Doctor Shared Health Snapshot
app.MapGet("/api/doctor-view/{pin}", (string pin) =>
{
    if (pin != "8924")
    {
        return Results.Unauthorized();
    }

    var doctorData = new
    {
        patientName = "Gurucharan",
        ageGender = "28 / Male",
        pinExpiredIn = "14 minutes",
        keyVitals = new[]
        {
            new { label = "Fasting Blood Sugar", value = "110 mg/dL", trend = "Borderline (High)" },
            new { label = "HbA1c", value = "5.8%", trend = "Pre-Diabetic" },
            new { label = "Blood Pressure", value = "120/80 mmHg", trend = "Normal" }
        },
        flaggedParameters = new[]
        {
            new { test = "Fasting Blood Sugar", value = "110 mg/dL", normalRange = "70 - 99 mg/dL", date = "Today" },
            new { test = "WBC Count", value = "12,500 /mcL", normalRange = "4,500 - 11,000 /mcL", date = "12-Jul-2026" }
        },
        recentHistory = new[]
        {
            new { date = "12-Jul-2026", eventName = "Complete Blood Count (CBC) - Apollo Diagnostic" },
            new { date = "05-May-2026", eventName = "Lipid Profile Panel - Dr. Lal PathLabs" }
        }
    };

    return Results.Ok(doctorData);
});

app.Run();

public record NewReportRequest(string TestName, string LabName, string TestDate, string SugarValue);