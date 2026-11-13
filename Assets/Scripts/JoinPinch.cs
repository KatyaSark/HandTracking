using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Hands;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Readers;

/// <summary>
/// Detects a thumb + index pinch from XR Hands tracking data and uses it
/// as the "select" (grab) input for a Near-Far Interactor.
/// Attach one to the Left Hand and one to the Right Hand.
/// </summary>
public class JoinPinch : MonoBehaviour
{
    [Tooltip("The Near-Far Interactor on this hand")]
    [SerializeField] NearFarInteractor interactor;

    [Tooltip("Which hand this script reads")]
    [SerializeField] Handedness handedness = Handedness.Right;

    [Tooltip("Fingertip distance (meters) that starts a pinch")]
    [SerializeField] float pinchStartDistance = 0.02f;

    [Tooltip("Fingertip distance (meters) that ends a pinch (a bit larger to avoid flicker)")]
    [SerializeField] float pinchEndDistance = 0.035f;

    [Header("Debug (read-only, watch these in Play mode)")]
    [SerializeField] bool handTracked;
    [SerializeField] float fingertipDistance;
    [SerializeField] bool isPinching;
    [SerializeField] bool isHoveringSomething;

    static readonly List<XRHandSubsystem> s_Subsystems = new List<XRHandSubsystem>();

    XRHandSubsystem m_Subsystem;
    bool m_Pinching;

    void OnEnable()
    {
        if (interactor != null)
            interactor.selectInput.inputSourceMode = XRInputButtonReader.InputSourceMode.ManualValue;
    }

    void OnDisable()
    {
        if (interactor != null)
            SetPinch(false);
    }

    void Update()
    {
        if (interactor == null)
            return;

        if (m_Subsystem == null || !m_Subsystem.running)
        {
            m_Subsystem = FindRunningHandSubsystem();
            if (m_Subsystem == null)
            {
                SetPinch(false);
                return;
            }
        }

        isHoveringSomething = interactor.hasHover;

        XRHand hand = handedness == Handedness.Left ? m_Subsystem.leftHand : m_Subsystem.rightHand;
        handTracked = hand.isTracked;
        if (!hand.isTracked)
        {
            SetPinch(false);
            return;
        }

        if (hand.GetJoint(XRHandJointID.ThumbTip).TryGetPose(out Pose thumb) &&
            hand.GetJoint(XRHandJointID.IndexTip).TryGetPose(out Pose index))
        {
            float distance = Vector3.Distance(thumb.position, index.position);
            fingertipDistance = distance;
            bool pinching = m_Pinching ? distance < pinchEndDistance : distance < pinchStartDistance;
            SetPinch(pinching);
        }
    }

    void SetPinch(bool pinching)
    {
        if (pinching != m_Pinching)
            Debug.Log($"[JoinPinch] {handedness} hand pinch {(pinching ? "STARTED" : "ended")}");

        m_Pinching = pinching;
        isPinching = pinching;
        interactor.selectInput.manualPerformed = pinching;
        interactor.selectInput.manualValue = pinching ? 1f : 0f;
    }

    static XRHandSubsystem FindRunningHandSubsystem()
    {
        SubsystemManager.GetSubsystems(s_Subsystems);
        foreach (XRHandSubsystem subsystem in s_Subsystems)
        {
            if (subsystem.running)
                return subsystem;
        }
        return null;
    }
}