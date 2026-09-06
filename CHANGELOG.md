# 更新履歴 (Changelog)

AutoStructureMaker のすべての重要な変更は、本ファイルに記録されます。  
本プロジェクトは [Keep a Changelog](https://keepachangelog.com/ja/1.0.0/) および [Semantic Versioning](https://semver.org/lang/ja/) に準拠しています。

---

## [2.0.0] - 2026-09-06 (Major Remake)

### 🌟 追加 (Added)
- **公式ドキュメント体系の完全整備**:
  - [**README.md**](README.md): プロジェクト概要、バッジ、クイックスタート、基本操作マニュアル、アーキテクチャ概要、公式ドキュメント索引。
  - [**MODULES.md**](MODULES.md): 全操作（Add, Delete, Boolean, Margin, ConvertHighRes）の詳細リファレンスマニュアル、解像度モデル（STD / HIGH）、DICOM Type 一覧、⚡ Auto-Align シーケンス。
  - [**TEMPLATES.md**](TEMPLATES.md): XML 構造化テンプレート仕様、メタデータ定義、各操作タグ詳細スキーマ、レガシー CSV 構文、部位別実践プロトコル例（前立腺癌、肺SBRT、頭頸部、乳房）。
  - [**ARCHITECTURE.md**](ARCHITECTURE.md): システム設計思想、Mermaid 全体構造図・実行パイプライン図、MVVM パターン詳細、ゼロクラッシュ例外安全原則、単体テスト基盤。
  - [**COMMISSIONING.md**](COMMISSIONING.md): AAPM TG-275 / MPPG 5.a 準拠の臨床受入試験手順書、幾何学的精度・論理演算・⚡ Auto-Align 検証プロトコル、受入承認署名票。
  - [**TROUBLESHOOTING.md**](TROUBLESHOOTING.md): Script Approval、Write-Access 権限、重複エラー、参照エラー、UNC パス遅延対策（1秒タイムアウト）、FAQ。
  - [**CONTRIBUTING.md**](CONTRIBUTING.md): 開発環境構築手順、MSBuild x64 ビルド・単体テストコマンド、コーディング規約、新規操作追加手順、PR ガイドライン。
  - [**CHANGELOG.md**](CHANGELOG.md): 本更新履歴文書。
- **統合操作カード UI (Card-based Pipeline UI)**:
  - 旧来の 4 分割個別タブから、Add・Delete・Boolean・Margin・ConvertHighRes を同一リスト上で直感的に順序制御可能なパイプライン型カード UI へ刷新。
  - ドラッグ＆ドロップおよび「▲ 上へ」「▼ 下へ」ボタンによる直感的なステップ並べ替え機能。
  - 操作カードごとの有効/無効トグル（チェックボックス）および削除機能。
- **ハイブリッド輪郭解像度バッジシステム (Structure Resolution Badges)**:
  - 輪郭ドロップダウン（ComboBox）の各アイテムに、既存輪郭の解像度バッジ（`[STD]` 青色 / `[HIGH]` オレンジ色）を表示。
  - 未登録の新規作成予定輪郭に対しても、先行ステップの定義をリアルタイム解析し `[NEW] [STD]` または `[NEW] [HIGH]` を動的に付与。
- **高分解能自動変換機能 (⚡ Boolean Auto-Align)**:
  - 標準分解能（STD: 256×256）と高分解能（HIGH: 512×512）の輪郭同士で Boolean 演算を行う際、低分解能側の輪郭を高分解能へ自動事前変換（`CanConvertToHighResolution` / `ConvertToHighResolution`）して演算を実行。
  - 異なる解像度による ESAPI 内部例外クラッシュをゼロ化。
- **先行ステップ輪郭の自動伝播 (Context Propagation)**:
  - パイプライン内で先行して新規作成（Add, Boolean, Margin 等）される輪郭名を、後続カードの ComboBox 選択肢へ自動的に伝播・供給。
  - テンプレート読み込み時や連続した論理演算（例: Target を Add してすぐに Margin を作成し、それを Boolean で差分演算）の選択をスムーズに支援。
- **構造化 XML テンプレートエンジン**:
  - 輪郭操作パイプラインを階層的・意味論的に記述可能な XML テンプレート形式（`Protocol` / `Operation`）のサポート。
  - レガシー CSV テンプレート（Add/Del, Boolean, Margin）の自動読み込み・双方向変換互換性の維持。
- **施設設定ファイル (`AutoStructureMaker.config.xml`)**:
  - 院内共有ファイルサーバー（UNC パス: `\\Server\Share\...`）上のテンプレートフォルダを一括参照・集中管理。
  - ネットワークオフライン時でも UI がフリーズしない非同期 1 秒高速タイムアウト機構を実装。
- **包括的自動単体テスト基盤 (`AutoStructureMaker.Tests`)**:
  - ESAPI バイナリに直接依存しないドメインロジック（ViewModels, Parsing, Context Propagation, PreFlight Validation）を徹底分離。
  - 全 40 件の自動テストを完備し、CI/CD およびワンクリックでの品質保証を実現。
- **ステップごとの有効/無効トグル（IsEnabled チェックボックス）**:
  - 各操作カードのチェックボックスで有効/無効を切り替え可能。
  - 無効化時はカード全体を Opacity 0.55 で半透明化し、実行時に安全にスキップ（`Status = "Skip"`）。
  - XML テンプレート（`TemplateStep.Enabled`）にも保存・復元可能。
- **ワンクリック操作ステップ複製（⧉ Duplicate 機能）**:
  - 各操作カードの右端ボタン群に「⧉」ボタンを追加。
  - 現在のステップの全パラメータを完全に保持したクローンを直下にワンクリックで挿入。
- **実行前一括妥当性検査（Pre-Flight Validation エンジン & ✔ Check ボタン）**:
  - 「RUN」実行前または「✔ Check」ボタン押下時に、パイプライン全体の整合性を一括自動検査。
  - 存在しない輪郭の参照、空の輪郭名、同一輪郭の重複 Add、自己減算（結果が空になる SUB）などを瞬時に検出して警告・エラーを可視化。
- **空輪郭・ロック中輪郭の完全安全ガード（Zero-Crash Guard）**:
  - 参照元輪郭が空（`IsEmpty == true`）の場合の安全スキップ処理および警告ログ出力。
  - 承認済み（Approved）輪郭や線量計算ロック中の輪郭への代入例外を安全に捕捉し、親切な臨床エラーメッセージを出力。

### 🔄 変更 (Changed)
- **UI / UX のモダン化**:
  - 分離されていた Add/Del、Boolean、Margin のタブを廃止し、全体を俯瞰可能な単一パイプラインビューへ一本化。
  - 各操作カードに「Add (青)」「Delete (赤)」「Boolean (紫)」「Margin (緑)」「Convert (橙)」の視覚的アクセントカラーを適用。
  - 上部アクションバーに「✔ Check」ボタンを追加。
- **MVVM アーキテクチャの厳格化**:
  - `MainViewModel`、`OperationItemViewModel`、`OperationStepViewModel` 等のデータバインディングモデルを再設計。
  - UI スレッドとデータモデル間の疎結合化とテスト容易性を大幅に向上。

### 🐛 修正 (Fixed)
- **解像度不一致によるクラッシュの根絶**:
  - 異なる解像度を持つストラクチャ間の Boolean 演算実行時に発生していた `System.InvalidOperationException`（または ESAPI 内部 C++ 例外）を ⚡ Auto-Align により完全防止。
- **不正入力・承認済み輪郭に対するゼロクラッシュ耐性**:
  - マージン値や輪郭名に空文字や無効な数値が入力された場合でも、安全なフォールバックと入力検証（Validation）により例外を捕捉。
  - 承認済み輪郭への代入時もホストプロセスを巻き込まず、わかりやすいエラーログを表示。
- **UNC パス探索時の UI フリーズ解消**:
  - 院内 LAN の応答待ちによる長時間ブロッキングを排除し、最大 1000ms で安全にローカルフォールバックするよう改善。

---

## [1.0.0] - 2021-09-14 (Initial Release)

### 🌟 初期リリース
- Varian Eclipse (ESAPI v15.6 / v16.1) 向け輪郭作成自動化スクリプトとして公開。
- 4 分割個別タブ UI によるストラクチャ操作:
  - Add / Del タブ: 輪郭の新規作成および削除。
  - Boolean タブ: 輪郭同士の論理演算（SUB, AND, OR, XOR）。
  - Margin タブ: 標的・リスク臓器に対する各軸マージン付加。
- レガシー CSV 形式によるテンプレート読み込み・保存に対応。
- Costura.Fody による単一 ESAPI DLL 出力構成の採用。
