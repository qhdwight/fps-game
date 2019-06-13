using System.Collections;
using UnityEngine;

public enum MoveMode
{
    Regular, Flying
};

[RequireComponent(typeof(Rigidbody))]
public class PlayerMotor : MonoBehaviour
{
    private MoveMode currentMoveMode = MoveMode.Regular;

    // Components
    [SerializeField] protected GameObject camHolder;
    [SerializeField] protected LayerMask mask;

    protected Rigidbody rb;

    // Looking
    public float sensitivityX = 1F;
    public float sensitivityY = 1F;
    public float minimumX = -360F;
    public float maximumX = 360F;
    public float minimumY = -90F;
    public float maximumY = 60F;
    protected float rotationY = 0f;

    // Movement
    public float walkMultiplier = 0.5F;
    public float runMultiplier = 1.2F;
    public float topSpeed = 4.2F;
    public float speedAcceleration = 0.4F;
    public float jumpSpeed = 8F;

    // Speed variables
    protected float rightSpeed;
    protected float forwardSpeed;
    protected float velocityVectorY;
    protected Vector3 velocityVector;
    protected float speedMultiplier;

    // Movement inputs
    protected bool jumpInput;
    protected bool walkInput;
    protected bool runInput;
    protected float rightMove;
    protected float forwardMove;

    // States    
    public bool running;
    public bool walking;
    public bool grounded;
    public bool idle = true;
    public bool canJump = true;

    // Graphics
    protected float animateSpeed;

    // Audio
    protected bool playFootstep;

    protected bool bhopEnabled;

    private void Start()
    {
        //Get components
        rb = GetComponent<Rigidbody>();
    }

    protected virtual void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            if (currentMoveMode == MoveMode.Regular)
            {
                currentMoveMode = MoveMode.Flying;
                rb.useGravity = false;
            }
            else
            {
                currentMoveMode = MoveMode.Regular;
                rb.useGravity = true;
            }
        }

        if (!CursorManagement.IsMenuOpen())
        {
            // Look
            Look();

            // Get jump input
            if (bhopEnabled)
                jumpInput = Input.GetButton("Jump");
            else
                jumpInput = Input.GetButtonDown("Jump");

            // Test if jumping
            if (currentMoveMode == MoveMode.Regular && jumpInput == true)
                StartCoroutine(OnJump());
        }
    }

    protected void FixedUpdate()
    {
        if (!CursorManagement.IsMenuOpen())
        {
            // Input
            runInput = Input.GetButton("Sprint");
            walkInput = Input.GetButton("Walk");

            // Calculate movement velocity as a 3D vector (Inputs)
            rightMove = Input.GetAxisRaw("Horizontal");
            forwardMove = Input.GetAxisRaw("Vertical");

            // Test if in the air or not
            grounded = CheckGround();

            if (grounded)
            {
                float absoluteHighestSpeed = Mathf.Max(Mathf.Abs(forwardSpeed), Mathf.Abs(rightSpeed));
                animateSpeed = absoluteHighestSpeed / (topSpeed * runMultiplier);
            }

            // Speed Manager Right/Left
            CalculateSpeed();

            NormalizeVelocity();

            // Movement
            ApplyVelocity();
        }
    }

    protected void CalculateSpeed()
    {
        if (currentMoveMode == MoveMode.Flying)
        {
            Vector3 rbVelocity = rb.velocity;

            // Lateral movement
            if (rightMove == 0 && forwardMove == 0)
            {
                rbVelocity.x = 0;
                rbVelocity.z = 0;
            }
            else
            {
                rbVelocity.x = (rightMove * topSpeed * transform.right.x) + (forwardMove * topSpeed * transform.forward.x);
                rbVelocity.z = (rightMove * topSpeed * transform.right.z) + (forwardMove * topSpeed * transform.forward.z);
            }

            // Vertical movement
            if (runInput)
            {
                rbVelocity.y = -topSpeed;
            }
            else if (Input.GetButton("Jump"))
            {
                rbVelocity.y = topSpeed;
            }
            else
            {
                rbVelocity.y = 0;
            }

            rb.velocity = rbVelocity;
        }
        else if (currentMoveMode == MoveMode.Regular)
        {

            if (walkInput == true)
            {
                if (currentMoveMode == MoveMode.Flying)
                {
                    Vector3 rbVecloity = rb.velocity;
                    rbVecloity.y = -topSpeed;
                    rb.velocity = rbVecloity;
                }

                walking = true;
                running = false;
                speedMultiplier = walkMultiplier;
            }
            else if (runInput)
            {
                walking = false;
                running = true;
                speedMultiplier = runMultiplier;
            }
            else
            {
                walking = false;
                running = false;
                speedMultiplier = 1F;
            }

            if (rightMove == 1)
            {
                if (rightSpeed < topSpeed * speedMultiplier)
                {
                    rightSpeed += speedAcceleration;
                }
                else
                {
                    rightSpeed = topSpeed * speedMultiplier;
                }
            }
            else if (rightMove == -1)
            {
                if (rightSpeed > -topSpeed * speedMultiplier)
                {
                    rightSpeed -= speedAcceleration;
                }
                else
                {
                    rightSpeed = -topSpeed * speedMultiplier;
                }
            }
            else if (rightMove == 0)
            {
                if (rightSpeed > 1)
                {
                    rightSpeed -= speedAcceleration;
                }
                else if (rightSpeed < -1)
                {
                    rightSpeed += speedAcceleration;
                }
                else
                {
                    rightSpeed = 0;
                }
            }

            //--------------------------------------------------------------------------------------------------------------------------------

            if (forwardMove == 1)
            {
                if (forwardSpeed < topSpeed * speedMultiplier)
                {
                    forwardSpeed += speedAcceleration;
                }
                else
                {
                    forwardSpeed = topSpeed * speedMultiplier;
                }
            }
            else if (forwardMove == -1)
            {
                if (forwardSpeed > -topSpeed * speedMultiplier)
                {
                    forwardSpeed -= speedAcceleration;
                }
                else
                {
                    forwardSpeed = -topSpeed * speedMultiplier;
                }
            }
            else if (forwardMove == 0)
            {
                if (forwardSpeed > 1)
                {
                    forwardSpeed -= speedAcceleration;
                }
                else if (forwardSpeed < -1)
                {
                    forwardSpeed += speedAcceleration;
                }
                else
                {
                    forwardSpeed = 0;
                }
            }

            Vector2 movementVelocity = new Vector2(forwardSpeed, rightSpeed);

        }
    }

    protected void NormalizeVelocity()
    {
        Vector2 vector = new Vector2(forwardSpeed, rightSpeed);
        float maxSpeed = 2F * topSpeed * speedMultiplier;

        if (vector.magnitude > maxSpeed) {
            vector = Util.ScaleVector(vector, maxSpeed);
        }

        forwardSpeed = vector.x;
        rightSpeed = vector.y;
    }

    protected void ApplyVelocity()
    {
        CheckIdle();

        if (idle == false && currentMoveMode == MoveMode.Regular)
        {
            velocityVector = rb.velocity;

            velocityVector.x = (rb.transform.forward.x * forwardSpeed) + (rb.transform.right.x * rightSpeed);
            velocityVector.z = (rb.transform.forward.z * forwardSpeed) + (rb.transform.right.z * rightSpeed);

            //rb.AddForce(velocityVector);
            rb.velocity = velocityVector;
        }
    }

    protected virtual void Look()
    {
        float rotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;

        rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
        rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);

        //Turn player
        transform.localEulerAngles = new Vector3(0, rotationX, 0);

        //Camera up/down
        camHolder.transform.localEulerAngles = new Vector3(-rotationY, 0, 0);
    }

    protected void CheckIdle()
    {
        if ((rightMove != 0) || (forwardMove != 0))
        {
            idle = false;
        }
        else
        {
            idle = true;
        }
    }

    protected IEnumerator OnJump()
    {
        if (grounded)
        {
            /* Jump event */

            grounded = false;

            // Add velocity
            rb.velocity = (Vector3.up * jumpSpeed);

            // Reset running animation when in air
            if (running)
            {
                StartCoroutine(CheckUntilHitGround());
            }

            JumpStarted();

            yield return new WaitForSeconds(0.1f);
        }
    }

    protected IEnumerator CheckUntilHitGround()
    {
        while (!grounded)
        {
            yield return new WaitForSeconds(0.1F);
        }

        JumpEnded();
    }

    protected virtual void JumpStarted() { }

    protected virtual void JumpEnded() { }

    protected bool CheckGround()
    {
        // Cast raycast
        RaycastHit hit;
        bool hitGround = Physics.Raycast(transform.position - new Vector3(0F, -0.1F, 0F), Vector3.down, out hit, 10f, mask);

        if (hitGround == true)
        {
            if (hit.distance <= 0.2f)
                return true;
        }
        return false;
    }

    protected virtual void OnGUI()
    {
        if (Input.GetButton("Debug"))
        {
            GUI.Label(new Rect(400, 10, 500, 20), "Velocity: " + rb.velocity.ToString());
            GUI.Label(new Rect(400, 30, 500, 20), "Right Speed: " + rightSpeed.ToString());
            GUI.Label(new Rect(400, 50, 500, 20), "Forward Speed: " + forwardSpeed.ToString());
            GUI.Label(new Rect(400, 70, 500, 20), "Speed Multiplier: " + speedMultiplier.ToString());
        }
    }
}



