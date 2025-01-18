using System;
using UnityEngine;
using Mirror;
using uSurvival;

// Alias for uSurvival.Utils to disambiguate from Mirror.Utils
using uUtils = uSurvival.Utils;

public partial class PlayerLook : NetworkBehaviour
{
    [Header("Components")]
    public Joystick joystick;
    public IKHandling ik;
    public PlayerMovement movement;
    public Health health;
#pragma warning disable CS0109 // member does not hide accessible member
    new Camera camera;
#pragma warning restore CS0109 // member does not hide accessible member

    float lastClientSendTime;

    [Header("Camera")]
    public float XSensitivity = 2;
    public float YSensitivity = 2;
    public float MinimumX = -90;
    public float MaximumX = 90;

    // head position is useful for raycasting etc.
    public Transform firstPersonParent;
    public Vector3 headPosition => firstPersonParent.position;
    public Transform freeLookParent;

    Vector3 originalCameraPosition;

    public KeyCode freeLookKey = KeyCode.LeftAlt;

    [SyncVar] bool syncedIsFreeLooking;

    public LayerMask viewBlockingLayers;
    public float zoomSpeed = 0.5f;
    public float distance = 0;
    public float minDistance = 0;
    public float maxDistance = 7;

    [Header("Physical Interaction")]
    [Tooltip("Layers to use for raycasting. Check Default, Walls, Player, Monster, Doors, Interactables, Item, etc. Uncheck IgnoreRaycast, AggroArea, Water, UI, etc.")]
    public LayerMask raycastLayers = Physics.DefaultRaycastLayers;

    [Header("Offsets - Standing")]
    public Vector2 firstPersonOffsetStanding = Vector2.zero;
    public Vector2 thirdPersonOffsetStanding = Vector2.up;
    public Vector2 thirdPersonOffsetStandingMultiplier = Vector2.zero;

    [Header("Offsets - Crouching")]
    public Vector2 firstPersonOffsetCrouching = Vector2.zero;
    public Vector2 thirdPersonOffsetCrouching = Vector2.up / 2;
    public Vector2 thirdPersonOffsetCrouchingMultiplier = Vector2.zero;
    public float crouchOriginMultiplier = 0.65f;

    [Header("Offsets - Crawling")]
    public Vector2 firstPersonOffsetCrawling = Vector2.zero;
    public Vector2 thirdPersonOffsetCrawling = Vector2.up;
    public Vector2 thirdPersonOffsetCrawlingMultiplier = Vector2.zero;
    public float crawlOriginMultiplier = 0.65f;

    [Header("Offsets - Swimming")]
    public Vector2 firstPersonOffsetSwimming = Vector2.zero;
    public Vector2 thirdPersonOffsetSwimming = Vector2.up;
    public Vector2 thirdPersonOffsetSwimmingMultiplier = Vector2.zero;
    public float swimOriginMultiplier = 0.65f;

    [SyncVar, HideInInspector] public Vector3 syncedLookDirectionFar;

    public Vector3 lookDirectionFar
    {
        get
        {
            return isLocalPlayer ? camera.transform.forward : syncedLookDirectionFar;
        }
    }

    public Vector3 lookDirectionRaycasted
    {
        get
        {
            return (lookPositionRaycasted - headPosition).normalized;
        }
    }

    public Vector3 lookPositionFar
    {
        get
        {
            Vector3 position = isLocalPlayer ? camera.transform.position : headPosition;
            return position + lookDirectionFar * 9999f;
        }
    }

    public Vector3 lookPositionRaycasted
    {
        get
        {
            if (isLocalPlayer)
            {
                return uUtils.RaycastWithout(camera.transform.position, camera.transform.forward, out RaycastHit hit, Mathf.Infinity, gameObject, raycastLayers)
                       ? hit.point
                       : lookPositionFar;
            }
            else
            {
                Debug.LogError("PlayerLook.lookPositionRaycasted isn't synced so you can only call it on the local player right now.\n" + Environment.StackTrace);
                return Vector3.zero;
            }
        }
    }

    void Awake()
    {
        camera = Camera.main;
    }

    void Start()
    {
        if (isLocalPlayer)
        {
            camera.transform.SetParent(transform, false);
            camera.transform.rotation = transform.rotation;
            camera.transform.position = headPosition;
        }

        originalCameraPosition = camera.transform.localPosition;
    }

    [Command]
    void CmdSetLookDirection(Vector3 lookDirectionFar)
    {
        syncedLookDirectionFar = lookDirectionFar;
    }

    [Command]
    void CmdSetFreeLooking(bool state)
    {
        syncedIsFreeLooking = state;
    }

    void Update()
    {
        if (!isLocalPlayer) return;

        if (joystick == null)
        {
            GameObject joystickObj = GameObject.Find("Variable Joystick Look");
            if (joystickObj != null)
            {
                joystick = joystickObj.GetComponent<Joystick>();
            }
        }

        if (Time.time - lastClientSendTime >= syncInterval)
        {
            if (Vector3.Distance(lookDirectionFar, syncedLookDirectionFar) > Mathf.Epsilon)
                CmdSetLookDirection(lookDirectionFar);

            bool freeLooking = IsFreeLooking();
            if (freeLooking != syncedIsFreeLooking)
                CmdSetFreeLooking(freeLooking);

            lastClientSendTime = Time.time;
        }

        if (health.current > 0)
        {
            if (joystick != null)
            {
                float xExtra = joystick.Horizontal * XSensitivity;
                float yExtra = joystick.Vertical * YSensitivity;

                if (movement.state == MoveState.CLIMBING ||
                    (Input.GetKey(freeLookKey) && !UIUtils.AnyInputActive() && distance > 0))
                {
                    if (camera.transform.parent != freeLookParent)
                        InitializeFreeLook();

                    freeLookParent.Rotate(new Vector3(0, xExtra, 0));
                    camera.transform.Rotate(new Vector3(-yExtra, 0, 0));
                }
                else
                {
                    if (camera.transform.parent != transform)
                        InitializeForcedLook();

                    transform.Rotate(new Vector3(0, xExtra, 0));
                    camera.transform.Rotate(new Vector3(-yExtra, 0, 0));
                }
            }
            else
            {
                // Handle alternative input or do nothing if joystick is not found
            }
        }
    }

    void LateUpdate()
    {
        if (!isLocalPlayer) return;

        // Ensure the cursor is always visible and movable
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        camera.transform.localRotation = uUtils.ClampRotationAroundXAxis(camera.transform.localRotation, MinimumX, MaximumX);

        if (!uUtils.IsCursorOverUserInterface())
        {
            float step = uUtils.GetZoomUniversal() * zoomSpeed;
            distance = Mathf.Clamp(distance - step, minDistance, maxDistance);
        }

        if (distance == 0) // first person
        {
            Vector3 headLocal = transform.InverseTransformPoint(headPosition);
            Vector3 origin = Vector3.zero;
            Vector3 offset = Vector3.zero;

            if (movement.state == MoveState.CROUCHING)
            {
                origin = headLocal * crouchOriginMultiplier;
                offset = firstPersonOffsetCrouching;
            }
            else if (movement.state == MoveState.CRAWLING)
            {
                origin = headLocal * crawlOriginMultiplier;
                offset = firstPersonOffsetCrawling;
            }
            else if (movement.state == MoveState.SWIMMING)
            {
                origin = headLocal;
                offset = firstPersonOffsetSwimming;
            }
            else
            {
                origin = headLocal;
                offset = firstPersonOffsetStanding;
            }

            Vector3 target = transform.TransformPoint(origin + offset);
            camera.transform.position = target;
        }
        else // third person
        {
            Vector3 origin = Vector3.zero;
            Vector3 offsetBase = Vector3.zero;
            Vector3 offsetMult = Vector3.zero;

            if (movement.state == MoveState.CROUCHING)
            {
                origin = originalCameraPosition * crouchOriginMultiplier;
                offsetBase = thirdPersonOffsetCrouching;
                offsetMult = thirdPersonOffsetCrouchingMultiplier;
            }
            else if (movement.state == MoveState.CRAWLING)
            {
                origin = originalCameraPosition * crawlOriginMultiplier;
                offsetBase = thirdPersonOffsetCrawling;
                offsetMult = thirdPersonOffsetCrawlingMultiplier;
            }
            else if (movement.state == MoveState.SWIMMING)
            {
                origin = originalCameraPosition * swimOriginMultiplier;
                offsetBase = thirdPersonOffsetSwimming;
                offsetMult = thirdPersonOffsetSwimmingMultiplier;
            }
            else
            {
                origin = originalCameraPosition;
                offsetBase = thirdPersonOffsetStanding;
                offsetMult = thirdPersonOffsetStandingMultiplier;
            }

            Vector3 target = transform.TransformPoint(origin + offsetBase + offsetMult * distance);
            Vector3 newPosition = target - (camera.transform.rotation * Vector3.forward * distance);

            float finalDistance = distance;
            Debug.DrawLine(target, camera.transform.position, Color.white);
            if (Physics.Linecast(target, newPosition, out RaycastHit hit, viewBlockingLayers))
            {
                finalDistance = Vector3.Distance(target, hit.point) - 0.1f;
                Debug.DrawLine(target, hit.point, Color.red);
            }
            else Debug.DrawLine(target, newPosition, Color.green);

            camera.transform.position = target - (camera.transform.rotation * Vector3.forward * finalDistance);
        }
    }

    public bool InFirstPerson()
    {
        return distance == 0;
    }

    public bool IsFreeLooking()
    {
        if (isLocalPlayer)
        {
            return camera != null &&
                   camera.transform.parent == freeLookParent;
        }
        return syncedIsFreeLooking;
    }

    public void InitializeFreeLook()
    {
        camera.transform.SetParent(freeLookParent, false);
        freeLookParent.localRotation = Quaternion.identity;
        ik.lookAtBodyWeightActive = false;
    }

    public void InitializeForcedLook()
    {
        camera.transform.SetParent(transform, false);
        ik.lookAtBodyWeightActive = true;
    }

    void OnDrawGizmos()
    {
        if (!isLocalPlayer) return;

        Gizmos.color = Color.white;
        Gizmos.DrawLine(headPosition, camera.transform.position + camera.transform.forward * 9999f);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(headPosition, lookPositionFar);
        Gizmos.color = Color.cyan;
        Gizmos.DrawLine(headPosition, lookPositionRaycasted);
    }
}
