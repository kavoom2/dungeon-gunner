using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovementToPosition : MonoBehaviour
{
    private Rigidbody2D rigidBody2D;
    private MovementToPositionEvent movementToPositionEvent;

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
    }

    private void OnEnable()
    {
        movementToPositionEvent.OnMovementToPosition += MovementToPosition_OnMovementToPosition;
    }

    private void OnDisable()
    {
        movementToPositionEvent.OnMovementToPosition -= MovementToPosition_OnMovementToPosition;
    }

    private void MovementToPosition_OnMovementToPosition(
        MovementToPositionEvent movementToPositionEvent,
        MovementToPositionArgs movementToPositionArgs
    )
    {
        MoveRigidBody(
            movementToPositionArgs.movePosition,
            movementToPositionArgs.currentPosition,
            movementToPositionArgs.moveSpeed
        );
    }

    /// <summary>
    /// RigidBody를 지정된 위치를 향해 이동시킵니다.
    /// </summary>
    private void MoveRigidBody(Vector3 movePosition, Vector3 currentPosition, float moveSpeed)
    {
        Vector2 unitVector = (movePosition - currentPosition).normalized;
        rigidBody2D.MovePosition(
            rigidBody2D.position + (unitVector * moveSpeed * Time.fixedDeltaTime)
        );
    }
}
