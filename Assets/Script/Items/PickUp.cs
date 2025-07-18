using System.Collections;
using UnityEngine;

public class PickUp : MonoBehaviour, Interactable,ISavable
{
    [SerializeField] ItemBase item;

    public bool Used {get; set;} = false;

    public IEnumerator Interact(Transform initiator)
    {
        if(!Used)
        {
            initiator.GetComponent<Inventory>().AddItem(item);

            Used = true;
            
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;

            string playerName = initiator.GetComponent<PlayerController>().Name;

            AudioManager.i.PlaySfx(AudioId.ItemObtained, pauseMusic:true);

            yield return DialogManager.Instance.ShowDialogText($"{playerName} found {item.Name}");
        }
    }

    public object CaptureState()
    {
        return Used;
    }

    public void RestoreState(object state)
    {
        Used = (bool)state;

        if(Used)
        {
            GetComponent<SpriteRenderer>().enabled = false;
            GetComponent<BoxCollider2D>().enabled = false;
        }
    }
}
