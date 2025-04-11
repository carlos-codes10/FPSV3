using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
public class FPSController : MonoBehaviour
{

    // references
    CharacterController controller;
    [SerializeField] GameObject cam;
    [SerializeField] Transform gunHold;
    [SerializeField] Gun initialGun;

    // stats
    [SerializeField] float movementSpeed = 2.0f;
    [SerializeField] float lookSensitivityX = 1.0f;
    [SerializeField] float lookSensitivityY = 1.0f;
    [SerializeField] float gravity = -9.81f;
    [SerializeField] float jumpForce = 10;

    // private variables
    Vector3 origin;
    Vector3 velocity;
    bool grounded;
    float xRotation;
    List<Gun> equippedGuns = new List<Gun>();
    int gunIndex = 0;
    Gun currentGun = null;
    Vector2 movementInput;
    Vector2 lookInput;
    bool jumped = false;
    bool sprintInput = false;
    bool isShooting = false;

    // properties
    public GameObject Cam { get { return cam; } }


    private void Awake()
    {

    }

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        Cursor.lockState = CursorLockMode.Locked;

        // start with a gun
        if (initialGun != null)
            AddGun(initialGun);

        origin = transform.position;
    }

    // Update is called once per frame
    void Update()
    {

        Movement();
        AutomaticFire();
        //HandleSwitchGun();
        Look();

        // always go back to "no velocity"
        // "velocity" is for movement speed that we gain in addition to our movement (falling, knockback, etc.)
        Vector3 noVelocity = new Vector3(0, velocity.y, 0);
        velocity = Vector3.Lerp(velocity, noVelocity, 5 * Time.deltaTime);

    }
    void AutomaticFire()
    {
        if (currentGun == null)
            return;

        if (isShooting)
        {

        if (currentGun.AttemptAutomaticFire())
        {
            Debug.Log("AUTOMATIC GUN FIRING");
            currentGun?.AttemptFire();
        }

        }
    }
    void Movement()
    {

        Debug.Log("moving...");

        grounded = controller.isGrounded;

        if (grounded && velocity.y < 0)
        {
            velocity.y = -1;// -0.5f;
        }

        Vector3 move = transform.right * movementInput.x + transform.forward * movementInput.y;
        controller.Move(move * movementSpeed * (sprintInput ? 2 : 1) * Time.deltaTime);

        velocity.y += gravity * Time.deltaTime;

        controller.Move(velocity * Time.deltaTime);
    }

    void OnMovement(InputValue v)
    {
        Debug.Log("Message called OnMovement!");
        movementInput = v.Get<Vector2>();
    }
       

    void OnJump(InputValue v)
    {
        jumped = v.isPressed;

        Debug.Log("Message OnJump Called");
        grounded = controller.isGrounded;

        if (jumped && grounded)
        {
            velocity.y += Mathf.Sqrt(jumpForce * -1 * gravity);
        }
    }

    void Look()
    {
        
        float lookX = lookInput.x * lookSensitivityX * Time.deltaTime;
        float lookY = lookInput.y * lookSensitivityY * Time.deltaTime;

        xRotation -= lookY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        cam.transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        transform.Rotate(Vector3.up * lookX);
    }

    public void OnLook(InputValue v)
    {
        lookInput = v.Get<Vector2>();
    }

    void OnScrollWheel(InputValue v)
    {

        Debug.Log("Scroll Wheel function called!");

        if (equippedGuns.Count == 0)
            return;

        if (v.Get<float>() > 0)
        {
            Debug.Log("Scroll Wheel Up");
            gunIndex++;
            if (gunIndex > equippedGuns.Count - 1)
                gunIndex = 0;

            EquipGun(equippedGuns[gunIndex]);
        }

        else if (v.Get<float>() < 0)
        {
            Debug.Log("Scroll Wheel Down");
            gunIndex--;
            if (gunIndex < 0)
                gunIndex = equippedGuns.Count - 1;

            EquipGun(equippedGuns[gunIndex]);
        }
    }


    void EquipGun(Gun g)
    {
        // disable current gun, if there is one
        currentGun?.Unequip();
        currentGun?.gameObject.SetActive(false);

        // enable the new gun
        g.gameObject.SetActive(true);
        g.transform.parent = gunHold;
        g.transform.localPosition = Vector3.zero;
        currentGun = g;

        g.Equip(this);
    }

    // public methods

    public void AddGun(Gun g)
    {
        // add new gun to the list
        equippedGuns.Add(g);

        // our index is the last one/new one
        gunIndex = equippedGuns.Count - 1;

        // put gun in the right place
        EquipGun(g);
    }

    public void IncreaseAmmo(int amount)
    {
        currentGun.AddAmmo(amount);
    }

    public void Respawn()
    {
        transform.position = origin;
    }

    // Input methods

    void OnShoot(InputValue v)
    {

        if (currentGun == null)
        {
            return;
        }

        isShooting = v.isPressed;  

        Debug.Log("OnShoot Message Called!");



        if (isShooting)
        {     
            
          currentGun?.AttemptFire();
            
        }
        

    }

    /*Vector2 GetPlayerLook()
    {
        return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
    }

    bool GetSprint()
    {
        return Input.GetButton("Sprint");
    }*/

    bool OnSprint(InputValue v)
    {
        return sprintInput = v.isPressed;
    }

    // Collision methods

    // Character Controller can't use OnCollisionEnter :D thanks Unity
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.GetComponent<Damager>())
        {
            var collisionPoint = hit.collider.ClosestPoint(transform.position);
            var knockbackAngle = (transform.position - collisionPoint).normalized;
            velocity = (20 * knockbackAngle);
        }

        if (hit.gameObject.GetComponent<KillZone>())
        {
            Respawn();
        }
    }


}
