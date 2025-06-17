using UnityEngine;
using UnityEngine.Tilemaps;

public class InvisibleWall : MonoBehaviour
{
    private void Start()
    {
        GetComponent<TilemapRenderer>().enabled = false;
    }
}
