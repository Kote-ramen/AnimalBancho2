using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    static string name = "イソーロー";
    [SerializeField] float moveSpeed = 3f;//移動速度

    float h; //前後入力
    float v; //左右入力
    bool isMoving; //移動フラグ
    bool isGrounded;//地面フラグ
    bool isTargetting = false;//ターゲットフラグ
    bool isJump;//ジャンプフラグ
    float jumpTime; //ジャンプしてからの時間を格納
    float delayTimeToLanding; //ジャンプ後の着地判定までの遅延時間
    [SerializeField] float jumpDistance; //ジャンプの高さ

    public bool isAttack = false; //攻撃フラグ
    float attackRange = 0f; //攻撃距離
    [SerializeField] float maxAttackRagnge;
    float attackTime; //攻撃してからの時間を格納
    Vector3 attackDirection;//攻撃方向を格納
    public int str = 50;//攻撃力

    Vector3 groundPositionOffset = new Vector3(0f, 0.5f, 0f);
    float groundColliderRadius = 0.5f;

    Animator anim;
    Rigidbody rb;
    Vector3 moveDirection;
    Vector3 cmrDirection;
    Vector3 latestPos;
    GameObject cmr;
    GameObject target;//ターゲットオブジェクトの格納
    public bool isDown;//ダウンフラグ
    public bool gameover;//ゲームオーバーフラグ
    float downTimer;//ダウンタイムのカウント
    float downTime = 2f;//ダウンタイムの上限

    public bool isTargetted;//被ターゲットフラグ

    [SerializeField] GameObject poopCube;//フンのオブジェクト

    [SerializeField] int level;
    GameObject[] meatsList;
    int lastColorIdx;

    PlayerHpBar playerHpBar;
    [SerializeField] GameObject gameManager;



    // Start is called before the first frame update
    void Start()
    {
        meatsList = new GameObject[level];
        GetMeats();

        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
        cmr = GameObject.Find("Main Camera");
        playerHpBar = GetComponent<PlayerHpBar>();

        latestPos = Vector3.Scale(transform.position, new Vector3(1, 0, 1));

        // v=v_0+at, x=v_0t+v_0t^2/2 を用いてジャンプしてから着地までの時間t_1を求める
        // t_1 = -2√-2ax/aより次のように計算する
        delayTimeToLanding = -2 * Mathf.Sqrt(-2 * Physics.gravity.y * jumpDistance) / Physics.gravity.y;
    }

    // Update is called once per frame
    void Update()
    {
        AnimationFlagController();

        CheckGround();

        TargetCube();

        if (isDown)
        {
            rb.velocity = Vector3.zero;
            if(downTimer < downTime)
            {
                downTimer += Time.deltaTime;
            } else
            {
                downTimer = 0;
                isDown = false;
                playerHpBar.RecoveryHp(30);
            }
        }
        else if (isGrounded)//地上にいる場合
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            cmrDirection = Vector3.Scale(cmr.transform.forward, new Vector3(1, 0, 1)).normalized;
            moveDirection = v * cmrDirection + h * cmr.transform.right;
            moveDirection = moveDirection * moveSpeed;

            //ジャンプ処理
            if (Input.GetButtonDown("Jump"))
            {
                isGrounded = false;
                isJump = true;
                jumpTime = 0f;
                moveDirection.y = Mathf.Sqrt(-2 * Physics.gravity.y * jumpDistance);
            }

            //ターゲッティング処理（ターゲッティング中にジャンプした場合の処理を未実装）
            if (Input.GetKey(KeyCode.T) && !isTargetting)
            {
                target = SerchWithTag(gameObject, "Enemy");
                if(target != null)
                {
                    isTargetting = true;
                    target.transform.GetComponent<EnemyController>().isTargetted = true;
                } else
                {
                    
                }
                
            }
            if ((Input.GetKeyUp(KeyCode.T) && isTargetting) || isAttack)
            {
                if(target != null)
                {
                    target.transform.GetComponent<EnemyController>().isTargetted = false;
                }
                isTargetting = false;
            }

            //攻撃入力処理
            if(isTargetting && Input.GetKeyDown(KeyCode.Return) && !isAttack)
            {
                attackRange = 0;
            }
            if(isTargetting && Input.GetKey(KeyCode.Return) && !isAttack)
            {
                if(attackRange <= maxAttackRagnge)
                {
                    attackRange += Time.deltaTime;
                }
            }
            if(isTargetting && Input.GetKeyUp(KeyCode.Return) && !isAttack)
            {
                isAttack = true;
                attackTime = 0f;
                attackDirection = transform.forward;
                isTargetting = false;
            }

            //脱糞処理
            if (Input.GetKeyUp(KeyCode.LeftControl)){
                Excretion();
            }
        }
        

        //　ジャンプ時間の計測
        if (isJump && jumpTime < delayTimeToLanding)
        {
            jumpTime += Time.deltaTime;
        }

        //入力方向に正面を向ける
        Vector3 tmp = Vector3.Scale(transform.position, new Vector3(1, 0, 1)) - latestPos;
        latestPos = Vector3.Scale(transform.position, new Vector3(1, 0, 1));  //前回のPositionの更新
        if (isTargetting)
        {
            transform.LookAt(target.transform);
        } else
        {
            //ベクトルの大きさが0.01以上の時に向きを変える処理をする(条件つけなしの場合入力終了と同時に正面を向いてしまう)
            if (tmp.magnitude > 0.01f)
            {
                isMoving = true;
                transform.rotation = Quaternion.LookRotation(tmp); //向きを変更する
            }
            else
            {
                isMoving = false;
            }
        }

    }

    private void FixedUpdate()
    {
        //プレイヤーの移動処理
        if (!isAttack)
        {
            rb.MovePosition(rb.position + moveDirection * Time.fixedDeltaTime);
        }
        else
        {
            if(attackTime <= attackRange)
            {
                attackTime += Time.fixedDeltaTime;
                rb.MovePosition(rb.position + attackDirection * Time.fixedDeltaTime * 7f);
            } else
            {
                isAttack = false;
            }
        }
    }

    //攻撃などの物理判定時の処理の入力
    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.transform.tag == "Enemy")
        {
            Transform e_transform = collision.gameObject.transform;
            bool e_down = e_transform.GetComponent<EnemyController>().isDown;
            bool e_attack = e_transform.GetComponent<EnemyController>().isAttack;
            int e_str = e_transform.GetComponent<EnemyController>().e_str;
            if (isAttack && !e_attack)
            {
                if (e_down)
                {
                    gameManager.GetComponent<GameManager>().StageTextUpdate(e_transform.GetComponent<EnemyController>().enemyName);
                    Color tmpColor = e_transform.GetComponent<EnemyController>().color;
                    tmpColor.a = 255;
                    ColorManage(tmpColor);
                    playerHpBar.ManageColorUI(tmpColor);
                    playerHpBar.RecoveryHp(15);
                    e_transform.GetComponent<EnemyController>().isDead = true;
                    e_transform.tag = "Untagged";
                } else
                {
                    e_transform.GetComponent<EnemyHpBar>().ManageHp(str);
                }
               
                isAttack = false;
            } else if(!isAttack && e_attack)
            {
                if (isDown)
                {
                    gameManager.transform.GetComponent<GameManager>().gameOver = true;
                } else
                {
                    e_transform.GetComponent<EnemyController>().isAttack = false;
                    playerHpBar.ManageHp(e_str);
                }
                
            } else if(isAttack && e_attack)
            {
                e_transform.GetComponent<EnemyHpBar>().ManageHp(1);
                playerHpBar.ManageHp(1);
            } else
            {

            }
            
        }
        // プレイヤーのHP管理やほかの物理判定の処理はここで行う
    }

    void GetMeats()
    {
        GameObject meats = transform.Find("Meats").gameObject;

        for(int i=0; i<level; i++)
        {
            meatsList[i] = meats.transform.GetChild(i).gameObject;
        }
    } //肉オブジェクトの取得、初期化用

    void Excretion()
    {
        if(lastColorIdx >= 1)
        {
            meatsList[lastColorIdx - 1].GetComponent<MeshRenderer>().material.color = Color.white;//脱糞による色の初期化
            playerHpBar.ManageColorUI(Color.white); //脱糞による色管理UIの初期化
            Instantiate(poopCube, new Vector3(transform.position.x, 0.2f, transform.position.z), Quaternion.identity);
            lastColorIdx -= 1;
        } else
        {

        }
    } //排泄時の色管理メソッド

    void TargetCube()
    {
        if (isTargetted)
        {
            transform.Find("TargetCube").gameObject.SetActive(true);
            //anim.SetBool("isTargetted",true);
        } else
        {
            transform.Find("TargetCube").gameObject.SetActive(false);
            //anim.SetBool("isTargetted", false);
        }
    }  //被ターゲット時にアイコンの表示用メソッド

    void CheckGround()
    {
        //　地面に接地しているか確認
        if (Physics.CheckSphere(groundPositionOffset, groundColliderRadius, ~LayerMask.GetMask("Player")))
        {
            if (isJump)
            {
                if (jumpTime >= delayTimeToLanding)
                {
                    isGrounded = true;
                    isJump = false;
                }
                else
                {
                    isGrounded = false;
                }
            }
            else
            {
                isGrounded = true;
            }
        }
        else
        {
            isGrounded = false;
        }
    }

    void Target()
    {
        GameObject target = SerchWithTag(gameObject, "Enemy");
        //LookAtでターゲットの方を向く
        if (target != null)
        {
            target.transform.Find("TargetCone").gameObject.SetActive(true);
            transform.LookAt(target.transform);
        }
        else
        {
            isTargetting = false;
            Debug.Log("周囲に敵はいません。");
        }
    }

    GameObject SerchWithTag(GameObject nowObj, string tag)
    {
        GameObject targetObj = null;
        float tmpDis; //距離用一時変数
        float nearDis = 0;//最も近いオブジェクトの距離

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
        {
            //自身と取得したオブジェクトの距離を取得
            tmpDis = Vector3.Distance(obj.transform.position, nowObj.transform.position);

            //オブジェクトの距離が近いか、距離0であればオブジェクト名を取得
            //一時変数に距離を格納
            if (nearDis == 0 || nearDis > tmpDis)
            {
                nearDis = tmpDis;
                //nearObjName = obs.name;
                targetObj = obj;
            }
        }
        //最も近かったオブジェクトを返す
        return targetObj;
    }

    void ColorManage(Color color)
    {
        if(lastColorIdx >= meatsList.Length)
        {
            lastColorIdx -= 1;
        }

        meatsList[lastColorIdx].transform.GetComponent<MeshRenderer>().material.color
            = color;
        lastColorIdx += 1;
    } //捕食時の色管理メソッド

    void AnimationFlagController()
    {
        if (isMoving) anim.SetBool("isMoving", true);
        else anim.SetBool("isMoving", false);

        if (isTargetting) anim.SetBool("isTargetting", true);
        else anim.SetBool("isTargetting", false);

        if (isAttack) anim.SetBool("isAttack", true);
        else anim.SetBool("isAttack", false);

        //if (isDown) anim.SetBool("isDown", true);
        //else anim.SetBool("isDown", false);
    }
}
