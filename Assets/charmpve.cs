using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharMovement : MonoBehaviour
{
    public float speed = 10.0f;
    public float jumpForce = 8.0f;
    public float gravity = 20.0f;
    public float rotationSpeed = 100.0f;

    public bool isGrounded = false;
    public bool isDef = false;
    public bool isDancing = false;
    public bool isWalking = false;
    public bool isTaking = false; // เพิ่มสถานะการเก็บไอเทม

    private Animator animator;
    private CharacterController characterController;
    private Vector3 inputVector = Vector3.zero;
    private Vector3 targetDirection = Vector3.zero;
    private Vector3 moveDirection = Vector3.zero;
    private Vector3 velocity = Vector3.zero;
    GameController gameController;
    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }

    void Start()
    {
        Time.timeScale = 1;
        isGrounded = characterController.isGrounded;
    }

    void Update()
    {
        float z = Input.GetAxis("Horizontal");
        float x = -(Input.GetAxis("Vertical"));
        Debug.Log("z:" + z);
        Debug.Log("x:" + x);

        animator.SetFloat("inputX", -(x));
        animator.SetFloat("inputZ", z);

        if (x != 0 || z != 0)
        {
            isWalking = true;
            animator.SetBool("isWalking", isWalking);
            Debug.Log("isWalking" + isWalking);
        }
        else
        {
            isWalking = false;
            animator.SetBool("isWalking", isWalking);
            Debug.Log("isWalking" + isWalking);
        }

        isGrounded = characterController.isGrounded;
        if (isGrounded)
        {
            moveDirection = new Vector3(Input.GetAxis("Horizontal"), 0.0f, Input.GetAxis("Vertical"));
            moveDirection *= speed;

            // Handle jumping
            if (Input.GetButton("Jump"))
            {
                moveDirection.y = jumpForce;
                animator.SetBool("isJumping", true);
            }
            else
            {
                animator.SetBool("isJumping", false);
            }
        }
        else
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        characterController.Move(moveDirection * Time.deltaTime);

        inputVector = new Vector3(x, 0.0f, z);

        // Update movement before rotation
        updateMovement();

        // Handle taking items
        if (isTaking && Input.GetKeyDown(KeyCode.E))
        {
            animator.SetBool("isTaking", true); // อัปเดตสถานะการเก็บไอเทมในอนิเมชัน
            StartCoroutine(WaitForTaking(2.0f)); // ตั้งค่าการเก็บไอเทม
        }
    }

    void updateMovement()
    {
        Vector3 motion = inputVector;
        motion = ((Mathf.Abs(motion.x) > 1) || (Mathf.Abs(motion.z) > 1)) ? motion.normalized : motion;

        // Call viewRelativeMovement first to update targetDirection
        viewRelativeMovement();
        rotatTowardMovement();
    }

    void rotatTowardMovement()
    {
        if (inputVector != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }

    void viewRelativeMovement()
    {
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;
        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);
        targetDirection = (Input.GetAxis("Horizontal") * right) + (Input.GetAxis("Vertical") * forward);
    }

    // เพิ่มฟังก์ชันการเก็บไอเทม
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Item")
        {
            isTaking = true;
            Debug.Log("isTaking: " + isTaking);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Item")
        {
            isTaking = false;
            animator.SetBool("isTaking", false); // รีเซ็ตสถานะการเก็บไอเทม
            Debug.Log("isTaking: " + isTaking);
        }
    }

    IEnumerator WaitForTaking(float time)
    {
        yield return new WaitForSeconds(time);
        isTaking = false;
        animator.SetBool("isTaking", false); // รีเซ็ตสถานะการเก็บไอเทมหลังจากทำการเก็บไอเทมเสร็จ
        Debug.Log("Item taken");
    }
}
