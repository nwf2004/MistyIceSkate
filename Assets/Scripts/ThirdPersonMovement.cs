using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{

    float horizontal;
    float vertical;

    public float maxSpeed = 18f;

    public float turnSmoothTime = 1f;
    float turnSmoothVelocity;

    public bool straffing;
    float previousStrafe;
    public float skateAcceleration;
    public float skateDeceleration;

    public GameManager GameM;

    public float SkatingSpeed;
    public float strafeDirection;
    float tempRotation;
    bool died;
    //

    [SerializeField] CharacterController moveController;
    [SerializeField] Transform interpolatedTransform;
    public CharacterController MoveController => moveController;

    Vector3 iPositionNow;
    Vector3 iPositionNext;

    Vector3 combinedVelocity;
    Vector3 moveVelocity;
    Vector3 fallVelocity;
    Vector3 externalVelocity;
    Vector3 physicsVelocity;

    public Vector3 Velocity => combinedVelocity;

    public Vector3 PrevRotation { get; private set; }
    public float RotationDelta { get; private set; }

    public float YSpeed => fallVelocity.y;
    public float Speed => currentMoveSpeed;

    Vector3 currentMoveDirection;
    float currentMoveSpeed;

    [SerializeField] float gravityMultiplier;
    [SerializeField] float fallVelocityDrag;
    [SerializeField] float externalVelocityDragGrounded;
    [SerializeField] float externalVelocityDragAir;

    [SerializeField] float moveSpeed;
    [SerializeField] float initialMoveSpeed;


    [SerializeField] float speedAccelerationGrounded;
    [SerializeField] float speedAccelerationInAir;
    [SerializeField] float speedDeccelerationGrounded;
    [SerializeField] float speedDeccelerationInAir;
    [SerializeField] float directionAccelerationGrounded;
    [SerializeField] float directionAccelerationInAir;
    [SerializeField] float directionDeccelerationGrounded;
    [SerializeField] float directionDeccelerationInAir;
    [SerializeField] float accelerationWhenSlowing;
    [SerializeField] AnimationCurve runSpeedAccelerationCurve;
    [SerializeField] float runSpeedAccelerationDuration;


    [SerializeField] [Range(0f, 1f)] float strafeSpeedMultiplier;
    [SerializeField] [Range(0f, 1f)] float strafeSpeedMultiplierWhileMoving;

    [Space(10)]
    [SerializeField] float jumpSpeed;
    bool wantsToJump;
    [SerializeField] [Range(0f, 1f)] float speedLostOnJump;
    [SerializeField] [Range(0f, 1f)] float speedLostOnLanding;
    //[field: SerializeField] public float LandingShockSpeedTheshold { get; private set; }

    [SerializeField] LayerMask groundLayerMask;
    [SerializeField] Vector3 groundCheckExtents;
    [SerializeField] Vector3 airborneGroundCheckExtents;
    [SerializeField] float downwardForceAlongSlope;
    float halfHeight;
    RaycastHit groundHit;

    [SerializeField] Animator animator;

    public Animator Animator => animator;
    static readonly int HashRunning = Animator.StringToHash("Running");
    static readonly int HashGrounded = Animator.StringToHash("Grounded");
    static readonly int HashJumping = Animator.StringToHash("Jumping");
    static readonly int HashFalling = Animator.StringToHash("Falling");
    static readonly int HashBraking = Animator.StringToHash("Braking");
    static readonly int HashForward = Animator.StringToHash("Forward");
    static readonly int HashStrafe = Animator.StringToHash("Strafe");
    static readonly int HashTurn = Animator.StringToHash("Turn");

    float toTurn = 0;

    public bool IsRunning { get; private set; }
    float runStartTime;

    public bool Grounded { get; private set; }

    public event System.Action OnStartRunning;
    public event System.Action OnStopRunning;
    public event System.Action OnJump;
    public event System.Action OnFalling;
    public event System.Action OnStopFalling;
    public event System.Action OnBraking;
    public event System.Action OnStopBraking;
    public event System.Action OnLanded;
    public event System.Action OnLeftGround;
    public event System.Action OnEnteredGround;

    int _movementLocks;
    public int MovementLocks
    {
        get => _movementLocks;
        set
        {
            if (_movementLocks == 0 && value > 0)
            {
                if (IsRunning)
                {
                    IsRunning = false;
                    OnStopRunning?.Invoke();
                }
            }

            _movementLocks = value;
            if (_movementLocks < 0)
            {
                _movementLocks = 0;
            }
        }
    }

    int _movementControlLocks;
    public int MovementControlLocks
    {
        get => _movementControlLocks;
        set
        {
            _movementControlLocks = value;
            if (_movementControlLocks < 0)
            {
                _movementControlLocks = 0;
            }
        }
    }
    void Awake()
    {
        halfHeight = moveController.height * 0.5f;
    }

    void OnEnable()
    {
        OnStartRunning += PlayRunAnimation;
        OnStopRunning += StopRunAnimation;
        OnEnteredGround += EnableAnimatorGrounded;
        OnLeftGround += DisableAnimatorGrounded;
        OnJump += PlayJumpAnimation;
        OnFalling += PlayFallingAnimation;
        OnStopFalling += StopFallingAnimation;
        OnBraking += PlayBrakingAnimation;
        OnStopBraking += StopBrakingAnimation;
    }
    void OnDisable()
    {
        OnStartRunning -= PlayRunAnimation;
        OnStopRunning -= StopRunAnimation;
        OnEnteredGround -= EnableAnimatorGrounded;
        OnLeftGround -= DisableAnimatorGrounded;
        OnJump -= PlayJumpAnimation;
        OnFalling -= PlayFallingAnimation;
        OnStopFalling -= StopFallingAnimation;
        OnBraking -= PlayBrakingAnimation;
        OnStopBraking -= StopBrakingAnimation;
    }

        // Start is called before the first frame update
        void Start()
    {

    }

    void Update()
    {
        // Interpolate position
        interpolatedTransform.position = Vector3.Lerp(iPositionNow, iPositionNext, (Time.time - Time.fixedTime) / Time.fixedDeltaTime);


        if (!Input.GetKey(KeyCode.LeftShift))
        {
            straffing = false;

        }
        else
        {
            straffing = true;
        }
        if (Input.GetKey(KeyCode.Space))
        {
            wantsToJump = true;
        }
    }

    void LateUpdate()
    {
        /*Vector3 rotation = transform.localEulerAngles;
         RotationDelta = Mathf.DeltaAngle(rotation.y, PrevRotation.y) * Time.deltaTime;
         PrevRotation = rotation;*/

        //.normalized is so that when going diagonal we don't move extra fast
        /*Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            //Returns the angle between the x axis and a vector starting at 0 and terminating at x,z direction (its kind of confusing)
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; //rad2 degrees returns radian to degrees

            //smooth movement
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            Cirno.transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //time.Deltatime to make it frame rate independent
            //Cirno.transform.Rotate(Quaternion.Euler(0f, angle, 0f));
        }*/
        /*if (died)
        {
            transform.position = new Vector3(215.8846f, 11.157f, 194.4f);
            iPositionNow = iPositionNext;
            iPositionNext = transform.localPosition;
            died = false;
        }*/
    }

    void FixedUpdate()
    {
        if (MovementLocks > 0 || died)
        {
            wantsToJump = false;
            iPositionNow = iPositionNext;
            iPositionNext = transform.localPosition;
            //currentSpeed = 0;
            return;
        }
        if (GameM.Died == false)
        {
            Movement();
        }
        else
        {
            currentMoveSpeed = 0;
        }
        animator.SetFloat(HashForward, currentMoveSpeed/20);
        Debug.Log(currentMoveSpeed/20);
        if (!straffing)
        {
            float turnPower = moveSpeed / currentMoveSpeed;
            horizontal = Input.GetAxisRaw("Horizontal");
            vertical = Input.GetAxisRaw("Vertical");
            if (horizontal != 0)
            {
                //Debug.Log(toTurn);
                //float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
                transform.Rotate(new Vector3(0.0f, horizontal * (50 + (turnPower * 2)), 0.0f) * Time.deltaTime);
                if (horizontal < 0 && toTurn > -1)
                {
                    toTurn -= 3 * Time.deltaTime;
                }
                if (horizontal > 0 && toTurn < 1)
                {
                    toTurn += 3 * Time.deltaTime;
                }
            }
            else
            {
                if (toTurn > .1f)
                {
                    toTurn -= 3 * Time.deltaTime;
                }
                else if (toTurn < -.1f)
                {
                    toTurn += 3 * Time.deltaTime;
                }
                else
                {
                    toTurn = 0;
                }
            }
            animator.SetFloat(HashTurn, toTurn);
        }
        else
        {
            if (toTurn > .1f)
            {
                toTurn -= 3 * Time.deltaTime;
            }
            else if (toTurn < -.1f)
            {
                toTurn += 3 * Time.deltaTime;
            }
            else
            {
                toTurn = 0;
            }
        }
        if (fallVelocity.y < 0)
        {
            animator.SetBool(HashFalling, true);
        }
        else
        {
            animator.SetBool(HashFalling, false);
        }
        wantsToJump = false;

    }

    // Update is called once per frame
    /*void FixedUpdate()
    {
        CheckKeys();
        //.normalized is so that when going diagonal we don't move extra fast
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            //Returns the angle between the x axis and a vector starting at 0 and terminating at x,z direction (its kind of confusing)
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg; //rad2 degrees returns radian to degrees

            //smooth movement
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnSmoothVelocity, turnSmoothTime);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            //time.Deltatime to make it frame rate independent
            controller.Move(direction * currentSpeed * Time.deltaTime);

        }
    }*/

    void GetTargetVelocityAndAcceleration(Vector2 moveInput, bool wasRunning, bool wasGrounded, out Vector3 moveDirection, out float targetSpeed, out float speedAcceleration, out float directionAcceleration)
    {
        IsRunning = moveInput != Vector2.zero && MovementLocks == 0;
        moveDirection = transform.forward * 1f + transform.right * moveInput.x;

        if (IsRunning)
        {
            if (!wasRunning)
            {
                runStartTime = Time.time;
                currentMoveSpeed = Mathf.Max(currentMoveSpeed, initialMoveSpeed);
            }

            float runDuration = Time.time - runStartTime;
            if (runDuration <= runSpeedAccelerationDuration)
            {
                targetSpeed = Mathf.LerpUnclamped(initialMoveSpeed, moveSpeed, runSpeedAccelerationCurve.Evaluate(runDuration / runSpeedAccelerationDuration));
            }
            else
            {
                targetSpeed = moveSpeed;
            }

            if (!Grounded)
            {
                targetSpeed *= Mathf.Abs(moveInput.y);
            }
        }
        else
        {
            targetSpeed = 0f;
        }

        // Accelerate toward new move velocity
        if (IsRunning)
        {
            if (Grounded)
            {
                speedAcceleration = speedAccelerationGrounded;
                directionAcceleration = directionAccelerationGrounded;
            }
            else
            {
                speedAcceleration = speedAccelerationInAir;
                directionAcceleration = directionAccelerationInAir;
            }
        }
        else
        {
            if (Grounded)
            {
                speedAcceleration = speedDeccelerationGrounded;
                directionAcceleration = directionDeccelerationGrounded;
            }
            else
            {
                speedAcceleration = speedDeccelerationInAir;
                directionAcceleration = directionDeccelerationInAir;
            }
        }
    }


    void Movement()
    {
        bool wasRunning = IsRunning;
        // Box cast at our feet to check if we're grounded
        bool groundCheck = Physics.BoxCast(transform.localPosition, Grounded ? groundCheckExtents : airborneGroundCheckExtents, Vector3.down, out groundHit, Quaternion.identity, halfHeight, groundLayerMask, QueryTriggerInteraction.Ignore);
        bool wasGrounded = Grounded;
        Grounded = groundCheck || moveController.isGrounded;

        if (Grounded && !wasGrounded)
        {
            OnEnteredGround?.Invoke();
            OnStopFalling?.Invoke();

            if (fallVelocity.y < 0f)
            {
                OnLanded?.Invoke();

                // Lower speed on landing
                currentMoveSpeed *= speedLostOnLanding;
            }
        }
        else if (!Grounded && wasGrounded)
        {
            OnLeftGround?.Invoke();
        }
        Vector2 moveInput;
        
            if (!straffing)
            {
            if (Grounded)
            {
                // Get move direction
                //horizontal = Input.GetAxisRaw("Horizontal");
                vertical = Input.GetAxisRaw("Vertical");
                if (vertical > 0)
                {
                    //
                }
                if (vertical < 0)
                {
                    if (currentMoveSpeed < 2)
                    {
                        currentMoveSpeed = 0;
                        speedDeccelerationGrounded = .2f;
                        StopBrakingAnimation();
                    }
                    else
                    {
                        speedDeccelerationGrounded = 1f;
                        PlayBrakingAnimation();
                    }
                }
                else
                {
                    speedDeccelerationGrounded = .2f;
                    StopBrakingAnimation();
                }
            }
            float verticalNotBelowZero;
            if (vertical < 0)
            {
                verticalNotBelowZero = 0;
            }
            else
            {
                verticalNotBelowZero = vertical;
            }
            //if they arent inputing, set the stafespeed for when there is no input
            moveInput = MovementControlLocks == 0 ? new Vector2(0.0f, verticalNotBelowZero).normalized : Vector2.zero;
                horizontal *= vertical == 0f ? strafeSpeedMultiplier : strafeSpeedMultiplierWhileMoving;
            }
            else
            {
            if (Grounded)
            {
                // Get move direction
                horizontal = Input.GetAxisRaw("Horizontal");
                vertical = Input.GetAxisRaw("Vertical");
                if (vertical > 0)
                {
                    //
                }
                if (vertical < 0)
                {
                    if (currentMoveSpeed < 2)
                    {
                        currentMoveSpeed = 0;
                        speedDeccelerationGrounded = .2f;
                        StopBrakingAnimation();
                    }
                    else
                    { 
                        speedDeccelerationGrounded = 1f;
                        PlayBrakingAnimation();
                    }
                }
                else
                {
                    speedDeccelerationGrounded = .2f;
                    StopBrakingAnimation();
                }
            }
            float verticalNotBelowZero;
            if (vertical < 0)
            {
                verticalNotBelowZero = 0;
            }
            else
            {
                verticalNotBelowZero = vertical;
            }
            //if conrols arent locking, take user input for the move inout

            if (vertical < 0) { moveInput = MovementControlLocks == 0 ? new Vector2(0.0f, verticalNotBelowZero).normalized : Vector2.zero; }
            else { moveInput = MovementControlLocks == 0 ? new Vector2(horizontal, verticalNotBelowZero).normalized : Vector2.zero; }
                
                //if they arent inputing, set the stafespeed for when there is no input
                horizontal *= vertical == 0f ? strafeSpeedMultiplier : strafeSpeedMultiplierWhileMoving;
            }

            if (horizontal > 0 && straffing)
            {
                strafeDirection += 1 * Time.deltaTime;
                if (strafeDirection > .5f)
                    strafeDirection = .5f;
                animator.SetFloat(HashStrafe, strafeDirection);
            }
            else if (horizontal < 0 && straffing)
            {
                strafeDirection -= 1 * Time.deltaTime;
                if (strafeDirection < -.5f)
                    strafeDirection = -.5f;
                animator.SetFloat(HashStrafe, strafeDirection);
            }
            else
            {
                if (strafeDirection > 0.1f)
                {
                    strafeDirection -= 1 * Time.deltaTime;
                }
                else if (strafeDirection < -0.1f)
                {
                    strafeDirection += 1 * Time.deltaTime;
                }
                else
                {
                    strafeDirection = 0;
                }
                animator.SetFloat(HashStrafe, strafeDirection);
            }
        //Debug.Log("Move Input :" + moveInput);
        
        GetTargetVelocityAndAcceleration(moveInput, wasRunning, wasGrounded, out Vector3 moveDirection, out float targetSpeed, out float speedAcceleration, out float directionAcceleration);
        
        if (currentMoveSpeed > targetSpeed && targetSpeed != 0f)
        {
            speedAcceleration = accelerationWhenSlowing;
        }
        //speedAcceleration = speedAcceleration - (moveSpeed / currentMoveSpeed);
        currentMoveSpeed = Mathf.Lerp(currentMoveSpeed, targetSpeed, speedAcceleration * Time.deltaTime);

        if (currentMoveSpeed < 0.001f)
        {
            currentMoveDirection = Vector3.zero;
        }
        else if (moveDirection != Vector3.zero)
        {
            currentMoveDirection = Vector3.Lerp(currentMoveDirection, moveDirection, directionAcceleration * Time.deltaTime);

        }

        moveVelocity = currentMoveDirection * (currentMoveSpeed);

        if (IsRunning && !wasRunning)
        {
            OnStartRunning?.Invoke();
        }
        else if (!IsRunning && wasRunning)
        {
            OnStopRunning?.Invoke();
        }
        if (Grounded)
        {
            fallVelocity.y = 0f;

            // Handle
            //
            //ump
            if (wantsToJump && MovementControlLocks == 0)
            {
                fallVelocity.y += jumpSpeed;
                Grounded = false;
                OnLeftGround?.Invoke();
                currentMoveSpeed *= speedLostOnJump;
                StartCoroutine(JustJumped());
            }
        }
        else
        {
            // Accelerate fallVelocity by gravity
            fallVelocity.y += Physics.gravity.y * gravityMultiplier * Time.deltaTime;
        }

        fallVelocity.y += physicsVelocity.y;
        physicsVelocity.y = 0f;

        iPositionNow = transform.localPosition;

        // Combine velocities and move
        //Debug.Log("Fall Velocity :" + fallVelocity);
        combinedVelocity = (moveVelocity) + fallVelocity + externalVelocity + physicsVelocity;
        Vector3 moveDelta = combinedVelocity;
        

        // Project our velocity onto the ground so we don't fly into the air when running downhill
        if (Grounded && Vector3.Dot(groundHit.normal, Vector3.up) != 1f)
        {
            //currentMoveDirection = Vector3.ProjectOnPlane(currentMoveDirection, Vector3.ProjectOnPlane(transform.forward, groundHit.normal)).normalized;
            moveDelta.y -= downwardForceAlongSlope;
        }
       
        
        var collisionFlags = moveController.Move(moveDelta * Time.deltaTime);

        iPositionNext = transform.localPosition;


        // Check if we hit a collider above us
        if ((collisionFlags & CollisionFlags.Above) != 0)
        {
            // We want to negate any positive y velocity so we don't stick to the collider above us
            if (fallVelocity.y > 0f)
            {
                fallVelocity.y = 0f;
            }
        }

        // Apply linear drag to fallVelocity
        fallVelocity *= 1f - fallVelocityDrag * Time.deltaTime;
        externalVelocity *= 1f - externalVelocityDragGrounded * Time.deltaTime;
        physicsVelocity *= 1f - (Grounded ? externalVelocityDragGrounded : externalVelocityDragAir) * Time.deltaTime;

        //Debug.Log("Current speed = " + currentMoveSpeed);

    }
        public void CheckKeys()
    {
        //horizontal = Input.GetAxisRaw("Horizontal");
        //vertical = Input.GetAxisRaw("Vertical");
        
        
    }

    void PlayRunAnimation()
    {
        //animator.SetBool(HashRunning, true);
    }

    void EnableAnimatorGrounded()
    {
        animator.SetBool(HashGrounded, true);
    }

    void StopRunAnimation()
    {
        animator.SetBool(HashRunning, false);
    }

    void DisableAnimatorGrounded()
    {
        animator.SetBool(HashGrounded, false);
        //PlayJumpAnimation();
    }
    void PlayJumpAnimation()
    {
        StartCoroutine(JustJumped());
    }

    void PlayBrakingAnimation()
    {
        animator.SetBool(HashBraking, true);
    }

    void PlayFallingAnimation()
    {
        animator.SetBool(HashFalling, true);
    }
    void StopFallingAnimation()
    {
        animator.SetBool(HashFalling, false);
    }

    void StopBrakingAnimation()
    {
        animator.SetBool(HashBraking, false);
    }
    private IEnumerator JustJumped()
    {
        
            animator.SetBool(HashJumping, true);
        
       
            yield return new WaitForSeconds(.1f);
        animator.SetBool(HashJumping, false);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Water")
        {
            strafeDirection = 0f;
            GameM.Died = true;
         
        }
    }
}
