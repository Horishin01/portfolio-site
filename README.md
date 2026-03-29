# Portfolio Site

参考サイトの `PROFILE / SKILL / WORKS / CONTACT` の骨格を残しつつ、インフラエンジニア向けに再構成したシングルページのポートフォリオです。

## Files

- `index.html`: ページ構造
- `styles.css`: 配色、レイアウト、レスポンシブ対応
- `site-data.js`: 名前、プロフィール、スキル、実績、連絡先
- `script.js`: `site-data.js` の内容を描画する処理

## まず編集する場所

通常の編集は `/admin/` から行います。`site-data.js` は初期値です。

- `OWNER`: 名前、肩書き、GitHub、メールなどの固定値
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

## Preview

```bash
python3 -m http.server 8000
```

その後 `http://localhost:8000` を開いて確認できます。

## Admin

- 管理画面は `/admin/` です
- 例: `http://localhost:8000/admin/`
- 簡易ログイン: `ID: admin` / `PW: 0000`
- 保存内容はブラウザの `localStorage` に入ります
- 静的サイトなので、別ブラウザや別端末に自動反映する仕組みはありません
- この認証はフロントエンドだけの簡易実装なので、本番用途ではありません
