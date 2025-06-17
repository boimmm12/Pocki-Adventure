using UnityEngine;

public class EssentialObjectSpawner : MonoBehaviour
{
    [SerializeField] GameObject essentialObjectPrefab;
    [SerializeField] GameObject spawner;

    void Awake()
    {
        var existingObjects = FindObjectsOfType<EssentialObject>();
        if (existingObjects.Length == 0)
        {
            //if there is a grid
            var spawnPos = new Vector3(0, 0, 0);

            var spawner = GameObject.Find("Spawner");
            if (spawner != null)
                spawnPos = spawner.transform.position;

            Instantiate(essentialObjectPrefab, spawnPos, Quaternion.identity);
        }
    }
}
