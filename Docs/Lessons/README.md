# VR輪投げ教材・初稿

2026-10-22：このフォルダー内の01/02と再現票は旧配布プロジェクト用の記録です。新規作成コース（UnityPackageで輪だけを読み込む）の教材正本はNotion、機械検証は `../CourseValidation/README.md` を参照。新規コースの第2章では右A復帰をまだ作らず、第5章で扱います。

追記（2026-09-27）：ハンズオン版の教材正本は[VR部 Unity教材](https://app.notion.com/p/3e721a05110d811186b0d097a478cd21)。[01_輪を掴もう](https://app.notion.com/p/3e721a05110d8152a2fec6dab8b6489a)は02_VR輪投げを作ろう配下へ移動済み。[00_教材ロードマップ](https://app.notion.com/p/3e721a05110d81b18a6de2aab598f211)に当初計画との照合結果を記録。以下は旧初稿の作成時点の記録です。「Notion未転記」は当時の状態です。新ハンズオン版のログスクリプトを含む完成保存点と通し検証は未整備です。

2026-09-27。LEVEL 5／VR教材群「02_VR輪投げを作ろう」の01・02。全体LEVEL番号と章番号は別のものです。

## 最初に読む

この教材は、配布されたプロジェクトから掴み・投擲の設定を学ぶものです。空のUnityプロジェクトからXRを構築する手順ではありません。
Unity 6000.3.9f1、XRI 3.3.1、OpenXR 1.16.1、Input System 1.18.0を使用。既存Input ActionsはXRI 3.3.0サンプル由来です。学習中に版を更新しません。
接続・ビルド操作は友人の既存「01_UnityとQuestを接続しよう」を前提とします。

## 配布済みのもの／学習者が行うこと

|配布済み|学習者が行うこと|
|---|---|
|床・照明・XR Origin・左右入力・Interaction Manager|Hierarchyで役割と参照先を確認する|
|輪のメッシュ・材料・16個のCollider・Rigidbody|物理設定と穴を保つ構成を理解する|
|無効状態のXR Grab Interactable・登録済みTransformer|設定を照合して掴み機能を有効化する|
|RingInitialPlacement・RingReturnToPlayer|練習補助として使う。コードの説明は05で扱う|

部員はプロジェクトの自分用コピーを使ってください。共有ファイルを同時に直接編集しません。
配布はこのプロジェクトのAssets／Packages／ProjectSettingsと本教材を一組にする案です。シーンファイルだけでは動きません。Library／Tempは配布対象に含めません。配布前の素材・サンプルの利用条件確認と配布経路の確定は別途必要です。

## 保存点の対応

|章|開始|参考完成状態|
|---|---|---|
|[01_輪を掴もう](01_Grab.md)|Assets/RingTossTutorial/Scenes/Lessons/01_Grab_Starter.unity|同フォルダーの01_Grab_Complete.unity|
|[02_輪を投げよう](02_Throw.md)|自分の01完成シーン（または参考01_Grab_Complete）|Assets/RingTossTutorial/Scenes/RingToss_v0_1.unity|

学習者は開始シーンを開き、File > Save Asで自分用の別名（例：My_01_Grab.unity）にしてから作業します。02もMy_02_Throw.unityとして別保存します。教材開始点・参考完成点は上書きしません。
02の参考完成シーンは既存の実機成功報告がある状態です。右Aによる回収も含みますが、05の教材全体が完成したという意味ではありません。

## 検証状態

- 既存の完成シーン：ユーザー本人がQuest 2のBuild And Run単体アプリで初期配置・掴み・投擲・再投球に成功。
- 新しい01開始／01完成シーン：Editorで保存・再読込、コンポーネント有効状態、投擲設定、16 Collider、欠落スクリプト0を確認。新シーンそのもののPlay・Quest試験は未実施。
- 教材手順による部員再現：未実施。**本資料は初稿です。**
- Quest Pro／Quest 3S：未確認。

新しいシーンや学習者の変更を、元の完成シーンの実機成功だけで「実機検証済み」とはしません。

作成時の確認記録：`../RingToss-v0.1/lesson-scene-checks.json`。元の完成シーンのSHA-256は作成前後で一致（83C01C8944B6FBE800E9B19307CE99BE6298ABA389C9C5F81D0B364CF52138B0）。現在Editorは元の完成シーンを開いた状態です。
Consoleには作成要求時のPipelineタイムアウト1件が残っています。処理の実完了と各保存点は別途確認済み。コンパイル失敗なし、警告0。Consoleログを消して成功扱いにはしていません。
最新の技術基準は[CurrentState](../RingToss-v0.1/CurrentState.md)、再現試験は[チェック票](ReproductionCheck.md)を使います。Notionへはまだ転記していません。
