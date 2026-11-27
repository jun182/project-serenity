using UnityEngine;

public class ObstacleController : MonoBehaviour
{
    [SerializeField]
    private float SPEED;

    public bool HitPlayer; // set by PlayerController when this obstacle hits
    bool passed;

    void Update()
    {
        transform.position += new Vector3(0, 0, -1) * SPEED * GameSettings.Get("obstacleSpeed") * Time.deltaTime;
        // Near miss: crossed the player's z without hitting, close in x, player on the obstacle side.
        var pc = PlayerController.Instance;
        if (!passed && pc != null && transform.position.z < pc.transform.position.z)
        {
            passed = true;
            if (!HitPlayer && !pc.IsDead && !pc.IsHealing
                && Mathf.Abs(transform.position.x - pc.transform.position.x) < GameSettings.Get("nearMissDist"))
            {
                CameraShake.NearMiss();
                if (GameDirector.Instance != null) GameDirector.Instance.NearMisses++;
            }
        }
        if (transform.position.z <= -30f)
        {
            Destroy(this.gameObject);
        }
    }
}
