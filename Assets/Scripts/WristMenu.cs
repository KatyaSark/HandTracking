using System.Collections.Generic;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.XR.Hands;

/// <summary>
/// Simple hand menu: follows one hand's palm and only shows the menu
/// when that palm is turned toward your face.
/// Put this on an empty GameObject and make your menu canvas a child of it.
/// </summary>
public class WristMenu : MonoBehaviour
{
    [Header("Menu")]
    [Tooltip("The object to show/hide (your menu canvas, a child of this object)")]
    [SerializeField] GameObject menuRoot;

    [Tooltip("Which hand the menu sits on")]
    [SerializeField] Handedness handedness = Handedness.Left;

    [Header("Placement (meters)")]
    [Tooltip("How far the menu floats out from the palm")]
    [SerializeField] float distanceFromPalm = 0.08f;

    [Tooltip("Extra height above the palm")]
    [SerializeField] float heightAbovePalm = 0.05f;

    [Tooltip("How quickly the menu catches up to the hand (higher = snappier)")]
    [SerializeField] float followSpeed = 15f;

    [Header("When to show")]
    [Tooltip("How directly the palm must face you (0 = loosely, 1 = exactly)")]
    [Range(0f, 1f)]
    [SerializeField] float palmFacingThreshold = 0.6f;

    [Tooltip("Tick this if the menu shows when your palm faces AWAY instead of toward you")]
    [SerializeField] bool invertPalmDirection;

    [Header("Debug (read-only)")]
    [SerializeField] bool handTracked;
    [SerializeField] bool palmFacingYou;

    static readonly List<XRHandSubsystem> s_Subsystems = new List<XRHandSubsystem>();

    XRHandSubsystem m_Subsystem;
    Transform m_TrackingSpace;
    Transform m_Head;
    bool m_WasShowing;

    void Start()
    {
        XROrigin origin = FindFirstObjectByType<XROrigin>();
        if (origin != null)
            m_TrackingSpace = origin.CameraFloorOffsetObject.transform;

        if (Camera.main != null)
            m_Head = Camera.main.transform;

        SetVisible(false);
    }

    void LateUpdate()
    {
        if (menuRoot == null || m_TrackingSpace == null || m_Head == null)
            return;

        if (m_Subsystem == null || !m_Subsystem.running)
        {
            m_Subsystem = FindRunningHandSubsystem();
            if (m_Subsystem == null)
            {
                SetVisible(false);
                return;
            }
        }

        XRHand hand = handedness == Handedness.Left ? m_Subsystem.leftHand : m_Subsystem.rightHand;
        handTracked = hand.isTracked;

        if (!hand.isTracked || !hand.GetJoint(XRHandJointID.Palm).TryGetPose(out Pose palmPose))
        {
            palmFacingYou = false;
            SetVisible(false);
            return;
        }

        // Hand joint poses are relative to the XR Origin's tracking space; convert to world space.
        Vector3 palmPosition = m_TrackingSpace.TransformPoint(palmPose.position);
        Quaternion palmRotation = m_TrackingSpace.rotation * palmPose.rotation;

        // The palm's "up" points out of the back of the hand, so the palm faces "down".
        Vector3 palmNormal = palmRotation * (invertPalmDirection ? Vector3.up : Vector3.down);
        Vector3 toHead = (m_Head.position - palmPosition).normalized;
        palmFacingYou = Vector3.Dot(palmNormal, toHead) > palmFacingThreshold;

        SetVisible(palmFacingYou);
        if (!palmFacingYou)
            return;

        // Target: float slightly out from the palm and a bit above it, turned to face your head.
        Vector3 targetPosition = palmPosition + palmNormal * distanceFromPalm + Vector3.up * heightAbovePalm;
        Quaternion targetRotation = Quaternion.LookRotation(targetPosition - m_Head.position, Vector3.up);

        if (!m_WasShowing)
        {
            // Snap into place the moment it appears, then follow smoothly.
            transform.SetPositionAndRotation(targetPosition, targetRotation);
        }
        else
        {
            float t = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);
            transform.SetPositionAndRotation(
                Vector3.Lerp(transform.position, targetPosition, t),
                Quaternion.Slerp(transform.rotation, targetRotation, t));
        }

        m_WasShowing = true;
    }

    void SetVisible(bool visible)
    {
        if (!visible)
            m_WasShowing = false;

        if (menuRoot != null && menuRoot.activeSelf != visible)
            menuRoot.SetActive(visible);
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