
using UnityEngine;

public class PinchCube : MonoBehaviour
{
    public Transform handAnchor;
    public GameObject cube;

    private bool isHolding = false;

    public void PickUpCube()
    {
        Debug.Log("PINCH DETECTED - PickUpCube called!");

        if (cube == null || handAnchor == null)
        {
            Debug.LogWarning("Cube or Hand Anchor is missing!");
            return;
        }

        isHolding = true;
        cube.transform.SetParent(null);
        Debug.Log("Cube pickup started!");
    }

    public void ReleaseCube()
    {
        Debug.Log("PINCH RELEASED");

        isHolding = false;
    }

    void LateUpdate()
    {
        if (isHolding && cube != null && handAnchor != null)
        {
            cube.transform.position = handAnchor.position;
        }
    }
}
