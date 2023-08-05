using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// NavMeshAgent�R���|�[�l���g���A�^�b�`����Ă��Ȃ��ꍇ�A�^�b�`
[RequireComponent(typeof(NavMeshAgent))]
public class EnemyController : MonoBehaviour
{
    public string enemyName;
    public Color color;

    public bool isTargetted;
    public bool isDown; //�_�E���t���O
    public bool isDead; //���S�t���O

    

    Rigidbody rb;
    Animator anim;
    [SerializeField] [Tooltip("���񂷂�n�_�̔z��")] Transform[] waypoints;
    NavMeshAgent navMeshAgent;
    int currentWaypointIndex;

    public enum EnemyAiState
    {
        WAIT,         //�s������U��~
        MOVE,         //�ړ��i�K���Ȝp�j�j
        ATTACK,       //�U��
        TARGET,       //�^�[�Q�b�e�B���O
        CHASE,        //�v���C���[��ǐ�
        IDLE,         //�ҋ@
        AVOID,        //���
        DOWN,         //�C��
        DEAD,         //���S
    }
    public EnemyAiState aiState = EnemyAiState.WAIT;
    public EnemyAiState nextState;
    bool wait; //�v�l���[�`����������~������t���O
    bool isAiStateRunning;//AI���쓮���Ă��邩

    GameObject player; //�v���C���[�I�u�W�F�N�g
    float distance; //�v���C���[�Ƃ̋���

    bool isTargetting; //�^�[�Q�b�g�t���O
    float attackRange; //�U������
    float targettingTime; //�^�[�Q�b�e�B���O���ԁi�`���[�W���ԁj
    public bool isAttack; //�U���t���O
    public int e_str = 10; //�U����

    float downTime = 3f; //�_�E���^�C���̏��
    float downTimer; //�_�E���^�C���̃J�E���g

    [SerializeField] float realizeDistance = 3f; //�v���C���[��F�����鋗��
    [SerializeField] float missingDistance = 4f;//�v���C���[������������
    [SerializeField] float maintainDistance = 2f; //�v���C���[�ƕۂ���
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

        // �ŏ��̖ړI�n������
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

    //AI�̏������s���֐�
    void SetAi()
    {
        //AI���쓮���Ă���Ή������Ȃ�
        if (isAiStateRunning)
        {
            return;
        }
        //AI���쓮���Ă��Ȃ��ꍇ��AI�̏������A���C�����[�`���̍쓮�A�X�e�[�g�̑J�ځA�C���^�[�o��
        InitAi();
        AiMainRoutine();
        aiState = nextState;
        StartCoroutine("AiTimer");
    }

    //AI�̎v�l�̃C���^�[�o�������߂�֐�
    IEnumerator AiTimer()
    {
        isAiStateRunning = true;
        //Debug.Log("�C���^�[�o�����ł��B");
        yield return new WaitForSeconds(0.5f);//���̍s���Ɉڂ�܂�0.5�b�҂�
        isAiStateRunning = false;
    }

    //AI�̏��������s���֐�
    void InitAi()
    {
        wait = false;
        isAiStateRunning = false;
        isChasing = false;
    }

    //AI�̃��C�����[�`��
    void AiMainRoutine()
    {
        if (wait)
        {
            //��~�t���O���������ꍇ��WAIT�X�e�[�g�ɑJ�ڂ�,���̌�̔��f�͍s��Ȃ�
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
        //Debug.Log("���݂̃X�e�[�g:" + aiState);
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
        //Debug.Log("�G��AI����~���܂����B");
    }
    
    void Move()
    {
        if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance) //�����Ƃ�navMeshAgent�̃t�B�[���h�A�܂�ړI�n�ɋ߂Â����玩���I�Ɏ��̖ړI�n�̎w��A�ړ����J�n����
        {
            // �ړI�n�̔ԍ����P�X�V�i�E�ӂ���]���Z�q�ɂ��邱�ƂŖړI�n�����[�v�������j
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            // �ړI�n�����̏ꏊ�ɐݒ�
            navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Chace()
    {
        //���������߂�
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;
        //�ړI�n�i�v���C���[����ԍ��������ꂽ�ʒu�j�����߂�
        Vector3 destinationPosition = player.transform.position - directionToPlayer * maintainDistance;

        isChasing = true;
        navMeshAgent.SetDestination(destinationPosition);
        //Debug.Log("�G�����Ȃ���ǐՂ��Ă��܂��B");
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
