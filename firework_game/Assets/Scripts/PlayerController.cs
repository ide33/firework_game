using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Movement Setting")]
    [SerializeField] private float moveSpeed = 3f;

    [Header("Mouse Setting")]
    [SerializeField] private float sensitivityX = 2.5f;
    [SerializeField] private float sensitivityY = 2.5f;
    [SerializeField] private float minVerticalAngle = -90f;
    [SerializeField] private float maxVerticalAngle = 90f;

    [Header("Camera Setting")]
    [SerializeField] private Camera playerCamera;

    private float verticalRotation = 0f;  // カメラの上下角度
    private float movex, movez;

    private void Start()
    {
        // カーソルを非表示
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        HandleMouseLock();
    }

    // 物理演算のタイミングで呼ばれる
    private void FixedUpdate()
    {
        HandleMovement();
    }

    private void HandleMouseLock()
    {
        float mousex = Input.GetAxis("Mouse X") * sensitivityX;
        float mousey = Input.GetAxis("Mouse Y") * sensitivityY;

        // プレイヤーの回転(横方向)
        transform.Rotate(Vector3.up * mousex);

        // カメラの回転(縦方向)
        verticalRotation -= mousey;
        verticalRotation = Mathf.Clamp(verticalRotation, minVerticalAngle, maxVerticalAngle);

        playerCamera.transform.localRotation = Quaternion.Euler(verticalRotation, 0, 0);
    }

    private void HandleMovement()
    {
        movex = Input.GetAxisRaw("Horizontal");
        movez = Input.GetAxisRaw("Vertical");

        Vector3 moveDirection = transform.right * movex + transform.forward * movez;
        moveDirection.Normalize();

        transform.position += moveDirection * moveSpeed * Time.fixedDeltaTime;
    }
}
