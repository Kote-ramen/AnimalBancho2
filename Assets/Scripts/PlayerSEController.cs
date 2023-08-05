using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerSEController : MonoBehaviour
{
    AudioSource audioSource;
    [SerializeField] [Tooltip("���펞")] AudioClip audioClip_1;
    [SerializeField] [Tooltip("�U��")] AudioClip audioClip_2;
    [SerializeField] [Tooltip("�ړ�")] AudioClip audioClip_3;


    // Start is called before the first frame update
    void Start()
    {
        audioSource = transform.Find("SE").GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void IdleSE()
    {
        audioSource.PlayOneShot(audioClip_1);
    }

    public void AttackSE()
    {
        audioSource.PlayOneShot(audioClip_2);
    }

    public void MoveSE()
    {
        audioSource.PlayOneShot(audioClip_3);
    }
}
