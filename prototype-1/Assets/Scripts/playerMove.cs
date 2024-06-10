using System;
using UnityEngine;

public class playerMove : MonoBehaviour
{
    // Movement parameters
    private float runSpeed = 10f;
    private float accel = 10f;
    private float floorFriction = 0.1f;
    public float jumpForce = 20f;
    public float speed;

    // State variables
    private bool onGround = true;
    private bool jumpQueue;
    private bool onRamp;

    // Components
    private Rigidbody rb;
    private mouseLook ml;
    private float rotationX;
    public float rotationY;
    private float lastRotationX;
    private float lastRotationY;

    // Initial position
    private Vector3 spawnPos;
    private Quaternion spawnRot;

    private void Start()
    {
        spawnPos = transform.position;
        spawnRot = transform.rotation;
        rb = GetComponent<Rigidbody>();
        ml = GameObject.Find("Main Camera").GetComponent<mouseLook>();

        rotationX = ml.getRotationX();
        rotationY = ml.getRotationY();
        lastRotationX = ml.getRotationX();
        lastRotationY = ml.getRotationY();
    }

    private void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.CompareTag("Ramp"))
        {
            onRamp = true;
        }
        else if (col.gameObject.CompareTag("platform"))
        {
            onRamp = false;
            onGround = true;
        }
    }

    private void OnCollisionStay(Collision col)
    {
        if (col.gameObject.CompareTag("Ramp"))
        {
            onRamp = rb.velocity.magnitude >= 2f;
        }
    }

    private void OnCollisionExit(Collision col)
    {
        if (col.gameObject.CompareTag("Ramp"))
        {
            onRamp = false;
        }
        else if (col.gameObject.CompareTag("platform"))
        {
            onGround = false;
        }
    }

    private void Update()
    {
        speed = rb.velocity.magnitude;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            jumpQueue = true;
        }
        if (Input.GetKeyUp(KeyCode.Space))
        {
            jumpQueue = false;
        }
        if (jumpQueue && onGround)
        {
            ApplyJump();
        }

        lastRotationX = rotationX;
        lastRotationY = rotationY;
        rotationX = ml.getRotationX();
        rotationY = ml.getRotationY();

        float forward = 0f;
        float side = 0f;
        if (Input.GetKey(KeyCode.W)) forward += 1f;
        if (Input.GetKey(KeyCode.S)) forward -= 1f;
        if (Input.GetKey(KeyCode.A)) side -= 1f;
        if (Input.GetKey(KeyCode.D)) side += 1f;

        if (onGround)
        {
            HandleGroundControl(forward, side);
        }
        else if (onRamp)
        {
            HandleRampControl(forward, side);
        }
        else
        {
            HandleAirControl(forward, side);
        }
    }

    private void ApplyJump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
        onGround = false;
    }

    private void HandleGroundControl(float forward, float side)
    {
        rb.useGravity = true;
        float angle = rotationX;

        if (forward != 0f || side != 0f)
        {
            if (forward == -1f) angle -= 180f;
            if (side == -1f) angle += forward != 0f ? 45f : -90f;
            if (side == 1f) angle += forward != 0f ? -45f : 90f;

            ApplyRunForce(angle);
        }
        else
        {
            rb.AddForce(-rb.velocity * floorFriction);
        }
    }

    private void HandleRampControl(float forward, float side)
    {
        ApplyRunForce(rotationX);
    }

    private void HandleAirControl(float forward, float side)
    {
        rb.useGravity = true;
        float rotationDifference = rotationX - lastRotationX;

        if (side == 1f && rotationDifference > 0f)
        {
            ApplyAirForce(rotationX);
        }
        else if (side == -1f && rotationDifference < 0f)
        {
            ApplyAirForce(rotationX);
        }
        else if (forward == -1f)
        {
            rb.velocity = new Vector3(0f, rb.velocity.y, 0f);
        }
    }

    private void ApplyRunForce(float angle)
    {
        angle *= Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));

        if (rb.velocity.magnitude < runSpeed)
        {
            rb.velocity = Vector3.ClampMagnitude(rb.velocity, runSpeed);
        }
        rb.AddForce(direction * accel);
        rb.velocity = Vector3.ClampMagnitude(rb.velocity, runSpeed);
    }

    private void ApplyAirForce(float angle)
    {
        angle *= Mathf.Deg2Rad;
        Vector3 direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
        direction.y = rb.velocity.y;
        rb.velocity = direction;
    }
}
