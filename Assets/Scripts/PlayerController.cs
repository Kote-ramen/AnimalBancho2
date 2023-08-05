using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    static string name = "�C�\�[���[";
    [SerializeField] float moveSpeed = 3f;//�ړ����x

    float h; //�O�����
    float v; //���E����
    bool isMoving; //�ړ��t���O
    bool isGrounded;//�n�ʃt���O
    bool isTargetting = false;//�^�[�Q�b�g�t���O
    bool isJump;//�W�����v�t���O
    float jumpTime; //�W�����v���Ă���̎��Ԃ��i�[
    float delayTimeToLanding; //�W�����v��̒��n����܂ł̒x������
    [SerializeField] float jumpDistance; //�W�����v�̍���

    public bool isAttack = false; //�U���t���O
    float attackRange = 0f; //�U������
    [SerializeField] float maxAttackRagnge;
    float attackTime; //�U�����Ă���̎��Ԃ��i�[
    Vector3 attackDirection;//�U���������i�[
    public int str = 50;//�U����

    Vector3 groundPositionOffset = new Vector3(0f, 0.5f, 0f);
    float groundColliderRadius = 0.5f;

    Animator anim;
    Rigidbody rb;
    Vector3 moveDirection;
    Vector3 cmrDirection;
    Vector3 latestPos;
    GameObject cmr;
    GameObject target;//�^�[�Q�b�g�I�u�W�F�N�g�̊i�[
    public bool isDown;//�_�E���t���O
    public bool gameover;//�Q�[���I�[�o�[�t���O
    float downTimer;//�_�E���^�C���̃J�E���g
    float downTime = 2f;//�_�E���^�C���̏��

    public bool isTargetted;//��^�[�Q�b�g�t���O

    [SerializeField] GameObject poopCube;//�t���̃I�u�W�F�N�g

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

        // v=v_0+at, x=v_0t+v_0t^2/2 ��p���ăW�����v���Ă��璅�n�܂ł̎���t_1�����߂�
        // t_1 = -2��-2ax/a��莟�̂悤�Ɍv�Z����
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
        else if (isGrounded)//�n��ɂ���ꍇ
        {
            h = Input.GetAxis("Horizontal");
            v = Input.GetAxis("Vertical");

            cmrDirection = Vector3.Scale(cmr.transform.forward, new Vector3(1, 0, 1)).normalized;
            moveDirection = v * cmrDirection + h * cmr.transform.right;
            moveDirection = moveDirection * moveSpeed;

            //�W�����v����
            if (Input.GetButtonDown("Jump"))
            {
                isGrounded = false;
                isJump = true;
                jumpTime = 0f;
                moveDirection.y = Mathf.Sqrt(-2 * Physics.gravity.y * jumpDistance);
            }

            //�^�[�Q�b�e�B���O�����i�^�[�Q�b�e�B���O���ɃW�����v�����ꍇ�̏����𖢎����j
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

            //�U�����͏���
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

            //�E������
            if (Input.GetKeyUp(KeyCode.LeftControl)){
                Excretion();
            }
        }
        

        //�@�W�����v���Ԃ̌v��
        if (isJump && jumpTime < delayTimeToLanding)
        {
            jumpTime += Time.deltaTime;
        }

        //���͕����ɐ��ʂ�������
        Vector3 tmp = Vector3.Scale(transform.position, new Vector3(1, 0, 1)) - latestPos;
        latestPos = Vector3.Scale(transform.position, new Vector3(1, 0, 1));  //�O���Position�̍X�V
        if (isTargetting)
        {
            transform.LookAt(target.transform);
        } else
        {
            //�x�N�g���̑傫����0.01�ȏ�̎��Ɍ�����ς��鏈��������(�������Ȃ��̏ꍇ���͏I���Ɠ����ɐ��ʂ������Ă��܂�)
            if (tmp.magnitude > 0.01f)
            {
                isMoving = true;
                transform.rotation = Quaternion.LookRotation(tmp); //������ύX����
            }
            else
            {
                isMoving = false;
            }
        }

    }

    private void FixedUpdate()
    {
        //�v���C���[�̈ړ�����
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

    //�U���Ȃǂ̕������莞�̏����̓���
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
        // �v���C���[��HP�Ǘ���ق��̕�������̏����͂����ōs��
    }

    void GetMeats()
    {
        GameObject meats = transform.Find("Meats").gameObject;

        for(int i=0; i<level; i++)
        {
            meatsList[i] = meats.transform.GetChild(i).gameObject;
        }
    } //���I�u�W�F�N�g�̎擾�A�������p

    void Excretion()
    {
        if(lastColorIdx >= 1)
        {
            meatsList[lastColorIdx - 1].GetComponent<MeshRenderer>().material.color = Color.white;//�E���ɂ��F�̏�����
            playerHpBar.ManageColorUI(Color.white); //�E���ɂ��F�Ǘ�UI�̏�����
            Instantiate(poopCube, new Vector3(transform.position.x, 0.2f, transform.position.z), Quaternion.identity);
            lastColorIdx -= 1;
        } else
        {

        }
    } //�r�����̐F�Ǘ����\�b�h

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
    }  //��^�[�Q�b�g���ɃA�C�R���̕\���p���\�b�h

    void CheckGround()
    {
        //�@�n�ʂɐڒn���Ă��邩�m�F
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
        //LookAt�Ń^�[�Q�b�g�̕�������
        if (target != null)
        {
            target.transform.Find("TargetCone").gameObject.SetActive(true);
            transform.LookAt(target.transform);
        }
        else
        {
            isTargetting = false;
            Debug.Log("���͂ɓG�͂��܂���B");
        }
    }

    GameObject SerchWithTag(GameObject nowObj, string tag)
    {
        GameObject targetObj = null;
        float tmpDis; //�����p�ꎞ�ϐ�
        float nearDis = 0;//�ł��߂��I�u�W�F�N�g�̋���

        foreach (GameObject obj in GameObject.FindGameObjectsWithTag(tag))
        {
            //���g�Ǝ擾�����I�u�W�F�N�g�̋������擾
            tmpDis = Vector3.Distance(obj.transform.position, nowObj.transform.position);

            //�I�u�W�F�N�g�̋������߂����A����0�ł���΃I�u�W�F�N�g�����擾
            //�ꎞ�ϐ��ɋ������i�[
            if (nearDis == 0 || nearDis > tmpDis)
            {
                nearDis = tmpDis;
                //nearObjName = obs.name;
                targetObj = obj;
            }
        }
        //�ł��߂������I�u�W�F�N�g��Ԃ�
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
    } //�ߐH���̐F�Ǘ����\�b�h

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
