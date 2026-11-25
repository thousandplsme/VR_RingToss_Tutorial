<callout icon="🎯" color="blue_bg">
<b>ゴール：輪に機能を付けて、Questのコントローラーで掴もう。</b>

Unity Hubで新しいプロジェクトを作るところから始めます。輪の素材だけを読み込み、VR設定や掴む機能は自分で作ります。
</callout>

このページは「01\_輪を掴もう」の<b>新規作成コース</b>です。既存の配布プロジェクト用手順と混ぜず、上から順に進めてください。

## 用意するもの

- Windows PC、Unity Hub、インターネット接続
- 配布素材：<code>RingShape_v2.unitypackage</code>（輪の見た目と16個のCollider）
- Quest 2と左右のコントローラー、データ通信できるUSBケーブル
- UnityとMetaのアカウント。学校・借用機の開発者設定は管理者と行います。

今回はコントローラー操作を使います。ハンドトラッキングや機種固有の機能は使いません。

<b>チェックはON＝入れる、OFF＝外す。設定変更はPlayを停止してから行い、Ctrl+Sで保存します。</b>

<table-of-contents />

# STEP 1｜新しいプロジェクトを作ろう

## ① UnityとAndroid用の部品を入れる

1. Unity HubのInstallsを開き、Unity <b>6000.3.9f1</b>を用意します。見つからない場合は[Unityのダウンロードアーカイブ](https://unity.com/releases/editor/archive)から対象版をHubで開きます。
2. 対象Editorの歯車メニュー → Add modulesで、<b>Android Build Support</b>と、その下の<b>Android SDK & NDK Tools／OpenJDK</b>を入れます。
3. C#を編集するエディターも用意します。Visual Studioを使う場合は「Unityによるゲーム開発」を入れます。

## ② 空の3Dプロジェクトを作る

1. HubのProjects → New projectを押します。
2. Editorは6000.3.9f1、テンプレートは<b>Universal 3D</b>を選びます。VRテンプレートではなく、空の3Dから設定します。
3. 名前を <code>VR_RingToss_FromScratch</code> にして作成します。
4. Unityが開いたら、ProjectのAssetsを右クリック → Create → Folderで <code>Scenes</code> を作ります。
5. File → Save Asで <code>Assets/Scenes/01_Grab.unity</code> として保存します。

<b>確認：</b> Projectに01\_GrabシーンがあればOKです。

<toggle>
画面の名前を確認する

Hierarchy：ステージに置いたものの一覧。Inspector：選んだものの設定。Project：ファイル置き場。Scene：配置を編集する画面。Console：メッセージとエラーを見る画面。
</toggle>

# STEP 2｜VRに必要な機能を入れよう

Unityに機能を追加する部品を「パッケージ」と呼びます。

1. Window → Package Management → Package Managerを開きます。
2. 左上の「＋」→ Install package by nameを選びます。
3. 次のNameとVersionを1行ずつ入力し、Installします。既に同じ版が入っているものは追加しません。

| Name | Version | 役割 |
| --- | --- | --- |
| com.unity.inputsystem | 1.18.0 | ボタンと追跡の入力 |
| [com.unity.xr.management](http://com.unity.xr.management) | 4.5.3 | VR機能の起動設定 |
| com.unity.xr.openxr | 1.16.1 | Questとの接続 |
| com.unity.xr.interaction.toolkit | 3.3.1 | 掴むなどの操作 |

1. Input Systemを有効にして再起動する案内が出たらYesを選びます。
2. Edit → Project Settings → Player → Other SettingsのActive Input Handlingを<b>Input System Package (New)</b>にします。再起動を求められたら保存して再起動します。
3. Package ManagerでXR Interaction Toolkitを選び、Samplesから<b>Starter Assets</b>をImportします。これはUnity公式の入力設定・VRプレイヤーのセットです。

<b>確認：</b> Projectに <code>Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets</code> ができています。

> 依存するパッケージは自動で入ります。この教材ではMeta XR SDKやHands Interaction Demoを追加しません。赤いエラーが出たら先へ進まず、Consoleの最初のエラーを確認します。

# STEP 3｜Quest用の設定をしよう

## ① Androidへ切り替える

1. File → Build Profilesを開きます。
2. Add Build Profileから<b>Android</b>を選び、追加したプロファイルへSwitch Profileします。切替が終わるまで待ちます。
3. Androidが選べない場合は、STEP 1のAndroid Build Supportを確認します。

このコースではAndroid側へ設定します。PC用のタブと間違えないようにしましょう。

## ② OpenXRを有効にする

1. Edit → Project Settings → XR Plug-in Managementを開きます。
2. <b>Androidのタブ</b>でOpenXRをONにし、Initialize XR on StartupもONにします。
3. XR Plug-in Management → OpenXRを開き、Android側の設定を選びます。
4. Enabled Interaction Profilesの「＋」から、次の3つを追加します。

- Oculus Touch Controller Profile
- Meta Quest Touch Pro Controller Profile
- Meta Quest Touch Plus Controller Profile

1. OpenXR Feature Groupsの<b>Meta Quest Support</b>をONにします。
2. Player → Other SettingsのAndroid設定を次に合わせます。

| 項目 | 値 |
| --- | --- |
| Scripting Backend | IL2CPP |
| Target Architectures | ARM64をON、ARMv7をOFF |
| Minimum API Level | Android 10.0（API Level 29） |
| Target API Level | Automatic（highest installed） |
| Auto Graphics API | OFF |
| Graphics APIs | Vulkanを残す |
| Package Name | com.vrclub.ringtoss.yourname（yournameを自分の英小文字名へ変更） |

1. XR Plug-in Management → Project Validationを開き、Android側の赤いエラーを確認します。修正内容を読んで該当行のFixを押します。版の変更や追加パッケージを求められたら、自己判断で更新せず担当者と確認します。

<b>確認：</b> OpenXRがAndroidで有効になり、Project Validationに未解決の赤いエラーがありません。

# STEP 4｜床とVRプレイヤーを置こう

## ① 床を作る

1. Hierarchyの空いている場所を右クリック → 3D Object → Cubeを選びます。
2. 名前を <code>Floor</code> にします。
3. InspectorのTransformを次の値にします。

| 項目 | X | Y | Z |
| --- | --- | --- | --- |
| Position | 0 | -0.05 | 0 |
| Rotation | 0 | 0 | 0 |
| Scale | 4 | 0.1 | 4 |

床の上面が高さ0になります。Box Colliderは残し、Is TriggerはOFFにします。

## ② VRプレイヤーを置く

1. Hierarchy直下の元の <code>Main Camera</code> を削除します。Directional Lightは残します。
2. Projectで <code>Assets/Samples/XR Interaction Toolkit/3.3.1/Starter Assets/Prefabs</code> を開きます。
3. <b>XR Origin (XR Rig)</b>をHierarchyへドラッグします。Prefabは、必要な部品をまとめたひな形です。
4. 追加したXR OriginのPositionとRotationをすべて0、Scaleをすべて1にします。
5. XR OriginコンポーネントのTracking Origin Modeを<b>Floor</b>にします。
6. Input Action ManagerのAction Assetsに<b>XRI Default Input Actions</b>が登録されているか確認します。空なら要素数を1にし、Starter Assets内の同名ファイルを入れます。
7. HierarchyにXR Interaction Managerがなければ、空いている場所を右クリック → Create Emptyで <code>XR Interaction Manager</code> を作り、Add Componentから<b>XR Interaction Manager</b>を追加します。既にあれば追加しません。
8. XR Originを展開し、その中の <code>Locomotion</code> を選びます。Inspector最上部の名前の左のチェックをOFFにします。今回はスティック移動を使いません。
9. Ctrl+Sで保存します。

<b>確認：</b> XR Origin内にMain Cameraと左右のControllerがあり、元のMain Cameraが別に残っていません。サンプルの入力参照はそのまま使います。

# STEP 5｜いったんQuestで動かそう

輪を作る前に、視点とコントローラーが動くところを確認します。

## ① Questを開発用にする

1. Questで使うMetaアカウントを、[Metaの開発用セットアップ](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/)に従って開発者登録・必要な確認を済ませます。
2. ペアリング済みのMeta Horizonスマートフォンアプリで対象ヘッドセットの設定を開き、Developer ModeをONにします。表示されない場合はアカウントの開発者設定を確認します。
3. Questをデータ通信対応USBケーブルでPCへ接続します。
4. Quest内に出る「USBデバッグを許可」で、使用するPCを許可します。

学校・借用機は所有者・管理者のアカウントで準備してもらいます。アカウント情報を部員同士で共有する必要はありません。

## ② Build And Runする

1. File → Build ProfilesでAndroidプロファイルを開きます。
2. Scene Listへ <code>Assets/Scenes/01_Grab.unity</code> を追加し、このシーンだけを有効にします。Global Scene Listを使う場合はそちらを編集します。
3. Run Deviceで接続したQuestを選びます。表示されなければRefreshし、USBデバッグの許可を確認します。
4. Build And Runを押し、プロジェクト内に作った <code>Builds</code> フォルダーへ <code>01_Grab.apk</code> として保存します。
5. Questを装着し、顔と左右のコントローラーを動かします。

<b>確認：床が見える／顔を動かすと視点が変わる／左右のコントローラー表示が動く。</b>

この3つができてから次へ進みましょう。PCのPlayボタンだけでは、Quest単体アプリの確認にはなりません。

# STEP 6｜配布された輪を置こう

輪の形と当たり判定は、配布ファイルから読み込みます。重力や掴む機能は、ここから自分で追加します。

## ① UnityPackageを読み込む

次のZIPをダウンロードし、Windowsで右クリック → <b>すべて展開</b>します。

<file source="notion-file-block://e7324279-e882-4ce6-999f-0af542cc3d1f/43418a31-ab95-4c6d-a744-8f61d18288cc?space_id=4c36ee7c-154a-4d6a-8a68-cfb25e01ff9f&name=RingShape_v2.zip" />

1. 展開した <code>RingShape_v2.unitypackage</code> を用意します。
2. Unityで自分の <code>VR_RingToss_FromScratch</code> プロジェクトを開き、Playを停止します。
3. 上のメニューから <b>Assets → Import Package → Custom Package</b> を選びます。
4. 保存した <code>RingShape_v2.unitypackage</code> を選びます。
5. Import Unity Package画面で、輪のPrefab・メッシュ・マテリアルなどの項目をすべて選び、<b>Import</b>を押します。

<b>確認：</b> 次の3つがProjectに入りました。

- <code>Assets/RingTossMaterials/RingShape.prefab</code>
- <code>Assets/RingTossTutorial/Meshes/RingPlaceholder.asset</code>
- <code>Assets/RingTossTutorial/Materials/Ring.mat</code>

Prefabが見つからない場合はProjectで<code>RingShape</code>を検索します。輪が表示されない場合は、3つともImportしたか確認します。

## ② 輪をシーンに置く

1. Projectの <b>RingShape.prefab</b> を、Hierarchyの空いている場所へドラッグします。XR OriginやFloorの子にはしません。
2. Hierarchyに追加したRingShapeを選び、名前を <b>Ring</b> に変えます。
3. InspectorのTransformを次に合わせます。

| 項目 | X | Y | Z |
| --- | --- | --- | --- |
| Position | 0 | 1.2 | 0.5 |
| Rotation | 0 | 0 | 0 |
| Scale | 1 | 1 | 1 |

1. Scene画面へマウスを置き、Fキーを押して輪を見やすく表示します。
2. HierarchyのRing左の三角を押し、子の <code>Collider_00</code>〜<code>Collider_15</code> があることを確認します。

<b>確認：</b> 丸い輪が表示され、中央に穴があります。子には有効なCapsule Colliderがあり、Is TriggerはOFFです。子の位置・回転・大きさは変更しません。

> 以後は<b>HierarchyのRing</b>を選んで作業します。ProjectのPrefabを直接編集したり、変更をPrefabへApplyしたりする必要はありません。

## ③ 重力を付けて落としてみる

1. <b>親のRing</b>を選び、Inspector下部の <b>Add Component → Rigidbody</b> を追加します。子のColliderには付けません。
2. 次の値にします。

| 項目 | 値 |
| --- | --- |
| Mass | 0.15 |
| Use Gravity | ON |
| Is Kinematic | OFF |
| Interpolate | Interpolate |
| Collision Detection | Continuous Dynamic |

1. Playを押し、Scene画面で輪が床へ落ちることを確認します。
2. Playを停止し、Use Gravityを<b>OFF</b>に戻します。次のSTEPでは輪を空中で待たせて掴みます。
3. Ctrl+Sで保存します。

<b>覚えること：</b> メッシュとマテリアルは見た目、Colliderはぶつかる形、Rigidbodyは物理で動かす部品です。16個のColliderを、親のRigidbody一つでまとめて動かします。

# STEP 7｜輪を掴めるようにしよう

1. 親のRingを選び、Add Component → <b>XR Grab Interactable</b>を追加します。
2. Interaction Manager欄へHierarchyのXR Interaction Managerをドラッグします。
3. 次の値にします。

| 項目 | 設定 |
| --- | --- |
| Interaction Layer Mask | Default |
| Select Mode | Single |
| Movement Type | Velocity Tracking |
| Track Position／Track Rotation | 両方ON |
| Use Dynamic Attach | ON |
| Attach Ease In Time | 0.15 |
| Throw On Detach | OFF |
| Force Gravity On Detach | OFF |

1. Collidersを開き、要素数を<b>16</b>にします。HierarchyでRingを展開し、Collider\_00をElement 0、Collider\_01をElement 1へドラッグします。同じようにCollider\_15まで登録し、16個のCapsule Colliderが重複なく入り、NoneやMissingがないことを確認します。
2. Ringへ<b>XR General Grab Transformer</b>を追加します。Permitted Displacement AxesをAll、Allow One Handed ScalingとAllow Two Handed ScalingをOFFにします。
3. XR Grab InteractableのAdd Default Grab TransformersをOFFにします。
4. Starting Single Grab Transformersを1件にし、Ringに追加したXR General Grab Transformerを登録します。コンポーネントの見出しを参照欄へドラッグできます。

<b>役割：</b> Grab Interactableは「掴める物」、Grab Transformerは「手に合わせて輪を動かす部品」です。

# STEP 8｜輪を顔の前に置くコードを書こう

実機で向いている方向は、人によって違います。起動後に一度だけ、顔の前へ輪を置きます。

## ① スクリプトを作る

1. ProjectのAssets内に <code>Scripts</code> フォルダーを作ります。
2. Scripts内を右クリック → Create → Scripting → MonoBehaviour Scriptで <code>PlaceRingAtStart</code> を作ります。
3. ダブルクリックして開き、コードをすべて次に置き換えて保存します。

```cs
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class PlaceRingAtStart : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private float distance = 0.45f;
    [SerializeField] private float belowEyes = 0.35f;

    private IEnumerator Start()
    {
        if (head == null)
        {
            Debug.LogError("HeadにMain Cameraを入れてください。", this);
            yield break;
        }

        Rigidbody body = GetComponent<Rigidbody>();
        XRGrabInteractable grab = GetComponent<XRGrabInteractable>();
        float trackedSeconds = 0f;

        while (true)
        {
            // 先に掴んだ場合は、その輪を移動させない。
            if (grab.isSelected)
                yield break;

            var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
            bool tracked = device.TryGetFeatureValue(
                CommonUsages.isTracked, out bool value) && value;
            trackedSeconds = tracked
                ? trackedSeconds + Time.unscaledDeltaTime : 0f;

            Vector3 forward = Vector3.ProjectOnPlane(
                head.forward, Vector3.up);

            if (trackedSeconds >= 0.25f && forward.sqrMagnitude > 0.01f)
            {
                body.useGravity = false;
                body.position = head.position
                    + forward.normalized * distance
                    - Vector3.up * belowEyes;
                body.rotation = Quaternion.identity;
                body.linearVelocity = Vector3.zero;
                body.angularVelocity = Vector3.zero;
                yield break;
            }

            yield return null;
        }
    }
}
```

## ② Unityでつなぐ

1. Unityへ戻り、Consoleに赤いエラーがないことを確認します。
2. PlaceRingAtStartをHierarchyのRingへドラッグします。
3. XR Origin → Camera Offsetを開き、Main Cameraをスクリプトの<b>Head</b>欄へドラッグします。
4. Distanceを0.45、Below Eyesを0.35にして保存します。

## コードのポイント

- <code>head</code>：カメラの位置と向き。Inspectorから登録します。
- <code>GetComponent</code>：同じRingに付いている部品を取得します。
- <code>if</code>：「条件を満たしたら」という分岐です。
- <code>yield return null</code>：次のフレームまで待ちます。顔の追跡を待つために使います。
- <code>head.position + ...</code>：顔の位置から、水平に前へ0.45m、下へ0.35m進んだ位置を求めます。
- <code>yield break</code>：処理を終了します。輪が毎回顔についてくるのを防ぎます。

# STEP 9｜掴む・動かす・離すを確かめよう

1. Ctrl+Sで保存し、もう一度Build And Runします。
2. Questで前を向き、輪が顔の前に出るのを待ちます。
3. 輪の縁へコントローラーを近づけ、<b>Grip（持ち手側のボタン）</b>を押し続けます。
4. 手をゆっくり移動・回転させます。
5. Gripを離します。左右の手で一度ずつ試します。

<b>できたらOK：輪を掴める／手について動く／Gripを離すと手から離れる。</b>

離した後に輪が動いても構いません。投げる調整は次の章です。輪が遠くへ行ったらアプリを再起動します。

## 今日の保存点

File → Save Asで <code>Assets/Scenes/01_Grab_Complete.unity</code> に保存します。次の章はこの完成状態から別名保存して進めます。

## 余裕があれば

Distanceを0.45から0.55へ変え、再ビルドして輪の出る位置を比べましょう。確認後は0.45へ戻します。

# 困ったとき

| 症状 | 確認するところ |
| --- | --- |
| QuestがRun Deviceに出ない | 開発者モード、USBデバッグ許可、通信対応ケーブル、Refresh |
| 床が見えない／視点が動かない | AndroidのOpenXR、Meta Quest Support、起動シーン、元のMain Cameraを消したか |
| コントローラーが動かない | 3つのInteraction Profile、Input Action ManagerのAction Assets、Starter Assetsを3.3.1で入れたか |
| 輪がピンク色になる | Universal 3Dで作ったか、配布素材を全項目Importしたかを確認。直らなければ担当者へ素材の確認を依頼する |
| 輪がない | Ringが有効か、HeadにMain Cameraが入っているか、Use GravityがOFFか。顔を水平に向ける |
| 掴めない | 近づけてからGrip。Grabの有効チェック、16個のCollider、Manager、手と輪のInteraction Layerが共通か |
| 輪が床を通り抜ける | 16個のCapsule Colliderが有効でIs TriggerはOFFか。Rigidbodyは親のRingだけに付ける |
| スクリプトを追加できない | ファイル名とクラス名をPlaceRingAtStartで一致させる。Consoleの最初の赤いエラーを確認 |

## 完成チェック

- [ ] 自分の新規プロジェクトを作った。
- [ ] Quest単体で視点と左右コントローラーが動いた。
- [ ] UnityPackageを読み込み、輪を配置できた。
- [ ] RigidbodyとColliderの役割を説明できる。
- [ ] 左右で輪を掴み、動かし、離せた。
- [ ] 完成シーンを別名保存した。

次へ：<mention url="https://app.notion.com/p/40121a05110d81e78b90db2d33207614" />

<toggle>
参考資料

[Unity公式：Starter Assets](https://docs.unity3d.com/Packages/com.unity.xr.interaction.toolkit@3.3/manual/samples-starter-assets.html)

[Unity公式：Meta Quest Support](https://docs.unity3d.com/Packages/com.unity.xr.openxr@1.16/manual/features/metaquest.html)

[Meta公式：開発用ヘッドセットの準備](https://developers.meta.com/horizon/documentation/unity/unity-env-device-setup/)
</toggle>
