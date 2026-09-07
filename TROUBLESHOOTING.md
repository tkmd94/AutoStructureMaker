# AutoStructureMaker トラブルシューティング & FAQ ガイド

本ドキュメントは、AutoStructureMaker (v2.0) の導入、設定、テンプレート作成、および実行時において発生しうるエラー・警告メッセージの原因と具体的な対処方法をまとめたトラブルシューティングガイドです。

---

## 目次

1. [スクリプト起動・環境関連トラブル](#1-スクリプト起動環境関連トラブル)
   - 1.1 [Script Approval で承認されない / エラーになる](#11-script-approval-で承認されない--エラーになる)
   - 1.2 [「Script file must provide implementation for class VMS.TPS.Script」エラーで起動できない](#12-script-file-must-provide-implementation-for-class-vmstpsscript-エラーで起動できない)
   - 1.3 [Write-Access 権限エラー（患者データを変更できない）](#13-write-access-権限エラー患者データを変更できない)
   - 1.4 [.NET Framework バージョン不一致で起動しない](#14-net-framework-バージョン不一致で起動しない)
2. [輪郭操作実行時トラブル](#2-輪郭操作実行時トラブル)
   - 2.1 [ステータスが「Fail」になる（原因の特定方法）](#21-ステータスがfailになる原因の特定方法)
   - 2.2 [既存輪郭との重複エラー (`Structure already exists`)](#22-既存輪郭との重複エラー-structure-already-exists)
   - 2.3 [参照輪郭が見つからない (`Structure not found`)](#23-参照輪郭が見つからない-structure-not-found)
   - 2.4 [承認済み輪郭の削除エラー (`Can not remove structure`)](#24-承認済み輪郭の削除エラー-can-not-remove-structure)
   - 2.5 [承認済み・ロック中輪郭への代入エラー (`Cannot modify approved or locked structure`)](#25-承認済みロック中輪郭への代入エラー-cannot-modify-approved-or-locked-structure)
   - 2.6 [参照元輪郭が空の場合の安全スキップ (`Source structure is empty`)](#26-参照元輪郭が空の場合の安全スキップ-source-structure-is-empty)
   - 2.7 [解像度タイプ不一致による Boolean エラー](#27-解像度タイプ不一致による-boolean-エラー)
   - 2.8 [事前検査（Pre-Flight Validation）でエラーが表示された場合の対処](#28-事前検査pre-flight-validationでエラーが表示された場合の対処)
   - 2.9 [入力欄（Target Structure や Create Margin From 等）が空欄になる / 選択が反映されない](#29-入力欄target-structure-や-create-margin-from-等が空欄になる--選択が反映されない)
3. [テンプレート・設定関連トラブル](#3-テンプレート設定関連トラブル)
   - 3.1 [XML テンプレート読み込み時にエラーが出る](#31-xml-テンプレート読み込み時にエラーが出る)
   - 3.2 [保存ダイアログの初期フォルダが開くのに非常に時間がかかる](#32-保存ダイアログの初期フォルダが開くのに非常に時間がかかる)
4. [よくある質問 (FAQ)](#4-よくある質問-faq)

---

## 1. スクリプト起動・環境関連トラブル

### 1.1 Script Approval で承認されない / エラーになる
- **現象**: Eclipse の Script Approval ツールに DLL を登録しようとするとエラーが発生する、または承認チェックボックスが有効にならない。
- **原因**:
  - `AutoStructureMaker.esapi.dll` が適切な管理者権限で配置されていない。
  - Web からダウンロードしたバイナリに Windows のセキュリティブロック（Mark of the Web）が付与されている。
- **対処法**:
  1. エクスプローラーで `AutoStructureMaker.esapi.dll` を右クリック →「プロパティ」を開きます。
  2. 全般タブの一番下にある **「セキュリティ: 許可する（Unblock）」** にチェックを入れ、「OK」をクリックします。
  3. Eclipse の Script Approval ツールを管理者として実行し、再登録します。

---

### 1.2 「Script file must provide implementation for class VMS.TPS.Script」エラーで起動できない
- **現象**: Eclipse からスクリプトを実行した際、「Script file must provide implementation for class VMS.TPS.Script」というダイアログが表示されて起動しない。
- **原因**:
  - Eclipse はプラグイン DLL をロードする際、リフレクション（`Assembly.GetTypes()`）によって `VMS.TPS.Script` クラスを探索します。
  - このとき `Script` クラスが外部ライブラリ（`EsapiEssentials.ScriptBase` 等）を継承していると、Costura.Fody のモジュール初期化子（`.cctor` による内包 DLL の自動展開）が動く前に外部 DLL の解決に失敗し、`ReflectionTypeLoadException` がスローされて Eclipse 側でエントリポイントクラスが「存在しない」と判定されてしまいます。
- **対処法（v2.0.2 で根本修正済み）**:
  - `AutoStructureMaker` v2.0.2 では、`VMS.TPS.Script` を外部アセンブリに依存しない**純粋な POCO クラス（`System.Object` 継承）**として設計刷新し、`[MethodImpl(MethodImplOptions.NoInlining)]` で Eclipse ネイティブの実行エントリポイントを実装しています。
  - v2.0.2 の最新 `AutoStructureMaker.esapi.dll` を配置してご利用ください。

---

### 1.3 Write-Access 権限エラー（患者データを変更できない）
- **現象**: スクリプト起動時に「Cannot modify patient」や「Write access denied」等のエラーが表示される。
- **原因**: ログイン中の Eclipse ユーザーアカウントに ESAPI の Write-Access（データ書き込み・変更権限）が付与されていない。
- **対処法**:
  1. Eclipse のユーザー権限管理（User Administration）にて、該当ユーザーアカウントに「Scripting: Write Access」権限が付与されているか確認してください。
  2. 患者が「Read-Only」モードで開かれていないか確認し、編集可能な状態で患者を開き直してください。

---

### 1.4 .NET Framework バージョン不一致で起動しない
- **現象**: スクリプトを実行しても画面が表示されず、無反応または即座に終了する。
- **原因**: ワークステーションに .NET Framework 4.6.1 以上がインストールされていない。
- **対処法**: Windows Update または Microsoft 公式サイトから .NET Framework 4.6.1（または 4.7.2 / 4.8）をインストールしてください。

---

## 2. 輪郭操作実行時トラブル

### 2.1 ステータスが「Fail」になる（原因の特定方法）
- **対処法**: メイン画面下部の **「Log Text」ウィンドウ** を確認してください。各ステップの実行詳細ログが `(LOG-Add)`, `(LOG-Bool)`, `(LOG-Margin)` 等のプレフィックスと共に出力されています。

---

### 2.2 既存輪郭との重複エラー (`Structure already exists`)
- **ログ例**: `(LOG-Add) Structure already exists: PTV_High`
- **原因**: `AddStructure` 操作を実行しようとした際、同名の輪郭がすでに患者の StructureSet に存在しています。
- **対処法**:
  - 輪郭名を別のユニークな名称に変更してください。
  - すでに存在する輪郭に対してマージンや Boolean を適用したい場合は、プロトコル内の `AddStructure` ステップを削除し、直接 Margin / Boolean の Target として指定してください。

---

### 2.3 参照輪郭が見つからない (`Structure not found`)
- **ログ例**: `(LOG-Bool) Structure not found: Target='Bladder_sub', StrA='Bladder', StrB='PTV_High'`
- **原因**:
  - 指定した輪郭名（大文字・小文字、半角・全角のスペース等）が現在の患者データと一致していません。
  - Margin や Boolean の出力先（Target Structure）が、そのステップより前の段階で作成されていません。
- **対処法**:
  - 先行ステップで該当する Target Structure を `Add` しているか確認してください。
  - 輪郭名を手打ちせず、ComboBox のドロップダウンから候補を選択してください（先行ステップで作成される輪郭もドロップダウンに自動表示されます）。

---

### 2.4 承認済み輪郭の削除エラー (`Can not remove structure`)
- **ログ例**: `(LOG-Del) Can not remove structure: SpinalCord`
- **原因**: 医師によって承認（Approved）されている輪郭、または他の治療計画で使用されている輪郭は ESAPI の安全制約により削除できません。
- **対処法**: Eclipse の輪郭プロパティから承認ステータスを解除（Unapproved）するか、削除ステップから除外してください。

---

### 2.5 承認済み・ロック中輪郭への代入エラー (`Cannot modify approved or locked structure`)
- **ログ例**: `(LOG-Bool) Cannot modify approved or locked structure: Target='Brainstem'`
- **原因**: Margin や Boolean、ConvertHighRes の出力先（TargetStructure）として、承認済み輪郭または線量計算済みプランで使用中のロック輪郭が指定されています。
- **対処法**: 承認された輪郭を誤って上書きしないよう保護されています。出力先を別の新しい輪郭名（例: `Brainstem_Opt`）に変更してください。

---

### 2.6 参照元輪郭が空の場合の安全スキップ (`Source structure is empty`)
- **ログ例**: `(LOG-Margin) Source structure is empty: GTV_Boost`
- **原因**: マージン元（OrigStructure）や論理演算元（Structure A / B）に指定された輪郭に、CT スライス上の輪郭線（セグメント）が全く描画されていません（体積が 0）。
- **対処法**: Eclipse 上で参照元輪郭の描画が完了しているか確認してください。空輪郭のまま実行された場合、スクリプトはクラッシュを回避するため該当ステップを安全にスキップ（Fail または Skip）します。

---

### 2.7 解像度タイプ不一致による Boolean エラー
- **現象**: 従来の ESAPI スクリプトでは「異なる解像度同士の Boolean」でクラッシュしていました。
- **AutoStructureMaker v2.0 での解決**:
  - 本ツールでは **⚡ Boolean Auto-Align 機能** が自動的に発動し、標準解像度輪郭を一時的に高解像度化して安全に結合します。
  - UI 上に `⚡ Auto-Align (High-Res)` バッジが表示されていることを確認してそのまま実行してください。

---

### 2.8 事前検査（Pre-Flight Validation）でエラーが表示された場合の対処
- **現象**: 「✔ Check」ボタンを押した際、または「▶ RUN」実行時に「Pre-Flight Validation Failed」ダイアログが表示され、実行が中断される。
- **原因**:
  - 存在しない輪郭や、まだ作成されていない輪郭を参照している。
  - 必須項目（TargetStructure 等）が空欄のままになっている。
  - 同一輪郭名に対して重複して `Add` が定義されている。
- **対処法**:
  - ダイアログに表示された「Step 番号」と「メッセージ」を確認し、該当ステップの輪郭名入力やステップの実行順序（▲/▼）を修正してください。
  - 一時的に不要なステップであれば、カードのチェックボックスを外して無効化（Disable）することでも回避可能です。

---

### 2.9 入力欄（Target Structure や Create Margin From 等）が空欄になる / 選択が反映されない
- **現象**:
  - 操作カード上の `Target Structure` や `Create Margin From`（または Boolean の `First Structure (A)` / `Second Structure (B)`）のドロップダウンから輪郭を選択しても、フォーカスが外れた際や直後に空欄に戻ってしまう。
  - 「▶ RUN」ボタンを押してマージン処理等を実行した後、またはステップの追加・削除・並び替えを行った際に、入力していた輪郭名が突然空欄（空文字）になる。
- **原因**:
  - **原因 1 (バインディング更新タイミング)**: WPF `ComboBox` のテキストバインディング既定値が `LostFocus` である場合、ドロップダウンからアイテムを選択した直後や未確定フォーカス時に ViewModel へのプロパティ更新が遅延・喪失していました。
  - **原因 2 (選択解除イベントの逆流)**: スクリプト実行後やステップ操作時に輪郭リストのコンテキスト同期を行う際、WPF の `ComboBox` がアイテムコレクションの再評価を検知して内部的に `SelectedItem = null` を強制設定（Coerce）し、それが TwoWay バインディングを通じて ViewModel に空文字 `""` を書き戻していました。先行ステップで作成した輪郭が消去されると、後続ステップの候補からも連鎖的に消去される現象が発生していました。
- **対策（v2.0.2 で根本修正・多層防御）**:
  - **即時反映 (`UpdateSourceTrigger=PropertyChanged`)**: 全ての輪郭選択 ComboBox のバインディングに `UpdateSourceTrigger=PropertyChanged` を指定し、ドロップダウンでの選択・入力が遅延なく ViewModel へ即時反映されます。
  - **同期中空文字遮断 (`IsSyncingContext` ガード)**: `OperationItemViewModel` に `IsSyncingContext` フラグを導入。コンテキスト同期処理中および ComboBox の選択解除イベントによって発生する空文字上書きをセッターレベルで厳密に遮断し、入力値を完全に保護しています。

---

## 3. テンプレート・設定関連トラブル

### 3.1 XML テンプレート読み込み時にエラーが出る
- **現象**: 「Failed to load file: ...」というエラーダイアログが表示される。
- **原因**: XML ファイル内のタグの閉じ忘れや、特殊記号（`&`, `<`, `>`）がエスケープされずに記述されている。
- **対処法**:
  - テキストエディタ等で XML を手動編集した場合は、構文チェッカーでタグの整合性を確認してください。
  - 本ツールの「Save Template」機能を用いて出力された XML ファイルを使用することを推奨します。

---

### 3.2 保存ダイアログの初期フォルダが開くのに非常に時間がかかる
- **現象**: 「Save Template」や「Load Template」を押した際、数秒〜数十秒間フリーズする。
- **原因**: `AutoStructureMaker.config.xml` の `<InitialDirectory>` に指定されたネットワーク共有フォルダ（UNCパス: `\\Server\Share\`）がオフライン、またはネットワーク遅延が発生している。
- **AutoStructureMaker v2.0 での対策**:
  - 本ツールには **UNC パス高速タイムアウト（1秒）** が組み込まれており、共有フォルダに 1 秒以上応答がない場合は自動的にローカルの「マイドキュメント」フォルダへ瞬時にフォールバックします。

---

## 4. よくある質問 (FAQ)

### Q1. スクリプト実行結果を元に戻したい（Undoしたい）場合はどうすればよいですか？
**A.** Eclipse の「Reload」機能を使用してください。
メニューの「File」→「Reload Patient」を選択すると、スクリプト実行前の患者状態へ完全に復旧できます。結果を確定したい場合のみ Eclipse の「Save」を実行してください。

---

### Q2. 輪郭の最大文字数に制限はありますか？
**A.** **16 文字以内** を推奨します。
DICOM 規格の仕様により、輪郭名（Structure ID）は 16 文字までに制限されています。16 文字を超える名前を指定すると、ESAPI 側で切り詰められたり例外が発生する場合があります。

---

### Q3. 新規作成される輪郭の色（Display Color）は指定できますか？
**A.** 現行の ESAPI 仕様上、新規追加された輪郭には Eclipse のカラーパレットから自動的に色が割り当てられます。色を変更したい場合は、作成後に Eclipse の Structure Properties で調整してください。

---

### Q4. 旧バージョンの CSV テンプレートはそのまま使えますか？
**A.** **はい、完全にそのまま読み込み可能です。**
「Load Template」ダイアログで `.csv` を選択すれば自動判別して読み込まれます。また、保存時にも「Legacy CSV (*.csv)」を選択すれば旧形式で書き出せます。
