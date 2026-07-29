using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance = 2f;
    [SerializeField] private float cameraSpeed = 3f;

    private float lookAheadX;

    private void Update()
    {
        if (player == null)
        {
            GameLogger.Warning(GameLogger.Category.General, "CameraController sin referencia al player.", this);
            return;
        }

        transform.position = new Vector3(player.position.x + lookAheadX, transform.position.y, transform.position.z);
        lookAheadX = Mathf.Lerp(lookAheadX, aheadDistance * player.localScale.x, Time.deltaTime * cameraSpeed);
    }
}
