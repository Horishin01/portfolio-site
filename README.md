# Portfolio Site

ASP.NET Core 8 の MVC 構成で動くポートフォリオサイトです。公開ページは Razor View でサーバー描画し、管理画面は `ASP.NET Core Identity` で保護しています。管理 URL は `/admin` のまま維持し、公開コンテンツは JSON ファイルではなく DB テーブルへ保存します。

運用前提は次のとおりです。

- 開発環境: SQLite `LocalData/Debug/portfolio-site.dev.db`
- 本番想定: MySQL
- 管理者情報: `appsettings.json` の `AdminAccount`
- アプリ設定ファイル: DB 接続文字列と管理者設定

## Structure

```text
.
├── Controllers/
│   ├── AdminController.cs
│   └── HomeController.cs
├── Data/
│   ├── PortfolioContentCollections.cs
│   ├── PortfolioContentRecord.cs
│   ├── PortfolioDatabaseOptions.cs
│   ├── PortfolioDbContext.cs
│   └── PortfolioDbContextFactory.cs
├── Migrations/
├── Models/
│   ├── Content/
│   │   └── PortfolioDefaults.cs
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
├── LocalData/
├── appsettings.json
├── appsettings.Development.json
├── portfolio-site.sln
├── PortfolioSite.csproj
└── Program.cs
```

## Runtime

- 公開ページ: `/`
- 管理ポータル: `/admin`
- 管理ログイン: `/admin/login`

`appsettings*.json` で DB 接続と管理者設定を読み込みます。初回起動時に `Administrator` ロールと管理者ユーザーを自動作成します。

## Configuration

- `AdminAccount:LoginId`
  `/admin/login` で使う管理ログイン ID です。未指定なら `admin` です。
- `AdminAccount:Password`
  管理者の平文パスワードです。起動時にアプリ側でハッシュ化して保存します。未指定なら `0000` です。
- `ConnectionStrings:PortfolioDatabase`
  開発では SQLite、その他の環境では MySQL を想定します。
- `ReverseProxy:TrustAllProxies`
  Docker や別ホストのリバースプロキシから `X-Forwarded-*` を受ける場合に `true` を設定します。同一ホスト上の Nginx から `127.0.0.1` 経由で流す構成なら通常は不要です。

## Development Run

VS Code の `F5` と `PortfolioSite` の launch profile は `Development` で起動します。このとき保存先は SQLite `LocalData/Debug/portfolio-site.dev.db` です。ローカル MySQL は不要です。

1. `appsettings.json` の `AdminAccount` を確認します。
2. 起動します。

```bash
dotnet restore
dotnet build
dotnet run --launch-profile PortfolioSite --project PortfolioSite.csproj
```

起動後の確認先:

- `http://localhost:5078/`
- `http://localhost:5078/admin/login`

開発環境では起動時に必要なフォルダを自動作成し、SQLite と Identity テーブルも初期化します。旧 JSON スキーマの SQLite ファイルが残っている場合は、一度作り直して新スキーマへ揃えます。

## Production / MySQL Run

1. MySQL で DB とユーザーを作成します。
2. 接続文字列を `appsettings.json` または `ConnectionStrings__PortfolioDatabase` で設定します。
3. `appsettings.json` の `AdminAccount` を設定します。
4. アプリを起動します。

```bash
dotnet run --project PortfolioSite.csproj
```

MySQL を使う環境では、アプリ起動時に EF Core migration を自動適用します。DB 自体の作成はアプリでは行わないので、先に MySQL 側で作成してください。

## Ubuntu Server Deploy

Ubuntu Server は公開用の実行環境として使う前提です。サーバー上で開発やシェル運用をする想定はなく、必要なのは `systemd + Nginx + .NET 8 Runtime` と DB 接続情報だけです。アプリ側は `X-Forwarded-*` を解釈するため、Nginx 配下でも `https` 判定が崩れません。

1. 配置先の MySQL で DB とユーザーを作成します。MySQL が別ホストでも構いません。
2. 手元の開発マシンで release publish を作成します。Ubuntu 側で `dotnet publish` や SDK は不要です。
3. publish 済みファイル一式を Ubuntu の任意ディレクトリへ配置します。
4. `www-data` など実行ユーザーがアプリ配置先を読めること、必要なら `wwwroot/uploads/` に書き込めることを確認します。
5. `deploy/ubuntu/portfolio-site.service.example` を `/etc/systemd/system/portfolio-site.service` として配置し、DB 接続文字列と管理者情報を実値へ置き換えます。
6. `deploy/ubuntu/nginx-portfolio-site.conf.example` を Nginx 設定へコピーして `server_name` を変更します。
7. `systemctl daemon-reload` と `systemctl enable --now portfolio-site` を実行します。
8. `nginx -t` の後に `systemctl reload nginx` を実行します。

ローカル publish 例:

```bash
dotnet restore
dotnet publish PortfolioSite.csproj -c Release -o ./artifacts/portfolio-site
```

生成した `./artifacts/portfolio-site` の内容を Ubuntu 側の `/var/www/portfolio-site/current` などへ配置してください。

サーバー初回起動時の挙動:

- 接続先 DB に EF Core migration を自動適用
- `AdminAccount` の値で管理者ロールと管理者ユーザーを自動作成
- 公開コンテンツが空なら初期データを投入

systemd / Nginx のサンプル:

- `deploy/ubuntu/portfolio-site.service.example`
- `deploy/ubuntu/nginx-portfolio-site.conf.example`

別コンテナや別ホストのプロキシから転送する場合は、必要に応じて次を設定してください。

```bash
export ReverseProxy__TrustAllProxies=true
```

この設定は信頼できる内部ネットワーク上のリバースプロキシに限定して使ってください。

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

管理者設定の例:

```json
{
  "AdminAccount": {
    "LoginId": "admin",
    "Password": "0000"
  }
}
```

## Data Model

公開コンテンツは次のテーブルへ保存します。

- `portfolio_contents`
- `portfolio_profile_highlights`
- `portfolio_career_items`
- `portfolio_skill_categories`
- `portfolio_skill_items`
- `portfolio_work_items`
- `portfolio_personal_items`
- `portfolio_contact_links`

認証系は `AspNetUsers`、`AspNetRoles` などの `Identity` テーブルを使います。

初回起動時の挙動:

- DB が空なら [PortfolioDefaults.cs](./Models/Content/PortfolioDefaults.cs) の初期データを投入
- 管理者ロールと管理者ユーザーを自動作成
- 管理画面のフォーム送信内容を DB に直接保存

旧 JSON 保存から移行する場合は migration `NormalizePortfolioContent` が `portfolio_contents.JsonContent` から新スキーマへ移し替えます。通常運用では JSON ファイルも JSON blob 保存も使いません。

## Notes

- 旧 `public/` 配下の静的 HTML と API 用 JavaScript は削除し、`Views/` と `wwwroot/` へ統合しています
- API エンドポイントは持ちません
- `LocalData/` 配下の開発用 DB はローカル専用で、Git には含めません
- `/admin` の保護は Identity の Cookie 認証で行い、失敗 5 回で 15 分ロックされます
- 管理者情報は `appsettings.json` の `AdminAccount` で管理します
- 管理画面は JSON import / export ではなく、各項目をフォーム編集して保存する構成です
