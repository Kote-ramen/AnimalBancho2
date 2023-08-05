using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBarDirection : MonoBehaviour
{

    public Canvas canvas;

    void Update()
    {
        //EnemyCanvasをMain Cameraに向かせる
        canvas.transform.rotation =
            Camera.main.transform.rotation;
    }
}
