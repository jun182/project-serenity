using UnityEngine;

public class SpawnerScript : MonoBehaviour
{
    [SerializeField]
    private GameObject spawner;
    [SerializeField]
    private GameObject obstacle;
    [SerializeField]
    private float timeBetweenSpawns; // scene: 0.15 s = the original rate (difficulty level 2)
    [SerializeField]
    private float chanceToSpawnInMiddle; // unused: it was 0 in the scene, so x 2..5 never got obstacles

    private float timer;

    void Start()
    {
        timer = 0f;
    }

    void Update()
    {
        var gd = GameDirector.Instance;
        if (gd != null && gd.SpawningStopped) return;
        float mult = gd != null ? gd.RateMultiplier : 1f;
        if (mult <= 0f) return;
        float interval = timeBetweenSpawns / mult;
        timer += Time.deltaTime;
        // Loop (not reset to 0) so high rates aren't capped by the frame rate.
        while (timer >= interval)
        {
            timer -= interval;
            // Uniform over the whole left side up to the border (x=5); beyond 5, HideObjectsPerSide fades them out (invisible hits).
            float x = Random.Range(-5f, GameSettings.Get("spawnMaxX"));
            Instantiate(obstacle, new Vector3(x, spawner.transform.position.y, spawner.transform.position.z), Quaternion.identity);
            if (gd != null) gd.Spawned++;
        }
    }
}
