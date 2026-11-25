**ゴール：輪が棒の根元に収まったとき、1回だけ点数を増やそう。**

前の章：<mention-page url="https://app.notion.com/p/40121a05110d819597c5d5ec4370c872"/>

第3章の `Assets/Scenes/03_Target.unity` から始めます。この章では点数をConsoleで確認します。Quest内に点数を表示するのは次の章です。

# STEP 1｜「成功」の条件を決めよう

棒に触れただけでは得点にしません。今回は次の条件が**全部そろった状態が0.5秒続いたら1点**にします。

- 輪を掴んでいない。
- 輪の中心が棒から横に0.075m以内。
- 輪の中心の高さがTargetから0.10〜0.18m。
- 輪がほぼ水平で、動きと回転が十分に小さい。
- 今の投球では、まだ得点していない。

この教材では「最後に輪が収まった状態」を判定します。手で置いて離した場合も条件を満たせば得点します。

1. Playを停止します。
2. File → Save Asで **`Assets/Scenes/04_Score.unity`** に保存します。

# STEP 2｜判定コードを作ろう

1. Projectの **Assets/Scripts** を開きます。
2. 右クリック → Create → Scripting → MonoBehaviour Script。
3. 名前を **RingScore** にします。
4. ダブルクリックして開き、次のコードへ置き換えて保存します。

```cs
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

// Targetに付ける。輪の中心・傾き・速さを毎回調べる。
public class RingScore : MonoBehaviour
{
    [SerializeField] private Rigidbody ring;
    [SerializeField] private float centerTolerance = 0.075f;
    [SerializeField] private float minHeight = 0.10f;
    [SerializeField] private float maxHeight = 0.18f;
    [SerializeField] private float uprightDot = 0.95f;
    [SerializeField] private float maxSpeed = 0.15f;
    [SerializeField] private float maxSpin = 0.5f;
    [SerializeField] private float settleSeconds = 0.5f;

    private XRGrabInteractable grab;
    private bool scoredThisThrow;
    private float settledTime;
    public int Score { get; private set; }

    private void Awake()
    {
        if (ring == null || !ring.TryGetComponent(out grab))
        {
            Debug.LogError("RingにRigidbody付きの輪を登録してください。", this);
            enabled = false;
            return;
        }
    }

    private void FixedUpdate() => CheckScore(Time.fixedDeltaTime);

    public void CheckScore(float seconds)
    {
        if (ring == null || grab == null ||
            grab.isSelected || scoredThisThrow)
        {
            settledTime = 0f;
            return;
        }

        Vector3 local = transform.InverseTransformPoint(ring.position);
        float horizontalDistance = new Vector2(local.x, local.z).magnitude;
        bool inside = horizontalDistance <= centerTolerance &&
            local.y >= minHeight && local.y <= maxHeight;
        bool flat = Mathf.Abs(Vector3.Dot(ring.transform.up, transform.up)) >= uprightDot;
        bool slow = ring.linearVelocity.magnitude <= maxSpeed &&
            ring.angularVelocity.magnitude <= maxSpin;

        settledTime = inside && flat && slow ? settledTime + seconds : 0f;
        if (settledTime < settleSeconds) return;

        scoredThisThrow = true;
        Score++;
        Debug.Log("Score: " + Score, this);
    }

    // 輪を戻せたときだけ、次の投球を得点できるようにする。
    public void BeginNextThrow()
    {
        scoredThisThrow = false;
        settledTime = 0f;
    }

    public void BeginNewPlayer()
    {
        Score = 0;
        BeginNextThrow();
    }
}
```

## コードを3つに分けて読む

1. **条件を調べる：** `inside`は場所、`flat`は傾き、`slow`は速さの判定です。`&&`は「どちらも満たす」を表します。
2. **時間を数える：** 条件がそろっていれば`settledTime`へ時間を足し、外れたら0へ戻します。
3. **1回だけ加点する：** `scoredThisThrow`をtrueにしてから`Score++`で1点増やします。次の確認では早めに処理を終えるので増え続けません。

`FixedUpdate`は物理の更新に合わせて繰り返し呼ばれます。`BeginNextThrow`と`BeginNewPlayer`は、次の章の再投球・交代で使います。

# STEP 3｜Inspectorで輪を登録しよう

1. Unityへ戻り、**Window → General → Console**を開きます。赤いコンパイルエラーがあれば、先に直します。
2. ProjectのRingScoreを、Hierarchyの**親のTarget**へドラッグします。BaseやPoleには付けません。
3. Targetを選び、Inspectorの**Ring Score**を開きます。
4. **Ring**欄へHierarchyの**親のRing**をドラッグします。Rigidbodyが参照されればOKです。
5. 次の値を確認し、Ctrl+Sで保存します。

| 項目 | 値 | 役割 |
| --- | --- | --- |
| Center Tolerance | 0.075 | 中心の横ずれの許容距離 |
| Min Height／Max Height | 0.10／0.18 | 成功になる高さ |
| Upright Dot | 0.95 | 水平に近い姿勢だけを許可 |
| Max Speed | 0.15 | 移動速度の上限（m/s） |
| Max Spin | 0.5 | 回転速度の上限（rad/s） |
| Settle Seconds | 0.5 | 条件を保つ時間（秒） |

**TargetのRotationは（0, 0, 0）、Scaleは（1, 1, 1）のまま使います。** 高さは親Targetを基準に測ります。輪のCollider16個へ判定スクリプトを追加する必要はありません。

# STEP 4｜正しく1点になるか試そう

第3章と同じ落下試験で、コードを確かめます。

1. Ringを選び、Place Ring At Startを一時的にOFFにします。
2. Position＝**（0, 1, 1.5）**、Rotation＝**（0, 0, 0）**、Use Gravity＝**ON**にします。
3. Consoleの**Clear**を押し、Playします。
4. 輪が土台に落ち着くと、Consoleに **Score: 1** が出るか確認します。
5. そのまま数秒待ち、Score: 2、Score: 3と増え続けないことを確認します。
6. Playを停止します。

**できたらOK：輪が収まった後にScore: 1が一度だけ出る。**

ConsoleのCollapseがONだと同じログがまとめられます。確認するときはOFFにすると回数を見やすくなります。

# STEP 5｜外れたときは得点しないか試そう

1. Play停止中に、RingのPositionを **（0.4, 1, 1.5）** へ変えます。
2. ConsoleをClearし、Playします。
3. 輪が棒の外側へ落ち、Scoreのログが出ないことを確認します。
4. Playを停止します。

## 試験用の変更を戻す

RingをPosition＝**（0, 1.2, 0.5）**、Rotation＝**（0, 0, 0）**、Use Gravity＝**OFF**、Place Ring At Start＝**ON**へ戻し、Ctrl+Sで保存します。

Questで続ける場合は、Scene Listを**04_ScoreだけON**にして `Builds/04_Score.apk` をBuild And Runします。単体アプリのログはPCのEditor Consoleには自動表示されません。この章の点数確認はEditorで行い、次の章でQuest内にも表示します。

# 困ったとき

| 症状 | 確認するところ |
| --- | --- |
| スクリプトを追加できない | ファイル名とクラス名がRingScoreで一致するか。Consoleの最初の赤いエラーを直す |
| Ringを登録するエラーが出る | TargetのRing Score → Ringへ、RigidbodyとXR Grab Interactableがある親のRingを登録する |
| 正面から落としても点が入らない | Targetの位置・回転・Scale、Baseの高さ、判定値を前の表と照合 |
| 一度得点すると再加点しない | この章では1投分だけ。Playを止めて再実行すると0から始まる。再投球は次の章 |
| 点数が増え続ける | scoredThisThrowの条件と、加点前のtrue代入を確認。RingScoreが複数付いていないか |

## 余裕があれば

Settle Secondsを0.5から1へ変え、輪が落ち着いてから得点するまでの時間を比べます。確認後は0.5へ戻します。

## 完成チェック

- [ ] 真上から落とすと1点になった。
- [ ] 待ち続けても、同じ輪で点数が増え続けなかった。
- [ ] 外側へ落としたときは得点しなかった。
- [ ] 試験用の輪の設定とSettle Secondsを元へ戻した。
- [ ] 成功条件と、重複加点を防ぐ変数を説明できる。

**今日の保存点：`Assets/Scenes/04_Score.unity`**

