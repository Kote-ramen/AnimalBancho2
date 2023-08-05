using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    private GameObject player;//playerオブジェクト
    private Vector3 offset;//playerとのズレ
    [SerializeField] private float cmrTurnRad = 1f; //カメラの回転速度
    private PlayerController playerController;


    // Start is called before the first frame update
    void Start()
    {
        //プレイヤーオブジェクトを取得
        player = GameObject.Find("Player");

        playerController = player.GetComponent<PlayerController>();

        //プレイヤーとカメラの位置関係(距離,向き)を取得
        offset = transform.position - player.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //レイキャストでカメラとプレイヤーの間に壁があった場合壁を半透明にする。
        Debug.DrawLine(transform.position, player.transform.position, Color.red, 0f, false);
    }

    private void LateUpdate()
    {
        //プレイヤーの位置からoffsetずれた位置へ移動
        transform.position = player.transform.position + offset;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            //プレイヤーを中心に-5f度回転
            this.transform.RotateAround(player.transform.position, Vector3.up, -1 * cmrTurnRad);
        }
        //右シフトが押されている時
        else if (Input.GetKey(KeyCode.RightShift))
        {
            //プレイヤーを中心に5f度回転
            this.transform.RotateAround(player.transform.position, Vector3.up, cmrTurnRad);
        }
        offset = transform.position - player.transform.position;
    }
}
