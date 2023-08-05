using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnemyHpBar : MonoBehaviour
{
    [SerializeField]int e_maxHp = 100;
    int e_currentHp;
    [SerializeField] Slider e_slider;
    [SerializeField] Text e_hpText;

    // Start is called before the first frame update
    void Start()
    {
        e_slider.value = 1;
        e_currentHp = e_maxHp;
        e_hpText.text = "�n���@" + e_maxHp + "/" + e_maxHp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ManageHp(int damage)
    {
        if(e_currentHp > damage)
        {
            e_currentHp = e_currentHp - damage;
        }
        else
        {
            e_currentHp = 0;
            transform.GetComponent<EnemyController>().isDown = true;
        }
        e_hpText.text = "�n���@" + e_currentHp + "/" + e_maxHp;

        //�ő�HP�ɂ����錻�݂�HP��Slider�ɔ��f�B
        //int���m�̊���Z�͏����_�ȉ���0�ɂȂ�̂ŁA
        //(float)������float�̕ϐ��Ƃ��ĐU���킹��B
        e_slider.value = (float)e_currentHp / (float)e_maxHp;
    }

    public void RecoveryHp(int damage)
    {
        e_currentHp = e_currentHp + damage;
        e_hpText.text = "�n���@" + e_currentHp + "/" + e_maxHp;
        e_slider.value = (float)e_currentHp / (float)e_maxHp;
    }
}
