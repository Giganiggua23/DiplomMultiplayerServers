using UnityEngine;
using Mirror;

public class LocalPlayerCameraSetup : NetworkBehaviour
{
    [SerializeField] private Camera playerCamera;        // камера внутри префаба
    [SerializeField] private AudioListener audioListener; // если есть
    [SerializeField] private Behaviour[] localOnly;       // MouseLook, UI input, etc.

    public override void OnStartClient()
    {
        // По умолчанию: выключаем всё на ВСЕХ, потом локальный включит себе
        if (playerCamera) playerCamera.enabled = false;
        if (audioListener) audioListener.enabled = false;

        if (localOnly != null)
            foreach (var b in localOnly)
                if (b) b.enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        if (playerCamera) playerCamera.enabled = true;
        if (audioListener) audioListener.enabled = true;

        if (localOnly != null)
            foreach (var b in localOnly)
                if (b) b.enabled = true;

        // Сделай главной только локальную камеру
        if (playerCamera) playerCamera.tag = "MainCamera";

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}
