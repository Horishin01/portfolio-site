namespace PortfolioSite.Models.Content;

public static class PortfolioDefaults
{
    public static PortfolioDocument Create()
    {
        return new PortfolioDocument
        {
            Locale = "ja",
            SiteTitle = "Sample Taro | Cloud Infrastructure Engineer Portfolio",
            MetaDescription = "仮名とテストデータで構成した、インフラエンジニア向けポートフォリオの初期サンプル",
            FaviconSrc = "",
            Adsense = new AdsenseContent
            {
                IsEnabled = false,
                PublisherId = "",
                HeadScript = "",
                BodyScript = ""
            },
            Profile = new ProfileContent
            {
                Name = "Sample Taro",
                ShortName = "ST",
                Role = "Cloud Infrastructure Engineer",
                HeroEyebrow = "Cloud Infrastructure Engineer",
                HeroTitle = "Sample Taro's portfolio",
                HeroImageSrc = "",
                HeroImageAlt = "",
                Summary = "AWS を中心に、設計・構築・運用改善・自動化までを一気通貫で整理したインフラエンジニア向けポートフォリオです。開発経験を土台に、開発チームが動きやすい基盤づくりを意識しています。",
                Tags = "AWS\nTerraform\nLinux\nMonitoring\nAutomation",
                Highlights =
                [
                    new LabeledValueItem { Label = "Primary Cloud", Value = "AWS / EC2 / RDS / Route 53" },
                    new LabeledValueItem { Label = "IaC / Delivery", Value = "Terraform / GitHub Actions" },
                    new LabeledValueItem { Label = "Operations", Value = "監視設計 / 障害対応 / 標準化" },
                    new LabeledValueItem { Label = "Strength", Value = "止まりにくく、引き継ぎしやすい基盤設計" }
                ]
            },
            ProfileSection = new ProfileSectionContent
            {
                Heading = "プロフィール",
                Intro = "元サイトの PROFILE セクションをベースに、インフラ領域で任せられる範囲が伝わるよう再構成しています。",
                Lead = "オンプレミス運用、アプリケーション開発、Web 開発の経験を経て、現在はクラウド基盤の設計・構築・運用改善に軸足を置いています。",
                Body = "インフラは表に出にくい仕事だからこそ、可用性だけでなく、運用しやすさ、変更のしやすさ、ドキュメントの残し方まで含めて品質だと考えています。\n\n要件整理から構成設計、IaC 化、監視設計、手順化、障害後の見直しまでをつなげて進めるのが得意です。",
                FocusHeading = "Focus Areas",
                FocusItems = "AWS 基盤設計 / 構築 / 移行\nTerraform を使った再現性のある構築\nLinux サーバー運用と設定標準化\n監視・アラート設計と障害時の初動整理\n作業手順書、構成図、引き継ぎ資料の整備\n開発チームと運用チームの橋渡し",
                CertificationsHeading = "Qualifications",
                Certifications = "基本情報技術者\nOracle Bronze\nJava SE 8 Silver"
            },
            CareerSection = new CareerSectionContent
            {
                Heading = "在籍歴",
                Intro = "在籍した会社や学校などを中心に、どの期間にどこで何をしていたかが分かる形で整理しています。",
                Items =
                [
                    new CareerItem
                    {
                        Period = "2024 - 現在",
                        Category = "勤務先",
                        Organization = "現在の勤務先",
                        Title = "インフラエンジニア",
                        Description = "AWS を中心としたインフラ設計、構築、運用改善に継続して携わっている想定のダミーテキストです。",
                        Highlights = "AWS 環境の設計、構築、運用改善を担当\n監視やアラート、手順書整備を継続的に実施\nTerraform やスクリプトを使った標準化を推進"
                    },
                    new CareerItem
                    {
                        Period = "2021 - 2024",
                        Category = "勤務先",
                        Organization = "前職の勤務先",
                        Title = "運用 / 開発担当",
                        Description = "アプリケーション運用や改修に携わり、業務システムの保守や改善に関わっていた想定のダミーテキストです。",
                        Highlights = "問い合わせ対応や改修を通じて業務運用の流れを把握\nアプリとインフラの接点を意識した改善を経験\n現在のインフラ領域へつながる基礎を形成"
                    },
                    new CareerItem
                    {
                        Period = "2018 - 2021",
                        Category = "学校",
                        Organization = "情報系の学校名",
                        Title = "情報システム学科",
                        Description = "IT の基礎、ネットワーク、サーバー、プログラミングを学び、現在の実務につながる土台を作った想定のダミーテキストです。",
                        Highlights = "基本情報技術やネットワークの基礎を学習\nLinux やデータベースを使った演習に取り組む\n卒業制作やチーム課題でシステム構築を経験"
                    }
                ]
            },
            SkillsSection = new SkillsSectionContent
            {
                Heading = "スキル",
                Intro = "元サイトの SKILL の見せ方をベースに、インフラエンジニア向けのカテゴリに整理しています。",
                Categories =
                [
                    new SkillCategory
                    {
                        Title = "クラウド / サーバー",
                        Summary = "AWS を中心とした構築と、Linux ベースの運用・設定変更。",
                        Items =
                        [
                            new SkillItem { Name = "AWS", Experience = "主担当", Note = "EC2 / RDS / ALB / Route 53 / IAM" },
                            new SkillItem { Name = "Linux", Experience = "主担当", Note = "ユーザー管理 / ミドルウェア設定 / ログ調査" },
                            new SkillItem { Name = "Windows Server", Experience = "実務", Note = "保守 / 設定変更 / 運用補助" },
                            new SkillItem { Name = "Docker", Experience = "実務", Note = "開発 / 検証環境での利用" }
                        ]
                    },
                    new SkillCategory
                    {
                        Title = "IaC / 自動化",
                        Summary = "変更の再現性と作業品質を高めるための技術。",
                        Items =
                        [
                            new SkillItem { Name = "Terraform", Experience = "主担当", Note = "VPC / EC2 / RDS / セキュリティ設定" },
                            new SkillItem { Name = "GitHub Actions", Experience = "実務", Note = "CI/CD / チェック自動化" },
                            new SkillItem { Name = "Shell Script", Experience = "主担当", Note = "定常作業の自動化" },
                            new SkillItem { Name = "Python", Experience = "実務", Note = "運用補助ツール / ログ整形" }
                        ]
                    },
                    new SkillCategory
                    {
                        Title = "監視 / 障害対応",
                        Summary = "検知から切り分け、再発防止までの運用改善。",
                        Items =
                        [
                            new SkillItem { Name = "CloudWatch", Experience = "実務", Note = "メトリクス / アラーム / ログ" },
                            new SkillItem { Name = "Datadog", Experience = "実務", Note = "監視設計 / ダッシュボード / 通知整理" },
                            new SkillItem { Name = "Zabbix", Experience = "実務", Note = "既存監視の棚卸し / しきい値調整" },
                            new SkillItem { Name = "障害対応", Experience = "主担当", Note = "一次切り分け / 連携 / 事後整理" }
                        ]
                    },
                    new SkillCategory
                    {
                        Title = "ミドルウェア / DB / 運用",
                        Summary = "インフラ設計と運用で接点が多い周辺領域。",
                        Items =
                        [
                            new SkillItem { Name = "Nginx / Apache", Experience = "実務", Note = "Web サーバー設定 / 証明書更新" },
                            new SkillItem { Name = "MySQL", Experience = "実務", Note = "接続 / バックアップ / 基本チューニング" },
                            new SkillItem { Name = "Oracle", Experience = "実務", Note = "既存環境の保守対応" },
                            new SkillItem { Name = "Slack / Backlog / Git", Experience = "主担当", Note = "チーム連携 / 変更管理 / 記録" }
                        ]
                    }
                ]
            },
            WorksSection = new WorksSectionContent
            {
                Heading = "実績",
                Intro = "インフラ領域では、どう安定させ、どう運用しやすくしたかが分かる形で整理しています。",
                Items =
                [
                    new WorkItem
                    {
                        Title = "AWS 基盤の段階移行と標準化",
                        Year = "2026",
                        Type = "Infrastructure Refresh",
                        Role = "要件整理 / 構成設計 / Terraform / 手順整備",
                        Summary = "オンプレミス中心の環境から AWS へ段階移行する想定で、構成設計と環境標準化を進めたサンプルです。",
                        Responsibilities = "既存サーバー、ネットワーク、権限の棚卸し\nTerraform による構築フローの統一\n切り戻しを含む移行手順の作成\n監視 / バックアップ / 権限設定の初期整備",
                        Outcomes = "環境差分を減らし、作業の再現性を確保\n引き継ぎしやすい状態を意識して構成と手順を文書化",
                        Stack = "AWS\nTerraform\nLinux\nGitHub Actions"
                    },
                    new WorkItem
                    {
                        Title = "監視 / アラート運用の再設計",
                        Year = "2025",
                        Type = "Operations Improvement",
                        Role = "監視棚卸し / しきい値設計 / 障害フロー整理",
                        Summary = "通知が多すぎて重要アラートが埋もれていた環境を見直し、監視の質を改善したサンプルです。",
                        Responsibilities = "監視項目と通知先の現状整理\nしきい値と通知ルールの再設計\n障害時の一次確認フローを簡素化\n障害後レビューで再発防止案を整理",
                        Outcomes = "不要アラートを減らして対応優先度を明確化\n当番でも追いやすい運用フローへ改善",
                        Stack = "Datadog\nCloudWatch\nSlack\nRunbook"
                    },
                    new WorkItem
                    {
                        Title = "定常作業の自動化と運用標準化",
                        Year = "2024",
                        Type = "Automation",
                        Role = "課題整理 / スクリプト化 / 運用定着",
                        Summary = "手作業が多かった確認・反映作業を洗い出し、日次 / 週次運用の負荷を減らしたサンプルです。",
                        Responsibilities = "作業棚卸しと自動化対象の優先付け\nShell / Python による実行スクリプト作成\n例外時の対応フローを含む runbook 化\nチーム展開とレビュー手順の整備",
                        Outcomes = "属人化しやすい定常作業を共有可能な形へ変更\n運用チーム内で継続しやすい粒度まで整備",
                        Stack = "Python\nBash\nAnsible\nGit"
                    }
                ]
            },
            PersonalSection = new PersonalSectionContent
            {
                Heading = "個人で取り組んでいること",
                Intro = "以下はダミーテキストです。業務外で継続している学習や検証内容を、採用担当者が把握しやすい形で整理する想定です。",
                Items =
                [
                    new PersonalItem
                    {
                        Category = "Home Lab",
                        Title = "検証環境の継続運用",
                        Summary = "クラウドとローカル環境の両方で、構成変更や監視設定を試せる小規模な検証基盤を継続的に触っている想定のダミーテキストです。",
                        DetailSummary = "構成変更や障害試験をすぐ試せるよう、クラウドとローカルを行き来できる検証環境を個人で継続運用している想定です。",
                        DetailBody = "Terraform を軸に構成を切り替えながら、監視、ログ収集、アラートの挙動を小さく検証しています。\n\n手元で再現した内容はメモと手順に残し、業務に近いオペレーションへ持ち込みやすい形に整理する想定です。",
                        Points = "Terraform で構成を切り替えながら検証し、変更手順を整理する\n監視設定やログ収集の挙動を確認し、業務に近い形で試す\n障害を意図的に起こして、初動確認の流れを見直す",
                        Stack = "AWS\nTerraform\nLinux\nCloudWatch",
                        ImageSrc = "",
                        ImageAlt = ""
                    },
                    new PersonalItem
                    {
                        Category = "Automation",
                        Title = "個人用の運用スクリプトと IaC の試作",
                        Summary = "日々の確認作業を短くするためのスクリプトや、使い回せる Terraform モジュールを小さく作って試している想定のダミーテキストです。",
                        DetailSummary = "手作業の確認や環境構築を減らすため、個人で使うスクリプトと IaC の部品を小さく試作し続けている想定です。",
                        DetailBody = "運用フローの中で繰り返し発生する確認作業をスクリプト化し、第三者が読んでも追える粒度まで整えることを意識しています。\n\n同時に Terraform モジュールも試作し、命名、責務の切り方、README の残し方まで含めて再利用しやすい形を検証する想定です。",
                        Points = "サーバー状態確認やログ整形を自動化するスクリプトを試作する\n再利用しやすい命名やディレクトリ構成を検証する\nREADME や runbook を付けて、第三者が追いやすい形を意識する",
                        Stack = "Python\nBash\nTerraform\nGitHub Actions",
                        ImageSrc = "",
                        ImageAlt = ""
                    },
                    new PersonalItem
                    {
                        Category = "Learning",
                        Title = "学習内容の整理と技術アウトプット",
                        Summary = "資格学習や検証で得た内容を、あとから見返せるメモや記事として残している想定のダミーテキストです。",
                        DetailSummary = "学習や検証で得た知見を流さず残すため、メモ、記事、図解として再編集する習慣を持っている想定です。",
                        DetailBody = "理解が浅いまま終わらないよう、構成の違い、制約、つまずいた点を自分の言葉で整理して残しています。\n\nあとから見返せる形にしておくことで、再学習や類似案件への横展開がしやすい状態を目指す想定です。",
                        Points = "構成パターンごとの差分や注意点をメモ化する\n障害対応や監視改善で学んだ観点を短い記事にまとめる\n新しく触ったサービスの用途と制約を整理して記録する",
                        Stack = "Markdown\nGitHub\nNotion\nDiagram",
                        ImageSrc = "",
                        ImageAlt = ""
                    }
                ]
            },
            Contact = new ContactContent
            {
                Heading = "連絡先",
                Note = "以下は初期表示用のテストデータです。公開前に GitHub、職務経歴書、各種リンクを実データへ差し替えてください。",
                Email = "sample@example.com",
                Links =
                [
                    new LinkItem { Label = "GitHub", Href = "https://example.com/github-profile" },
                    new LinkItem { Label = "Resume", Href = "https://example.com/resume.pdf" },
                    new LinkItem { Label = "Blog", Href = "https://example.com/blog" }
                ]
            },
            FooterRole = "Cloud Infrastructure Engineer Portfolio"
        };
    }
}
