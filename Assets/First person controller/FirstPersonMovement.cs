using UnityEngine;
using UnityEngine.UI;

public class FirstPersonMovement : MonoBehaviour
{
    public float speed = 5;
    public World world;
    Vector2 velocity;
    public float reach = 5;
    public int selectedBlockIndex = 1;
    public GameObject selectedBlockText;


    private void Start()
    {
        selectedBlockIndex = 1;
        selectedBlockText.GetComponent<Text>().text = world.blocktypes[selectedBlockIndex].blockName;
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update()
    {
        velocity.y = Input.GetAxis("Vertical") * speed * Time.deltaTime;
        velocity.x = Input.GetAxis("Horizontal") * speed * Time.deltaTime;
        transform.Translate(velocity.x, 0, velocity.y);

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if(scroll != 0)
        {
            if(scroll > 0)
            {
                selectedBlockIndex++;
            }
            else
            {
                selectedBlockIndex--;
            }

            if(selectedBlockIndex > world.blocktypes.Length - 1)
            {
                selectedBlockIndex = 1;
            }

            if(selectedBlockIndex < 1)
            {
                selectedBlockIndex = world.blocktypes.Length - 1;
            }

            selectedBlockText.GetComponent<Text>().text = world.blocktypes[selectedBlockIndex].blockName;
        }

        
        if (Input.GetButtonDown("Fire1"))
        {
            CastRay(0);
        }

        if (Input.GetButtonDown("Fire2"))
        {
            CastRay(selectedBlockIndex);
        }
    }

    void CastRay(int type)
    {
        
        Vector3 position;
        Vector3 roundedPosition;
        RaycastHit hit;

        position = Camera.main.transform.position + (Camera.main.transform.forward * reach);
        

        if(Physics.Linecast(Camera.main.transform.position, position, out hit))
        {
            
            if (hit.transform.GetComponent<MeshRenderer>())
            {

                if (type == 0)
                {
                    float yPos = hit.point.y + GetYCoord(hit.normal);
                    float xPos = hit.point.x + GetXCoord(hit.normal);
                    float zPos = hit.point.z + GetZCoord(hit.normal);
                    roundedPosition = new Vector3(Mathf.FloorToInt(xPos), Mathf.FloorToInt(yPos), Mathf.FloorToInt(zPos));
                    if (world.CheckForBlock(roundedPosition))
                    {
                        world.GetChunkVector3(roundedPosition).EditVoxel(roundedPosition, 0);
                    }
                }
                else
                {
                    float yPos = hit.point.y + GetYNorm(hit.normal);
                    float xPos = hit.point.x + GetXNorm(hit.normal);
                    float zPos = hit.point.z + GetZNorm(hit.normal);

                    roundedPosition = new Vector3(Mathf.FloorToInt(xPos), Mathf.FloorToInt(yPos), Mathf.FloorToInt(zPos));

                    if (!world.CheckForBlock(roundedPosition))
                    {
                        world.GetChunkVector3(roundedPosition).EditVoxel(roundedPosition, selectedBlockIndex);
                    }
                }
            }

        }




    }

    int GetYNorm(Vector3 norm)
    {
        int y = 0;
        if(norm.y < 0)
        {
            y -= 1;
        }
        return y;
    }

    int GetXNorm(Vector3 norm)
    {
        int x = 0;
        if (norm.x < 0)
        {
            x -= 1;
        }
        return x;
    }

    int GetZNorm(Vector3 norm)
    {
        int z = 0;
        if (norm.z < 0)
        {
            z -= 1;
        }
        return z;
    }


    int GetYCoord(Vector3 norm)
    {
        int y = 0;
        if(norm.y > 0)
        {
            y = -1;
        }
        return y;
    }

    int GetZCoord(Vector3 norm)
    {
        int z = 0;
        if(norm.z > 0)
        {
            z -= 1;
        }
        return z;
    }

    int GetXCoord(Vector3 norm)
    {
        int x = 0;
        if (norm.x > 0)
        {
            x -= 1;
        }
        return x;
    }


}


