# Portfolio Site

ASP.NET Core 8 の標準的な MVC 構成へ整理したポートフォリオサイトです。公開ページは Razor View でサーバー描画し、見た目は既存の `HTML / CSS` を維持しています。管理画面は `ASP.NET Core Identity` で保護し、URL は `/admin` のまま維持しています。保存先は MySQL です。

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
│   ├── Identity/
│   └── Options/
├── Services/
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

`appsettings*.json` には DB 接続だけを置き、管理者情報は環境変数で渡します。初回起動時に Identity の管理者ユーザーと `Administrator` ロールを自動作成します。

## Local Run

1. MySQL で DB とユーザーを作成します。
2. `appsettings.json` または環境変数で接続文字列を設定します。
3. 管理者情報の環境変数を設定します。
4. 起動します。

```bash
export ADMIN_LOGIN_ID=admin
export ADMIN_PASSWORD_HASH='generated-hash'
dotnet restore
dotnet build
dotnet run --project PortfolioSite.csproj
```

`launchSettings.json` で `Development` が有効になるため、通常の `dotnet run` で開発設定を使えます。開発環境では `appsettings.Development.json` により SQLite ファイル `LocalData/Debug/portfolio-site.dev.db` を使うため、ローカル MySQL がなくても起動できます。起動時には必要なフォルダごと自動生成し、DB と Identity テーブルも初期化されます。

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

管理者パスワードのハッシュは次で生成できます。生成される値は `ASP.NET Core Identity` 互換です。

```bash
dotnet run --project PortfolioSite.csproj -- hash-password "your-password"
```

生成した値を `ADMIN_PASSWORD_HASH` へ設定してください。`ADMIN_LOGIN_ID` は省略時 `admin` で、必要なら `/admin/login` に使うログイン ID を上書きできます。

## Data Flow

- DB テーブルは `portfolio_contents` 1 テーブルです
- 保存内容はポートフォリオ全体の JSON ドキュメントです
- 初回起動時に migration を適用し、Identity の管理者ユーザーとロールを作成します
- 保存データが空なら `portfolio-seed.json` を投入します
- 管理画面では JSON をそのまま編集、import、export、reset できます

## Notes

- 旧 `public/` 配下の静的 HTML と API 用 JavaScript は削除し、`Views/` と `wwwroot/` へ統合しています
- API エンドポイントは持ちません
- DB 自体の作成はアプリでは行いません。事前に MySQL 側で作成してください
- `/admin` の保護は Identity の Cookie 認証で行い、失敗 5 回で 15 分ロックされます
- 管理者情報は `appsettings*.json` には置かず、`ADMIN_LOGIN_ID` と `ADMIN_PASSWORD_HASH` の環境変数で渡します
