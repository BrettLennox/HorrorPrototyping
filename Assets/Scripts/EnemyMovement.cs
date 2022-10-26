using System.Collections;
using System.Collections.Generic;
using System;
using System.Numerics;
using UnityEngine;
using UnityEngine.AI;
using Vector3 = UnityEngine.Vector3;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class EnemyMovement : MonoBehaviour
{
    private Animator _animator;
    private NavMeshAgent _agent;
    [SerializeField] private Transform _movePosition;
    [SerializeField] private Vector3[] _pathCorners;
    [SerializeField] private NavMeshPath _path;
    [SerializeField] private float pathUpdateTimer = 1f;
    private float timer = Mathf.Infinity;
    [SerializeField] private int _pathPointsCount;
    [SerializeField] private int index;
    [SerializeField] private float stepTime = 10f;
    [SerializeField] private float angle = 0.9f;
    [SerializeField] private float yMove = 0.5f;
    [SerializeField] private float xMove = 1f;
    [SerializeField] private float dampTime = 1f;
    [SerializeField] private float _moveOffset = 0.5f;

    private bool shouldMove = true;
    private float turnDir;
    private bool checkedTurn = false;
    // Start is called before the first frame update
    void Start()
    {
        _animator = GetComponent<Animator>();
        _agent = GetComponent<NavMeshAgent>();
        _path = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= pathUpdateTimer)
        {
            timer = 0f;
            NavMesh.CalculatePath(transform.position, _movePosition.position, NavMesh.AllAreas, _path);
            _pathPointsCount = _path.GetCornersNonAlloc(_pathCorners);

            while (_pathCorners.Length <= _pathPointsCount)
            {
                Array.Resize(ref _pathCorners, _pathCorners.Length * 2);
                _pathPointsCount = _path.GetCornersNonAlloc(_pathCorners);
            }

            for (int i = 0; i < _path.corners.Length - 1; i++)
            {
                if (i < _path.corners.Length)
                {
                    Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.red, 1f);
                }
                else { break; }
            }
        }

        if (shouldMove)
        {
            if (Vector3.Distance(transform.position, _movePosition.position) < _moveOffset)
            {
                shouldMove = false;
            }
            Vector3 dirFromAToB = (_pathCorners[index] - transform.position).normalized;
            float dotProd = Vector3.Dot(dirFromAToB, transform.forward);
            if (Vector3.Distance(transform.position, _pathCorners[index]) < _moveOffset)
            {
                index++;
                if (index == _path.corners.Length)
                {
                    shouldMove = false;
                }
            }

            if (dotProd > angle)
            {
                _animator.SetFloat("y", yMove, dampTime, Time.deltaTime);
                _animator.SetFloat("x", 0f, dampTime, Time.deltaTime);
                checkedTurn = false;
            }
            else
            {
                if (!checkedTurn)
                {
                    Vector3 from = _pathCorners[index] - transform.position;
                    Vector3 to = transform.up;
                    float turnAngle = Vector3.SignedAngle(from, to, transform.forward);
                    if (turnAngle == 90f)
                    {
                        turnDir = xMove;
                    }
                    else if (turnAngle == -90f)
                    {
                        turnDir = -xMove;
                    }
                    checkedTurn = true;
                }
                _animator.SetFloat("y", 0f, dampTime, Time.deltaTime);
                _animator.SetFloat("x", turnDir, dampTime, Time.deltaTime);
            }
        }
        else if (!shouldMove && Vector3.Distance(transform.position, _movePosition.position) > 0.5f)
        {
            shouldMove = true;
        }
        else
        {
            _animator.SetFloat("y", Mathf.SmoothStep(_animator.GetFloat("y"), 0f, stepTime));
            _animator.SetFloat("x", Mathf.SmoothStep(_animator.GetFloat("x"), 0f, stepTime));
        }
    }
}
