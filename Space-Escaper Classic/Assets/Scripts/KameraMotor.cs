using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Security.Cryptography;
using UnityEngine;

public class KameraMotor : MonoBehaviour
{
    public Transform lookAt;
    public Vector3 offset = new Vector3(0, 0f, 0f);
    public Vector3 rotation = new Vector3(0, 0, 0);

    private float originalSpeed = 0f;
    private float speed = 0f;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 5f;
    private float speedIncreaseAmount = 0.2f;

    public bool IsMoving { set; get; }

    private void start()
    {
        transform.position = lookAt.position + offset;
    }

    private void LateUpdate()
    {
        if (!IsMoving)
            return;

        if (Time.time - speedIncreaseLastTick > speedIncreaseTime)
        {
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;
        }

        offset.z = -3 + speed;

        Vector3 desiredPosition = lookAt.position + offset;
        //desiredPosition.x = 0;
        transform.position = Vector3.Lerp(transform.position,desiredPosition,Time.deltaTime);
        transform.rotation = Quaternion.Lerp(transform.rotation,Quaternion.Euler(rotation),Time.deltaTime * 1f);
    }
}
