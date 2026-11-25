## 今回の進め方

<b>まずA01〜A06の6か所を撮ります。環境設定のB01〜B06、的・得点・完成のC01〜C06は、その後に撮ります。</b> 長いInspectorは文字を小さくせず、上下に分けて構いません。

対象は新規作成コースです。

- 第1章：<mention url="https://app.notion.com/p/3eb21a05110d81eab68dd57811014617" />
- 第2章：<mention url="https://app.notion.com/p/40121a05110d81e78b90db2d33207614" />

撮影対象は、本文どおりに作った <b>VR\_RingToss\_FromScratch</b>。第1章は01\_Grab\_Complete、第2章は02\_Throwを使います。既存のVR\_RingToss\_TutorialはVRプレイヤーや入力構成が異なるため、同じ画面として流用しません。

## 分担

- <b>人間：</b> 教材どおりに画面を開く、スクリーンショットを撮る、Quest 2で試す。「見つからない項目」「本文と違う表示」があれば、その画面と一言を送る。
- <b>AI：</b> 画像と本文の照合、必要箇所の切り出し・注釈、画像直下の短い説明、Notionへの配置、章間の設定と保存先の整合性確認。
- <b>別の部員：</b> 修正版の教材だけを見て進め、止まったSTEPと原因を伝える。

画像の加工・掲載手段は利用できるツールで確認してから実施します。第3〜5章の追加撮影はC01〜C06を使います。

# 先に撮る6か所

## A01｜Editor全体と輪の選択場所

<b>掲載：第1章 STEP 6「② 輪をシーンに置く」〜「③ 重力を付けて落としてみる」</b>

- 01\_Grab\_Completeを開き、Playを停止。
- Hierarchyで親のRingを選択。子のColliderを数個見えるところまで展開。
- Sceneにマウスを置き、Fキーで輪を表示。
- InspectorでTransformとRigidbodyを開く。Hierarchy・Scene・Projectの見出しも含めて撮る。
- 完成状態：Position（0, 1.2, 0.5）、Scale（1, 1, 1）、Mass 0.15、Use Gravity OFF、Is Kinematic OFF、Interpolate、Continuous Dynamic。
- 収まらなければ全体をA01a、RigidbodyをA01bに分ける。

<b>付ける説明：</b> 「Hierarchyの親のRingを選びます。Inspectorに、選んだ輪の設定が表示されます。」

## A02｜掴む設定と16個のColliderの登録

<b>掲載：第1章 STEP 7「輪を掴めるようにしよう」</b>

- 親のRingを選び、XR Grab Interactableを開く。
- Interaction Manager、Interaction Layer Mask＝Default、Select Mode＝Single、Movement Type＝Velocity Trackingを写す。
- Collidersを展開し、要素数16と登録されたCapsule Colliderを写す。長ければ上下に分ける。
- 第1章の状態はThrow On Detach／Force Gravity On DetachともOFF。
- 期待状態：NoneやMissing、同じColliderの重複がない。子のCollider\_00〜15が対応する要素へ入っている。

<b>付ける説明：</b> 「輪の縁にある16個のColliderを登録します。親のRing自体を16回入れるわけではありません。」

## A03｜Grab Transformerの接続

<b>掲載：第1章 STEP 7の最後</b>

- RingのXR Grab InteractableでGrab Transformerの設定を展開。
- Add Default Grab Transformers＝OFF、Starting Single Grab Transformers＝1件、その参照先を写す。
- XR General Grab Transformerも開き、Permitted Displacement Axes＝All、Allow One Handed Scaling／Allow Two Handed Scaling＝OFFを写す。
- 離れていて同時に写らなければ、A03aとA03bに分ける。

<b>付ける説明：</b> 「Ringに付けたXR General Grab Transformerを、この参照欄へ登録します。」

## A04｜Main CameraをHeadへ登録した状態

<b>掲載：第1章 STEP 8「② Unityでつなぐ」</b>

- HierarchyでXR Origin → Camera Offset → Main Cameraが見えるよう展開。
- Ringを選んだまま、InspectorのPlace Ring At Startを表示。
- Head＝XR Origin内のMain Camera、Distance＝0.45、Below Eyes＝0.35を写す。
- HierarchyのMain CameraとInspectorのHead欄を一枚に入れる。

<b>付ける説明：</b> 「XR Originの中のMain Cameraを、RingのHead欄へドラッグします。」

## A05｜投げる設定の完成状態

<b>掲載：第2章 STEP 2「② 手を離すときの設定を変える」</b>

- 02\_Throwを開き、Playを停止。親のRingを選択。
- XR Grab InteractableのThrow On Detach周辺を写す。
- Throw On Detach＝ON、Throw Smoothing Duration＝0.25、Throw Velocity Scale＝1、Throw Angular Velocity Scale＝1、Force Gravity On Detach＝ON。
- Ringの選択とコンポーネント名も分かるようにする。

<b>付ける説明：</b> 「Throw On DetachをONにすると倍率などが表示されます。Force Gravity On DetachもONにします。」

## A06｜ビルドするシーンとQuestの選択

<b>掲載：第2章 STEP 3「第2章をQuestへ入れよう」。第1章へは設定箇所の説明として再利用</b>

- File → Build Profilesを開き、Androidプロファイルを選択。
- Scene Listで02\_ThrowだけがチェックONになっている状態を写す。Global Scene Listを使う場合はその画面。
- Run Deviceで接続したQuestを選んだ画面とBuild And Runボタンも写す。必要なら2枚に分ける。
- 第1章で再利用するときは「この画像は第2章。第1章では01\_Grabを選ぶ」と明記する。

<b>付ける説明：</b> 「保存しただけではビルド対象は変わりません。この章のシーンだけを有効にします。」

# 次に撮る6か所

## B01｜Unity Hubの新規作成

<b>掲載：第1章 STEP 1「② 空の3Dプロジェクトを作る」</b>

New project画面でEditor＝6000.3.9f1、Universal 3D、名前＝VR\_RingToss\_FromScratchが分かる状態。撮影のために同名のプロジェクトを新しく作る必要はありません。

<b>説明：</b> 「Editorの版とテンプレートを選んでから、プロジェクト名を入れます。」

## B02｜Starter AssetsのImport場所

<b>掲載：第1章 STEP 2の末尾</b>

Package ManagerでXR Interaction Toolkit 3.3.1を選択。Samplesを開き、Starter AssetsとImportボタン、またはImport済み表示を写す。再Importは不要。

<b>説明：</b> 「パッケージ本体を入れた後、SamplesからStarter Assetsも読み込みます。」

## B03｜Android側のOpenXR

<b>掲載：第1章 STEP 3「② OpenXRを有効にする」</b>

2画面に分ける。

- B03a：Project Settings → XR Plug-in Management。Androidタブ、OpenXR＝ON、Initialize XR on Startup＝ON。
- B03b：Android側のOpenXR設定。3つのInteraction ProfilesとMeta Quest Support＝ON。

<b>説明：</b> 「Androidの設定を開きます。PC用タブへ設定しないように注意しましょう。」

## B04｜XR Originと入力設定

<b>掲載：第1章 STEP 4「② VRプレイヤーを置く」</b>

XR Originを選び、HierarchyのCamera Offset・Main Camera・左右Controller・Locomotionを展開。InspectorでTracking Origin Mode＝FloorとInput Action ManagerのAction Assets＝XRI Default Input Actionsを写す。Locomotionは無効。収まらなければ2枚。

<b>説明：</b> 「カメラと左右のControllerはXR Originの中にあります。入力用のファイルも登録します。」

## B05｜UnityPackageのImport画面

<b>掲載：第1章 STEP 6「① UnityPackageを読み込む」</b>

Assets → Import Package → Custom PackageでRingShape\_v2.unitypackageを開き、Import Unity Package画面を写す。配布素材のPrefab・メッシュ・マテリアルの項目が見える状態。既にImport済みなら撮影後にCancelしてよい。

<b>説明：</b> 「配布された素材の項目をすべて選んでImportします。」

## B06｜AndroidのPlayer設定

<b>掲載：第1章 STEP 3のPlayer設定表の直後</b>

Project Settings → Player → Android → Other Settings。IL2CPP、ARM64、Minimum API Level 29、Active Input Handling＝Input System Package (New)など本文で指定した項目を写す。Graphics APIs＝Vulkanは別画像でもよい。

<b>説明：</b> 「Android側のOther Settingsで、表と同じ値にします。」

# 撮影・受け渡しのルール

1. Windowsの <b>Win＋Shift＋S</b>などで撮影し、A01.png、A02a.pngのように番号を付けて保存する。
2. <b>選んだオブジェクト名・ウィンドウ名・設定名・値</b>が読める範囲を残す。全体を縮めて一枚へ詰め込まない。
3. 設定画面はPlay停止中に撮る。第1章と第2章でON／OFFが違う写真を混ぜない。
4. メールアドレス、個人名を含む保存先、端末の識別番号は切り取り・マスクする。
5. 赤枠や矢印は後付けでよい。まず元画像をこのチャットへ渡し、表示が違う箇所だけ一言添える。

「この設定で動くはず」と値を合わせただけでは実機確認済みにしません。違う表示を見つけたら、無理に本文に合わせる前に現状の画面を共有します。

# 第2章の確認待ち

以下は第2章本文作成時の確認待ちです。その後の独立検証プロジェクトの機械試験は、親の「講師用｜教材の検証・配布準備」に記録します。写真・Quest実機・部員の自習確認は別途必要です。

<b>資料上で照合済み：</b> 現行の第1章との接続、保存先、導入済みXRI 3.3.1のXRGrabInteractable本体・Editorソースにある設定名と意味。Throw On DetachをONにすると倍率欄が表示されることもEditorソースで確認しました。

<b>未実施：</b> 新規作成コースの通し操作、今回の設定を使ったEditor画面の目視確認・撮影、Quest 2での第2章再現、部員による自習確認。既存プロジェクトについて以前報告された実機成功と区別します。

- [ ] A01〜A06を受領し、本文と一致する画像を掲載した。
- [ ] B01〜B06を受領し、本文と一致する画像を掲載した。
- [ ] Quest 2のBuild And Run単体アプリで、左右の掴み・投擲・床への衝突を確認した。
- [ ] 倍率1と0.5を比較し、1へ戻したシーンを保存・再ビルドした。
- [ ] 別の部員が教材だけで進め、詰まった場所を修正した。

実機結果は、実施日・実施者・シーン・APK名とビルド日時・機種・実行方式・結果を記録します。第2章の再試行はアプリ再起動です。右Aで戻す機能は第5章で扱います。

# 第3〜5章で追加する6か所

撮影用の独立検証プロジェクトは元プロジェクト内の<code>CourseValidation/VRRT</code>です。教材上のプロジェクト名はVR\_RingToss\_FromScratchです。PCの長いパスによる読み込み問題を避けるため、検証用の保存フォルダー名だけ短くしています。既存Editorを閉じてから開き、同時起動を避けます。

## C01｜Target・Base・Poleの親子関係

<b>掲載：第3章 STEP 2〜3。</b> <code>03_Target.unity</code>を開き、Play停止。HierarchyのTargetを展開し、Sceneの棒と土台を同時に写します。C01aはTargetのPosition（0,0,1.5）とScale（1,1,1）、C01bは子BaseのPosition（0,0.05,0）とScale（0.5,0.1,0.5）。

<b>説明：</b>「BaseとPoleはTargetの子です。子のPositionは親から見た位置です。」

## C02｜Poleの大きさとCollider

<b>掲載：第3章 STEP 3の最後。</b> 同じ03\_TargetでPoleを選択。Position（0,0.4,0）、Scale（0.06,0.3,0.06）、有効なCapsule Collider、Is Trigger OFFを写します。Sceneで輪の穴と棒の太さも比較できるようにします。

<b>説明：</b>「棒のColliderは残します。Rigidbodyは輪だけに付けます。」

## C03｜Ring Scoreへの登録と得点

<b>掲載：第4章 STEP 3〜4。</b> <code>04_Score.unity</code>を開き、停止中のTargetを選択。Ring ScoreのRing参照、Center Tolerance 0.075、Min/Max Height 0.10/0.18、Upright Dot 0.95、Max Speed 0.15、Max Spin 0.5、Settle Seconds 0.5を写します。

別のC03bで本文どおり真上から落下させ、Play中の輪とConsoleの<code>Score: 1</code>を写します。Collapse OFF。撮影後は停止し、Ringの位置・重力・Place Ring At Startを本文の完成値に戻します。

<b>説明：</b>「親Targetへ判定を付け、親RingのRigidbodyを登録します。条件が続いたら1点です。」

## C04｜World Spaceと文字の配置

<b>掲載：第5章 STEP 2。</b> <code>05_RingToss.unity</code>を開き、Play停止。C04aはScoreCanvasのWorld SpaceとRect Transform（位置0,1.35,2／Scale全軸0.0025／600×200）。C04bはScoreTextの中央Anchor、Pos（0,40,0）、600×100、Font Size 48、Font Asset、Raycast Target OFF。Sceneの看板も写します。

<b>説明：</b>「Canvasを空間に置き、その中の文字を設定します。Canvasと文字のScaleは異なります。」

## C05｜表示・再投球スクリプトの接続

<b>掲載：第5章 STEP 3〜4。</b> C05aはScoreCanvasのRing Score Display（Target、Label）。C05bはRingのRing Practice Controls（Head＝Main Camera、Target＝Target、Distance 0.45、Below Eyes 0.35）。Hierarchyから登録元と登録先の両方が見える構図にします。Place Ring At Startも残っていることを確認します。

<b>説明：</b>「スクリプトを付けただけでは動きません。使うカメラ・判定・文字をInspectorでつなぎます。」

## C06｜完成シーンのビルドと表示

<b>掲載：第5章 STEP 5〜6。</b> C06aはAndroidのScene Listで05\_RingTossだけONにした画面。C06bは本文の落下試験でScore: 1になったScene表示。Questが接続されていないときにRun Deviceの選択済み画面は作りません。

<b>説明：</b>「完成シーンをビルド対象にします。Editorでの点数更新とQuestのA・B操作は、それぞれ確認します。」

- [ ] C01〜C06を撮影し、章と値の一致を確認して掲載した。
- [ ] 撮影用に変更した位置・重力・スクリプトの有効状態を戻した。
- [ ] Quest 2単体で第5章のチェック項目を試し、APKと実施日時を記録した。
