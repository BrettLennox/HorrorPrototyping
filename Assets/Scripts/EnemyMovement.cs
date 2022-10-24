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
    [SerializeField] private int _pathPointsCount;
    [SerializeField] private int index;
    bool isFacing;
    [SerializeField] private float stepTime = 10f;
    [SerializeField] private float angle = 0.9f;
    [SerializeField] private float yMove = 0.5f;
    [SerializeField] private float xMove = 1f;
    [SerializeField] private float dampTime = 1f;

    private bool shouldMove = true;
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
        NavMeshPath path = new NavMeshPath();
        if (NavMesh.CalculatePath(transform.position, _movePosition.transform.position, NavMesh.AllAreas, _path))
        {
            _agent.SetPath(_path);
            _pathPointsCount = _path.GetCornersNonAlloc(_pathCorners);
            _path.GetCornersNonAlloc(_pathCorners);
//
            while (_pathCorners.Length <= _pathPointsCount)
            {
                Array.Resize(ref _pathCorners, _pathCorners.Length * 2);
                _pathPointsCount = _path.GetCornersNonAlloc(_pathCorners);
            }
        }

        if (shouldMove)
        {
            if (Vector3.Distance(transform.position, _movePosition.position) < 0.5f)
            {
                shouldMove = false;
            }
            Vector3 dirFromAToB = (_pathCorners[index] - transform.position).normalized;
            float dotProd = Vector3.Dot(dirFromAToB, transform.forward);
            if (Vector3.Distance(transform.position, _pathCorners[index]) < 0.5f)
            {
                //isFacing = false;
                index++;
                if (index == path.corners.Length)
                {
                    shouldMove = false;
                }
            }

            if (dotProd > angle)
            {
                isFacing = true;
            }
            else
            {
                isFacing = false;
            }

            if (isFacing)
            {
                _animator.SetFloat("y", yMove, dampTime,Time.deltaTime);
                _animator.SetFloat("x", 0f, dampTime, Time.deltaTime);
            }
            else
            {
                _animator.SetFloat("y", 0f, dampTime, Time.deltaTime);
                _animator.SetFloat("x", xMove, dampTime, Time.deltaTime);
            }

            //if (isFacing)
            //{
            //    _animator.SetFloat("y", Mathf.SmoothStep(_animator.GetFloat("y"), 1f, stepTime));
            //    _animator.SetFloat("x", Mathf.SmoothStep(_animator.GetFloat("x"),0f, stepTime));
            //    if (dotProd < 0.5f)
            //    {
            //        isFacing = false;
            //    }
            //}
            //else
            //{
            //    _animator.SetFloat("y", Mathf.SmoothStep(_animator.GetFloat("y"),0.5f, 2f));
            //    _animator.SetFloat("x", Mathf.SmoothStep(_animator.GetFloat("x"),1f,stepTime));
            //    if (dotProd > angle)
            //    {
            //        _animator.SetFloat("x", Mathf.SmoothStep(_animator.GetFloat("x"),0f, stepTime));
            //        isFacing = true;
            //    }
            //    else
            //    {
            //        _animator.SetFloat("x", Mathf.SmoothStep(_animator.GetFloat("x"),xMove, stepTime));
            //    }
            //}
        }
        else if(!shouldMove && Vector3.Distance(transform.position, _movePosition.position) > 0.5f)
        {
            shouldMove = true;
        }
        else
        {
            _animator.SetFloat("y", Mathf.SmoothStep(_animator.GetFloat("y"),0f, stepTime));
            _animator.SetFloat("x", Mathf.SmoothStep(_animator.GetFloat("x"),0f, stepTime));
        }
    }
}
