using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PushNotice : MonoBehaviour
{
    public static PushNotice instance;
    private Animator anim;
    public float countdown = 0;
    public float timer;
    public bool sTimer;

    private void Awake()
    {
        instance = this;
        anim = GetComponent<Animator>();
    }

    private void Update()
    {
        if (sTimer)
        {
            countdown += Time.deltaTime;
            if (countdown >= timer) { sTimer = false; CloseNotice(); }
        }
    }

    public void Push(string text, PushType type)
    {
        transform.Find("Push Notice Surface/Text").GetComponent<TextMeshProUGUI>().text = text;
        if (type == PushType.Alert)
            transform.Find("Push Notice Surface").GetComponent<Image>().color = new Color32(200, 100, 0, 255);
        else if (type == PushType.Notice)
            transform.Find("Push Notice Surface").GetComponent<Image>().color = new Color32(0, 100, 255, 255);
        else if (type == PushType.Achivement)
            transform.Find("Push Notice Surface").GetComponent<Image>().color = new Color32(0, 100, 0, 255);

        anim.SetBool("Push Notice", true);
    }

    private void StartTimer()
    {
        countdown = 0;
        sTimer = true;
    }

    public void CloseNotice()
    {
        anim.SetBool("Push Notice", false);
        countdown = 0;
        sTimer = false;
    }
}
