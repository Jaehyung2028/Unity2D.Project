using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SoulBox : MonoBehaviour
{
    [SerializeField] GameObject Idle, Open, Key;
    [SerializeField] ParticleSystem GetEffet;
    bool Close = false, Active = true, InPlayer = false;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
            InPlayer = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.collider.tag == "Player")
            InPlayer = false;
    }

    IEnumerator KeyOpen()
    {
        if (Player.Instance.Scroll > 0)
        {
            if (!Close)
            {
                GetEffet.Play();
                Idle.SetActive(false);
                Open.SetActive(true);
                Key.SetActive(true);

                yield return new WaitForSeconds(1);

                Close = true;
            }
        }
        else
        {
            StartCoroutine(ButtonManager.instance.AlarmString("Scrolls must be collected."));
        }
    }

    private void Update()
    {
        if (InPlayer && Input.GetKeyDown(KeyCode.E) && Close && Player.Instance.Scroll > 0 && Active)
        {
            Key.SetActive(false);
            Active = false;
            Player.Instance.Soul++;
            Player.Instance.Scroll--;
            ButtonManager.instance.SoulText.text = "X " + Player.Instance.Soul;
            ButtonManager.instance.ItemText.text = "X " + Player.Instance.Scroll;

            if (Player.Instance.Soul == Map.Instance.HiddenCount)
                StartCoroutine(ButtonManager.instance.AlarmString("The boss room has been opened."));
        }
        else if(InPlayer && Input.GetKeyDown(KeyCode.E) && !Close && Active)
        {
            StartCoroutine(KeyOpen());
        }
    }
}
