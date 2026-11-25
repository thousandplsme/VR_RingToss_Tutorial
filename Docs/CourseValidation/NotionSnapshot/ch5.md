<b>ゴール：点数をQuest内に表示し、立ったまま繰り返し遊べるようにしよう。</b>

前の章：<mention url="https://app.notion.com/p/40121a05110d813f86d4f2bb40ab7045" />

第4章の <code>Assets/Scenes/04_Score.unity</code> から始めます。完成すると、右コントローラーの<b>A＝輪を戻す、B＝点数を0にして交代</b>になります。

# STEP 1｜完成用シーンを保存しよう

1. Playを停止します。
2. File → Save Asで <code>Assets/Scenes/05_RingToss.unity</code> に保存します。
3. RingのUse Gravity＝OFF、Place Ring At Start＝ON、Throw On Detach／Force Gravity On Detach＝ONを確認します。

# STEP 2｜空間に点数を表示しよう

## ① Canvasを作る

1. Hierarchyの空いている場所を右クリック → <b>UI → Canvas</b>。
2. 名前を <b>ScoreCanvas</b> にします。
3. InspectorのCanvasで<b>Render ModeをWorld Space</b>にします。Event Camera欄にはXR Origin内のMain Cameraを入れます。
4. Rect Transformを次の値にします。

| 項目 | 値 |
| --- | --- |
| Position | X＝0、Y＝1.35、Z＝2 |
| Rotation | X＝0、Y＝0、Z＝0 |
| Scale | X＝0.0025、Y＝0.0025、Z＝0.0025 |
| Width／Height | 600／200 |

<b>World Spaceは、文字を3D空間の看板として置く設定です。</b> ScoreCanvasはXR OriginやMain Cameraの子にせず、Hierarchyの一番外側へ置きます。

今回は文字を表示するだけです。ScoreCanvasのGraphic Raycasterは右上メニュー → Remove Componentで外します。Canvas作成時にHierarchyへEventSystemが自動追加された場合は、そのEventSystemも削除します。

## ② 点数の文字を作る

1. ScoreCanvasを右クリック → <b>UI → Text - TextMeshPro</b>。
2. 初回にTMP Importerが出たら<b>Import TMP Essentials</b>を押し、完了後にウィンドウを閉じます。
3. 作った文字を <b>ScoreText</b> に改名します。
4. Rect TransformのAnchor Presetsで中央を選び、Pivotを（0.5, 0.5）にします。
5. 次の値にします。

| 項目 | 値 |
| --- | --- |
| Pos X／Pos Y／Pos Z | 0／40／0 |
| Width／Height | 600／100 |
| Rotation | 0／0／0 |
| Scale | 1／1／1 |
| Text Input | Score: 0 |
| Font Size | 48 |
| Auto Size | OFF |
| Alignment | 横中央・縦中央 |
| Vertex Color | 白 |
| Raycast Target | OFF |

TextMeshProのFont Assetが空なら、TMP Essentialsに入っている<b>LiberationSans SDF</b>を選びます。

## ③ 操作説明を置く

1. HierarchyのScoreTextを選んで<b>Ctrl+D</b>で複製し、名前を <b>ControlsText</b> にします。
2. Pos Yを<b>−45</b>、Heightを<b>60</b>、Font Sizeを<b>24</b>にします。
3. Text Inputを <b>A: Return   B: New Player</b> にします。

文字が小さければSceneでScoreCanvasを選び、Fキーで近づきます。実機での見やすさは最後に確かめます。

# STEP 3｜点数と文字をつなごう

1. Assets/Scriptsで <b>RingScoreDisplay</b> を作ります。
2. 次のコードへ置き換えて保存します。

```cs
using TMPro;
using UnityEngine;

public class RingScoreDisplay : MonoBehaviour
{
    [SerializeField] private RingScore target;
    [SerializeField] private TMP_Text label;
    private int shownScore = -1;

    private void Update()
    {
        if (target == null || label == null || shownScore == target.Score) return;
        shownScore = target.Score;
        label.text = "Score: " + shownScore;
    }
}
```

3. Unityへ戻り、Consoleの赤いエラーがないことを確認します。
4. RingScoreDisplayを<b>ScoreCanvas</b>へドラッグします。
5. Inspectorの<b>Target</b>欄へHierarchyのTarget、<b>Label</b>欄へScoreTextをドラッグします。

<b>役割：</b> Targetの点数が変わったときだけ、Labelの文字を更新します。<code>TMP_Text</code>はTextMeshProの文字を操作する型です。

# STEP 4｜Aで戻す・Bで交代するコードを書こう

1. Assets/Scriptsで <b>RingPracticeControls</b> を作ります。
2. 次のコードへ置き換えて保存します。

```cs
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

[RequireComponent(typeof(Rigidbody), typeof(XRGrabInteractable))]
public class RingPracticeControls : MonoBehaviour
{
    [SerializeField] private Transform head;
    [SerializeField] private RingScore target;
    [SerializeField] private float distance = 0.45f;
    [SerializeField] private float belowEyes = 0.35f;

    private Rigidbody body;
    private XRGrabInteractable grab;
    private InputAction returnAction;
    private InputAction newPlayerAction;
    private int lastReleaseFrame = -10;

    private void Awake()
    {
        body = GetComponent<Rigidbody>();
        grab = GetComponent<XRGrabInteractable>();
        grab.selectExited.AddListener(OnReleased);
        returnAction = new InputAction("Return", InputActionType.Button,
            "<XRController>{RightHand}/{PrimaryButton}");
        newPlayerAction = new InputAction("New Player", InputActionType.Button,
            "<XRController>{RightHand}/{SecondaryButton}");
    }

    private void OnEnable()
    {
        returnAction.Enable();
        newPlayerAction.Enable();
    }

    private void OnDisable()
    {
        returnAction.Disable();
        newPlayerAction.Disable();
    }

    private void OnDestroy()
    {
        grab.selectExited.RemoveListener(OnReleased);
        returnAction.Dispose();
        newPlayerAction.Dispose();
    }

    private void OnReleased(SelectExitEventArgs args)
        => lastReleaseFrame = Time.frameCount;

    private void Update()
    {
        bool newPlayer = newPlayerAction.WasPressedThisFrame();
        if (!newPlayer && !returnAction.WasPressedThisFrame()) return;

        var device = InputDevices.GetDeviceAtXRNode(XRNode.Head);
        if (!device.TryGetFeatureValue(
            UnityEngine.XR.CommonUsages.isTracked, out bool tracked) || !tracked) return;

        TryReturn(newPlayer);
    }

    public bool TryReturn(bool clearScore)
    {
        if (head == null || target == null || grab.isSelected ||
            Time.frameCount <= lastReleaseFrame + 1) return false;

        Vector3 forward = Vector3.ProjectOnPlane(head.forward, Vector3.up);
        if (forward.sqrMagnitude < 0.01f) return false;

        // 起動時の待機処理が残っていても、後から輪を動かさない。
        var placement = GetComponent<PlaceRingAtStart>();
        if (placement != null) placement.StopAllCoroutines();

        body.useGravity = false;
        body.position = head.position + forward.normalized * distance
            - Vector3.up * belowEyes;
        body.rotation = Quaternion.identity;
        body.linearVelocity = Vector3.zero;
        body.angularVelocity = Vector3.zero;

        if (clearScore) target.BeginNewPlayer();
        else target.BeginNextThrow();
        return true;
    }
}
```

3. UnityでConsoleの赤いエラーがないことを確認します。
4. RingPracticeControlsを<b>Hierarchyの親のRing</b>へドラッグします。
5. Inspectorの値を登録します。

| 項目 | 登録するもの・値 |
| --- | --- |
| Head | XR Origin → Camera Offset → Main Camera |
| Target | HierarchyのTarget |
| Distance | 0.45 |
| Below Eyes | 0.35 |

<b>Place Ring At Startは残します。</b> 起動時はそのコードで配置し、ボタンを押した後はRingPracticeControlsが置き直します。2つのDistanceとBelow Eyesは同じ値にします。

## コードのポイント

- <code>InputAction</code>：ボタンの入力を受け取る部品です。ここでは右手のPrimaryButton＝A、SecondaryButton＝Bを使います。
- <code>WasPressedThisFrame()</code>：押し始めた瞬間だけtrueになります。長押しで輪が顔についてくることを防ぎます。
- <code>grab.isSelected</code>：掴んでいる間は輪を動かしません。
- <code>lastReleaseFrame</code>：手を離した直後の投擲処理と、戻す処理が重ならないようにします。
- <code>body.position</code>：輪を今の顔の前へ移動します。速度と回転速度も0へ戻します。
- <code>BeginNextThrow()</code>：点数を残して次の投球へ。<code>BeginNewPlayer()</code>：点数も0に戻します。

# STEP 5｜Editorで点数表示を確認しよう

1. 第4章と同じように、RingのPlace Ring At StartをOFF、Positionを（0, 1, 1.5）、Use GravityをONにします。
2. Playし、輪が土台に落ち着いたら<b>Score: 1</b>に変わることをScene画面で確認します。
3. Playを停止します。
4. RingをPosition＝（0, 1.2, 0.5）、Rotation＝（0, 0, 0）、Use Gravity＝OFF、Place Ring At Start＝ONに戻し、Ctrl+Sで保存します。

この確認だけではQuestのボタン入力を試したことにはなりません。次に実機で試します。

# STEP 6｜Questで一連の操作を試そう

1. File → Build Profilesで<b>05\_RingTossだけをScene Listで有効</b>にします。
2. Run DeviceでQuestを選び、Build And Runで <code>Builds/05_RingToss.apk</code> に保存します。
3. 輪を投げ、右の<b>A</b>を押して戻します。もう一度掴んで投げます。
4. 成功したら点数が1増えること、Aでは点数が消えないことを確認します。
5. 右の<b>B</b>を押し、点数が0になって輪も戻ることを確認します。
6. 輪を掴んだままA・Bを押し、輪が移動せず、点数も消えないことを確認します。
7. Aを長押ししてから顔をゆっくり動かし、輪が追いかけてこないことを確認します。

<b>手を離す操作とA・Bは同時に押さず、離してから押します。</b> 頭の追跡が途切れている間も置き直しません。追跡が戻ったら、ボタンをいったん離して押し直します。

## 次の人へ交代するとき

輪を離し、次の人が装着して前を向いてからBを押します。点数が0で輪が届く位置にあることを確認して始めます。的と看板は固定位置なので、全員が同じ場所・方向から遊びます。

# 困ったとき

| 症状 | 確認するところ |
| --- | --- |
| 文字が見つからない | ScoreCanvasがWorld Spaceか、Position・Rotation・Scaleが表どおりか |
| 四角い記号になる／文字が出ない | TMP EssentialsをImportしたか、Font Assetが入っているか。まず教材と同じ英数字で試す |
| 点数が変わらない | RingScoreDisplayのTarget・LabelがNoneでないか。第4章の得点判定が動くか |
| A・Bを押しても戻らない | Head・Targetの登録、右手のボタン、輪を離しているか、頭の追跡を確認 |
| 戻した輪がすぐ飛ぶ | linearVelocityとangularVelocityを0へ戻す行、解放直後のガードを確認 |
| 戻した後に点が入らない | 戻せた後にBeginNextThrowが呼ばれているか。scoredThisThrowをリセットしているか |

## 最後の確認

- [ ] 左右で掴み、投げられる。
- [ ] 的に入ると1点増え、外れや同じ投球で誤加点しない。
- [ ] Aで戻す→再投球を10回繰り返せる。
- [ ] Aを押しても累計点数は残る。
- [ ] Bで点数と輪を戻し、次の人が始められる。
- [ ] 保持中や長押しで意図しない移動が起きない。
- [ ] Questで点数と操作説明が読める。
- [ ] 保存した05\_RingTossから最後のAPKをビルドした。

<b>今日の保存点：</b><b><code>Assets/Scenes/05_RingToss.unity</code></b>

これで基本の輪投げが一周つながりました。仕組みを説明できたら、棒までの距離を一つだけ変えて遊びやすさを比べてみましょう。元の完成シーンは残しておきます。
