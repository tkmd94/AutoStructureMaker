# AutoStructureMaker
自動輪郭作成ツール

VARIAN社製治療計画装置EclipseのScripting codeです。
- Binary-Plugin
- Write-Access

[![LicenseBadges](https://badges.frapsoft.com/os/mit/mit.svg?v=102)](https://github.com/ellerbrock/open-source-badge/)  

実装機能︓開いているStructureSetに対してユーザーが定義した順序で輪郭操作を一括して行えます。

提供する機能は次の４つのモジュールです。
- 輪郭の追加・削除
- 輪郭同士の論理演算
- マージン付加
- 既存輪郭の高分解能変換

作成環境：v15.6

動作検証：v15.6

注）本スクリプトは **Write-Access** につき、データに変更を加えますので、使用する際には十分にご注意ください。

# 操作説明
1. 必要に応じて輪郭を承認(Approval)して保護します。
2. 操作したい順番にモジュールを追加します。
3. モジュール内の各項目を設定します。
4. 「RUN」ボタンを押し、処理を開始します。
5. 2列目のステータス欄に処理結果が表示されます。
    - 「Done」処理成功
    - 「Fail」処理失敗：既存輪郭と同名の輪郭を追加した場合も処理失敗となります。
6. 処理内容を再利用したい場合は設定をファイルに保存します。

- 承認(Approval)されている輪郭は処理できません。
- 分解能の異なる輪郭同士の論理演算は処理できません。
- 元に戻したい場合はEclipseの「Reload」機能を使用して下さい。

# ボタン説明
- 「Run」設定した輪郭操作を実行します。
- 「Load parameter」保存された設定ファイルを読み込みます。
- 「Save parameter」設定をファイルに保存します。
- 「Add/Dell Structure」：輪郭の追加・削除モジュールを追加します。 
- 「Boolean Operators」：論理演算モジュールを追加します。
- 「Margin for Strucutre」：マージン付加モジュールを追加します。
- 「Convert to High Resolution Segment」：既存輪郭を高分解能タイプに変換するモジュールを追加します。
- 「Delete　Control」最下段のモジュールを削除します。

# UI画面
![Screen capture of planCompare UI](https://github.com/tkmd94/AutoStructureMaker/blob/master/Animation.gif)
