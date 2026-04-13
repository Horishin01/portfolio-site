# Portfolio Site

ASP.NET Core 8 の MVC 構成で動くポートフォリオサイトです。公開ページは Razor View でサーバー描画し、管理画面は `ASP.NET Core Identity` で保護しています。管理 URL は `/admin` のまま維持し、公開コンテンツは JSON ファイルではなく DB テーブルへ保存します。

運用前提は次のとおりです。

- 開発環境: SQLite `LocalData/Debug/portfolio-site.dev.db` + `dotnet user-secrets`
- 本番想定: MySQL + `systemd EnvironmentFile`
- `appsettings.json`: repo に含めてもよいダミー値だけを置く
- 秘密情報: repo に置かず、開発は `user-secrets`、サーバーは env ファイルで管理

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
├── deploy/
│   └── ubuntu/
│       ├── apache-portfolio-site.conf.example
│       ├── nginx-portfolio-site.conf.example
│       └── portfolio-site.service.example
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
├── scripts/
│   ├── add-migration.sh
│   ├── create-runtime-env.sh
│   └── stop-portfolio-site.ps1
├── portfolio-site.sln
├── PortfolioSite.csproj
└── Program.cs
```

## Runtime

- 公開ページ: `/`
- 管理ポータル: `/admin`
- 管理ログイン: `/admin/login`

`appsettings*.json`、`user-secrets`、環境変数を合わせて DB 接続と管理者設定を読み込みます。初回起動時に `Administrator` ロールと管理者ユーザーを自動作成します。

## Configuration

- `AdminAccount:LoginId`
  `/admin/login` で使う管理ログイン ID です。未指定なら `Admin` です。
- `AdminAccount:Password`
  管理者の平文パスワードです。起動時にアプリ側でハッシュ化して保存します。未指定なら `0000` を使います。必要なら `user-secrets`、env ファイル、環境変数で上書きできます。
- `ConnectionStrings:PortfolioDatabase`
  開発では SQLite、その他の環境では MySQL を想定します。repo 上の `appsettings.json` にはダミー値を置き、実値は開発では `user-secrets`、本番では `ConnectionStrings__PortfolioDatabase` で渡してください。
- `ReverseProxy:KnownProxies`
  `X-Forwarded-*` を信頼する必要があるリバースプロキシの IP をカンマ区切りで指定します。同一ホスト上の Nginx または Apache から `127.0.0.1` 経由で流す構成なら通常は不要です。
- `ReverseProxy:KnownNetworks`
  信頼するリバースプロキシのネットワークを CIDR でカンマ区切り指定します。例: `10.0.0.0/24`

## Development Run

VS Code の `F5` と `PortfolioSite` の launch profile は `Development` で起動します。このとき保存先は SQLite `LocalData/Debug/portfolio-site.dev.db` です。ローカル MySQL は不要です。

未設定のまま起動すると、管理ログインは `Admin / 0000` になります。必要なら `user-secrets` で上書きします。

上書きしたい場合:

```bash
dotnet user-secrets set "AdminAccount:Password" "ChangeThisForDev123!"
```

管理ログイン ID も変えたい場合:

```bash
dotnet user-secrets set "AdminAccount:LoginId" "Admin"
```

開発でも MySQL を使いたい場合だけ、接続文字列も `user-secrets` へ入れます。SQLite のままなら不要です。

```bash
dotnet user-secrets set "ConnectionStrings:PortfolioDatabase" "Server=127.0.0.1;Port=3306;Database=portfolio_site;User ID=portfolio_app;Password=change-me;CharSet=utf8mb4;"
```

```bash
dotnet restore
dotnet build
dotnet run --launch-profile PortfolioSite --project PortfolioSite.csproj
```

起動後の確認先:

- `http://localhost:5078/`
- `http://localhost:5078/admin/login`

開発環境では起動時に必要なフォルダを自動作成し、SQLite と Identity テーブルも migration 経由で初期化します。過去に `EnsureCreated` で作った SQLite DB でも、現在スキーマと一致していれば migration 履歴を自動ベースライン化してから起動します。旧 JSON スキーマの SQLite ファイルが残っている場合は、一度作り直して新スキーマへ揃えます。

## Schema Management

DB スキーマの正本は `Migrations/` です。今後の列追加やテーブル追加では、`EnsureCreated` や手動 `ALTER TABLE` ではなく、必ず migration を追加してください。

基本ルール:

- アプリ起動時の DB 更新は `Database.MigrateAsync()` のみを使います
- SQLite と MySQL の両方で同じ migration セットを適用します
- 開発 DB が既存でも、migration 履歴がなければ起動時に自動でベースライン化します
- これ以降、スキーマ差分を埋めるためのアプリ側手書き SQL は増やさない前提です

推奨手順:

1. `Data/` と `Models/` のエンティティを変更する
2. `bash scripts/add-migration.sh <MigrationName>` で migration を追加する
3. `dotnet build PortfolioSite.csproj` でビルド確認する
4. ローカル起動して SQLite に自動適用されることを確認する
5. 本番は再起動時に MySQL へ自動適用される

`scripts/add-migration.sh` は既定で `mysql` を design-time provider に使います。これで migration snapshot の provider 依存差分がぶれにくくなります。

例:

```bash
bash scripts/add-migration.sh AddProjectTags
```

必要なら provider を明示できます。

```bash
bash scripts/add-migration.sh AddProjectTags sqlite
dotnet ef database update --project PortfolioSite.csproj --context PortfolioDbContext -- --provider sqlite
```

## Production / MySQL Run

1. MySQL で DB とユーザーを作成します。
2. 実行用 env ファイルを shell スクリプトで作成します。
3. env ファイルを読み込んでからアプリを起動します。

`create-runtime-env.sh` の起動方法:

```bash
bash scripts/create-runtime-env.sh
```

出力先を明示したい場合:

```bash
bash scripts/create-runtime-env.sh ./.secrets/portfolio-site.env
```

この script で入力する項目:

- `ASPNETCORE_ENVIRONMENT`
  本番では通常 `Production`
- `ASPNETCORE_URLS`
  Nginx または Apache の背後で動かすなら通常 `http://127.0.0.1:5000`
- `ConnectionStrings__PortfolioDatabase`
  接続先 MySQL の接続文字列
- `Configure admin credentials?`
  `yes` なら `/admin/login` 用の ID とパスワードを続けて入力
  `no` なら `Admin / 0000` を自動設定
- `ReverseProxy__KnownProxies`
  必要な場合だけ、信頼するリバースプロキシ IP をカンマ区切りで指定
- `ReverseProxy__KnownNetworks`
  必要な場合だけ、信頼するリバースプロキシネットワークを CIDR のカンマ区切りで指定

入力時の注意:

- 値は `"..."` で囲まず、そのまま入力します
- `ConnectionStrings__PortfolioDatabase` には MySQL の接続文字列をそのまま入れます
- 接続文字列の例:
  `Server=127.0.0.1;Port=3306;Database=portfolio_site;User ID=portfolio_app;Password=change-me;CharSet=utf8mb4;`

```bash
bash scripts/create-runtime-env.sh
set -a
. ./.secrets/portfolio-site.env
set +a
dotnet run --project PortfolioSite.csproj
```

MySQL を使う環境では、アプリ起動時に EF Core migration を自動適用します。DB 自体の作成はアプリでは行わないので、先に MySQL 側で作成してください。

この運用では、repo に置くのはコードと migration だけです。DB 接続文字列や管理者パスワードは env ファイルへ分離します。

## Ubuntu Server Deploy

Ubuntu Server は公開用の実行環境として使う前提です。サーバー上で開発やシェル運用をする想定はなく、必要なのは `systemd + Nginx または Apache + .NET 8 Runtime` と DB 接続情報だけです。アプリ側は `X-Forwarded-*` を解釈するため、Nginx / Apache 配下でも `https` 判定が崩れません。

サーバーでやることを順番に書くと次のとおりです。

1. 配置先の MySQL で DB とユーザーを作成します。MySQL が別ホストでも構いません。
2. 手元の開発マシンで release publish を作成します。Ubuntu 側で `dotnet publish` や SDK は不要です。
3. publish 済みファイル一式を Ubuntu の任意ディレクトリへ配置します。
4. `www-data` など実行ユーザーがアプリ配置先を読めること、必要なら `wwwroot/uploads/` に書き込めることを確認します。
5. `bash scripts/create-runtime-env.sh /etc/portfolio-site/portfolio-site.env` で env ファイルを作成し、秘密情報をそこへ保存します。
6. `deploy/ubuntu/portfolio-site.service.example` を `/etc/systemd/system/portfolio-site.service` として配置します。
7. Nginx を使うなら `deploy/ubuntu/nginx-portfolio-site.conf.example` を、Apache を使うなら `deploy/ubuntu/apache-portfolio-site.conf.example` をコピーして `server_name` または `ServerName` を変更します。
8. `systemctl daemon-reload` と `systemctl enable --now portfolio-site` を実行します。
9. Nginx なら `nginx -t` の後に `systemctl reload nginx`、Apache なら `apache2ctl configtest` の後に `systemctl reload apache2` を実行します。

サーバーで script 実行時に入力する内容:

- `ASPNETCORE_ENVIRONMENT`
  `Production`
- `ASPNETCORE_URLS`
  `http://127.0.0.1:5000`
- `ConnectionStrings__PortfolioDatabase`
  本番 MySQL への接続文字列
- `Configure admin credentials?`
  `yes` なら管理画面ログイン ID とパスワードを入力
  `no` なら `Admin / 0000`
- `ReverseProxy__KnownProxies`
  同一サーバー内の Nginx または Apache で受けるなら通常は空欄
- `ReverseProxy__KnownNetworks`
  同一サーバー内の Nginx または Apache で受けるなら通常は空欄

最初にサーバーへ入れておくもの:

- `.NET 8 Runtime`
- `nginx` または `apache2`
- `mysql-client` など接続確認用の CLI

systemd 反映の流れ:

```bash
sudo mkdir -p /etc/portfolio-site
cd /var/www/portfolio-site/current
sudo bash scripts/create-runtime-env.sh /etc/portfolio-site/portfolio-site.env
sudo chown root:root /etc/portfolio-site/portfolio-site.env
sudo chmod 600 /etc/portfolio-site/portfolio-site.env
sudo cp deploy/ubuntu/portfolio-site.service.example /etc/systemd/system/portfolio-site.service
sudo systemctl daemon-reload
sudo systemctl enable --now portfolio-site
sudo systemctl status portfolio-site
```

Nginx 反映の流れ:

```bash
cd /var/www/portfolio-site/current
sudo cp deploy/ubuntu/nginx-portfolio-site.conf.example /etc/nginx/sites-available/portfolio-site.conf
sudo ln -sf /etc/nginx/sites-available/portfolio-site.conf /etc/nginx/sites-enabled/portfolio-site.conf
sudo nginx -t
sudo systemctl reload nginx
```

Apache 反映の流れ:

```bash
cd /var/www/portfolio-site/current
sudo a2enmod proxy proxy_http headers
sudo cp deploy/ubuntu/apache-portfolio-site.conf.example /etc/apache2/sites-available/portfolio-site.conf
sudo a2ensite portfolio-site.conf
sudo apache2ctl configtest
sudo systemctl reload apache2
```

Apache を使う場合は `mod_proxy_http` が `X-Forwarded-For` と `X-Forwarded-Host` を付与し、サンプル設定で `X-Forwarded-Proto` も付けています。公開側を HTTPS にする場合も `RequestHeader set X-Forwarded-Proto expr=%{REQUEST_SCHEME}` を残してください。

更新時にやること:

1. 手元で `dotnet publish` を作り直す
2. サーバーへ差し替える
3. migration を含めてアプリを再起動するため `sudo systemctl restart portfolio-site`
4. `sudo journalctl -u portfolio-site -n 100 --no-pager` で起動確認

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

systemd / リバースプロキシのサンプル:

- `deploy/ubuntu/portfolio-site.service.example`
- `deploy/ubuntu/nginx-portfolio-site.conf.example`
- `deploy/ubuntu/apache-portfolio-site.conf.example`

別コンテナや別ホストのプロキシから転送する場合は、必要に応じて信頼する IP または CIDR を明示してください。

```bash
export ReverseProxy__KnownProxies="10.0.0.5,10.0.0.6"
export ReverseProxy__KnownNetworks="10.0.0.0/24"
```

`TrustAllProxies` は安全上の理由でサポートしていません。

## MySQL Example

```sql
CREATE DATABASE portfolio_site CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;

CREATE USER 'portfolio_app'@'%' IDENTIFIED BY 'change-me';
GRANT ALL PRIVILEGES ON portfolio_site.* TO 'portfolio_app'@'%';
FLUSH PRIVILEGES;
```

接続文字列の例:

```bash
ConnectionStrings__PortfolioDatabase="Server=127.0.0.1;Port=3306;Database=portfolio_site;User ID=portfolio_app;Password=change-me;CharSet=utf8mb4;"
```

本番では `scripts/create-runtime-env.sh` で env ファイルを作り、`ConnectionStrings__PortfolioDatabase` をそこへ保存する構成を想定しています。

管理者設定の例:

```bash
AdminAccount__LoginId="Admin"
AdminAccount__Password="0000"
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
- `.secrets/` 配下の env ファイルはローカル専用で、Git には含めません
- `/admin` の保護は Identity の Cookie 認証で行い、失敗 5 回で 15 分ロックされます
- 管理者情報と DB 接続は env ファイルまたは環境変数で上書きできます
- 管理画面は JSON import / export ではなく、各項目をフォーム編集して保存する構成です
