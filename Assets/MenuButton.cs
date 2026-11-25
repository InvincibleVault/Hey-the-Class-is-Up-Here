using UnityEngine;
using UnityEngine.InputSystem;

public class MenuToggle : MonoBehaviour
{
    [SerializeField] private InputActionProperty menuAction; // bind to Menu button
    [SerializeField] private GameObject menu;                // your menu canvas
    [SerializeField] private Transform head;                 // Main Camera in XR Origin
    [SerializeField] private float distance = 1.5f;

    private void OnEnable()
    {
        menuAction.action.Enable();
        menuAction.action.performed += OnMenuPressed;
    }

    private void OnDisable()
    {
        menuAction.action.performed -= OnMenuPressed;
    }

    private void OnMenuPressed(InputAction.CallbackContext ctx)
    {
        bool show = !menu.activeSelf;
        menu.SetActive(show);

        if (show) PlaceInFrontOfPlayer();
    }

    private void PlaceInFrontOfPlayer()
    {
        Vector3 forward = head.forward;
        forward.y = 0f;                       // ignore looking up/down
        forward.Normalize();

        menu.transform.position = head.position + forward * distance;
        menu.transform.rotation = Quaternion.LookRotation(forward);
    }
}