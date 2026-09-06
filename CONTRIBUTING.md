# AutoStructureMaker 開発・貢献ガイドライン (Contributing Guide)

AutoStructureMaker プロジェクトへの貢献をご検討いただきありがとうございます。
本ドキュメントは、本プラグインの開発環境のセットアップ、ビルド手順、コーディング規約、およびプルリクエスト（PR）の提出方針を定めた開発者向けガイドラインです。

---

## 目次

1. [開発環境のセットアップ](#1-開発環境のセットアップ)
2. [ビルド手順](#2-ビルド手順)
3. [単体テストの実行](#3-単体テストの実行)
4. [コーディング規約と設計原則](#4-コーディング規約と設計原則)
   - 4.1 [ゼロクラッシュ例外安全規約](#41-ゼロクラッシュ例外安全規約)
   - 4.2 [MVVM パターンとインプレース同期](#42-mvvm-パターンとインプレース同期)
   - 4.3 [ESAPI 一時作業構造体のクリーンアップ保証](#43-esapi-一時作業構造体のクリーンアップ保証)
5. [新規操作カテゴリー（モジュール）の追加手順](#5-新規操作カテゴリーモジュールの追加手順)
6. [プルリクエスト (PR) ガイドライン](#6-プルリクエスト-pr-ガイドライン)

---

## 1. 開発環境のセットアップ

### 推奨ツール
- **IDE**: Visual Studio 2017 / 2019 / 2022（Community 版可）
- **ワークロード**: 「.NET デスクトップ開発」（.NET Framework 4.6.1 開発者パック必須）
- **ターゲットプラットフォーム**: **x64**（ESAPI は 64-bit ネイティブライブラリであるため）

### ESAPI 参照 DLL の配置
ビルドには以下の Varian 公式ライブラリが必要です：
- `VMS.TPS.Common.Model.API.dll`
- `VMS.TPS.Common.Model.Types.dll`

`AutoStructureMaker.csproj` は、以下の標準インストールパスを自動探索します：
```
C:\Program Files\Varian\RTM\16.1\esapi\API\
C:\Program Files\Varian\RTM\15.6\esapi\API\
```
これら以外のパスに DLL がある場合は、環境変数 `EsapiDir` を設定するか、csproj 内の `<EsapiDir>` を指定してください。

---

## 2. ビルド手順

Developer PowerShell または Visual Studio コマンドプロンプトから以下のコマンドを実行します：

```powershell
# x64 Release ビルドの実行
& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" AutoStructureMaker.sln /p:Configuration=Release /p:Platform=x64 /t:Rebuild
```

- ビルド成果物は `AutoStructureMaker\bin\x64\Release\AutoStructureMaker.esapi.dll` に出力されます。
- `Costura.Fody` により、依存するマネージド DLL が単一のバイナリ内に自動マージ（ILRepack / 埋め込み）されます。

---

## 3. 単体テストの実行

本プロジェクトは ESAPI DLL が存在しない CI / 開発環境でもテストが動作するように設計されています。
リポジトリ直下に配置された `test.bat` を実行することで、ソリューションのビルドと全 40 件の単体テストをワンクリックで連続実行できます。

```bat
:: リポジトリ直下の test.bat を実行 (MSBuild x64 ビルド + vstest 一括実行)
test.bat
```

手動で `vstest.console.exe` を直接呼び出す場合は以下を実行します：

```powershell
# 単体テストの実行 (vstest.console.exe)
& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" AutoStructureMaker.Tests\bin\x64\Release\AutoStructureMaker.Tests.dll /Platform:x64
```

> **コミット前必須条件**: 全 40 件の単体テストが 100% PASS することを確認してください。

---

## 4. コーディング規約と設計原則

### 4.1 ゼロクラッシュ例外安全規約
- **文字列から数値への変換**:
  `int.Parse()` や `Convert.ToInt32()` の直接呼び出しは**全面禁止**です。必ず `int.TryParse()` を用い、パース失敗時は適切なデフォルト値（7mm等）へ安全にフォールバックさせてください。
- **空輪郭（`IsEmpty`）ガード**:
  参照元輪郭（`StructureA`, `StructureB`, `OrigStructure`）を取り扱う際は、必ず `origStr.IsEmpty` を事前に確認し、セグメントの存在しない空輪郭に対する Boolean や Margin 計算による ESAPI 内部例外を防止してください。
- **承認済み・ロック輪郭保護**:
  出力先輪郭（`TargetStructure`）に対する変更や輪郭削除の実行時は、承認済み輪郭や線量計算ロック中輪郭への不用意な代入例外を `try-catch` で安全に捕捉し、ホストプロセスを巻き込まずにユーザーへ親切なログを表示してください。
- **null ガード**:
  ESAPI の戻り値（`structureSet.Structures.FirstOrDefault(...)` など）は常に `null` の可能性を考慮し、処理前に必ず null チェックを行ってください。

### 4.2 MVVM パターンとインプレース同期
- コレクションの更新時、`new ObservableCollection<...>` で参照ごと再代入すると、WPF のバインディングが切断されたりちらつきの原因となります。
- `SyncCollection()` や `SyncStructureInfos()` を用いて、既存のコレクションインスタンスを維持したまま要素をインプレース同期（Clear & Add）してください。
- `IsEnabled` の切り替えやステップ複製（`Duplicate`）時も、`RefreshStepStructureContexts()` が自動発動して輪郭コンテキストが正しく同期される設計を維持してください。

### 4.3 ESAPI 一時作業構造体のクリーンアップ保証
一時作業構造体（Auto-Align 時の `_tA_1` など）を生成する場合は、必ず `try ... finally` ブロックを構築し、途中で例外が発生した場合でも `finally` 内で確実に StructureSet から削除してください。

---

## 5. 新規操作カテゴリー（モジュール）の追加手順

1. **Enum の追加**: `OpControlItem.cs` 内の `OperationCategory` enum に新しいカテゴリーを追加。
2. **ViewModel の拡張**: `OperationItemViewModel.cs` に新しいカテゴリー用のプロパティ、表示制御フラグ、および `ExecuteXxx()` メソッドを追加。
3. **UI の拡張**: `UC_OperationCard.xaml` に新しいカテゴリー用の入力パネル（Border / StackPanel）を追加し、Visibility を ViewModel にバインド。
4. **伝播シミュレーションの更新**: `MainViewModel.cs` の `ApplyStepToContext()` に新しい操作結果の反映ロジックを追加。
5. **XML / CSV シリアライザの更新**: `TemplateService.cs` に新操作の保存・読み込み処理を追加。
6. **単体テストの追加**: `OperationStepViewModelTests.cs` に新操作の振る舞いテストを追加。

---

## 6. プルリクエスト (PR) ガイドライン

1. **ブランチ命名**: `feature/新機能名` または `fix/修正内容`
2. **コミットメッセージ**: 変更理由と臨床的影響を簡潔に記載してください。
3. **ドキュメントの更新**: 新しい操作やパラメータを追加した場合は、必ず `MODULES.md` および `TEMPLATES.md` も合わせて更新してください。
