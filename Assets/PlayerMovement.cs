using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    // พารามิเตอร์การเคลื่อนที่ของตัวละคร
    public float speed = 10.0f;          // ความเร็วในการเคลื่อนที่
    public float jumpForce = 8.0f;       // แรงกระโดด
    public float gravity = 20.0f;         // แรงโน้มถ่วง
    public float rotationSpeed = 100.0f;  // ความเร็วในการหมุน

    // สถานะการอนิเมชัน
    public bool isGrounded = false;       // ตัวละครอยู่บนพื้นหรือไม่
    public bool isDef = false;             // สถานะป้องกัน
    public bool isDancing = false;         // สถานะเต้น
    public bool isWalking = false;         // สถานะเดิน
    //------------------------------------------------------------
    public bool isTaking = false;         // สถานะวิ่ง
    //------------------------------------------------------------
    private Animator animator;             // อ้างอิงถึง Animator
    private CharacterController characterController; // อ้างอิงถึง CharacterController
    private Vector3 inputVector = Vector3.zero;  // เวกเตอร์อินพุต
    private Vector3 targetDirection = Vector3.zero; // ทิศทางเป้าหมาย
    private Vector3 moveDirection = Vector3.zero; // ทิศทางการเคลื่อนที่
    private Vector3 velocity = Vector3.zero;       // ความเร็ว
    //------------------------------------------------------------
    GameController gameController; // อ้างอิงถึง GameController
    //------------------------------------------------------------
    void Awake()
    {
        // เริ่มต้นคอมโพเนนต์
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
    }                               // Start is called before the first frame update
    void Start()
    {
        // กำหนดค่า time scale ในระบบ
        Time.timeScale = 1;
        isGrounded = characterController.isGrounded;
        //------------------------------------------------------------
        if (gameController == null)
        {
            gameController = GameObject.Find("GameController").GetComponent<GameController>();
        }
        //------------------------------------------------------------
    }

    // Update is called once per frame
    void Update()
    {
        float z = Input.GetAxis("Horizontal");
        float x = -(Input.GetAxis("Vertical"));
        Debug.Log("z:" + z);
        Debug.Log("x:" + x);
        animator.SetFloat("inputX", x);
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
        UpdateMovement();

        //------------------------------------------------------------
        if (isTaking)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                animator.SetBool("isTaking", isTaking);
                Debug.Log("isTaking:" + isTaking);
                StartCoroutine(WaitforTaking(4.7f));
                // getItem = true;
                gameController.GetItem();
                Debug.Log("Item:" + gameController.getItem);
            }
        }
        //------------------------------------------------------------
    }//end of update

    IEnumerator WaitforTaking(float time)
    {
        yield return new WaitForSeconds(time);
        isTaking = false;
        animator.SetBool("isTaking", isTaking);
        Debug.Log("isTaking:" + isTaking);
    }
    void UpdateMovement()
    {
        Vector3 motion = inputVector;
        motion = ((Mathf.Abs(motion.x) > 1) || (Mathf.Abs(motion.z) > 1)) ? motion.normalized : motion;
        RotationMovement();
        ViewRelativeMovement();
    }// end of updateMovement
    void RotationMovement()
    {
        if (inputVector != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(targetDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
        }
    }//end of RotationMovement
    void ViewRelativeMovement()
    {
        // คำนวณการเคลื่อนที่ตามมุมมองของกล้อง
        Transform cameraTransform = Camera.main.transform;
        Vector3 forward = cameraTransform.TransformDirection(Vector3.forward);
        forward.y = 0.0f;
        forward = forward.normalized;
        Vector3 right = new Vector3(forward.z, 0.0f, -forward.x);
        targetDirection = (Input.GetAxis("Horizontal") * right) + (Input.GetAxis("Vertical") * forward);
    }

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.tag == "Ground")
        {
            isGrounded = true;
        }
    }
    //------------------------------------------------------------
    void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "item")
        {
            isTaking = true;
            Debug.Log("isTaking:" + isTaking);
        }
    }
    //------------------------------------------------------------
    void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "item")
        {
            isTaking = false;
            Debug.Log("isTaking:" + isTaking);
        }
    }
    //------------------------------------------------------------
}