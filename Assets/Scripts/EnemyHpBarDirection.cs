using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBarDirection : MonoBehaviour
{

    public Canvas canvas;

    void Update()
    {
        //EnemyCanvas��Main Camera�Ɍ�������
        canvas.transform.rotation =
            Camera.main.transform.rotation;
    }
}
