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

    public static IReadOnlyList<PortfolioEditorOptionGroup> TechnologyOptionGroups { get; } =
    [
        new("クラウド基盤", ["AWS", "Azure", "GCP", "EC2", "RDS", "S3", "Lambda", "Route 53", "IAM", "VPC"]),
        new("IaC / CI/CD", ["Terraform", "CloudFormation", "Ansible", "GitHub Actions", "GitLab CI"]),
        new("OS / コンテナ", ["Linux", "Windows Server", "Docker", "Kubernetes"]),
        new("スクリプト / 開発", ["Bash", "Shell Script", "Python", "PowerShell", ".NET", "C#"]),
        new("監視 / 運用", ["Datadog", "CloudWatch", "Prometheus", "Grafana", "Zabbix"]),
        new("Web / ミドルウェア", ["Nginx", "Apache"]),
        new("データベース", ["MySQL", "PostgreSQL", "Oracle", "SQL Server", "Redis"]),
        new("ドキュメント / コラボレーション", ["Git", "Slack", "Backlog", "Notion", "Markdown", "Runbook", "Diagram"])
    ];

    public static IReadOnlySet<string> KnownProfileTags { get; } = new HashSet<string>(
        ProfileTagGroups.SelectMany(group => group.Options),
        StringComparer.OrdinalIgnoreCase
    );

    public static IReadOnlySet<string> KnownTechnologyOptions { get; } = new HashSet<string>(
        TechnologyOptionGroups.SelectMany(group => group.Options),
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
