////Local motion controller player////
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(CharacterController))]
public class bl_PlayerMovement : MonoBehaviour {

    private bl_PlayerDamageManager pdm;
    private bl_Ladder m_ladder;
    private bl_LadderPlayer m_PlayerLadder;
    [Header("Inputs")]
    public KeyCode CrouchKey = KeyCode.C;
    public KeyCode RunKey = KeyCode.LeftShift;
    public KeyCode JumpKey = KeyCode.Space;
    public KeyCode SuperJumpKey = KeyCode.Space;
    public bool ToggleCrouch = false;
    [Header("Movement")]
    public float crouchSpeed = 2;
    public float walkSpeed = 6.0f;
    public float runSpeed = 11.0f;
    [Space(5)]
    public float m_crounchDelay = 8f;
    public float jumpSpeed = 8.0f;

    public float gravity = 20.0f;
    public float speed;
    public float ladderJumpSpeed = 5f;
    [Space(5)]
    [Header("SuperJump")]
    //Can the player take a super jump
    public bool CanSuperJump = true;
    public float SecondJumpDelay = 0.2f;
    public bool BoostMovement = true;
    public Vector2 AirControlFactor = new Vector2(0.75f, 1.23f);
    public float SuperJumpSpeed = 14.0f;
    public AudioSource BoostSource = null;
    public AudioClip SuperJumpSound;
    [Space(5)]
    [Header("Others")]
    // If the player ends up on a slope which is at least the Slope Limit as set on the character controller, then he will slide down
    public bool slideWhenOverSlopeLimit = false;

    // If checked and the player is on an object tagged "Slide", he will slide down it regardless of the slope limit
    public bool slideOnTaggedObjects = false;

    public float slideSpeed = 12.0f;

    public bool airControl = false; // If checked, then the player can change direction while in the air
	public float antiBumpFactor = .75f; // Small amounts of this results in bumping when walking down slopes, but large amounts results in falling too fast

	// Player must be grounded for at least this many physics frames before being able to jump again; set to 0 to allow bunny hopping 
	public int antiBunnyHopFactor = 1;

    public float PushPower = 2;

    private Vector3 moveDirection = Vector3.zero;
    private Vector3 lastPosition = Vector3.zero;

    [HideInInspector]
    public bool grounded = false;
    [HideInInspector]
    public bool m_OnLadder;
    [Space(5)]
    [Header("Fall Player")]
    // Units that player can fall before a falling damage function is run. To disable, type "infinity" in the inspector
    public float fallingDamageThreshold = 10.0f;
    public float FallDamageMultipler = 2.3f;

    private CharacterController controller;
    private Transform myTransform;
    private RaycastHit hit;
    private bool falling = false;
    private float slideLimit;
    private float rayDistance;
    private bool playerControl = false;
    private int jumpTimer;
    private bool canSecond = false;
    private float fallDistance;
    [HideInInspector]
    public int state = 0;
    [HideInInspector]
    public bool run;
    [HideInInspector]
    public bool canRun = true;
    [HideInInspector]
    public bool running;
    private float distanceToObstacle;
    public GameObject cameraHolder;
    private float normalHeight = 0.7f;
    private float crouchHeight = 0.0f;
    private float m_HPoint;
    private bool isSuperJump = false;
    private Vector3 vel = Vector3.zero;
    [HideInInspector]
    public float velMagnitude;

    void Awake() {
        pdm = this.GetComponent<bl_PlayerDamageManager>();
        m_PlayerLadder = this.GetComponent<bl_LadderPlayer>();
    }

    void Start() {
        controller = GetComponent<CharacterController>();
        myTransform = this.transform;
        speed = walkSpeed;
        rayDistance = controller.height * .5f + controller.radius;
        slideLimit = controller.slopeLimit - .1f;
        jumpTimer = antiBunnyHopFactor;

    }

    void OnEnable() {
        bl_EventHandler.OnRoundEnd += this.OnRoundEnd;
    }

    void OnDisable() {
        bl_EventHandler.OnRoundEnd -= this.OnRoundEnd;
    }

    public void Update() {
        this.vel = this.controller.velocity;
        this.velMagnitude = this.vel.magnitude;
        if (this.m_OnLadder) {
            this.m_PlayerLadder.LadderUpdate();
            this.m_HPoint = this.myTransform.position.y;
            this.fallDistance = 0;
            this.grounded = false;
            this.run = false;
            this.running = false;
        }
        else {
            float inputX = Input.GetAxis("Horizontal");
            float inputY = Input.GetAxis("Vertical");

            float inputModifyFactor = (inputX != 0.0f && inputY != 0.0f) ? .7071f : 1.0f;

            if (grounded) {
                bool sliding = false;
                canSecond = false;
                isSuperJump = false;
                // See if surface immediately below should be slid down. We use this normally rather than a ControllerColliderHit point,
                // because that interferes with step climbing amongst other annoyances
                if (Physics.Raycast(myTransform.position, -Vector3.up, out hit, rayDistance)) {
                    if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit)
                        sliding = true;
                }
                // However, just raycasting straight down from the center can fail when on steep slopes
                // So if the above raycast didn't catch anything, raycast down from the stored ControllerColliderHit point instead
                else {
                    if (Vector3.Angle(hit.normal, Vector3.up) > slideLimit) sliding = true;
                }
                // If we were falling, and we fell a vertical distance greater than the threshold, run a falling damage routine
                if (falling) {
                    falling = false;
                    this.fallDistance = this.m_HPoint - this.myTransform.position.y;
                    if (this.fallDistance > this.fallingDamageThreshold)
                        FallingDamageAlert(fallDistance);
                    if ((this.fallDistance < this.fallingDamageThreshold) && (this.fallDistance > 0.0075f))
                        bl_EventHandler.OnSmallImpactEvent();
                }
                if (canRun && cameraHolder.transform.localPosition.y > normalHeight - 0.1f) {
                    if (Input.GetKey(RunKey) && Input.GetKey(KeyCode.W) && !Input.GetMouseButton(1))
                        run = true;
                    else
                        run = false;
                }
                // If sliding (and it's allowed), get a vector pointing down the slope we're on
                if ((sliding && slideWhenOverSlopeLimit)) {
                    Vector3 hitNormal = hit.normal;
                    moveDirection = new Vector3(hitNormal.x, -hitNormal.y, hitNormal.z);
                    Vector3.OrthoNormalize(ref hitNormal, ref moveDirection);
                    moveDirection *= slideSpeed;
                    playerControl = false;
                }
                else {
                    if (state == 0) {
                        if (run)
                            speed = runSpeed;
                        else {
                            if (Input.GetButton("Fire2"))
                                speed = crouchSpeed;
                            else
                                speed = walkSpeed;
                        }
                    }
                    else if (state == 1) {
                        speed = crouchSpeed;
                        run = false;
                        if ((Input.GetKey(RunKey) && Input.GetKey(KeyCode.W)) && !Input.GetMouseButton(1))
                            this.state = 0;
                    }
                    if (bl_UtilityHelper.GetCursorState)
                        moveDirection = new Vector3(inputX * inputModifyFactor, -antiBumpFactor, inputY * inputModifyFactor);
                    else
                        moveDirection = new Vector3(0, -antiBumpFactor, 0);

                    moveDirection = myTransform.TransformDirection(moveDirection);
                    moveDirection *= speed;

                    if (Input.GetKeyDown(JumpKey) && jumpTimer >= antiBunnyHopFactor && !canSecond) {
                        jumpTimer = 0;
                        moveDirection.y = jumpSpeed;
                        isSuperJump = false;
                        if (CanSuperJump)
                            StartCoroutine(secondJump());
                        if (state > 0) {
                            CheckDistance();
                            if (distanceToObstacle > 1.6f)
                                state = 0;
                        }
                    }
                    else
                        jumpTimer++;
                }

            }
            else {
                this.run = false;
                if (this.myTransform.position.y > this.lastPosition.y) {
                    this.m_HPoint = this.myTransform.position.y;
                    this.falling = true;
                }
                // If we stepped over a cliff or something, set the height at which we started falling
                if (!falling) {
                    falling = true;
                    m_HPoint = myTransform.position.y;
                }
                // If air control is allowed, check movement but don't touch the y component
                if (airControl && playerControl) {
                    moveDirection.x = inputX * speed * inputModifyFactor;
                    moveDirection.z = inputY * speed * inputModifyFactor;
                    moveDirection = myTransform.TransformDirection(moveDirection);
                }
                if (Input.GetKeyDown(SuperJumpKey) && canSecond && CanSuperJump) {
                    jumpTimer = 0;
                    moveDirection.y = SuperJumpSpeed;
                    canSecond = false;
                    isSuperJump = true;
                    bl_EventHandler.OnSmallImpactEvent();
                    if (SuperJumpSound && GetComponent<AudioSource>() != null) {
                        GetComponent<AudioSource>().clip = SuperJumpSound;
                        GetComponent<AudioSource>().Play();
                    }
                    if (state > 0) {
                        CheckDistance();
                        if (distanceToObstacle > 1.6f)
                            state = 0;
                    }
                }
            }
            if (ToggleCrouch) {
                if (Input.GetKeyDown(CrouchKey)) {
                    CheckDistance();
                    if (state == 0) state = 1;
                    else if (state == 1 && distanceToObstacle > 1.6f) state = 0;
                }
            }
            else {
                if (Input.GetKey(CrouchKey)) {
                    CheckDistance();
                    if (state == 0) state = 1;
                }
                else {
                    if (state == 1 && distanceToObstacle > 1.6f) state = 0;
                }
            }
            if (state == 0) Stand(); //Stand Position
			else if (state == 1) Crouch(); //Crouch Position

			moveDirection.y -= gravity * Time.deltaTime; // Apply gravity
			grounded = (controller.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0; // Move the controller, and set grounded true or false depending on whether we're standing on something
		}
        //When is super jump / jet pack, whe have more forced in moved
        if (BoostMovement && CanSuperJump && isSuperJump && !grounded)
            AirControl();
        else {
            if (BoostSource != null) {
                if (BoostSource.isPlaying)
                    BoostSource.Stop();
            }
        }
    }

    /// <param name="hit"></param>
    void OnControllerColliderHit(ControllerColliderHit hit) {
        Rigidbody mRig = hit.collider.attachedRigidbody;

        if (mRig == null || mRig.isKinematic || hit.moveDirection.y < -0.3f)
            return;

        mRig.AddForce(hit.moveDirection * PushPower, ForceMode.Impulse);
    }

    /// Player Control / Boost Movement when is super jump
    void AirControl() {
        float inputX = Input.GetAxis("Horizontal");
        float inputY = Input.GetAxis("Vertical");
        float inputModifyFactor = (inputX != 0 && inputY != 0) ? AirControlFactor.y : AirControlFactor.x;
        moveDirection.x = inputX * speed * inputModifyFactor;
        moveDirection.z = inputY * speed * inputModifyFactor;
        moveDirection = myTransform.TransformDirection(moveDirection);

        if (BoostSource != null) {
            if (BoostSource.clip == null)
                BoostSource.clip = SuperJumpSound;
            float t = (inputX + inputY) / 2;
            if (t < 0)
                t = t * -1;
            float mVolumen = BoostSource.volume;
            mVolumen = Mathf.Lerp(mVolumen, t, Time.deltaTime * 3);
            BoostSource.volume = mVolumen;
            if (!BoostSource.isPlaying)
                BoostSource.Play();
        }
    }

    /// When player is Walk,Run or Idle
    void Stand() {
        if (controller.height != 2.0f)
            controller.height = 2.0f;
        if (controller.center != Vector3.zero)
            controller.center = Vector3.zero;

        Vector3 ch = cameraHolder.transform.localPosition;
        if (cameraHolder.transform.localPosition.y > normalHeight)
            ch.y = normalHeight;
        else if (cameraHolder.transform.localPosition.y < normalHeight)
            ch.y = Mathf.SmoothStep(ch.y, normalHeight, Time.deltaTime * m_crounchDelay);
        cameraHolder.transform.localPosition = ch;
    }

    /// When player is in Crounch
    void Crouch() {
        if (controller.height != 1.4f)
            controller.height = 1.4f;
        controller.center = new Vector3(0, -0.3f, 0);
        Vector3 ch = cameraHolder.transform.localPosition;

        if (cameraHolder.transform.localPosition.y != crouchHeight) {
            ch.y = Mathf.SmoothStep(ch.y, crouchHeight, Time.deltaTime * m_crounchDelay);
            cameraHolder.transform.localPosition = ch;
        }
    }

    void LateUpdate() {
        this.lastPosition = this.myTransform.position;
    }

    void CheckDistance() {
        Vector3 pos = transform.position + controller.center - new Vector3(0, controller.height / 2, 0);
        RaycastHit hit;
        if (Physics.SphereCast(pos, controller.radius, transform.up, out hit, 10)) {
            distanceToObstacle = hit.distance;
            Debug.DrawLine(pos, hit.point, Color.red, 2.0f);
        }
        else
            distanceToObstacle = 3;
    }

    public void OnLadder() {
        this.m_OnLadder = true;
        this.moveDirection = Vector3.zero;
        this.grounded = false;
    }

    /// <param name="ladderMovement"></param>
    public void OffLadder(object ladderMovement) {
        this.m_OnLadder = false;
        Vector3 forward = this.gameObject.transform.forward;
        if (Input.GetAxis("Vertical") > 0)
            this.moveDirection = forward.normalized * 5;
    }

    public void JumpOffLadder() {
        this.m_OnLadder = false;
        Vector3 vector = this.cameraHolder.transform.forward + new Vector3((float)0, 0.2f, (float)0);
        this.moveDirection = (Vector3)(vector * this.ladderJumpSpeed);
    }
	// If falling damage occured, handle here. You can make player have hitpoints and remove some of them based on the distance fallen, add sound effects, etc.
    void FallingDamageAlert(float fallDistance) {
        pdm.GetFallDamage(fallDistance * FallDamageMultipler);
        bl_EventHandler.EventFall(fallDistance * FallDamageMultipler);
    }

    void OnRoundEnd() {
        this.enabled = false;
    }

    IEnumerator secondJump() {
        yield return new WaitForSeconds(SecondJumpDelay);
        canSecond = true;
    }
}