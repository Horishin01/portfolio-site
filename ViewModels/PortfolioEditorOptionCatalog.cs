namespace PortfolioSite.ViewModels;

public static class PortfolioEditorOptionCatalog
{
    public static IReadOnlyList<PortfolioEditorOptionGroup> ProfileTagGroups { get; } =
    [
        new("プログラム", ["C#", ".NET", "Python", "Java", "JavaScript", "TypeScript", "Bash", "PowerShell"]),
        new("クラウド / インフラ", ["AWS", "Azure", "GCP", "Terraform", "Ansible", "Linux", "Windows Server", "Docker", "Kubernetes", "IaC", "CI/CD", "SRE", "Platform Engineering"]),
        new("ハードウェア", ["PC", "サーバー", "ネットワーク機器", "ストレージ", "オンプレミス環境", "組み込み機器"]),
        new("データベース", ["MySQL", "PostgreSQL", "Oracle", "SQL Server", "Redis"]),
        new("監視 / 運用", ["Datadog", "CloudWatch", "Prometheus", "Grafana", "Zabbix", "Monitoring", "Observability", "Automation", "Security", "Networking"]),
        new("ツール / コラボレーション", ["Git", "GitHub Actions", "GitLab CI", "Slack", "Backlog", "Notion", "Markdown", "Runbook", "Diagram"])
    ];

    public static IReadOnlyList<string> TechnologyOptions { get; } =
    [
        "AWS",
        "Azure",
        "GCP",
        "Terraform",
        "CloudFormation",
        "Ansible",
        "Linux",
        "Windows Server",
        "Docker",
        "Kubernetes",
        "GitHub Actions",
        "GitLab CI",
        "Bash",
        "Shell Script",
        "Python",
        "PowerShell",
        ".NET",
        "C#",
        "Datadog",
        "CloudWatch",
        "Prometheus",
        "Grafana",
        "Zabbix",
        "Nginx",
        "Apache",
        "MySQL",
        "PostgreSQL",
        "Oracle",
        "SQL Server",
        "Redis",
        "EC2",
        "RDS",
        "S3",
        "Lambda",
        "Route 53",
        "IAM",
        "VPC",
        "Git",
        "Slack",
        "Backlog",
        "Notion",
        "Markdown",
        "Runbook",
        "Diagram"
    ];

    public static IReadOnlySet<string> KnownProfileTags { get; } = new HashSet<string>(
        ProfileTagGroups.SelectMany(group => group.Options),
        StringComparer.OrdinalIgnoreCase
    );
}

public sealed class PortfolioEditorOptionGroup
{
    public PortfolioEditorOptionGroup(string label, IReadOnlyList<string> options)
    {
        Label = label;
        Options = options;
    }

    public string Label { get; }
    public IReadOnlyList<string> Options { get; }
}
