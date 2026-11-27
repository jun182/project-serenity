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
                // Slide toward where the player is heading; if standing still, away from the obstacle.
                float dir = Mathf.Abs(pc.MoveSpeed) > 0.001f ? Mathf.Sign(pc.MoveSpeed) : Mathf.Sign(pc.transform.position.x - transform.position.x);
                CameraShake.NearMiss(dir);
                Haptics.NearMiss();
                if (GameDirector.Instance != null) GameDirector.Instance.NearMisses++;
            }
        }
        if (transform.position.z <= -30f)
        {
            Destroy(this.gameObject);
        }
    }
}
