using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// NavMeshAgentコンポーネントがアタッチされていない場合アタッチ
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public string enemyName;
    public Color color;

    public bool isTargetted;
    public bool isDown; //ダウンフラグ
    public bool isDead; //死亡フラグ

    

    Rigidbody rb;
    Animator anim;
    [SerializeField] [Tooltip("巡回する地点の配列")] Transform[] waypoints;
    NavMeshAgent navMeshAgent;
    int currentWaypointIndex;

    public enum EnemyAiState
    {
        WAIT,         //行動を一旦停止
        MOVE,         //移動（適当な徘徊）
        ATTACK,       //攻撃
        TARGET,       //ターゲッティング
        CHASE,        //プレイヤーを追跡
        IDLE,         //待機
        AVOID,        //回避
        DOWN,         //気絶
        DEAD,         //死亡
    }
    public EnemyAiState aiState = EnemyAiState.WAIT;
    public EnemyAiState nextState;
    bool wait; //思考ルーチンを強制停止させるフラグ
    bool isAiStateRunning;//AIが作動しているか

    GameObject player; //プレイヤーオブジェクト
    float distance; //プレイヤーとの距離

    bool isTargetting; //ターゲットフラグ
    float attackRange; //攻撃距離
    float targettingTime; //ターゲッティング時間（チャージ時間）
    public bool isAttack; //攻撃フラグ
    public int e_str = 10; //攻撃力

    float downTime = 3f; //ダウンタイムの上限
    float downTimer; //ダウンタイムのカウント

    [SerializeField] float realizeDistance = 3f; //プレイヤーを認識する距離
    [SerializeField] float missingDistance = 4f;//プレイヤーを見失う距離
    [SerializeField] float maintainDistance = 2f; //プレイヤーと保つ距離
    bool isChasing;

    [SerializeField] int level = 1;
    GameObject[] meatsList;

    // Start is called before the first frame update
    void Start()
    {
        meatsList = new GameObject[level];
        GetMeats();
        ApplyMeatsColor();
        player = GameObject.Find("Player");
        isTargetted = false;
        isDown = false;


        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        navMeshAgent = GetComponent<NavMeshAgent>();

        // 最初の目的地を入れる
        navMeshAgent.SetDestination(waypoints[0].position);
    }

    // Update is called once per frame
    void Update()
    {
        TargetCube();
        AnimationFlagController();

        distance = Vector3.Distance(transform.position, player.transform.position);

        UpdateAI();
    }

    //AIの準備を行う関数
    void SetAi()
    {
        //AIが作動していれば何もしない
        if (isAiStateRunning)
        {
            return;
        }
        //AIが作動していない場合はAIの初期化、メインルーチンの作動、ステートの遷移、インターバル
        InitAi();
        AiMainRoutine();
        aiState = nextState;
        StartCoroutine("AiTimer");
    }

    //AIの思考のインターバルを決める関数
    IEnumerator AiTimer()
    {
        isAiStateRunning = true;
        //Debug.Log("インターバル中です。");
        yield return new WaitForSeconds(0.5f);//次の行動に移るまで0.5秒待つ
        isAiStateRunning = false;
    }

    //AIの初期化を行う関数
    void InitAi()
    {
        wait = false;
        isAiStateRunning = false;
        isChasing = false;
    }

    //AIのメインルーチン
    void AiMainRoutine()
    {
        if (wait)
        {
            //停止フラグがたった場合はWAITステートに遷移し,その後の判断は行わない
            nextState = EnemyAiState.WAIT;
            wait = false;
            return;
        }

        
        if (isDown && !isDead)
        {
            nextState = EnemyAiState.DOWN;
        }
        else if (isDead)
        {
            nextState = EnemyAiState.DEAD;
        }
        else if (isAttack)
        {
            nextState = EnemyAiState.ATTACK;
        }
        else if (isTargetting)
        {
            nextState = EnemyAiState.TARGET;
        }
        else if(distance <= maintainDistance + 0.5)
        {
            float rdm = Random.value;
            if(rdm <= 0.9f)
            {
                attackRange = Random.value;
                targettingTime = 0;
                nextState = EnemyAiState.TARGET;
            } else
            {
                nextState = EnemyAiState.WAIT;
            }
        }
        else if (distance <= realizeDistance && distance > maintainDistance && !isChasing) 
        {
            nextState = EnemyAiState.CHASE;
        } 
        else if(distance >= missingDistance && isChasing)
        {
            nextState = EnemyAiState.WAIT;
        } 
        else
        {
            nextState = EnemyAiState.MOVE;
        }
    }

    void UpdateAI()
    {
        SetAi();
        //Debug.Log("現在のステート:" + aiState);
        switch (aiState)
        {
            case EnemyAiState.WAIT:
                Wait();
                break;
            case EnemyAiState.MOVE:
                Move();
                break;
            case EnemyAiState.CHASE:
                Chace();
                break;
            case EnemyAiState.IDLE:
                Idle();
                break;
            case EnemyAiState.TARGET:
                Target();
                break;
            case EnemyAiState.ATTACK:
                Attack();
                break;
            case EnemyAiState.DOWN:
                Down();
                break;
            case EnemyAiState.DEAD:
                Dead();
                break;
        }
    }

    void Wait()
    {
        wait = true;
        //Debug.Log("敵のAIが停止しました。");
    }
    
    void Move()
    {
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) //両方ともnavMeshAgentのフィールド、つまり目的地に近づいたら自動的に次の目的地の指定、移動を開始する
        {
            // 目的地の番号を１更新（右辺を剰余演算子にすることで目的地をループさせれる）
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            // 目的地を次の場所に設定
            navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Chace()
    {
        //方向を求める
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        //目的地（プレイヤーから間合い分離れた位置）を求める
        Vector3 destinationPosition = player.transform.position - directionToPlayer * maintainDistance;

        isChasing = true;
        navMeshAgent.SetDestination(destinationPosition);
        //Debug.Log("敵があなたを追跡しています。");
    }

    void Idle()
    {

    }

    void Target()
    {
        isTargetting = true;
        navMeshAgent.updatePosition = false;
        navMeshAgent.updateRotation = false;
        transform.LookAt(player.transform);
        //anim.SetBool("isTargetting", true);
        if(targettingTime < attackRange)
        {
            //player.transform.Find("TargetCube").gameObject.SetActive(true);
            player.transform.GetComponent<PlayerController>().isTargetted = true;
            targettingTime += Time.deltaTime;
        } else
        {
            //player.transform.Find("TargetCube").gameObject.SetActive(false);
            player.transform.GetComponent<PlayerController>().isTargetted = false;
            isTargetting = false;
            isAttack = true;
            //anim.SetBool("isTargetting", false);
        }
        
    }

    void Attack()
    {
        if(targettingTime > 0)
        {
            targettingTime -= Time.deltaTime;
            rb.MovePosition(rb.position + transform.forward * 0.1f);
        } else
        {
            isAttack = false;
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = true;
        }
    }

    void Down()
    {
        isAttack = false;
        isChasing = false;
        isTargetting = false;

        if (downTimer < downTime)
        {
            //isAttack = false;
            downTimer += Time.deltaTime;
            navMeshAgent.updatePosition = false;
            navMeshAgent.updateRotation = false;
        }
        else
        {
            downTimer = 0;
            isDown = false;
            transform.GetComponent<EnemyHpBar>().RecoveryHp(10);
            navMeshAgent.updatePosition = true;
            navMeshAgent.updateRotation = true;
        }
    }

    void Dead()
    {
        isAttack = false;
        isChasing = false;
        isDown = false;
        isTargetting = false;
        isTargetted = false;
        player.transform.GetComponent<PlayerController>().isTargetted = false;

        Destroy(meatsList[0]);
        transform.Find("SE").GetComponent<AudioSource>().volume = 0;
        transform.Find("UJaw").GetComponent<MeshRenderer>().material.color = Color.black;
        transform.Find("LJaw").GetComponent<MeshRenderer>().material.color = Color.black;
        navMeshAgent.Stop(true);
        rb.isKinematic = false;
        transform.Find("EnemyCanvas").gameObject.SetActive(false);
        Destroy(this.gameObject, 3f);
    }

    void TargetCube()
    {
        if (isTargetted)
        {
            transform.Find("TargetCube").gameObject.SetActive(true);
        }
        else
        {
            transform.Find("TargetCube").gameObject.SetActive(false);
        }
    }

    void GetMeats()
    {
        GameObject meats = transform.Find("Meats").gameObject;

        for (int i = 0; i < level; i++)
        {
            meatsList[i] = meats.transform.GetChild(i).gameObject;
        }
    }

    void ApplyMeatsColor()
    {
        foreach(GameObject ob in meatsList)
        {
            ob.transform.GetComponent<MeshRenderer>().material.color = color;
        }
    }

    void AnimationFlagController()
    {
        if (isTargetting) anim.SetBool("isTargetting", true);
        else anim.SetBool("isTargetting", false);

        if (isAttack) anim.SetBool("isAttack", true);
        else anim.SetBool("isAttack", false);

        if (isDown) anim.SetBool("isDown", true);
        else anim.SetBool("isDown", false);
    }
}
