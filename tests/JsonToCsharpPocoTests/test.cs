using System.Text.Json.Serialization;

namespace JsonToCsharp
{
    public record Subsidiaries
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("employeeCount")]
        public int EmployeeCount { get; set; }

        [JsonPropertyName("revenue")]
        public int Revenue { get; set; }
    }

    public record Company
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("founded")]
        public DateTime Founded { get; set; }

        [JsonPropertyName("isPublic")]
        public bool IsPublic { get; set; }

        [JsonPropertyName("stockPrice")]
        public double StockPrice { get; set; }

        [JsonPropertyName("marketCap")]
        public int MarketCap { get; set; }

        [JsonPropertyName("activeMarkets")]
        public IReadOnlyList<string> ActiveMarkets { get; set; }

        [JsonPropertyName("subsidiaries")]
        public IReadOnlyList<Subsidiaries> Subsidiaries { get; set; }
    }

    public record Deadlines
    {
        [JsonPropertyName("start")]
        public DateTime Start { get; set; }

        [JsonPropertyName("end")]
        public DateTime End { get; set; }
    }

    public record Projects
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("completionRate")]
        public int CompletionRate { get; set; }

        [JsonPropertyName("hoursLogged")]
        public double HoursLogged { get; set; }

        [JsonPropertyName("deadlines")]
        public Deadlines Deadlines { get; set; }
    }

    public record _2023
    {
        [JsonPropertyName("q1")]
        public double Q1 { get; set; }

        [JsonPropertyName("q2")]
        public double Q2 { get; set; }

        [JsonPropertyName("q3")]
        public double Q3 { get; set; }

        [JsonPropertyName("q4")]
        public double Q4 { get; set; }
    }

    public record Performance
    {
        [JsonPropertyName("2023")]
        public _2023 _2023 { get; set; }
    }

    public record Employees
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("firstName")]
        public string FirstName { get; set; }

        [JsonPropertyName("lastName")]
        public string LastName { get; set; }

        [JsonPropertyName("email")]
        public string Email { get; set; }

        [JsonPropertyName("role")]
        public string Role { get; set; }

        [JsonPropertyName("department")]
        public string Department { get; set; }

        [JsonPropertyName("startDate")]
        public DateTime StartDate { get; set; }

        [JsonPropertyName("salary")]
        public int Salary { get; set; }

        [JsonPropertyName("active")]
        public bool Active { get; set; }

        [JsonPropertyName("badges")]
        public IReadOnlyList<string> Badges { get; set; }

        [JsonPropertyName("skills")]
        public IReadOnlyList<string> Skills { get; set; }

        [JsonPropertyName("projects")]
        public IReadOnlyList<Projects> Projects { get; set; }

        [JsonPropertyName("performance")]
        public Performance Performance { get; set; }
    }

    public record UsageStats
    {
        [JsonPropertyName("activeUsers")]
        public int ActiveUsers { get; set; }

        [JsonPropertyName("averageDailyRequests")]
        public int AverageDailyRequests { get; set; }

        [JsonPropertyName("storageUsed")]
        public string StorageUsed { get; set; }
    }

    public record Limitations
    {
        [JsonPropertyName("maxUsers")]
        public int MaxUsers { get; set; }

        [JsonPropertyName("maxStorage")]
        public string MaxStorage { get; set; }

        [JsonPropertyName("maxRequests")]
        public int MaxRequests { get; set; }
    }

    public record Features
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("isEnabled")]
        public bool IsEnabled { get; set; }

        [JsonPropertyName("usageStats")]
        public UsageStats UsageStats { get; set; }

        [JsonPropertyName("limitations")]
        public Limitations Limitations { get; set; }
    }

    public record Products
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("version")]
        public DateTime Version { get; set; }

        [JsonPropertyName("price")]
        public double Price { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; }

        [JsonPropertyName("isSubscription")]
        public bool IsSubscription { get; set; }

        [JsonPropertyName("activeSubscribers")]
        public int ActiveSubscribers { get; set; }

        [JsonPropertyName("features")]
        public IReadOnlyList<Features> Features { get; set; }
    }

    public record Support_tickets
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("customer")]
        public string Customer { get; set; }

        [JsonPropertyName("priority")]
        public string Priority { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }

        [JsonPropertyName("created")]
        public DateTime Created { get; set; }

        [JsonPropertyName("updated")]
        public DateTime Updated { get; set; }

        [JsonPropertyName("subject")]
        public string Subject { get; set; }

        [JsonPropertyName("assignee")]
        public string Assignee { get; set; }

        [JsonPropertyName("responseTime")]
        public double ResponseTime { get; set; }

        [JsonPropertyName("tags")]
        public IReadOnlyList<string> Tags { get; set; }
    }

    public record Last_24h
    {
        [JsonPropertyName("timestamp")]
        public DateTime Timestamp { get; set; }

        [JsonPropertyName("cpu_usage")]
        public double Cpu_usage { get; set; }

        [JsonPropertyName("memory_usage")]
        public double Memory_usage { get; set; }

        [JsonPropertyName("active_users")]
        public int Active_users { get; set; }

        [JsonPropertyName("response_time")]
        public double Response_time { get; set; }
    }

    public record Daily_average
    {
        [JsonPropertyName("cpu_usage")]
        public double Cpu_usage { get; set; }

        [JsonPropertyName("memory_usage")]
        public double Memory_usage { get; set; }

        [JsonPropertyName("active_users")]
        public int Active_users { get; set; }

        [JsonPropertyName("response_time")]
        public double Response_time { get; set; }
    }

    public record Peak_values
    {
        [JsonPropertyName("cpu_usage")]
        public double Cpu_usage { get; set; }

        [JsonPropertyName("memory_usage")]
        public double Memory_usage { get; set; }

        [JsonPropertyName("active_users")]
        public int Active_users { get; set; }

        [JsonPropertyName("response_time")]
        public double Response_time { get; set; }
    }

    public record Aggregates
    {
        [JsonPropertyName("daily_average")]
        public Daily_average Daily_average { get; set; }

        [JsonPropertyName("peak_values")]
        public Peak_values Peak_values { get; set; }
    }

    public record System_metrics
    {
        [JsonPropertyName("last_24h")]
        public IReadOnlyList<Last_24h> Last_24h { get; set; }

        [JsonPropertyName("aggregates")]
        public Aggregates Aggregates { get; set; }
    }

    public record Debug
    {
        [JsonPropertyName("enabled")]
        public bool Enabled { get; set; }

        [JsonPropertyName("level")]
        public string Level { get; set; }

        [JsonPropertyName("retentionDays")]
        public int RetentionDays { get; set; }
    }

    public record Metadata
    {
        [JsonPropertyName("lastUpdated")]
        public DateTime LastUpdated { get; set; }

        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("environment")]
        public string Environment { get; set; }

        [JsonPropertyName("dataCenter")]
        public string DataCenter { get; set; }

        [JsonPropertyName("tags")]
        public IReadOnlyList<string> Tags { get; set; }

        [JsonPropertyName("debug")]
        public Debug Debug { get; set; }
    }

    public record Root
    {
        [JsonPropertyName("company")]
        public Company Company { get; set; }

        [JsonPropertyName("employees")]
        public IReadOnlyList<Employees> Employees { get; set; }

        [JsonPropertyName("products")]
        public IReadOnlyList<Products> Products { get; set; }

        [JsonPropertyName("support_tickets")]
        public IReadOnlyList<Support_tickets> Support_tickets { get; set; }

        [JsonPropertyName("system_metrics")]
        public System_metrics System_metrics { get; set; }

        [JsonPropertyName("metadata")]
        public Metadata Metadata { get; set; }
    }
}