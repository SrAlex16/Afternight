using System;
using System.Collections;
using System.Collections.Generic;
using System.Timers;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private float aheadDistance;
    [SerializeField] private float cameraSpeed;
    private float lookAheadX;
    private float lookAheadY;

    private void Update()
    {
        transform.position = new Vector3(player.position.x + lookAheadX, transform.position.y, transform.position.z);
        lookAheadX = Mathf.Lerp(lookAheadX, (aheadDistance * player.localScale.x), Time.deltaTime * cameraSpeed);
        //lookAheadY = Mathf.Lerp(lookAheadY, (aheadDistance * player.localScale.y), Time.deltaTime * cameraSpeed);
    }
}