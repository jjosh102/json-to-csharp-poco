using System.Text.Json.Serialization;

namespace JsonToCsharp
{
    public record Subsidiaries
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("location")]
        public string Location { get; init; }

        [JsonPropertyName("employeeCount")]
        public int EmployeeCount { get; init; }

        [JsonPropertyName("revenue")]
        public int Revenue { get; init; }
    }

    public record Company
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("founded")]
        public DateTime Founded { get; init; }

        [JsonPropertyName("isPublic")]
        public bool IsPublic { get; init; }

        [JsonPropertyName("stockPrice")]
        public double StockPrice { get; init; }

        [JsonPropertyName("marketCap")]
        public int MarketCap { get; init; }

        [JsonPropertyName("activeMarkets")]
        public IReadOnlyList<string> ActiveMarkets { get; init; }

        [JsonPropertyName("subsidiaries")]
        public IReadOnlyList<Subsidiaries> Subsidiaries { get; init; }
    }

    public record Deadlines
    {
        [JsonPropertyName("start")]
        public DateTime Start { get; init; }

        [JsonPropertyName("end")]
        public DateTime End { get; init; }
    }

    public record Projects
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("status")]
        public string Status { get; init; }

        [JsonPropertyName("completionRate")]
        public int CompletionRate { get; init; }

        [JsonPropertyName("hoursLogged")]
        public double HoursLogged { get; init; }

        [JsonPropertyName("deadlines")]
        public Deadlines Deadlines { get; init; }
    }

    public record _2023
    {
        [JsonPropertyName("q1")]
        public double Q1 { get; init; }

        [JsonPropertyName("q2")]
        public double Q2 { get; init; }

        [JsonPropertyName("q3")]
        public double Q3 { get; init; }

        [JsonPropertyName("q4")]
        public double Q4 { get; init; }
    }

    public record Performance
    {
        [JsonPropertyName("_2023")]
        public _2023 _2023 { get; init; }
    }

    public record Employees
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; init; }

        [JsonPropertyName("lastName")]
        public string LastName { get; init; }

        [JsonPropertyName("email")]
        public string Email { get; init; }

        [JsonPropertyName("role")]
        public string Role { get; init; }

        [JsonPropertyName("department")]
        public string Department { get; init; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; init; }

        [JsonPropertyName("salary")]
        public int Salary { get; init; }

        [JsonPropertyName("active")]
        public bool Active { get; init; }

        [JsonPropertyName("badges")]
        public IReadOnlyList<string> Badges { get; init; }

        [JsonPropertyName("skills")]
        public IReadOnlyList<string> Skills { get; init; }

        [JsonPropertyName("projects")]
        public IReadOnlyList<Projects> Projects { get; init; }

        [JsonPropertyName("performance")]
        public Performance Performance { get; init; }
    }

    public record UsageStats
    {
        [JsonPropertyName("activeUsers")]
        public int ActiveUsers { get; init; }

        [JsonPropertyName("averageDailyRequests")]
        public int AverageDailyRequests { get; init; }

        [JsonPropertyName("storageUsed")]
        public string StorageUsed { get; init; }
    }

    public record Limitations
    {
        [JsonPropertyName("maxUsers")]
        public int MaxUsers { get; init; }

        [JsonPropertyName("maxStorage")]
        public string MaxStorage { get; init; }

        [JsonPropertyName("maxRequests")]
        public int MaxRequests { get; init; }
    }

    public record Features
    {
        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; init; }

        [JsonPropertyName("usageStats")]
        public UsageStats UsageStats { get; init; }

        [JsonPropertyName("limitations")]
        public Limitations Limitations { get; init; }
    }

    public record Products
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("name")]
        public string Name { get; init; }

        [JsonPropertyName("version")]
        public DateTime Version { get; init; }

        [JsonPropertyName("price")]
        public double Price { get; init; }

        [JsonPropertyName("currency")]
        public string Currency { get; init; }

        [JsonPropertyName("isSubscription")]
        public bool IsSubscription { get; init; }

        [JsonPropertyName("activeSubscribers")]
        public int ActiveSubscribers { get; init; }

        [JsonPropertyName("features")]
        public IReadOnlyList<Features> Features { get; init; }
    }

    public record Support_tickets
    {
        [JsonPropertyName("id")]
        public string Id { get; init; }

        [JsonPropertyName("customer")]
        public string Customer { get; init; }

        [JsonPropertyName("priority")]
        public string Priority { get; init; }

        [JsonPropertyName("status")]
        public string Status { get; init; }

        [JsonPropertyName("created")]
        public DateTime Created { get; init; }

        [JsonPropertyName("updated")]
        public DateTime Updated { get; init; }

        [JsonPropertyName("subject")]
        public string Subject { get; init; }

        [JsonPropertyName("assignee")]
        public string Assignee { get; init; }

        [JsonPropertyName("responseTime")]
        public double ResponseTime { get; init; }

        [JsonPropertyName("tags")]
        public IReadOnlyList<string> Tags { get; init; }
    }

    public record Last_24h
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; init; }

        [JsonPropertyName("cpu_usage")]
        public double Cpu_usage { get; init; }

        [JsonPropertyName("memory_usage")]
        public double Memory_usage { get; init; }

        [JsonPropertyName("active_users")]
        public int Active_users { get; init; }

        [JsonPropertyName("response_time")]
        public double Response_time { get; init; }
    }

    public record Daily_average
    {
        [JsonPropertyName("cpu_usage")]
        public double Cpu_usage { get; init; }

        [JsonPropertyName("memory_usage")]
        public double Memory_usage { get; init; }

        [JsonPropertyName("active_users")]
        public int Active_users { get; init; }

        [JsonPropertyName("response_time")]
        public double Response_time { get; init; }
    }

    public record Peak_values
    {
        [JsonPropertyName("cpu_usage")]
        public double Cpu_usage { get; init; }

        [JsonPropertyName("memory_usage")]
        public double Memory_usage { get; init; }

        [JsonPropertyName("active_users")]
        public int Active_users { get; init; }

        [JsonPropertyName("response_time")]
        public double Response_time { get; init; }
    }

    public record Aggregates
    {
        [JsonPropertyName("daily_average")]
        public Daily_average Daily_average { get; init; }

        [JsonPropertyName("peak_values")]
        public Peak_values Peak_values { get; init; }
    }

    public record System_metrics
    {
        [JsonPropertyName("last_24h")]
        public IReadOnlyList<Last_24h> Last_24h { get; init; }

        [JsonPropertyName("aggregates")]
        public Aggregates Aggregates { get; init; }
    }

    public record Debug
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; init; }

        [JsonPropertyName("level")]
        public string Level { get; init; }

        [JsonPropertyName("retentionDays")]
        public int RetentionDays { get; init; }
    }

    public record Metadata
    {
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; init; }

        [JsonPropertyName("version")]
        public string Version { get; init; }

        [JsonPropertyName("environment")]
        public string Environment { get; init; }

        [JsonPropertyName("dataCenter")]
        public string DataCenter { get; init; }

        [JsonPropertyName("tags")]
        public IReadOnlyList<string> Tags { get; init; }

        [JsonPropertyName("debug")]
        public Debug Debug { get; init; }
    }

    public record Root
    {
        [JsonPropertyName("company")]
        public Company Company { get; init; }

        [JsonPropertyName("employees")]
        public IReadOnlyList<Employees> Employees { get; init; }

        [JsonPropertyName("products")]
        public IReadOnlyList<Products> Products { get; init; }

        [JsonPropertyName("support_tickets")]
        public IReadOnlyList<Support_tickets> Support_tickets { get; init; }

        [JsonPropertyName("system_metrics")]
        public System_metrics System_metrics { get; init; }

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; init; }
    }
}