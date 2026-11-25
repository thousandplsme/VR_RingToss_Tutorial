# 新規作成コース 機械検証（2026-10-22）

教材本文の正本はNotion。ここは実装・検証証跡と再実行用ファイルです。

## 開発効率の改善（2026-10-23）

以下は開発者向けです。学習者がInspector操作を理解するための教材手順は変更しません。

- 改善前の教材・検証環境をローカルGitコミット `4eec99b` に保存。元からあった追跡済みファイル13件の変更は含めていません。外部へのPushはしていません。
- `Efficiency/before.json` は改善前のファイルハッシュ。再生成・上書きは禁止です。`--protect-baseline` を付けた今回の改善検証では、元プロジェクト、検証用のランタイムコード・Scene・Prefab・Asset・Package・ProjectSettingsを改善前と照合します。通常の検証では実行開始時から終了時の不変性を照合し、今後承認された実装変更を過去の基準で禁止し続けることはしません。
- UnityがPlay時に `Mobile_RPAsset.asset` の `k_AssetPreviousVersion` だけを12から13へ更新する現象を確認。検証プロセス終了後、この完全一致する自動更新だけを開始時のバイト列へ戻し、`restoredAutomaticChanges` に記録します。他の差分は復元せず失敗として報告します。
- 一つの入口で、静的検査、章別参照監査、選択したPlayテスト、Androidビルドを実行できます。初回生成・Import・Package変更はこの入口では実行しません。
- 実行結果は `Efficiency/runs/<日時-ID>/` へ保存。`inputs.json` に実行開始時のコード・設定・アセットのSHA-256、`result.json` に対象・所要時間・成否を記録します。古い成功JSONと今回の成功を混同しません。生成ログ・APKはGit管理から除外し、必要な証跡だけ選んで保存してください。
- 章別監査は5章を各1回読み込みます。以前は基礎検査と詳細監査で計10回でした。検査後は開始時のScene構成へ戻します（復帰時の読込は章の検査回数と別）。
- 03以降のTarget、04以降の有効なRingScore、05だけの復帰処理と表示を必須検査します。初期配置と復帰の距離・高さも照合します。
- ランタイム4スクリプトとSources、ローカルNotionSnapshot内のコードを比較します。Notionの現在の本文をオンライン確認する検査ではありません。`CourseAudit.cs` の保管版と実装版も照合します。
- Playテストは `score / physics / return / input / placement / all` を選択できます。検査項目の不一致では残りの検査も続け、初期化不能などの例外は失敗として記録します。疑似入力と実機試験は分けます。
- 疑似入力試験は保存済みInput Settingsの一時コピーでフォーカスの影響を除き、終了時に元の設定を復帰します。A/Bのバインドと押下状態の検査であり、実HMD追跡を含む端末での一連の操作を合格にするものではありません。

リポジトリ直下で実行します。Python 3.11以降、標準ライブラリだけを使用します。

```powershell
# Unity不要：教材コード一致、章別部品の存在、配布素材依存
python -B Docs/tools/verify_project.py check

# 今回の改善前と照合し、元のコード・Scene・Asset等の保全も確認
python -B Docs/tools/verify_project.py check --protect-baseline

# 検証ツール自体の回帰テスト
python -B -m unittest discover -s Docs/tools -p test_verify_project.py -v

# Unityを使う場合：作業を保存し、既存Editorを閉じてから実行
python -B Docs/tools/verify_project.py check --task audit
python -B Docs/tools/verify_project.py check --task play --suite all
python -B Docs/tools/verify_project.py check --task play --suite input
python -B Docs/tools/verify_project.py check --task build
```

Unity CLIの常駐プロセスも名前が `unity.exe` ですが、Editorとは区別して判定します。別Editorの強制終了はしません。対象版のEditor実行ファイルを既定のHub配下に見つけられない場合は `--editor "絶対パス/Unity.exe"` を指定します。各実行の上限は既定1200秒で `--timeout` で変更可能。時間超過時に停止するのは、このツール自身が起動した検証Editorだけです。

通常のEditorからは `Tools > Ring Toss > Validate Saved Chapters` / `Play Tests > All` も利用可能。未保存・Play中・コンパイル中は開始を拒否します。Playテスト終了で通常のEditorを終了させません。Sceneの生成は初回専用で、既存Sceneの有無を確認してから設定変更へ進みます。

Androidビルドは検証用プロジェクトがAndroidへ切替済みの場合だけ実行し、今回の実行フォルダーへAPKを出力します。既存の配布APKは上書きせず、端末へのインストールも行いません。

### 改善前後の比較

実行別の `result.json.seconds` は静的検査・Editor起動待ちを含む総時間です。監査JSONの `seconds` は章の検査時間です。BuildReportの時間だけを総待ち時間と比較しません。

同一PC・同一変更内容で3〜5回ずつ測り、初回インポートとキャッシュ済みを分けます。記録する項目は総時間、手動操作数、AI再試行数、見逃し件数、実機への手戻り回数です。部員の再現成功率と所要時間は人間が測定します。改善前の時間測定は未実施であり、章読込回数の削減を、そのまま時間短縮率とはしません。

以下の2026-10-22の結果は改善前の証跡です。改善後の結果・未検証範囲は `Efficiency/implementation.json` を参照してください。

2026-10-23の確認結果：静的検査26件、開発者ツールの回帰テスト16件、Playテスト47件、全5章の参照監査に成功。AndroidビルドはSucceeded／エラー0／警告5で、生成APKのハッシュも照合済みです。既存APKは上書きしていません。元のゲームコード・Scene・Prefab・Asset・設定は改善前のハッシュと一致しています。

通常Editorのメニュー操作・中断後の復帰はコード上で保護していますが、手動操作では未確認です。検証用プロジェクトで保存済みSceneからメニューを実行し、終了・中断後もEditorが閉じず、開始Sceneへ戻ることを確認してください。実機、投げ心地、文字の見やすさ、部員の再現試験は引き続き人間の確認待ちです。

## 対象と保護

- 既存プロジェクト：`VR_RingToss_Tutorial`。変更前の1265ファイルのSHA-256を `original-project-hashes.json` に保存。
- 既存Editor開始状態：`Assets/RingTossTutorial/Scenes/RingExport_Work.unity`、未保存変更なし、Play停止。
- 独立検証プロジェクト：`CourseValidation/VRRT`。プロジェクト名はVR_RingToss_FromScratch、Universal 3D、Unity 6000.3.9f1。
- 長いパス（Collections内DLLまで265文字）でEditorの読み込み例外が出たため、検証用フォルダー名をVRRTへ短縮。元のプロジェクトは移動しない。
- 元のStarter Assets 3.3.0と教材指定3.3.1を混在させないためにプロジェクトを分ける。

## 完了判定（未完了の項目はチェックしない）

- [x] 1：01/02の手順・版・保存先と旧記録の整理
- [x] 2：03 的の教材・対応シーン
- [x] 3：04 得点の教材・コード・対応シーン
- [x] 4：05 表示・再投球・交代の教材・コード・対応シーン
- [x] 5：コンパイル・参照・得点／復帰処理の機械検証
- [x] 6：章の開始／完成状態一覧と読み戻し
- [x] 7：03〜05の撮影指示
- [x] 8：Notionリンク・配布前チェック・検証区分の整理

## 完了結果

AI側の先行作業1〜8は完了。Playテスト25件成功、全5シーンの保存後の参照・Inspector値照合に成功。Android BuildReportはSucceeded、エラー0、警告5。APKの存在とaaptによる内容確認も完了。画像掲載・実機試験・部員の通し学習は下の人間の確認待ちに残す。

初回ビルドはAPK生成後、900秒の時間制限で終了したため合格にしなかった。`android-build-timeout.log`に保全し、再実行で正常終了を確認した。検証用フォルダー短縮後の最終ビルドログに、Collections DLLのDirectoryNotFoundExceptionはない。

警告5件は、OpenXRの任意提案4件とPipelineがPlayerで無効という通知1件。性能・見やすさは実機で確認する。検証用Pipelineは学習者の導入必須パッケージではない。

APK：`CourseValidation/VRRT/Builds/05_RingToss.apk`。アプリID `com.vrclub.ringtoss.validation`、ARM64、Minimum API 29、Target API 36。端末へのインストールは未実施。

SHA-256：`D4B3E1A778E4284073EA67D029E526876A1DEA6C573DDE9458EB59C0C4D8C163`。

## 証跡

- `scenes.json`：全5完成シーンの保存後の読み戻し。
- `inspector-audit.json`：保存後のXR・入力・物理・得点・表示の参照とInspector値。
- `android-build.json`／`apk-identity.txt`／`apk-artifact.json`：ビルド結果、APK内の対象機種設定、ファイル識別情報。
- `play-tests.json`：Playフレーム8〜13で25件成功。別PhysicsSceneで実際に輪を落下させる試験を含む。
- `play-tests-first-failure.*`：初回の傾き判定失敗とnographicsによる表示初期化エラー。判定をRigidbody.rotationへ修正し、描画デバイスありで再試験して成功。
- `code-comparison.json`：Notionの4スクリプト、Sources、検証用Assetsのコードが一致。
- `original-project-comparison.json`：元のAssets／Packages／ProjectSettings 1265ファイルのハッシュ差分0。
- `xr-validation.json`：Android OpenXRの必須エラー0。任意提案4件（PoseControl、StickControl、SSAO、入力遅延最適化）。
- `Distribution/RingShape_v2.unitypackage`：Prefab・メッシュ・マテリアルのみ。旧v1に欠けていた依存アセットを含めた。
- `Distribution/RingShape_v2.zip`：同unitypackageのZIP。Notion第1章STEP 6にファイルとして掲載済み。
- `NotionSnapshot/`：読戻し時点の保管コピー。本文編集はNotionを正本にする。

## 保存点

パスは検証用プロジェクト内の`Assets/Scenes/`。

| 章 | 開始 | 完成 | 追加する機能 |
| --- | --- | --- | --- |
| 01 | Universal 3D新規。本文中で01_Grabへ保存 | 01_Grab_Complete.unity | 床・XR Origin・輪・掴み・初期配置 |
| 02 | 01_Grab_Complete | 02_Throw.unity | 解放時の速度・重力 |
| 03 | 02_Throw | 03_Target.unity | 棒と土台 |
| 04 | 03_Target | 04_Score.unity | 得点判定 |
| 05 | 04_Score | 05_RingToss.unity | 点数表示・A復帰・B交代 |

全シーンでRingの開始位置(0,1.2,0.5)、Use Gravity OFF、PlaceRingAtStart有効。1章のみThrow On Detach／Force Gravity On Detach OFF。2〜5章はON。

## 人間の確認待ち

- 新規コースをQuest 2へBuild And Runし、頭の追跡・左右の掴み投擲・実A/B入力・的と得点・10回反復・文字の見やすさを確認。
- スクリーンショット：まずA01〜A06、その後B01〜B06、C01〜C06。Notionの撮影チェックリストに選択場所と期待値を記載。
- 別の部員が教材だけで通し再現。展示機種のQuest Pro／3Sは別途試験。
- 既存プロジェクトの実機成功を新規コースの合格へ転用しない。機械試験の復帰はコードからの呼び出しであり、実ボタンの確認ではない。

## 再実行

Unity Editorは一度に一つだけ起動する。元のEditorを閉じてから検証用を開く。

- `CourseScenes.Build`は初回専用。既存の章シーンがあると上書きを拒否する。
- `CourseScenes.Verify`は保存シーンの読み戻し。
- `CourseTests.Run`はPlayテスト。非同期処理のため`-quit`を付けず起動し、テスト自身の終了を待つ。
- `CourseBuild.Android`はAndroid検証後に05だけをAPKビルド。実機へインストールしない。
- `CourseAudit.Run`はInspector値と参照の詳しい照合。

Editorの再実行用C#は検証用プロジェクトの`Assets/Editor`。テスト用コードはランタイム教材に含めない。CLIやEditorのexit 0だけでなく、JSONの結果と実ファイルを確認する。

## 教材向けに採用する最小仕様

輪1個、棒1本。手を離した輪が、棒の根元・水平に近い姿勢・低速という条件を一定時間満たしたら1点。同じ投球で重複加点しない。Aで輪を戻し次の投球、Bで得点を0にして次の人へ。保持中／解放直後／頭の追跡なしでは移動しない。時間制限・効果音・複数的・ボウリングは追加しない。

棒の配置や判定の許容値は教材用の仮決定。機械試験で成立しても、投げやすさ・見やすさ・Quest実機での再現は人間の確認待ち。

## 開始時の環境証跡

Unity CLI 1.0.0-beta.14（更新チェックで最新）。サンドボックス外のローカル接続でEditorに到達。Safe Modeなし。開始時ConsoleにはPipelineのコマンド解決エラーと5000msタイムアウトが各1件あり、コンパイル失敗なし。これらを新規コードのエラーと混同しない。
