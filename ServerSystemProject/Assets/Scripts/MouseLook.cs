using Mirror;
using UnityEngine;

public class MouseLook : NetworkBehaviour
{
    [SerializeField] private float mouseSensitivity = 200f;

    // playerBody — корень игрока (то, что поворачиваем по Y)
    [SerializeField] private Transform playerBody;

    // То, что наклоняем по X (обычно Pivot/CameraRoot, на котором висит этот скрипт)
    private float xRotation = 0f;

    public override void OnStartLocalPlayer()
    {
        // Лочим курсор только у локального игрока
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        enabled = true;
    }

    public override void OnStartClient()
    {
        // На не-локальных отключаем
        if (!isLocalPlayer)
            enabled = false;
    }

    private void Update()
    {
        if (!isLocalPlayer) return;

        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        // Наклон вверх/вниз (X) — на этом объекте (обычно CameraPivot)
        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);

        // Поворот игрока влево/вправо (Y) — на playerBody
        if (playerBody != null)
            playerBody.Rotate(Vector3.up * mouseX);
    }
}
