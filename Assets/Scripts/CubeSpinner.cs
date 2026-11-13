using UnityEngine;

public class CubeSpinner : MonoBehaviour
{
    [SerializeField] float degreesPerSecond = 90f;
    bool spinning;

    public void StartSpinning() { spinning = true; }
    public void StopSpinning()  { spinning = false; }

    void Update()
    {
        if (spinning)
            transform.Rotate(Vector3.up, degreesPerSecond * Time.deltaTime, Space.World);
    }
}