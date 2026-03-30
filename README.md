# Portfolio Site

参考サイトの `PROFILE / SKILL / WORKS / CONTACT` の骨格を残しつつ、インフラエンジニア向けに再構成したシングルページのポートフォリオです。

## Files

- `public/index.html`: 公開ページ
- `public/assets/css/styles.css`: 公開ページのスタイル
- `public/assets/js/`: 公開ページの設定と描画処理
- `public/admin/`: 管理画面の HTML
- `public/admin/css/admin.css`: 管理画面のスタイル
- `public/admin/js/`: 管理画面の認証と編集処理
- `backend/config.py`: `.env` と環境変数の読込
- `backend/database.py`: MySQL 接続と CRUD
- `backend/security.py`: 管理者認証のハッシュ検証
- `server.py`: 静的ファイル配信、管理認証 API、MySQL DB API
- `portfolio-seed.json`: 初回データ
- `sql/mysql/init.sql`: MySQL の初期化 SQL
- `scripts/setup_ubuntu.sh`: Ubuntu Server 用のセットアップ補助
- `scripts/generate_admin_password_hash.py`: 管理者パスワードハッシュ生成
- `.env.example`: 接続設定のひな形
- `requirements.txt`: Python 依存パッケージ

## Directory Layout

```text
.
├── backend/
│   ├── config.py
│   └── database.py
├── public/
│   ├── index.html
│   ├── assets/
│   │   ├── css/
│   │   └── js/
│   └── admin/
│       ├── index.html
│       ├── editor.html
│       ├── portal.html
│       ├── css/
│       └── js/
├── scripts/
│   ├── setup_ubuntu.sh
│   └── generate_admin_password_hash.py
├── sql/
│   └── mysql/
│       └── init.sql
├── .env.example
├── requirements.txt
├── portfolio-seed.json
└── server.py
```

## まず編集する場所

通常の編集は `/admin/` から行います。`public/assets/js/site-data.js` は初期値です。
初期状態では、名前・メール・GitHub などは仮名とテストデータを入れています。
公開前に必ず実データへ差し替えてください。

- `OWNER`: 名前、肩書き、GitHub、メールなどの固定値
- `contact.links`: GitHub、職務経歴書、ブログなどの外部リンク
- 本文: 複数行文字列で書けます。段落を分けたいときは 1 行空けます
- 箇条書き: 1 行 1 項目で書けます

- `profile`: ヒーローの肩書き、タイトル、タグ、強み
- `profileSection`: 自己紹介、担当領域、資格、経歴
- `skillsSection.categories`: スキルカテゴリと補足
- `worksSection.items`: 実績ごとの役割、担当内容、成果、使用技術
- `personalSection.items`: 個人で取り組んでいること、学習、検証、発信
- `contact`: GitHub、職務経歴書、メールなど

## 実績の書き方

- アプリの制作物が少なくても、基盤更改、監視改善、自動化、標準化を「実績」として載せる
- `担当内容` には自分が受け持った範囲を書く
- `成果` には安定性、運用性、再現性がどう改善したかを書く
- `使用技術` は面接で深掘りされても説明できるものに絞る

## Local Preview

```bash
python3 -m venv .venv
. .venv/bin/activate
pip install -r requirements.txt
cp .env.example .env
python3 server.py
```

その後 `http://localhost:8000` を開いて確認できます。MySQL が未起動の状態では API は動きません。

## DB 構成

- 保存先はフロントエンドの `localStorage` ではなく、サーバー側の MySQL です
- 接続設定は `.env` または環境変数から読み込みます
- `backend/database.py` が DB とテーブルを確認します
- `PORTFOLIO_DB_AUTO_CREATE=true` のときは DB を自動作成します
- テーブルが空のときは `portfolio-seed.json` の内容で初期化されます
- 公開ページも管理画面も `portfolio-data-source.js` 経由で API を呼びます
- `public/assets/js/portfolio-config.js` は API endpoint の変更用です

### API

- 公開データ取得: `GET /api/portfolio`
- 管理画面ログイン: `POST /api/admin/login`
- 管理画面ログアウト: `POST /api/admin/logout`
- 管理セッション確認: `GET /api/admin/session`
- 管理画面データ取得: `GET /api/admin/portfolio`
- 管理画面データ保存: `PUT /api/admin/portfolio`
- 管理画面データ削除: `DELETE /api/admin/portfolio`

ログイン時のリクエストボディ:

```json
{
  "id": "admin",
  "password": "0000"
}
```

保存時のリクエストボディは次の形式を想定しています。

```json
{
  "data": {
    "locale": "ja",
    "siteTitle": "Sample Taro | Cloud Infrastructure Engineer Portfolio"
  }
}
```

レスポンスは以下のどちらでも動くようにしてあります。

```json
{
  "locale": "ja",
  "siteTitle": "Sample Taro | Cloud Infrastructure Engineer Portfolio"
}
```

```json
{
  "data": {
    "locale": "ja",
    "siteTitle": "Sample Taro | Cloud Infrastructure Engineer Portfolio"
  },
  "updatedAt": "2026-03-30T12:34:56+00:00"
}
```

## Ubuntu Server + MySQL Setup

```bash
bash scripts/setup_ubuntu.sh
cp .env.example .env
```

`.env` を編集して、Ubuntu Server 上の MySQL 接続先を設定してください。

### サーバ接続設定

Ubuntu Server 上の MySQL を使う場合は、アプリを動かすサーバーからその MySQL に到達できることが前提です。最低限、次の 4 点を揃えてください。

1. MySQL の待受設定  
   `mysqld.cnf` などで `bind-address` を接続元から到達できる IP に設定します。外部公開が不要なら、VPN や同一 VPC 内のプライベート IP に限定してください。
2. 接続用ユーザー作成  
   アプリ専用ユーザーを作成し、`portfolio_site` にだけ権限を付与します。
3. ファイアウォール開放  
   `3306/tcp` は必要な接続元だけ許可してください。全公開は避ける方が安全です。
4. アプリ側 `.env` 設定  
   `PORTFOLIO_DB_HOST` に Ubuntu Server のホスト名か IP、`PORTFOLIO_DB_USER` / `PORTFOLIO_DB_PASSWORD` に作成した認証情報を入れます。

`.env` の例:

```bash
PORTFOLIO_HOST=0.0.0.0
PORTFOLIO_PORT=8000
PORTFOLIO_ADMIN_ID=admin
PORTFOLIO_ADMIN_PASSWORD=0000

PORTFOLIO_DB_HOST=192.168.10.20
PORTFOLIO_DB_PORT=3306
PORTFOLIO_DB_NAME=portfolio_site
PORTFOLIO_DB_USER=portfolio_app
PORTFOLIO_DB_PASSWORD=strong-password
PORTFOLIO_DB_CHARSET=utf8mb4
PORTFOLIO_DB_TABLE=portfolio_documents
PORTFOLIO_DB_CONNECT_TIMEOUT=10
PORTFOLIO_DB_AUTO_CREATE=false
```

`PORTFOLIO_DB_AUTO_CREATE=false` にしておくと、本番でアプリから DB 作成を試みません。最初に管理者権限で DB とテーブルを作ってから使う構成の方が安全です。

MySQL の初期化を手動で行う場合:

```bash
mysql -u root -p < sql/mysql/init.sql
```

専用ユーザー例:

```sql
CREATE USER 'portfolio_app'@'%' IDENTIFIED BY 'change-me';
GRANT ALL PRIVILEGES ON portfolio_site.* TO 'portfolio_app'@'%';
FLUSH PRIVILEGES;
```

`PORTFOLIO_DB_AUTO_CREATE=true` のまま起動すれば、アプリ側で DB 作成も試みます。起動ユーザーに `CREATE DATABASE` 権限が無い場合は、先に `sql/mysql/init.sql` を実行してください。

起動:

```bash
.venv/bin/python server.py
```

### VS Code で `init.sql` にエラーが出る場合

`sql/mysql/init.sql` は MySQL 用です。VS Code の `mssql` 拡張は `.sql` を T-SQL として扱うため、`CREATE DATABASE IF NOT EXISTS` やバッククォート記法で誤検知します。

このリポジトリでは [.vscode/settings.json](/mnt/c/Users/Owner/Documents/GitHub/portfolio-site/.vscode/settings.json) で `sql/mysql/*.sql` を `plaintext` 扱いにして、`mssql` の誤判定を止めています。  
MySQL 用の色分けが必要なら、MySQL 対応拡張を入れたうえで VS Code の言語モードを `MySQL` に切り替えてください。

## Security

- 管理者 ID とパスワードは DB ではなく `.env` または環境変数から読み込みます
- `.env` は Git 管理対象外です
- 本番では `PORTFOLIO_ADMIN_PASSWORD` の平文保存ではなく、`PORTFOLIO_ADMIN_PASSWORD_HASH` を使う前提です
- パスワードハッシュは `scrypt` 形式で保存し、照合は [backend/security.py](/mnt/c/Users/Owner/Documents/GitHub/portfolio-site/backend/security.py) で行います
- ID とパスワードの比較は固定時間比較を使っています
- セッション cookie には `HttpOnly` と `SameSite=Lax` を付けています
- HTTPS 配下では `PORTFOLIO_COOKIE_SECURE=true` にして `Secure` cookie を有効化できます

ハッシュ生成:

```bash
python3 scripts/generate_admin_password_hash.py
```

生成された文字列を `.env` の `PORTFOLIO_ADMIN_PASSWORD_HASH` に設定し、`PORTFOLIO_ADMIN_PASSWORD` は空にするか開発用ダミー値へ変更してください。

現状の制約:

- ログイン試行回数のレート制限は未実装です
- セッションはプロセス内メモリ保持なので、サーバ再起動で失効します
- 複数台構成でセッション共有はしていません
- `.env` 自体が漏えいすると認証情報ハッシュや DB 接続情報も漏れます

## RDB 正規化方針

現状の `portfolio_documents` は、ポートフォリオ全体を JSON 文字列 1 件で保持する運用重視の構成です。これは実装は単純ですが、RDB の正規化という観点では非正規化です。

将来 MySQL を RDB として正しく育てるなら、次のように分割するのが基本です。

- `admin_users`: 管理者アカウント
- `admin_sessions`: ログインセッション
- `site_profiles`: サイトの基本プロフィール
- `contact_links`: 連絡先リンク
- `skill_categories`: スキルカテゴリ
- `skills`: 個別スキル
- `work_items`: 実績
- `technologies`: 技術マスタ
- `work_item_technologies`: 実績と技術の中間テーブル
- `personal_items`: 個人活動

正規形の考え方:

1. 第1正規形  
   1 カラム 1 値にします。JSON 配列や改行区切り文字列で複数値を持たせず、`contact_links` や `skills` のように行へ分解します。
2. 第2正規形  
   複合主キーの一部にだけ依存する列を分離します。たとえば `work_item_technologies` に技術名そのものを持たせず、技術名は `technologies` に寄せます。
3. 第3正規形  
   主キー以外の列に対する従属を排除します。たとえば `skills` にカテゴリ名を重複保持せず、カテゴリ情報は `skill_categories` に持たせて `category_id` で参照します。
4. BCNF に近づける考え方  
   決定項が候補キーだけになるようにします。`admin_users.login_id` を一意制約にし、`login_id` から別の属性が決まるなら主キー同等に扱えるよう整理します。

テキストベースの論理モデル例:

```text
[admin_users]
- user_id (PK)
- login_id (UNIQUE)
- password_hash
- is_active
- created_at
- updated_at
    |
    | 1 : n
    |
[admin_sessions]
- session_id (PK)
- user_id (FK -> admin_users.user_id)
- expires_at
- created_at
- revoked_at


[site_profiles]
- profile_id (PK)
- locale
- site_title
- owner_name
- owner_role
- hero_title
- hero_summary
- updated_at
    |
    | 1 : n
    |
[contact_links]
- link_id (PK)
- profile_id (FK -> site_profiles.profile_id)
- link_type
- label
- url
- sort_order
- is_public


[site_profiles]
- profile_id (PK)
    |
    | 1 : n
    |
[skill_categories]
- category_id (PK)
- profile_id (FK -> site_profiles.profile_id)
- category_name
- description
- sort_order
    |
    | 1 : n
    |
[skills]
- skill_id (PK)
- category_id (FK -> skill_categories.category_id)
- skill_name
- proficiency_label
- note
- sort_order


[site_profiles]
- profile_id (PK)
    |
    | 1 : n
    |
[work_items]
- work_item_id (PK)
- profile_id (FK -> site_profiles.profile_id)
- title
- role
- summary
- outcome
- period_start
- period_end
- sort_order
    |
    | 1 : n
    |
[work_item_technologies]
- work_item_id (FK -> work_items.work_item_id)
- technology_id (FK -> technologies.technology_id)
- PRIMARY KEY (work_item_id, technology_id)
    |
    | n : 1
    |
[technologies]
- technology_id (PK)
- technology_name (UNIQUE)
- technology_type


[site_profiles]
- profile_id (PK)
    |
    | 1 : n
    |
[personal_items]
- personal_item_id (PK)
- profile_id (FK -> site_profiles.profile_id)
- title
- description
- sort_order
```

この形にすると、検索・並び替え・集計・部分更新がしやすくなります。逆に、今の JSON 1 件保存は実装は速いですが、技術別検索や実績単位の更新には不向きです。

`.env.example` は単なる変数一覧ではなく、実際にどう書くかの記入例として使えるようにしています。Ubuntu Server の MySQL を使う場合は [.env.example](/mnt/c/Users/Owner/Documents/GitHub/portfolio-site/.env.example) をベースに、`PORTFOLIO_DB_HOST`、`PORTFOLIO_DB_USER`、`PORTFOLIO_DB_PASSWORD`、`PORTFOLIO_ADMIN_PASSWORD_HASH` を自分の環境値へ置き換えてください。

## Admin

- 管理画面は `/admin/` です
- 例: `http://localhost:8000/admin/`
- 簡易ログイン: `ID: admin` / `PW: 0000`
- 本番では `PORTFOLIO_ADMIN_PASSWORD_HASH` の利用を推奨します
- ログイン状態はサーバーセッション cookie で管理します
- 編集内容は MySQL に保存され、ブラウザ保存は使用しません

## Environment Variables

```bash
PORTFOLIO_HOST=0.0.0.0
PORTFOLIO_PORT=8000
PORTFOLIO_ADMIN_ID=admin
PORTFOLIO_ADMIN_PASSWORD=0000
PORTFOLIO_ADMIN_PASSWORD_HASH=
PORTFOLIO_COOKIE_SECURE=false
PORTFOLIO_DB_HOST=127.0.0.1
PORTFOLIO_DB_PORT=3306
PORTFOLIO_DB_NAME=portfolio_site
PORTFOLIO_DB_USER=portfolio_app
PORTFOLIO_DB_PASSWORD=change-me
PORTFOLIO_DB_CHARSET=utf8mb4
PORTFOLIO_DB_TABLE=portfolio_documents
PORTFOLIO_DB_CONNECT_TIMEOUT=10
PORTFOLIO_DB_AUTO_CREATE=true
```
