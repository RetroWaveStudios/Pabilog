using UnityEngine;

public class FruitDrop : MonoBehaviour
{
    public int slotNumber;
    public void shutdown()
    {
        ForestLogic.instance.Slots[slotNumber].GetComponent<TreeSlot>().UpdateFruitImages();
    }

    public void Message()
    {
        Debug.Log("Fruit Dropped");
    }
}