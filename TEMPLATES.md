# AutoStructureMaker テンプレート作成ガイド & 実践サンプル集

本ドキュメントは、AutoStructureMaker (v2.0) における**構造化 XML テンプレートおよびレガシー CSV の仕様、記法、ベストプラクティス、および主要部位別の実践プロトコルサンプル集**をまとめたガイドです。

---

## 目次

1. [テンプレート形式の概要と特長](#1-テンプレート形式の概要と特長)
2. [XML テンプレート仕様リファレンス](#2-xml-テンプレート仕様リファレンス)
   - 2.1 [メタデータ要素](#21-メタデータ要素)
   - 2.2 [ステップ要素と操作タイプ別スキーマ](#22-ステップ要素と操作タイプ別スキーマ)
3. [レガシー CSV フォーマット仕様](#3-レガシー-csv-フォーマット仕様)
4. [部位別実践プロトコル集](#4-部位別実践プロトコル集)
   - 4.1 [前立腺癌 (Prostate IMRT / VMAT) プロトコル](#41-前立腺癌-prostate-imrt--vmat-プロトコル)
   - 4.2 [肺定位照射 (Lung SBRT) プロトコル](#42-肺定位照射-lung-sbrt-プロトコル)
   - 4.3 [頭頸部癌 (Head & Neck Multi-Target) プロトコル](#43-頭頸部癌-head--neck-multi-target-プロトコル)
   - 4.4 [乳房温存・接線照射 (Breast Tangential) プロトコル](#44-乳房温存接線照射-breast-tangential-プロトコル)
5. [テンプレート作成のベストプラクティス](#5-テンプレート作成のベストプラクティス)

---

## 1. テンプレート形式の概要と特長

AutoStructureMaker では、輪郭作成ワークフローを再利用可能なプロトコルとして保存・共有するために **XML 形式** を標準採用しています。また、過去のバージョンとの完全な後方互換性を担保するため、**レガシー CSV 形式** の読み込み・書き出しもネイティブサポートしています。

| 項目 | 構造化 XML 形式（推奨） | レガシー CSV 形式 |
|:---|:---|:---|
| **拡張子** | `.xml` | `.csv` |
| **メタデータ保持** | プロトコル名、作成者、作成日時、説明を完全保持 | ファイル名のみ |
| **可読性・構造** | タグによる階層構造（マージンや演算の意図が明確） | カンマ区切りのプレーンテキスト |
| **拡張性** | 新パラメータ追加時も既存ファイルが壊れにくい | カラム数固定のため変更に脆弱 |
| **互換性** | v2.0 以降の標準形式 | v1.x 系列との相互運用に対応 |

---

## 2. XML テンプレート仕様リファレンス

### 2.1 メタデータ要素
ルート要素 `<StructureTemplate>` の直下に、プロトコル全体の基本情報を記述します：

```xml
<?xml version="1.0" encoding="utf-8"?>
<StructureTemplate>
  <!-- プロトコル識別名 (UI 上の Protocol 名にも反映) -->
  <ProtocolName>Prostate_VMAT_Standard</ProtocolName>

  <!-- 作成者 (Windows ユーザーID または施設チーム名) -->
  <Author>MedicalPhysicist_T</Author>

  <!-- 作成日時 (yyyy-MM-dd HH:mm:ss 形式) -->
  <CreatedAt>2026-09-06 15:00:00</CreatedAt>

  <!-- プロトコルの目的・注意書き・適応症例の説明文 -->
  <Description>前立腺癌に対する CTV から PTV 作成、および膀胱・直腸引き算プロトコル</Description>

  <!-- 順次実行されるステップリスト -->
  <Steps>
    ...
  </Steps>
</StructureTemplate>
```

---

### 2.2 ステップ要素と操作タイプ別スキーマ

`<Steps>` タグ内に `<Step Number="連番" Type="操作種別" Enabled="true|false">` を記述します。
- `Number`: 実行順序を表す 1 始まりの整数値。
- `Type`: 操作種別（`Add`, `Del`, `Boolean`, `Margin`, `ConvertHighRes`）。
- `Enabled`: （任意）ステップの有効/無効フラグ。`false` に設定すると実行時にスキップされます（省略時は `true`）。

#### A. Add Structure（新規輪郭追加）
```xml
<Step Number="1" Type="Add" Enabled="true">
  <TargetStructure>PTV_High</TargetStructure>
  <DicomType>PTV</DicomType>
</Step>
```

#### B. Delete Structure（輪郭削除）
```xml
<Step Number="2" Type="Del" Enabled="true">
  <TargetStructure>Temp_Ring</TargetStructure>
</Step>
```

#### C. Boolean Operators（論理演算）
`<BooleanOperation>` には `SUB`（差分）、`AND`（積集合）、`OR`（和集合）、`XOR`（排他）を指定します。
```xml
<Step Number="3" Type="Boolean" Enabled="true">
  <TargetStructure>Bladder_sub</TargetStructure>
  <BooleanOperation>SUB</BooleanOperation>
  <StructureA>Bladder</StructureA>
  <StructureB>PTV_High</StructureB>
</Step>
```

#### D. Margin（マージン付加）
`<MarginGeometry>` は `Outer`（拡大）または `Inner`（縮小）を指定します。
マージン値は `<Margins>` タグの属性として 6 方向（ミリメートル）を指定します。
```xml
<Step Number="4" Type="Margin" Enabled="true">
  <TargetStructure>PTV_High</TargetStructure>
  <OrigStructure>CTV</OrigStructure>
  <MarginGeometry>Outer</MarginGeometry>
  <Margins X1="5" X2="5" Y1="5" Y2="3" Z1="5" Z2="5" />
</Step>
```

#### E. Convert High Res（高分解能変換）
```xml
<Step Number="5" Type="ConvertHighRes" Enabled="true">
  <TargetStructure>PTV_High</TargetStructure>
</Step>
```

---

## 3. レガシー CSV フォーマット仕様

AutoStructureMaker では、従来の CSV ファイルもそのまま読み込み・保存可能です。CSV の各行は先頭カラムの識別文字列によって処理が分岐します：

```csv
# 1. Add (新規作成)
AddDelControl,Add,輪郭名,DICOM_Type

# 2. Delete (削除)
AddDelControl,Del,輪郭名,DICOM_Type

# 3. Boolean (論理演算: SUB / AND / OR / XOR)
BoolOpControl,演算タイプ,出力先輪郭,第1輪郭,第2輪郭

# 4. Margin (マージン: Asymmetry, 出力先, 参照元, ジオメトリ, X1, X2, Y1, Y2, Z1, Z2)
AddMarginControl,Asymmetry,出力先輪郭,参照元輪郭,Outer,5,5,5,3,5,5

# 5. ConvertHighRes (高解像度化)
ConvertHighResControl,HiRes,対象輪郭
```

---

## 4. 部位別実践プロトコル集

### 4.1 前立腺癌 (Prostate IMRT / VMAT) プロトコル
前立腺癌の標準的な治療計画において、CTV から直腸側マージンを詰めた PTV を作成し、OAR（直腸・膀胱）の評価用引き算輪郭および最適化用リング構造を自動生成するプロトコルです。

```xml
<?xml version="1.0" encoding="utf-8"?>
<StructureTemplate>
  <ProtocolName>Prostate_VMAT_78Gy</ProtocolName>
  <Author>Medical Physics Team</Author>
  <Description>Prostate CTV to PTV (Post: 3mm, others: 5mm) &amp; OAR sub structures</Description>
  <Steps>
    <!-- Step 1: PTV_78Gy 輪郭を作成 (初期: STD) -->
    <Step Number="1" Type="Add">
      <TargetStructure>PTV_78Gy</TargetStructure>
      <DicomType>PTV</DicomType>
    </Step>

    <!-- Step 2: CTV から後方のみ 3mm、その他 5mm のマージンで PTV_78Gy を生成 -->
    <Step Number="2" Type="Margin">
      <TargetStructure>PTV_78Gy</TargetStructure>
      <OrigStructure>CTV_Prostate</OrigStructure>
      <MarginGeometry>Outer</MarginGeometry>
      <Margins X1="5" X2="5" Y1="5" Y2="3" Z1="5" Z2="5" />
    </Step>

    <!-- Step 3: PTV を高解像度セグメントに昇格 -->
    <Step Number="3" Type="ConvertHighRes">
      <TargetStructure>PTV_78Gy</TargetStructure>
    </Step>

    <!-- Step 4: 膀胱の重複除外輪郭 (Bladder - PTV) の受け皿を作成 -->
    <Step Number="4" Type="Add">
      <TargetStructure>Bladder_Opt</TargetStructure>
      <DicomType>AVOIDANCE</DicomType>
    </Step>

    <!-- Step 5: Boolean SUB (Bladder から PTV_78Gy を差し引く: Auto-Align 自動適用) -->
    <Step Number="5" Type="Boolean">
      <TargetStructure>Bladder_Opt</TargetStructure>
      <BooleanOperation>SUB</BooleanOperation>
      <StructureA>Bladder</StructureA>
      <StructureB>PTV_78Gy</StructureB>
    </Step>

    <!-- Step 6: 直腸の重複除外輪郭 (Rectum - PTV) の受け皿を作成 -->
    <Step Number="6" Type="Add">
      <TargetStructure>Rectum_Opt</TargetStructure>
      <DicomType>AVOIDANCE</DicomType>
    </Step>

    <!-- Step 7: Boolean SUB (Rectum から PTV_78Gy を差し引く: Auto-Align 自動適用) -->
    <Step Number="7" Type="Boolean">
      <TargetStructure>Rectum_Opt</TargetStructure>
      <BooleanOperation>SUB</BooleanOperation>
      <StructureA>Rectum</StructureA>
      <StructureB>PTV_78Gy</StructureB>
    </Step>
  </Steps>
</StructureTemplate>
```

---

### 4.2 肺定位照射 (Lung SBRT) プロトコル
4D-CT から作成された ITV（内部標的体積）から呼吸性移動・セットアップ誤差を含む全方位 5mm マージンで PTV を作成し、胸壁（ChestWall）や肋骨（Ribs）の評価用構造を作成します。

```xml
<?xml version="1.0" encoding="utf-8"?>
<StructureTemplate>
  <ProtocolName>Lung_SBRT_48Gy</ProtocolName>
  <Author>SBRT Planning Group</Author>
  <Description>Lung SBRT ITV to PTV (Uniform 5mm) &amp; ChestWall Subtraction</Description>
  <Steps>
    <!-- Step 1: PTV_SBRT を追加 -->
    <Step Number="1" Type="Add">
      <TargetStructure>PTV_SBRT</TargetStructure>
      <DicomType>PTV</DicomType>
    </Step>

    <!-- Step 2: ITV から等方 5mm マージンを付加 -->
    <Step Number="2" Type="Margin">
      <TargetStructure>PTV_SBRT</TargetStructure>
      <OrigStructure>ITV</OrigStructure>
      <MarginGeometry>Outer</MarginGeometry>
      <Margins X1="5" X2="5" Y1="5" Y2="5" Z1="5" Z2="5" />
    </Step>

    <!-- Step 3: PTV を高解像度化 (小体積・急峻な線量勾配用) -->
    <Step Number="3" Type="ConvertHighRes">
      <TargetStructure>PTV_SBRT</TargetStructure>
    </Step>

    <!-- Step 4: 肺全域から ITV を引いた健常肺 (Lung_Total - ITV) を作成 -->
    <Step Number="4" Type="Add">
      <TargetStructure>Lung_Eval</TargetStructure>
      <DicomType>ORGAN</DicomType>
    </Step>

    <Step Number="5" Type="Boolean">
      <TargetStructure>Lung_Eval</TargetStructure>
      <BooleanOperation>SUB</BooleanOperation>
      <StructureA>Lungs</StructureA>
      <StructureB>ITV</StructureB>
    </Step>
  </Steps>
</StructureTemplate>
```

---

### 4.3 頭頸部癌 (Head & Neck Multi-Target) プロトコル
高線量標的（PTV_High: 70Gy）と予防領域（PTV_Low: 54Gy）が混在する頭頸部 VMAT において、重複領域の引き算輪郭および脊髄 PRV（Planning Organ at Risk Volume）を作成します。

```xml
<?xml version="1.0" encoding="utf-8"?>
<StructureTemplate>
  <ProtocolName>HeadAndNeck_DualPTV</ProtocolName>
  <Author>Head &amp; Neck Team</Author>
  <Description>H&amp;N Simultaneous Integrated Boost (SIB) PTV and SpinalCord PRV</Description>
  <Steps>
    <!-- Step 1: 脊髄 PRV (SpinalCord + 3mm) の作成 -->
    <Step Number="1" Type="Add">
      <TargetStructure>SpinalCord_PRV</TargetStructure>
      <DicomType>AVOIDANCE</DicomType>
    </Step>

    <Step Number="2" Type="Margin">
      <TargetStructure>SpinalCord_PRV</TargetStructure>
      <OrigStructure>SpinalCord</OrigStructure>
      <MarginGeometry>Outer</MarginGeometry>
      <Margins X1="3" X2="3" Y1="3" Y2="3" Z1="3" Z2="3" />
    </Step>

    <!-- Step 3: PTV_70Gy を作成 -->
    <Step Number="3" Type="Add">
      <TargetStructure>PTV_70Gy</TargetStructure>
      <DicomType>PTV</DicomType>
    </Step>

    <Step Number="4" Type="Margin">
      <TargetStructure>PTV_70Gy</TargetStructure>
      <OrigStructure>CTV_70Gy</OrigStructure>
      <MarginGeometry>Outer</MarginGeometry>
      <Margins X1="5" X2="5" Y1="5" Y2="5" Z1="5" Z2="5" />
    </Step>

    <!-- Step 5: 予防標的 (PTV_54Gy) から高線量標的 (PTV_70Gy) を引いた制御輪郭を作成 -->
    <Step Number="5" Type="Add">
      <TargetStructure>PTV_54_Opt</TargetStructure>
      <DicomType>CONTROL</DicomType>
    </Step>

    <Step Number="6" Type="Boolean">
      <TargetStructure>PTV_54_Opt</TargetStructure>
      <BooleanOperation>SUB</BooleanOperation>
      <StructureA>PTV_54Gy</StructureA>
      <StructureB>PTV_70Gy</StructureB>
    </Step>
  </Steps>
</StructureTemplate>
```

---

### 4.4 乳房温存・接線照射 (Breast Tangential) プロトコル
乳房 CTV から 5mm マージンを付加して PTV を作成し、皮膚表面から 5mm 内側に収まるように体表面（Body / Skin）との Boolean 積集合（AND）またはトリミングを行うプロトコルです。

```xml
<?xml version="1.0" encoding="utf-8"?>
<StructureTemplate>
  <ProtocolName>Breast_Tangential_WholeBreast</ProtocolName>
  <Author>Breast Group</Author>
  <Description>Whole Breast PTV with 5mm skin cut</Description>
  <Steps>
    <!-- Step 1: 皮膚内側領域 (Body から 5mm 縮小) を作成 -->
    <Step Number="1" Type="Add">
      <TargetStructure>Body_Inner5mm</TargetStructure>
      <DicomType>CONTROL</DicomType>
    </Step>

    <Step Number="2" Type="Margin">
      <TargetStructure>Body_Inner5mm</TargetStructure>
      <OrigStructure>BODY</OrigStructure>
      <MarginGeometry>Inner</MarginGeometry>
      <Margins X1="5" X2="5" Y1="5" Y2="5" Z1="5" Z2="5" />
    </Step>

    <!-- Step 3: CTV_Breast から 5mm 拡大マージンで一時 PTV を作成 -->
    <Step Number="3" Type="Add">
      <TargetStructure>PTV_Raw</TargetStructure>
      <DicomType>CONTROL</DicomType>
    </Step>

    <Step Number="4" Type="Margin">
      <TargetStructure>PTV_Raw</TargetStructure>
      <OrigStructure>CTV_Breast</OrigStructure>
      <MarginGeometry>Outer</MarginGeometry>
      <Margins X1="5" X2="5" Y1="5" Y2="5" Z1="5" Z2="5" />
    </Step>

    <!-- Step 5: 皮膚から 5mm 内側でクリップした正式 PTV_Breast を作成 -->
    <Step Number="5" Type="Add">
      <TargetStructure>PTV_Breast</TargetStructure>
      <DicomType>PTV</DicomType>
    </Step>

    <Step Number="6" Type="Boolean">
      <TargetStructure>PTV_Breast</TargetStructure>
      <BooleanOperation>AND</BooleanOperation>
      <StructureA>PTV_Raw</StructureA>
      <StructureB>Body_Inner5mm</StructureB>
    </Step>

    <!-- Step 7: 一時作成した中間構造体を自動クリーンアップ (削除) -->
    <Step Number="7" Type="Del">
      <TargetStructure>PTV_Raw</TargetStructure>
    </Step>
    <Step Number="8" Type="Del">
      <TargetStructure>Body_Inner5mm</TargetStructure>
    </Step>
  </Steps>
</StructureTemplate>
```

---

## 5. テンプレート作成のベストプラクティス

1. **先行作成の原則**:
   - Margin や Boolean 操作の出力先（Target Structure）として指定する輪郭は、**そのステップより前に必ず `Add` 操作で作成しておく**か、または既存の StructureSet に存在している必要があります。
2. **中間作業構造体のクリーンアップ**:
   - 複雑な Boolean 演算やマージンクリッピングで一時的に生成した中間輪郭（`Temp_` や `Raw_`）は、**プロトコルの最終ステップ群で `Del` 操作を呼んで削除**しておくと、StructureSet 内が常に整然と保たれます。
3. **輪郭名（ID）の命名規約**:
   - DICOM 規格および ESAPI の制限により、輪郭名は **16 文字以内** に収めることが強く推奨されます。
   - 特殊記号や全角文字を避け、半角英数字およびアンダースコア（`_`）で記述してください。
4. **Auto-Align を前提とした安心設計**:
   - 小体積 OAR や PTV を高解像度（`ConvertHighRes`）にした場合でも、後続の Boolean で標準解像度輪郭（例: 膀胱や直腸）との引き算を行って問題ありません。スクリプトが自動的に整合（Auto-Align）します。
