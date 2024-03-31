using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Unity.VisualScripting.StickyNote;

public class InputManager : MonoSingleton<InputManager>
{
    float kTimeBetweenInputSamples = 0.03f;

    InputState inputState = new InputState();

    MoveList mMoveList = new MoveList();

    Move mPendingMove;

    float mNextTimeToSampleInput;

    byte mColor;

    private float SwitchColorCountDown;
    private bool SwitchOnlyOnce;
    
    public Transform MainPlayer { get; set; }

    void Start()
    {
        //Initialize();
    }

    void Update()
    {
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        float worldX, worldZ;

        if (direction.magnitude >= 0.1f && MainPlayer != null)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            Quaternion rotation = Quaternion.Euler(0f, targetAngle, 0f);
            
            Vector3 fromCamera = MainPlayer.transform.position - Camera.main.transform.position;
            Vector3 cameraForward = new Vector3(fromCamera.x, 0, fromCamera.z).normalized;
            direction = rotation * cameraForward;
            worldX = direction.x;
            worldZ = direction.z;
        }
        else
        {
            worldX = 0;
            worldZ = 0;
        }

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        float rotationY = 0;
        LayerMask mask = LayerMask.GetMask("Map");
        if (Physics.Raycast(ray, out hit, 1000, mask) && MainPlayer != null)
        {
            Vector3 fromMouse = hit.point - MainPlayer.position;
            Vector3 mouseForward = new Vector3(fromMouse.x, 0, fromMouse.z).normalized;
            float angle = Vector3.SignedAngle(Vector3.forward, mouseForward, Vector3.up);
            rotationY = angle;
        }

        bool isShooting = false;
        if (Input.GetMouseButton(0))
        {
            isShooting = true;
        }

        bool isSwitchColor = false;
        if(Input.GetKey(KeyCode.Tab))
        {
            isSwitchColor = true;
        }

        SwitchColorCountDown -= Time.deltaTime;

        if (IsTimeToSampleInput())
        {
            if (SwitchColorCountDown > 0) { isSwitchColor = false; }
            inputState.UpdateInputValue(worldX, worldZ, rotationY, isShooting, isSwitchColor, mColor);

            mPendingMove = SampleInputAsMove();
            if (!SwitchOnlyOnce && isSwitchColor)
            {
                SwitchColorCountDown = 0.3f;
            }
            
            SwitchOnlyOnce = true;
        }
        else
        {
            SwitchOnlyOnce = false;
        }

        if (Input.GetKeyDown(KeyCode.Equals))
        {
            float latency = (NetworkClient.Instance as NetworkClient).GetSimulatedLatency();
            latency += 0.1f;
            if (latency > 0.5f)
            {
                latency = 0.5f;
            }
            (NetworkClient.Instance as NetworkClient).SetSimulatedLatency(latency);
        }
        else if(Input.GetKeyDown(KeyCode.Minus))
        {
            float latency = (NetworkClient.Instance as NetworkClient).GetSimulatedLatency();
            latency -= 0.1f;
            if (latency < 0.0f)
            {
                latency = 0.0f;
            }
            (NetworkClient.Instance as NetworkClient).SetSimulatedLatency(latency);
        }
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            float dropChance = (NetworkClient.Instance as NetworkClient).GetDropPacketChance();
            dropChance += 1f;
            if (dropChance > 30f)
            {
                dropChance = 30f;
            }
            (NetworkClient.Instance as NetworkClient).SetDropPacketChance(dropChance);
        }
        else if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            float dropChance = (NetworkClient.Instance as NetworkClient).GetDropPacketChance();
            dropChance -= 1f;
            if (dropChance < 0.0f)
            {
                dropChance = 0.0f;
            }
            (NetworkClient.Instance as NetworkClient).SetDropPacketChance(dropChance);
        }
    }

    private void Initialize()
    {
        inputState = new InputState();
    }

    public InputState GetState() { return inputState; }

    public MoveList GetMoveList() { return mMoveList; }

    public Move GetAndClearPendingMove() { var toRet = mPendingMove; mPendingMove = null; return toRet; }

    private Move SampleInputAsMove()
    {
        return mMoveList.AddMove(new InputState(GetState()), Time.time);
    }

    private bool IsTimeToSampleInput()
    {
        float time = Time.time;
        if (time > mNextTimeToSampleInput)
        {
            mNextTimeToSampleInput = mNextTimeToSampleInput + kTimeBetweenInputSamples;
            return true;
        }

        return false;
    }

    public void ResetAndClear()
    {
        mPendingMove = null;
        mMoveList.Clear();
        mColor = 0;
        MainPlayer = null;
        SwitchColorCountDown = 0;
        SwitchOnlyOnce = true;
    }
}
