using UnityEngine;
using UnityEngine.UI;

public class ChangeIconSprite : MonoBehaviour
{
    private Animator anim;

    private void Awake()
    {
        anim = GetComponent<Animator>();
    }
    private void ChangeFHIcon()
    {
        transform.Find("Open").GetComponent<Image>().sprite = FoodPL.instance.FHIcons[anim.GetBool("Open Food Holder") ? 1 : 0];
    }
}
