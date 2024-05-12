using UnityEngine;
using UnityEngine.InputSystem;
using Unity.Netcode;


[RequireComponent(typeof(CharacterController))]
public class FirstPersonController_NFGO : NetworkBehaviour
{
    [Header("Camera")] public Transform playerRoot;
    public Transform playerCam;

    public float cameraSensitivity;
    private float rotX;
    private float rotY;

    [Header("Movement")] public CharacterController controller;
    public float speed;
    public float jumpHeight;
    public float gravity;
    public bool isGrounded;
    private Vector3 velocity;

    //Input System
    [Header("Input")] public InputAction move;
    public InputAction jump;
    public InputAction mouseX;
    public InputAction mouseY;

    //Block
    public GameObject blockPrefab;
    public float maxPlaceDistance = 10f;
    public float maxRemoveDistance = 10f;
    public float gridSize = 0.75f;

    //Highlighting
    public Material highlightMaterial;
    private GameObject highlightedBlock;

    void OnEnable()
    {
        move.Enable();
        jump.Enable();
        mouseX.Enable();
        mouseY.Enable();
        
        jump.performed += ctx => Jump();
    }

    void OnDisable()
    {
        move.Disable();
        jump.Disable();
        mouseX.Dispose();
        mouseY.Disable();
        
        jump.performed -= ctx => Jump();
    }
    

    void Start()
    {
        if (!IsOwner)
        {
            foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
            {
                if (renderer.CompareTag("Player"))
                {
                    renderer.enabled = true;
                }
            }
            return;
        }

        Cursor.lockState = CursorLockMode.Locked;

        controller = GetComponent<CharacterController>();

         // Disable rendering of the local player's body
        foreach (Renderer renderer in GetComponentsInChildren<Renderer>())
        {
            if (renderer.CompareTag("Player"))
            {
                renderer.enabled = false;
            }
        }
    }

    void Update()
    {
        // Log player activity
        
        if (!IsOwner)
        {
            playerCam.GetComponent<Camera>().enabled = false;
            return;
        }
        
        HighlightBlock();
        HandleBlocks();
        HandleCamera();
        HandleMovement();
    }
        // Placing blocks
        // if (Mouse.current.leftButton.wasPressedThisFrame)
        // {
        //     PlaceBlockRpc(playerCam.position, playerCam.forward);
        // }
        // else if (Mouse.current.rightButton.wasPressedThisFrame)
        // {
        //     RemoveBlockRpc(playerCam.position, playerCam.forward);
        // }

        // controller.Move(velocity * Time.deltaTime);

        // // Camera movement
        // Vector2 mouseInput = new Vector2(mouseX.ReadValue<float>() * cameraSensitivity,
        //     mouseY.ReadValue<float>() * cameraSensitivity);
        // rotX -= mouseInput.y;
        // rotX = Mathf.Clamp(rotX, -90, 90);
        // rotY += mouseInput.x;

        // playerRoot.rotation = Quaternion.Euler(0f, rotY, 0f);
        // playerCam.localRotation = Quaternion.Euler(rotX, 0f, 0f);

        // // Player movement
        // Vector2 moveInput = move.ReadValue<Vector2>();
        // Vector3 moveVelocity = playerRoot.forward * moveInput.y + playerRoot.right * moveInput.x;

        // controller.Move(moveVelocity * (speed * Time.deltaTime));

        // isGrounded = Physics.Raycast(feet.position, feet.TransformDirection(Vector3.down), 0.15f);
        
        // // Gravity
        // if (!isGrounded)
        // {
        //     velocity.y -= gravity * Time.deltaTime;
        // }
        // else
        // {
        //     velocity.y = -0.1f;
        // }

        // controller.Move(velocity * Time.deltaTime);
    //}
 void HandleCamera()
    {
        Vector2 mouseInput = new Vector2(mouseX.ReadValue<float>() * cameraSensitivity,
            mouseY.ReadValue<float>() * cameraSensitivity);
        rotX -= mouseInput.y;
        rotX = Mathf.Clamp(rotX, -90, 90);
        rotY += mouseInput.x;

        playerRoot.rotation = Quaternion.Euler(0f, rotY, 0f);
        playerCam.localRotation = Quaternion.Euler(rotX, 0f, 0f); 
    }

    void HandleBlocks()
    {
        if (Mouse.current.rightButton.wasPressedThisFrame)
        {
            PlaceBlockRpc(playerCam.position, playerCam.forward);
        }
        else if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            RemoveBlockRpc(playerCam.position, playerCam.forward);
        }

        controller.Move(velocity * Time.deltaTime);
    }
    
    void HandleMovement()
    {
        Vector2 moveInput = move.ReadValue<Vector2>();
        Vector3 moveVelocity = playerRoot.forward * moveInput.y + playerRoot.right * moveInput.x;
        moveVelocity *= speed;

        if (controller.isGrounded)
        {
            velocity.y = -0.1f; // Reset velocity when grounded
            if (jump.triggered)
            {
                velocity.y = Mathf.Sqrt(2f * jumpHeight * gravity); // Jump
            }
        }
        else
        {
            velocity.y -= gravity * Time.deltaTime; // Apply gravity when jumping or falling
        }

        controller.Move((moveVelocity + velocity) * Time.deltaTime);
    }
    void Jump()
    {
        if (isGrounded)
        {
            velocity.y = Mathf.Sqrt(2f * jumpHeight * gravity);
        }
    }

    // Placing and removing blocks
    // [Command]
    [Rpc(SendTo.Server)]
    void PlaceBlockRpc(Vector3 cameraPosition, Vector3 cameraForward)
    {
         RaycastHit hit;
        int layerMask = ~(1 << LayerMask.NameToLayer("Player")); 

        if (Physics.Raycast(cameraPosition, cameraForward, out hit, maxPlaceDistance, layerMask))
        {
            if (hit.collider.CompareTag("Block") || hit.distance <= maxPlaceDistance)
            {
                Vector3 gridPosition = RoundToNearestGrid(hit.point + hit.normal * 0.5f);
                GameObject newBlock = Instantiate(blockPrefab, gridPosition, Quaternion.identity);
                NetworkObject blockNetworkObject = newBlock.GetComponent<NetworkObject>();
                blockNetworkObject.Spawn();
            }
        }
    }
    
    

    Vector3 RoundToNearestGrid(Vector3 position)
    {
        float x = Mathf.Round(position.x / gridSize) * gridSize;
        float y = Mathf.Round(position.y / gridSize) * gridSize;
        float z = Mathf.Round(position.z / gridSize) * gridSize;
        return new Vector3(x, y, z);
    }

    bool IsBlockAtPosition(Vector3 position)
    {
        Collider[] colliders = Physics.OverlapSphere(position, 0.35f);
        foreach (Collider collider in colliders)
        {
            if (collider.CompareTag("Block"))
            {
                return true;
            }
        }

        return false;
    }

    // [Command]
    [Rpc(SendTo.Server)]
    void RemoveBlockRpc(Vector3 cameraPosition, Vector3 cameraForward)
    {
        RaycastHit hit;
        int layerMask = ~(1 << LayerMask.NameToLayer("Player")); 

        if (Physics.Raycast(cameraPosition, cameraForward, out hit, maxPlaceDistance, layerMask))
        {
            if (hit.collider.CompareTag("Block") || hit.distance <= maxPlaceDistance) 
            {
                NetworkObject blockNetworkObject = hit.collider.gameObject.GetComponent<NetworkObject>();
                blockNetworkObject.Despawn();
                Destroy(hit.collider.gameObject);
                
            }
        }
    }

    void HighlightBlock()
    {
        RaycastHit hit;
        int layerMask = ~(1 << LayerMask.NameToLayer("Player")); // Ignore the player layer

        Vector3 raycastDirection = playerCam.forward;
        Vector3 raycastDirectionDownward = Vector3.down;

        if (Physics.Raycast(playerCam.position, raycastDirection, out hit, maxPlaceDistance, layerMask) ||
            Physics.Raycast(playerCam.position, raycastDirectionDownward, out hit, maxPlaceDistance, layerMask))
        {
            GameObject hitBlock = hit.collider.gameObject;
            if (hitBlock.CompareTag("Block"))
            {
                Renderer blockRenderer = hitBlock.GetComponent<Renderer>();
                Material[] materials = blockRenderer.materials;

                if (materials.Length < 2)
                {
                    if (highlightedBlock != null && highlightedBlock != hitBlock)
                    {
                        RemoveHighlight(highlightedBlock);
                    }

                    if (highlightedBlock != hitBlock)
                    {
                        ApplyHighlight(hitBlock);
                        highlightedBlock = hitBlock;
                    }
                }
            }
        }
        else
        {
            if (highlightedBlock != null)
            {
                RemoveHighlight(highlightedBlock);
                highlightedBlock = null;
            }
        }
    }

    void ApplyHighlight(GameObject block)
    {
        Renderer blockRenderer = block.GetComponent<Renderer>();
        Material[] materials = blockRenderer.materials;
        Material[] newMaterials = new Material[materials.Length + 1];
        for (int i = 0; i < materials.Length; i++)
        {
            newMaterials[i] = materials[i];
        }

        newMaterials[materials.Length] = highlightMaterial;
        blockRenderer.materials = newMaterials;
    }

    void RemoveHighlight(GameObject block)
    {
        Renderer blockRenderer = block.GetComponent<Renderer>();
        Material[] materials = blockRenderer.materials;

        if (materials.Length > 1)
        {
            Material[] newMaterials = new Material[materials.Length - 1];
            for (int i = 0; i < newMaterials.Length; i++)
            {
                newMaterials[i] = materials[i];
            }

            blockRenderer.materials = newMaterials;
        }
    }
}