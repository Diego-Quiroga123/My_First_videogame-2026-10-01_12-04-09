using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float rotationSpeed = 10f;

    [Header("Salto y gravedad")]
    [SerializeField] private float jumpHeight = 1.5f;
    [SerializeField] private float gravity = -20f;

    private CharacterController controller;
    private Health health; // opcional: si existe, el jugador muerto no se mueve
    private float verticalVelocity;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        health = GetComponent<Health>();
    }

    private void Update()
    {
        bool isDead = health != null && health.IsDead;

        Vector3 horizontalMove = isDead ? Vector3.zero : ReadHorizontalMove();

        // Mantiene al jugador pegado al suelo
        if (controller.isGrounded && verticalVelocity < 0f)
        {
            verticalVelocity = -2f;
        }

        // Salto con la barra espaciadora
        if (!isDead && controller.isGrounded && Input.GetButtonDown("Jump"))
        {
            verticalVelocity = Mathf.Sqrt(jumpHeight * -2f * gravity);
        }

        verticalVelocity += gravity * Time.deltaTime;

        // UN solo Move por frame: así isGrounded es fiable
        // (con dos llamadas, la de movimiento horizontal pisaba el estado del suelo)
        Vector3 motion = horizontalMove * speed + Vector3.up * verticalVelocity;
        controller.Move(motion * Time.deltaTime);
    }

    // Devuelve la dirección de movimiento y gira al jugador hacia ella
    private Vector3 ReadHorizontalMove()
    {
        float horizontal = Input.GetAxisRaw("Horizontal"); // A / D o flechas
        float vertical = Input.GetAxisRaw("Vertical");     // W / S o flechas

        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (direction.magnitude < 0.1f) return Vector3.zero;

        // Gira suavemente hacia donde se mueve
        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);

        return direction;
    }
}
