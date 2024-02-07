using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody _playerRigidbody;
    [SerializeField]
    private float _speed;

    private void Awake()
    {
        _playerRigidbody = GetComponent<Rigidbody>();
    }

    private void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveZ = Input.GetAxisRaw("Vertical");

        _playerRigidbody.MovePosition(transform.position + _speed * Time.deltaTime * new Vector3(moveX, 0, moveZ));
    }
}
