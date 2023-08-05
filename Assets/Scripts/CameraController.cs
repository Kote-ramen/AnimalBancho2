using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject player;//player�I�u�W�F�N�g
    private Vector3 offset;//player�Ƃ̃Y��
    [SerializeField] private float cmrTurnRad = 1f; //�J�����̉�]���x
    private PlayerController playerController;


    // Start is called before the first frame update
    void Start()
    {
        //�v���C���[�I�u�W�F�N�g���擾
        player = GameObject.Find("Player");

        playerController = player.GetComponent<PlayerController>();

        //�v���C���[�ƃJ�����̈ʒu�֌W(����,����)���擾
        offset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //���C�L���X�g�ŃJ�����ƃv���C���[�̊Ԃɕǂ��������ꍇ�ǂ𔼓����ɂ���B
        Debug.DrawLine(transform.position, player.transform.position, Color.red, 0f, false);
    }

    private void LateUpdate()
    {
        //�v���C���[�̈ʒu����offset���ꂽ�ʒu�ֈړ�
        transform.position = player.transform.position + offset;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //�v���C���[�𒆐S��-5f�x��]
            this.transform.RotateAround(player.transform.position, Vector3.up, -1 * cmrTurnRad);
        }
        //�E�V�t�g��������Ă��鎞
        else if (Input.GetKey(KeyCode.RightShift))
        {
            //�v���C���[�𒆐S��5f�x��]
            this.transform.RotateAround(player.transform.position, Vector3.up, cmrTurnRad);
        }
        offset = transform.position - player.transform.position;
    }
}
