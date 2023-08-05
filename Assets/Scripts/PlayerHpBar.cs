using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHpBar : MonoBehaviour
{
    [SerializeField] int maxHp = 100;//�ő�HP
    int currentHp;//���݂�HP

    public Slider slider;
    public Image image;
    public Text hpText;

    // Start is called before the first frame update
    void Start()
    {
        //slider�̒�����maxHp�ɂ��킹�ĕύX����
        //slider.transform.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, maxHp);
        //Slider�𖞃^���ɂ���
        slider.value = 1;
        //���݂�HP���ő�HP�Ɠ����ɂ���
        currentHp = maxHp;
        //HP�̃e�L�X�g��������
        hpText.text = "�n�� " + maxHp + "/" + maxHp;
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
        hpText.text = "�n��" + currentHp + "/" + maxHp;
    }

    public void RecoveryHp(int damage)
    {
        currentHp = currentHp + damage;
        if(currentHp > maxHp)
        {
            currentHp = maxHp;
        }
        slider.value = (float)currentHp / (float)maxHp;
        hpText.text = "�n��" + currentHp + "/" + maxHp;
    }

    public void ManageColorUI(Color color)
    {
        image.color = color;
    }
}
