using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using UnityEngine;

public class PlayerMotor : MonoBehaviour
{
    private const float LANE_DISTANCE = 3.5f;
    private const float TURN_SPEED = 0.05f;

    //VFX
    public GameObject explosionVFX;
    public ParticleSystem drive;

    public Vector3 targetPosition;

    //
    private bool isRunning = false;

    //Animation
    private Animator anim;

    private SoundManager boom;

    //Movement
    private CharacterController controller;
    private float verticalVelocity;
    private int desiredLane = 1; // 0 = Left, 1 = Middle, 3 = Right

    //Speed Modifier
    private float originalSpeed = 11f;
    private float speed;
    private float speedIncreaseLastTick;
    private float speedIncreaseTime = 5f;
    private float speedIncreaseAmount = 0.2f;

    private SoundManager engine;

    // Start is called before the first frame update
    void Start()
    {
        engine = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
        speed = originalSpeed;
        controller = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        Vector3 explosionPos = transform.position;
        var emission = drive.GetComponent<ParticleSystem>().emission;
        emission.enabled = false;
        boom = GameObject.FindGameObjectWithTag("Audio").GetComponent<SoundManager>();
    }

    // Update is called once per frame
    /// <summary>
    ///
    /// </summary>
    void Update()
    {
        if (!isRunning)
            return;

        var emission = drive.GetComponent<ParticleSystem>().emission;
        emission.enabled = true;

        if (Time.time - speedIncreaseLastTick > speedIncreaseTime)
        {
            speedIncreaseLastTick = Time.time;
            speed += speedIncreaseAmount;
            GameManager.Instance.UpdateModifier(speed - originalSpeed);
        }

        //Gather the input on which lane we should be
        if (MobileInput.Instance.SwipeLeft)
            MoveLane(false);

        if (MobileInput.Instance.SwipeRight)
            MoveLane(true);

        // Calculate where we should be in the future
        Vector3 targetPosition = transform.position.z * Vector3.forward;
        if (desiredLane == 0)
            targetPosition += Vector3.left * LANE_DISTANCE;
        else if (desiredLane == 2)
            targetPosition += Vector3.right * LANE_DISTANCE;

        // Let's calculate our move delta
        Vector3 moveVector = Vector3.zero;
        moveVector.x = (targetPosition - transform.position).x * speed;
        moveVector.z = speed;

        //Move the Ship
        controller.Move(moveVector * Time.deltaTime);

        //Rotate the Ship were it is going
        //Vector3 dir = controller.velocity;
        //if(dir != Vector3.zero)
        //{
        //dir.y = 0;
        //transform.forward = Vector3.Lerp(transform.forward, dir, TURN_SPEED);
        //}
    }

    private void MoveLane(bool goingRight)
    {
        desiredLane += (goingRight) ? 1 : -1;
        desiredLane = Mathf.Clamp(desiredLane, 0, 2);
    }

    public void StartRunning()
    {
        isRunning = true;
        engine.StartEngine();
        engine.StopMenuMusic();
    }

    private void Crash()
    {
        anim.SetTrigger("Death");
        isRunning = false;
        GameManager.Instance.OnDeath();
        Instantiate(explosionVFX, transform.position, Quaternion.identity);
        var emission = drive.GetComponent<ParticleSystem>().emission;
        emission.enabled = false;
        boom.PlayExplosion();
        engine.StopEngine();
    }

    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        switch (hit.gameObject.tag)
        {
            case "Obstacle":
                Crash();
                break;
        }
    }
}
