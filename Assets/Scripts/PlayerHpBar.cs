using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpBar : MonoBehaviour
{
    [SerializeField] int maxHp = 100;//最大HP
    int currentHp;//現在のHP

    public Slider slider;
    public Image image;
    public Text hpText;

    // Start is called before the first frame update
    void Start()
    {
        //sliderの長さをmaxHpにあわせて変更する
        //slider.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxHp);
        //Sliderを満タンにする
        slider.value = 1;
        //現在のHPを最大HPと同じにする
        currentHp = maxHp;
        //HPのテキストを初期化
        hpText.text = "ハラ " + maxHp + "/" + maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ManageHp(int damage)
    {
        currentHp = currentHp - damage;
        if(currentHp <= 0)
        {
            currentHp = 0;
            transform.GetComponent<PlayerController>().isDown = true;
        }
        slider.value = (float)currentHp / (float)maxHp;
        hpText.text = "ハラ" + currentHp + "/" + maxHp;
    }

    public void RecoveryHp(int damage)
    {
        currentHp = currentHp + damage;
        if(currentHp > maxHp)
        {
            currentHp = maxHp;
        }
        slider.value = (float)currentHp / (float)maxHp;
        hpText.text = "ハラ" + currentHp + "/" + maxHp;
    }

    public void ManageColorUI(Color color)
    {
        image.color = color;
    }
}
