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
    [SerializeField] private Transform _movePosition;
    [SerializeField] private Vector3[] _pathCorners = new Vector3[10];
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
        _path = new NavMeshPath();
    }

    // Update is called once per frame
    void Update()
    {
        CalculatePath();
        CalculateAndUpdateMovement();
    }

    private void CalculateAndUpdateMovement()
    {
        if (shouldMove)
        {
            CalculateDistance();
            IncrementPathpointIndex();

            
            if (IsFacing()) //if dotProd is greater then angle
            {
                //sets the animator float values to move the AI forward and prevent turning 
                //damps the values to the new values over a period determined by dampTime variable 
                //runs over Time.deltaTime
                _animator.SetFloat("y", yMove, dampTime, Time.deltaTime);
                _animator.SetFloat("x", 0f, dampTime, Time.deltaTime);
                //sets checkedTurn to false to allow the AI to determine turn direction when dotProd is less than angle
                checkedTurn = false;
            }
            else
            {
                if (!checkedTurn) //if checkedTurn is false
                {
                    //determines angle from transform forward and current pathCorners target
                    //returns the value signed
                    Vector3 from = _pathCorners[index] - transform.position;
                    Vector3 to = transform.up;
                    float turnAngle = Vector3.SignedAngle(from, to, transform.forward);
                    if (turnAngle == 90f) //if turnAngle is 90f
                    {
                        turnDir = xMove; //turn right
                    }
                    else if (turnAngle == -90f) //if turn angle -90f
                    {
                        turnDir = -xMove; //turn left
                    }
                    checkedTurn = true;
                }
                //sets the animator float values to move the AI forward and prevent turning 
                //damps the values to the new values over a period determined by dampTime variable 
                //runs over Time.deltaTime
                _animator.SetFloat("y", 0f, dampTime, Time.deltaTime);
                _animator.SetFloat("x", turnDir, dampTime, Time.deltaTime);
            }
        }
        //checks if shouldMove is false && distance between transform position and movePosition is greater than 0.5f
        else if (!shouldMove && Vector3.Distance(transform.position, _movePosition.position) > 0.5f)
        {
            shouldMove = true; //sets shouldMove to false
        }
        else //if shouldMove is false
        {
            //sets the float values in animator to be 0 using Mathf.Smoothstep from the current value of the floats
            //run over the duration of stepTime
            _animator.SetFloat("y", Mathf.SmoothStep(_animator.GetFloat("y"), 0f, stepTime));
            _animator.SetFloat("x", Mathf.SmoothStep(_animator.GetFloat("x"), 0f, stepTime));
            if (IsFacing())
            {
                _animator.SetTrigger("attack");
            }
        }
    }

    private bool IsFacing()
    {
        Vector3 dirFromAToB = (_pathCorners[index] - transform.position).normalized; // gets the direction from A to B normalized
        float dotProd = Vector3.Dot(dirFromAToB, transform.forward); //dot product from dirFromAToB and the transform.forward //1 being looking at //-1 being looking away
        if(dotProd > angle)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void IncrementPathpointIndex()
    {
        if (Vector3.Distance(transform.position, _pathCorners[index]) < _moveOffset) //if the distance between this transform and the current pathCorner target is less than moveOffset
        {
            index++; //increment the index
            if (index == _path.corners.Length) //if the index is equal to the legnth of the paths corners
            {
                shouldMove = false; //shouldMove is set to false
            }
        }
    }

    private void CalculateDistance()
    {
        //checks the distance between transform position and movePosition
        //if value is less than moveOffset
        if (Vector3.Distance(transform.position, _movePosition.position) < _moveOffset)
        {
            shouldMove = false; //shouldMove is set to false
        }
    }

    private void CalculatePath()
    {
        //increments timer over Time.deltaTime
        timer += Time.deltaTime;
        if (timer >= pathUpdateTimer) //if timer is greater than or eual to pathUpdateTimer
        {
            //reset timer to 0
            timer = 0f;
            //Calculates the path from transform position to movePosition
            //using all NavMesh Areas
            //returns the path data back to path variable
            NavMesh.CalculatePath(transform.position, _movePosition.position, NavMesh.AllAreas, _path);
            //calculates the amount of corners in the path
            //adjusts pathPointsCount value to be the amount of corners
            //creates an array of corners Vector position and sets to pathCorners
            _pathPointsCount = _path.GetCornersNonAlloc(_pathCorners);

            while (_pathCorners.Length <= _pathPointsCount) //while pathCorners length is less than or equal to pathPointsCount
            {
                //Resizes the array to allow for wiggle room when adding new members to the array
                Array.Resize(ref _pathCorners, _pathCorners.Length * 2);
                //recalculates corners and adds to array
                _pathPointsCount = _path.GetCornersNonAlloc(_pathCorners);
            }

#if UNITY_EDITOR
            //whilst in UnityEditor
            //creates a Debug Drawline between all path corners
            for (int i = 0; i < _path.corners.Length - 1; i++)
            {
                if (i < _path.corners.Length)
                {
                    Debug.DrawLine(_path.corners[i], _path.corners[i + 1], Color.red, 1f);
                }
                else { break; }
            }
        }
#endif
    }
}
