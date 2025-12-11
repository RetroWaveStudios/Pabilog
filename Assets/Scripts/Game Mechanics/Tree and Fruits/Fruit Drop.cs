using UnityEngine;

public class FruitDrop : MonoBehaviour
{
    public int slotNumber;
    public void shutdown()
    {
        ForestLogic.instance.Slots[slotNumber].GetComponent<TreeSlot>().TheTree.hFrutis.RemoveAt(ForestLogic.instance.Slots[slotNumber].GetComponent<TreeSlot>().TheTree.hFrutis[ForestLogic.instance.Slots[slotNumber].GetComponent<TreeSlot>().dropindex]);
        ForestLogic.instance.Slots[slotNumber].GetComponent<TreeSlot>().fruits[ForestLogic.instance.Slots[slotNumber].GetComponent<TreeSlot>().TheTree.hFrutis[ForestLogic.instance.Slots[slotNumber].GetComponent<TreeSlot>().dropindex]].SetActive(false);
        ForestLogic.instance.Slots[slotNumber].GetComponent<TreeSlot>().UpdateFruitImages();
    }

    public void Message()
    {
        Debug.Log("Fruit Dropped");
    }
}