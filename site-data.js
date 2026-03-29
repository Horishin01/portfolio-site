/*
  編集しやすくするための基本ルール

  1. 名前や GitHub など固定値は OWNER にまとめる
  2. 本文は複数行文字列で書ける
     段落を分けたいときは 1 行空ける
  3. 箇条書きは 1 行 1 項目で書ける
*/

const OWNER = {
  name: "Yoshimura Takuya",
  shortName: "YT",
  role: "Infrastructure Engineer",
  github: "https://github.com/horishin06",
  email: ""
};

const PORTFOLIO_STORAGE_KEY = "portfolio-site.admin-data";

const defaultPortfolioData = {
  locale: "ja",
  siteTitle: `${OWNER.name} | ${OWNER.role} Portfolio`,
  metaDescription:
    "元サイトの骨格を踏まえて再構成した、インフラエンジニア向けポートフォリオ",
  profile: {
    name: OWNER.name,
    shortName: OWNER.shortName,
    role: OWNER.role,
    heroEyebrow: OWNER.role,
    heroTitle: `${OWNER.name}'s portfolio`,
    heroImageSrc: "",
    heroImageAlt: "",
    summary:
      "AWS を中心に、設計・構築・運用改善・自動化までを一気通貫で整理したインフラエンジニア向けポートフォリオです。開発経験を土台に、開発チームが動きやすい基盤づくりを意識しています。",
    tags: `
      AWS
      Terraform
      Linux
      Monitoring
      Automation
    `,
    highlights: [
      { label: "Primary Cloud", value: "AWS / EC2 / RDS / Route 53" },
      { label: "IaC / Delivery", value: "Terraform / GitHub Actions" },
      { label: "Operations", value: "監視設計 / 障害対応 / 標準化" },
      { label: "Strength", value: "止まりにくく、引き継ぎしやすい基盤設計" }
    ]
  },
  profileSection: {
    heading: "プロフィール",
    intro:
      "元サイトの PROFILE セクションをベースに、インフラ領域で任せられる範囲が伝わるよう再構成しています。",
    lead:
      "オンプレミス運用、アプリケーション開発、Web 開発の経験を経て、現在はクラウド基盤の設計・構築・運用改善に軸足を置いています。",
    body: `
      インフラは表に出にくい仕事だからこそ、可用性だけでなく、運用しやすさ、変更のしやすさ、ドキュメントの残し方まで含めて品質だと考えています。

      要件整理から構成設計、IaC 化、監視設計、手順化、障害後の見直しまでをつなげて進めるのが得意です。
    `,
    focusHeading: "Focus Areas",
    focusItems: `
      AWS 基盤設計 / 構築 / 移行
      Terraform を使った再現性のある構築
      Linux サーバー運用と設定標準化
      監視・アラート設計と障害時の初動整理
      作業手順書、構成図、引き継ぎ資料の整備
      開発チームと運用チームの橋渡し
    `,
    certificationsHeading: "Qualifications",
    certifications: `
      基本情報技術者
      Oracle Bronze
      Java SE 8 Silver
    `
  },
  careerSection: {
    heading: "在籍歴",
    intro:
      "在籍した会社や学校などを中心に、どの期間にどこで何をしていたかが分かる形で整理しています。",
    items: [
      {
        period: "2024 - 現在",
        category: "勤務先",
        organization: "現在の勤務先",
        title: "インフラエンジニア",
        description:
          "AWS を中心としたインフラ設計、構築、運用改善に継続して携わっている想定のダミーテキストです。",
        highlights: `
          AWS 環境の設計、構築、運用改善を担当
          監視やアラート、手順書整備を継続的に実施
          Terraform やスクリプトを使った標準化を推進
        `
      },
      {
        period: "2021 - 2024",
        category: "勤務先",
        organization: "前職の勤務先",
        title: "運用 / 開発担当",
        description:
          "アプリケーション運用や改修に携わり、業務システムの保守や改善に関わっていた想定のダミーテキストです。",
        highlights: `
          問い合わせ対応や改修を通じて業務運用の流れを把握
          アプリとインフラの接点を意識した改善を経験
          現在のインフラ領域へつながる基礎を形成
        `
      },
      {
        period: "2018 - 2021",
        category: "学校",
        organization: "情報系の学校名",
        title: "情報システム学科",
        description:
          "IT の基礎、ネットワーク、サーバー、プログラミングを学び、現在の実務につながる土台を作った想定のダミーテキストです。",
        highlights: `
          基本情報技術やネットワークの基礎を学習
          Linux やデータベースを使った演習に取り組む
          卒業制作やチーム課題でシステム構築を経験
        `
      }
    ]
  },
  skillsSection: {
    heading: "スキル",
    intro:
      "元サイトの SKILL の見せ方をベースに、インフラエンジニア向けのカテゴリに整理しています。",
    categories: [
      {
        title: "クラウド / サーバー",
        summary: "AWS を中心とした構築と、Linux ベースの運用・設定変更。",
        items: [
          { name: "AWS", experience: "主担当", note: "EC2 / RDS / ALB / Route 53 / IAM" },
          { name: "Linux", experience: "主担当", note: "ユーザー管理 / ミドルウェア設定 / ログ調査" },
          { name: "Windows Server", experience: "実務", note: "保守 / 設定変更 / 運用補助" },
          { name: "Docker", experience: "実務", note: "開発 / 検証環境での利用" }
        ]
      },
      {
        title: "IaC / 自動化",
        summary: "変更の再現性と作業品質を高めるための技術。",
        items: [
          { name: "Terraform", experience: "主担当", note: "VPC / EC2 / RDS / セキュリティ設定" },
          { name: "GitHub Actions", experience: "実務", note: "CI/CD / チェック自動化" },
          { name: "Shell Script", experience: "主担当", note: "定常作業の自動化" },
          { name: "Python", experience: "実務", note: "運用補助ツール / ログ整形" }
        ]
      },
      {
        title: "監視 / 障害対応",
        summary: "検知から切り分け、再発防止までの運用改善。",
        items: [
          { name: "CloudWatch", experience: "実務", note: "メトリクス / アラーム / ログ" },
          { name: "Datadog", experience: "実務", note: "監視設計 / ダッシュボード / 通知整理" },
          { name: "Zabbix", experience: "実務", note: "既存監視の棚卸し / しきい値調整" },
          { name: "障害対応", experience: "主担当", note: "一次切り分け / 連携 / 事後整理" }
        ]
      },
      {
        title: "ミドルウェア / DB / 運用",
        summary: "インフラ設計と運用で接点が多い周辺領域。",
        items: [
          { name: "Nginx / Apache", experience: "実務", note: "Web サーバー設定 / 証明書更新" },
          { name: "MySQL", experience: "実務", note: "接続 / バックアップ / 基本チューニング" },
          { name: "Oracle", experience: "実務", note: "既存環境の保守対応" },
          { name: "Slack / Backlog / Git", experience: "主担当", note: "チーム連携 / 変更管理 / 記録" }
        ]
      }
    ]
  },
  worksSection: {
    heading: "実績",
    intro:
      "インフラ領域では、どう安定させ、どう運用しやすくしたかが分かる形で整理しています。",
    items: [
      {
        title: "AWS 基盤の段階移行と標準化",
        year: "2026",
        type: "Infrastructure Refresh",
        role: "要件整理 / 構成設計 / Terraform / 手順整備",
        summary:
          "オンプレミス中心の環境から AWS へ段階移行する想定で、構成設計と環境標準化を進めたサンプルです。",
        responsibilities: `
          既存サーバー、ネットワーク、権限の棚卸し
          Terraform による構築フローの統一
          切り戻しを含む移行手順の作成
          監視 / バックアップ / 権限設定の初期整備
        `,
        outcomes: `
          環境差分を減らし、作業の再現性を確保
          引き継ぎしやすい状態を意識して構成と手順を文書化
        `,
        stack: `
          AWS
          Terraform
          Linux
          GitHub Actions
        `
      },
      {
        title: "監視 / アラート運用の再設計",
        year: "2025",
        type: "Operations Improvement",
        role: "監視棚卸し / しきい値設計 / 障害フロー整理",
        summary:
          "通知が多すぎて重要アラートが埋もれていた環境を見直し、監視の質を改善したサンプルです。",
        responsibilities: `
          監視項目と通知先の現状整理
          しきい値と通知ルールの再設計
          障害時の一次確認フローを簡素化
          障害後レビューで再発防止案を整理
        `,
        outcomes: `
          不要アラートを減らして対応優先度を明確化
          当番でも追いやすい運用フローへ改善
        `,
        stack: `
          Datadog
          CloudWatch
          Slack
          Runbook
        `
      },
      {
        title: "定常作業の自動化と運用標準化",
        year: "2024",
        type: "Automation",
        role: "課題整理 / スクリプト化 / 運用定着",
        summary:
          "手作業が多かった確認・反映作業を洗い出し、日次 / 週次運用の負荷を減らしたサンプルです。",
        responsibilities: `
          作業棚卸しと自動化対象の優先付け
          Shell / Python による実行スクリプト作成
          例外時の対応フローを含む runbook 化
          チーム展開とレビュー手順の整備
        `,
        outcomes: `
          属人化しやすい定常作業を共有可能な形へ変更
          運用チーム内で継続しやすい粒度まで整備
        `,
        stack: `
          Python
          Bash
          Ansible
          Git
        `
      }
    ]
  },
  personalSection: {
    heading: "個人で取り組んでいること",
    intro:
      "以下はダミーテキストです。業務外で継続している学習や検証内容を、採用担当者が把握しやすい形で整理する想定です。",
    items: [
      {
        category: "Home Lab",
        title: "検証環境の継続運用",
        summary:
          "クラウドとローカル環境の両方で、構成変更や監視設定を試せる小規模な検証基盤を継続的に触っている想定のダミーテキストです。",
        points: `
          Terraform で構成を切り替えながら検証し、変更手順を整理する
          監視設定やログ収集の挙動を確認し、業務に近い形で試す
          障害を意図的に起こして、初動確認の流れを見直す
        `,
        stack: `
          AWS
          Terraform
          Linux
          CloudWatch
        `
      },
      {
        category: "Automation",
        title: "個人用の運用スクリプトと IaC の試作",
        summary:
          "日々の確認作業を短くするためのスクリプトや、使い回せる Terraform モジュールを小さく作って試している想定のダミーテキストです。",
        points: `
          サーバー状態確認やログ整形を自動化するスクリプトを試作する
          再利用しやすい命名やディレクトリ構成を検証する
          README や runbook を付けて、第三者が追いやすい形を意識する
        `,
        stack: `
          Python
          Bash
          Terraform
          GitHub Actions
        `
      },
      {
        category: "Learning",
        title: "学習内容の整理と技術アウトプット",
        summary:
          "資格学習や検証で得た内容を、あとから見返せるメモや記事として残している想定のダミーテキストです。",
        points: `
          構成パターンごとの差分や注意点をメモ化する
          障害対応や監視改善で学んだ観点を短い記事にまとめる
          新しく触ったサービスの用途と制約を整理して記録する
        `,
        stack: `
          Markdown
          GitHub
          Notion
          Diagram
        `
      }
    ]
  },
  contact: {
    heading: "連絡先",
    note: "GitHub や職務経歴書など、採用担当者が確認しやすいリンクをまとめています。",
    email: OWNER.email,
    links: [
      { label: "GitHub", href: OWNER.github }
    ]
  },
  footerRole: `${OWNER.role} Portfolio`
};

const deepClone = (value) => JSON.parse(JSON.stringify(value));

const isPlainObject = (value) =>
  Boolean(value) && typeof value === "object" && !Array.isArray(value);

const mergePortfolioData = (baseValue, overrideValue) => {
  if (Array.isArray(baseValue)) {
    return Array.isArray(overrideValue) ? overrideValue : deepClone(baseValue);
  }

  if (isPlainObject(baseValue)) {
    const result = {};
    const overrideObject = isPlainObject(overrideValue) ? overrideValue : {};
    const keys = new Set([
      ...Object.keys(baseValue),
      ...Object.keys(overrideObject)
    ]);

    keys.forEach((key) => {
      result[key] = mergePortfolioData(baseValue[key], overrideObject[key]);
    });

    return result;
  }

  return overrideValue ?? baseValue;
};

const normalizePortfolioData = (value, rawOverride = null) => {
  const normalized = deepClone(value || {});
  const overrideObject = isPlainObject(rawOverride) ? rawOverride : null;
  const legacyProfileSection =
    overrideObject && isPlainObject(overrideObject.profileSection) ? overrideObject.profileSection : null;
  const hasCareerSectionOverride =
    Boolean(overrideObject) && Object.prototype.hasOwnProperty.call(overrideObject, "careerSection");
  const legacyTimeline = Array.isArray(legacyProfileSection && legacyProfileSection.timeline)
    ? legacyProfileSection.timeline
    : [];

  if (!isPlainObject(normalized.careerSection)) {
    normalized.careerSection = {};
  }

  if (!Array.isArray(normalized.careerSection.items)) {
    normalized.careerSection.items = [];
  }

  if (!hasCareerSectionOverride && legacyTimeline.length > 0) {
    normalized.careerSection.items = deepClone(legacyTimeline);

    if (!normalized.careerSection.intro && legacyProfileSection && legacyProfileSection.timelineIntro) {
      normalized.careerSection.intro = legacyProfileSection.timelineIntro;
    }
  }

  return normalized;
};

const readStoredPortfolioData = () => {
  try {
    const raw = window.localStorage.getItem(PORTFOLIO_STORAGE_KEY);

    if (!raw) {
      return null;
    }

    const parsed = JSON.parse(raw);

    if (!parsed || typeof parsed !== "object" || Array.isArray(parsed)) {
      return null;
    }

    return normalizePortfolioData(mergePortfolioData(defaultPortfolioData, parsed), parsed);
  } catch (error) {
    console.warn("Failed to read stored portfolio data.", error);
    return null;
  }
};

window.PORTFOLIO_STORAGE_KEY = PORTFOLIO_STORAGE_KEY;
window.defaultPortfolioData = defaultPortfolioData;
window.mergePortfolioData = mergePortfolioData;
window.normalizePortfolioData = normalizePortfolioData;
window.portfolioData = readStoredPortfolioData() || normalizePortfolioData(defaultPortfolioData);
