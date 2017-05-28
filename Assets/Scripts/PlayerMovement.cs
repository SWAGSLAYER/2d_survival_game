using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

// PlayerScript requires the GameObject to have a Rigidbody component

[RequireComponent(typeof(Rigidbody2D))]

public class PlayerMovement : NetworkBehaviour
{

    GameObject server;
    public GameObject bulletPrefab;
    public GameObject tilePrefab;
    public Transform bulletSpawn;
    public float playerSpeed = 2f;

    Direction currentDir;
    bool moving = false;

    public Sprite northSprite;
    public Sprite eastSprite;
    public Sprite southSprite;
    public Sprite westSprite;
    void Start()
    {
        if (isLocalPlayer)
        {
            var x = Random.Range(50, 200);
            var y = Random.Range(50, 200);
            transform.position = new Vector3(x, y, -1);
        }
    }

    void Update()
    {
        if (!isLocalPlayer)
            return;

        //movement input
        moving = true;
        if (Input.GetKey(KeyCode.A))
            currentDir = Direction.West;
        else if (Input.GetKey(KeyCode.D))
            currentDir = Direction.East;
        else if (Input.GetKey(KeyCode.S))
            currentDir = Direction.South;
        else if (Input.GetKey(KeyCode.W))
            currentDir = Direction.North;
        else
            moving = false;

        CmdChangeAnim(currentDir,moving);

        //firing
        if (Input.GetKeyDown(KeyCode.Mouse0))
            CmdFire(currentDir); 
    }

    void FixedUpdate()
    {
        if (!isLocalPlayer)
            return;

        Vector2 targetVelocity = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
        GetComponent<Rigidbody2D>().velocity = targetVelocity * playerSpeed;
    }
    

    [ClientRpc]
    void RpcChangeAnim(Direction currentDir,bool moving)
    {
        switch (currentDir)
        {
            case Direction.West:
                if (!moving)
                    GetComponent<SpriteRenderer>().sprite = westSprite;
                else
                    GetComponent<Animator>().Play("playerLeftWalk", 0);
                break;
            case Direction.East:
                if (!moving)
                    GetComponent<SpriteRenderer>().sprite = eastSprite;
                else
                    GetComponent<Animator>().Play("playerRightWalk", 0);
                break;
            case Direction.South:
                if (!moving)
                    GetComponent<SpriteRenderer>().sprite = southSprite;
                else
                    GetComponent<Animator>().Play("playerDownWalk", 0);
                break;
            case Direction.North:
                if (!moving)
                    GetComponent<SpriteRenderer>().sprite = northSprite;
                else
                    GetComponent<Animator>().Play("playerUpWalk", 0);
                break;
        }
            


        Debug.Log(gameObject.GetComponent<Animator>().GetInteger("direction"));
    }
    [Command]
    void CmdChangeAnim(Direction currentDir,bool moving)
    {
        RpcChangeAnim(currentDir,moving);
    }
   
    [ClientRpc]
    void RpcFire(Direction currentDir)
    {
        var bullet = Instantiate(bulletPrefab, bulletSpawn.position, bulletSpawn.rotation) as GameObject;
        int bulletSpeed = 12;
        switch (currentDir)
        {
            case Direction.West:
                bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.right * -bulletSpeed;
                break;
            case Direction.East:
                bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.right * bulletSpeed;
                break;
            case Direction.South:
                bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * -bulletSpeed;
                break;
            case Direction.North:
                bullet.GetComponent<Rigidbody2D>().velocity = bullet.transform.up * bulletSpeed;
                break;
        }
        Destroy(bullet, 2.0f);
    }
    [Command]
    void CmdFire(Direction currentDir)
    {
        RpcFire(currentDir);
    }
    enum Direction
    {
        North,
        East,
        South,
        West
    }
}



