using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class PlayerCamera : NetworkBehaviour
{
    GameObject Camera;
    void Start () {
        if (isLocalPlayer)
        {
            Camera = Instantiate(Resources.Load("PlayerCamera"), this.transform.position, this.transform.rotation) as GameObject;
            GetComponent<Rigidbody2D>().freezeRotation = true;
        }
    }
	void FixedUpdate () {
        if (!isLocalPlayer)
            return;

        var d = Input.GetAxis("Mouse ScrollWheel");
        var size = Camera.GetComponent<Camera>().orthographicSize;
        if (d < 0f)
        {
            if (size < 1)
                Camera.GetComponent<Camera>().orthographicSize += 0.1f;
            else if (size >= 1 && size < 100)
                Camera.GetComponent<Camera>().orthographicSize += 1;
        }
        else if (d > 0f)
        {
            if (size > 1)
                Camera.GetComponent<Camera>().orthographicSize -= 1;
            else if (size > 0.1)
                Camera.GetComponent<Camera>().orthographicSize -= 0.1f;
        }

        Camera.GetComponent<Transform>().position = new Vector3(this.transform.position.x, this.transform.position.y, -5f);
    }
}
