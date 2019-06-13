using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(Rigidbody))]
public class PlayerMultiplayerMotor : PlayerMotor
{
    // Components
    [SerializeField] private Animator animator;
    [SerializeField] private GameObject head;
    [SerializeField] private GameObject playerArmsContainer, camContainer;
    private Player player;
    private PlayerUse playerUse;

    private void Start()
    {
        //Get components
        rb = GetComponent<Rigidbody>();
        player = GetComponent<Player>();
        playerUse = GetComponent<PlayerUse>();
    }

    private void SetAnimateSpeed(float animateSpeed)
    {
        //Set walking animation based on speed
        animator.SetFloat("Speed", animateSpeed);
    }

    protected override void Update()
    {
        if (player.isLocalPlayer)
        {
            base.Update();
        
            if (!CursorManagement.IsMenuOpen()) 
            {
                if (Input.GetButtonDown("Sprint")) playerUse.SetRunningAnimation();
                else if (Input.GetButtonUp("Sprint")) playerUse.ResetAnimation();
            }
        }
        else
        {
            // Set player arms and head equal to the camera rotation
            float camRotX = -camContainer.transform.localEulerAngles.x;
            if (camRotX < -180F) camRotX = 360F+camRotX;
            Vector3 clampedRot = new Vector3(Mathf.Clamp(camRotX, -50F, 25F), 0F, 0F);
            playerArmsContainer.transform.localEulerAngles = -clampedRot;
            head.transform.localEulerAngles = clampedRot;
        }

        bhopEnabled = GameManager.instance.bhopEnabled;
    }

    //protected override void Look()
    //{
    //    base.Look();

    //    // Head mesh up/down (Clamp between -25 and 50 degrees)
    //    //float _rotationY = rotationY;
    //    //_rotationY = Mathf.Clamp(rotationY, -25, 50);
    //    //head.transform.localEulerAngles = new Vector3(_rotationY, 0, 0);
    //}

    protected override void JumpEnded()
    {
        if (running)
            playerUse.SetRunningAnimation();
    }

    protected override void JumpStarted()
    {
        playerUse.ResetAnimation();
    }

    protected override void OnGUI()
    {
        if (player.isLocalPlayer)
        {
            base.OnGUI();
        }
    }
}



