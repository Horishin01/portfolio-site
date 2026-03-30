# Portfolio Site

ASP.NET Core 8 の標準的な MVC 構成へ整理したポートフォリオサイトです。公開ページは Razor View でサーバー描画し、見た目は既存の `HTML / CSS` を維持しています。管理画面は Cookie 認証付きのコントローラー + フォーム送信で動き、保存先は MySQL です。

## Structure

```text
.
├── Controllers/
│   ├── AdminController.cs
│   └── HomeController.cs
├── Data/
│   ├── PortfolioContentRecord.cs
│   ├── PortfolioDbContext.cs
│   └── PortfolioDbContextFactory.cs
├── Migrations/
├── Models/
│   ├── Content/
│   └── Options/
├── Services/
│   ├── AdminCredentialService.cs
│   ├── PasswordHashService.cs
│   ├── PortfolioContentService.cs
│   └── PortfolioDbInitializer.cs
├── ViewModels/
├── Views/
│   ├── Admin/
│   ├── Home/
│   └── Shared/
├── wwwroot/
│   ├── css/
│   └── js/
├── appsettings.json
├── appsettings.Development.json
├── portfolio-seed.json
├── PortfolioSite.csproj
└── Program.cs
```

## Runtime

- 公開ページ: `/`
- 管理ポータル: `/admin`
- 管理ログイン: `/admin/login`

開発用の初期ログインは `admin / 0000` です。公開前に `AdminAccount:PasswordHash` を差し替えてください。

## Local Run

1. MySQL で DB とユーザーを作成します。
2. `appsettings.json` または環境変数で接続文字列を設定します。
3. 起動します。

```bash
dotnet restore
dotnet build
dotnet run --project PortfolioSite.csproj
```

`launchSettings.json` で `Development` が有効になるため、通常の `dotnet run` で開発設定を使えます。

## MySQL Example

```sql
CREATE DATABASE portfolio_site CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE USER 'portfolio_app'@'%' IDENTIFIED BY 'change-me';
GRANT ALL PRIVILEGES ON portfolio_site.* TO 'portfolio_app'@'%';
FLUSH PRIVILEGES;
```

接続文字列の例:

```json
{
  "ConnectionStrings": {
    "PortfolioDatabase": "Server=127.0.0.1;Port=3306;Database=portfolio_site;User ID=portfolio_app;Password=change-me;CharSet=utf8mb4;"
  }
}
```

本番では環境変数 `ConnectionStrings__PortfolioDatabase` で上書きする構成を想定しています。

## Password Hash

管理者パスワードのハッシュは次で生成できます。

```bash
dotnet run --project PortfolioSite.csproj -- hash-password "your-password"
```

生成した値を `AdminAccount:PasswordHash` へ設定してください。

## Data Flow

- DB テーブルは `portfolio_contents` 1 テーブルです
- 保存内容はポートフォリオ全体の JSON ドキュメントです
- 初回起動時に migration を適用し、保存データが空なら `portfolio-seed.json` を投入します
- 管理画面では JSON をそのまま編集、import、export、reset できます

## Notes

- 旧 `public/` 配下の静的 HTML と API 用 JavaScript は削除し、`Views/` と `wwwroot/` へ統合しています
- API エンドポイントは持ちません
- DB 自体の作成はアプリでは行いません。事前に MySQL 側で作成してください
