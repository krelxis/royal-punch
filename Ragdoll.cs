using UnityEngine;
using System.Collections.Generic;
 
[RequireComponent(typeof(Animator))] 
[RequireComponent(typeof(PlayerController))]

public class Ragdoll : MonoBehaviour
{
    [SerializeField] private float blendSpeed;
    [SerializeField] private float ragdollTimeMax;
    private float ragdollTime;

    [SerializeField] private float force;
    [SerializeField] private Rigidbody forcePos;

    [SerializeField] private Transform rotatePos;
    private TargetObserver observer;

    private List<Rigidbody> rigids = new List<Rigidbody>();
    private int rigidsLendth;

    private List<Vector3> storedPositions = new List<Vector3>();
    private List<Quaternion> storedRotations = new List<Quaternion>();

    private State state = State.Animator;

    private Animator anim;
    private PlayerController controller;

    public void ActivateAnimator()
    {
        anim.enabled = true;
        controller.enabled = true;

        state = State.Animator;
    }

    public void ActivateRagdoll()
    {
        SaveBones();
        state = State.Ragdoll;

        for (int i = 0; i < rigidsLendth; i++)
        {
            rigids[i].velocity = new Vector3(0, 0, 0);
            rigids[i].isKinematic = false;
            rigids[i].GetComponent<Collider>().isTrigger = false;
        }

        anim.enabled = false;
        observer.enabled = false;
        controller.enabled = false;

        forcePos.AddForce(-rotatePos.forward * force);
    }

    public void DisableRagdoll()
    {
        for (int i = 0; i < rigidsLendth; i++)
        {
            rigids[i].isKinematic = true;
            rigids[i].GetComponent<Collider>().isTrigger = true;
        }
        observer.enabled = true;
    }

    private void SaveBones()
    {
        Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();

        List<Vector3> storedPosition = new List<Vector3>();
        List<Quaternion> storedRotation = new List<Quaternion>();

        foreach (Rigidbody rigid in rigidBodies)
        {
            storedPosition.Add(rigid.transform.localPosition);
            storedRotation.Add(rigid.transform.localRotation);
        }

        storedPositions = storedPosition;
        storedRotations = storedRotation;
    }

    private void Start()
    {
        anim = GetComponent<Animator>();
        controller = GetComponent<PlayerController>();
        observer = rotatePos.GetComponent<TargetObserver>();

        Rigidbody[] rigidBodies = GetComponentsInChildren<Rigidbody>();
        rigidsLendth = rigidBodies.Length - 1;

        foreach (Rigidbody rigid in rigidBodies)
        {
            rigids.Add(rigid);

            storedPositions.Add(rigid.transform.localPosition);
            storedRotations.Add(rigid.transform.localRotation);
        }

        DisableRagdoll();
        ActivateAnimator();
    }

    private void FixedUpdate()
    {
        if(state == State.Ragdoll)
        {
            ragdollTime++;

            if (ragdollTime >= ragdollTimeMax)
            {
                ragdollTime = 0;
                state = State.GetUp;
            }
        }

        if (state == State.GetUp)
        {
            for (int i = 0; i < rigidsLendth; i++)
            {
                DisableRagdoll();

                if (rigids[i].transform.localPosition != storedPositions[i])
                    rigids[i].transform.localPosition = Vector3.Lerp(rigids[i].transform.localPosition, storedPositions[i], blendSpeed);

                if (rigids[i].transform.localRotation != storedRotations[i])
                    rigids[i].transform.localRotation = Quaternion.Lerp(rigids[i].transform.localRotation, storedRotations[i], blendSpeed);

                if (rigids[i].transform.localRotation == storedRotations[i] && rigids[i].transform.localPosition == storedPositions[i])
                    ActivateAnimator();

            }
        }
    } 
    enum State
    {
        Ragdoll, 
        GetUp, 
        Animator
    }
}
