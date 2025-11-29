using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Vector2 inputVec2;

    public void OnMove(InputValue value)
    {
        inputVec2 = value.Get<Vector2>();
        Debug.Log(inputVec2);
    }

    public void OnJump(InputValue value)
    {
        Debug.Log("Jump!");
    }

    public void OnUseSkill(InputValue value)
    {
        Debug.Log("Skill!");
    }

    void Update()
    {
        Vector3 move = new Vector3(inputVec2.x, 0f, inputVec2.y);
        transform.Translate(move * 5f * Time.deltaTime);
    }
}
