# 更新履歴 (Changelog)

AutoStructureMaker のすべての重要な変更は、本ファイルに記録されます。  
本プロジェクトは [Keep a Changelog](https://keepachangelog.com/ja/1.0.0/) および [Semantic Versioning](https://semver.org/lang/ja/) に準拠しています。

---

## [2.0.4] - 2026-09-10

### 🐛 修正 (Fixed)
- **ステップ複製時におけるコレクション参照共有および ComboBox 選択値消失の修正**:
  - `OperationItemViewModel.CreateFromTemplateStep` において、`sharedStructures` / `sharedStructureInfos` が渡された場合でも各ステップ ViewModel が独立した新規コレクションインスタンスを保持するように改修。
  - `MainViewModel.RefreshStepStructureContexts` において、同期開始前に全ステップの `IsSyncingContext = true` を一括適用し、入力値を事前に完全退避・復元する多層防御を導入。複製した Boolean モジュール等でドロップダウン選択時に輪郭名が空欄化する現象を根絶。
  - `MainViewModel.ExecuteDuplicateStep` および `ExecuteLoad` における `PropertyChanged` イベントの多重購読を解消。

### 🌟 追加 (Added)
- **自動単体テストの全方位拡充 (全 81 件 100% PASS)**:
  - 複製ステップのコレクション独立性および WPF ComboBox 連携検証テストを追加。
  - 4 種類の Boolean 操作（SUB, AND, OR, XOR）、マージン負数・0 パース、ToCsvLine、CategoryBadgeColor、Null 安全性、Pre-Flight Check エッジケース（StructureA/B 未入力、HiRes 対象非存在、パイプライン重複 Add、非存在 Del）、ステップ並べ替え（中間要素の MoveUp/MoveDown）等の網羅テストを追加し、テスト件数を 60 件から 81 件へ拡充。
- **出力バイナリへのバージョン明記**:
  - 生成される ESAPI プラグイン DLL 名を `AutoStructureMaker_v2.0.4.esapi.dll` に更新。

---

## [2.0.3] - 2026-09-07

### 🐛 修正 (Fixed)
- **Pre-Flight Validation における未定義 Target Structure 検出漏れの修正**:
  - `BooleanOperation` および `Margin` 操作において、書き込み先となる `TargetStructure` が利用可能輪郭リスト（既存輪郭または先行ステップで作成される輪郭）に存在するかどうかの検証を追加。
  - 事前に `AddStructure` 等で定義されていない未知の輪郭を Target に指定した場合、実行前にエラー（`Target structure '{target}' does not exist in patient dataset or earlier steps. You must add it first.`）として確実に捕捉・警告するように改善。

### 🌟 追加 (Added)
- **「✔ Check」ボタン実行結果の各操作カードステータスへのリアルタイム可視化**:
  - `MainViewModel.ApplyValidationResultToOperations` を新設し、Pre-Flight Check（および RUN 実行前検証）の検証結果を各操作カードのステータスバッジ（`OK` / `Warn` / `Error` / `Skip`）に即座にカラー反映（緑・琥珀・赤・青灰）。
  - `StatusToolTip` プロパティを導入し、ステータスバッジへのマウスホバーで具体的なエラー理由や警告メッセージをツールチップ表示。
- **自動単体テストの拡充 (全 60 件 100% PASS)**:
  - Boolean / Margin における TargetStructure 存在検証テスト、および検証結果のステータス・ツールチップ反映テストを追加し、テスト件数を 57 件から 60 件へ拡充。
- **出力バイナリへのバージョン明記**:
  - 生成される ESAPI プラグイン DLL 名を `AutoStructureMaker_v2.0.3.esapi.dll` に統一。

---

## [2.0.2] - 2026-09-07

### 🐛 修正 (Fixed)
- **ESAPI プラグイン起動エラー (`Script file must provide implementation for class VMS.TPS.Script`) の根本解消**:
  - `AutoStructureMaker/Script.cs` において、外部アセンブリ（`EsapiEssentials.ScriptBase`）の継承を廃止し、純粋な POCO クラス（`System.Object` 継承）として再実装。
  - Eclipse の型走査（`GetTypes()`）が Costura.Fody のモジュール初期化子（`.cctor` / `AssemblyResolve`）より先に実行されることで発生していた `ReflectionTypeLoadException` を完全に回避。
  - スタンドアロン実行環境 (`AutoStructureMaker.Runner`) 向けに `RunnerScript : ScriptBase` アダプタクラスを導入し、開発時・テスト時の互換性を維持。
  - `FodyWeavers.xml` およびプロジェクト参照から ESAPI ランタイム依存（`VMS.TPS.Common.Model.API`, `Types`）の不要な埋め込み・重複参照を整理。
- **ComboBox 入力値（Target Structure / Create Margin From 等）の消失バグの恒久修正**:
  - `UC_OperationCard.xaml` 内の各輪郭選択 ComboBox (`TargetStructure`, `OrigStructure`, `StructureA`, `StructureB`) のデータバインディングに `UpdateSourceTrigger=PropertyChanged` を明示指定。ドロップダウン選択やフォーカス移動時に入力値が即時 ViewModel に反映されるよう改善。
  - `OperationItemViewModel` に `IsSyncingContext` プロパティを新設し、コンテキスト同期中および ComboBox 選択解除イベント（WPF の `CoerceText`）による空文字上書きをセッターレベルで完全遮断（多層防御）。
  - 先行ステップで定義された輪郭がドロップダウンから選択された後、後続ステップやコンテキスト同期によって連鎖的に消去される不具合を根絶。

### 🌟 追加 (Added)
- **包括的自動 UI レンダリング・検証スイート (`scratch/CaptureUi.cs`)**:
  - 実機 WPF ウィンドウレンダリングによる全 8 シナリオ（初期状態、5 カテゴリーカード展開、複製・無効化トグル、Uniform Margin 切替、Pre-Flight Check ログ表示、埋め込み PDF マニュアル認識、XML テンプレート保存・読込ラウンドトリップ、ESAPI エントリポイント実行）の自動スクリーンショット記録・検証を完了。
- **自動単体テストの拡充 (全 57 件 100% PASS)**:
  - WPF ComboBox バインディング同期テスト、コンテキスト伝播テスト、ステップ無効化連動テストを追加し、テスト件数を 40 件から 57 件へ拡充。

---

## [2.0.1] - 2026-09-07

### 🐛 修正 (Fixed)
- **輪郭再同期に伴う ComboBox 入力値（Target / Margin元 / 論理演算元）消失の根絶**:
  - `ExecuteRun` 後の輪郭同期処理において、`ObservableCollection<StructureInfo>` の全破棄・全再生成（`Clear()`）を廃止し、既存インスタンス参照を維持してプロパティのみ更新するスマート・インプレース同期（Smart In-Place Sync）へ刷新。
  - `StructureInfo` に `INotifyPropertyChanged` を実装し、解像度や新規フラグの変更が UI バッジに即座に通知されるよう改善。
  - 同期処理前後の多層入力値保護ガード（Defense-in-Depth）を導入し、マージン処理等の実行後に `Target Structure` や `Create Margin From` が空欄にリセットされる不具合を完全修正。
  - UI ComboBox 結合単体テストを含む回帰防止テストを追加。

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
  - 標準分解能（STD）と高分解能（HIGH）の輪郭同士で Boolean 演算を行う際、低分解能側の輪郭を高分解能へ自動事前変換（`CanConvertToHighResolution` / `ConvertToHighResolution`）して演算を実行。
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
