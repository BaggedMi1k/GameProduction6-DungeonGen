using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;

public class PickUpScript : NetworkBehaviour
{
    public GameObject player;
    public Transform holdPos;

    public float throwForce = 500f; //force at which the object is thrown at
    public float pickUpRange = 5f; //how far the player can pickup the object from

    private float rotationSensitivity = 1f; //how fast/slow the object is rotated in relation to mouse movement

    private GameObject heldObj; //object which we pick up
    private Rigidbody heldObjRb; //rigidbody of object we pick up

    private bool canDrop = true; //this is needed so we don't throw/drop object when rotating the object
    private int LayerNumber; //layer index
    private PlayerBehaviour playerBehaviour;

    void Start()
    {
        LayerNumber = LayerMask.NameToLayer("holdLayer");
        playerBehaviour = player.GetComponent<PlayerBehaviour>();
        //mouseLookScript = player.GetComponent<MouseLookScript>();
    }
    void Update()
    {
        if (!IsOwner)
        {
            Debug.Log("Not owner, skipping input");
            return;
        }

        Debug.Log("IsOwner OK");

        if (!playerBehaviour.isPaused)
        {
            Debug.Log("Calling GrabObj()");
            GrabObj();
        }
        else
        {
            Debug.Log("Game is paused, cannot grab");
        }

    }

    void GrabObj()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (heldObj == null)
            {
                //perform raycast to check if player is looking at object within pickuprange
                RaycastHit hit;
                if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, pickUpRange))
                {
                    Debug.Log("Raycast HIT: " + hit.transform.name);

                    if (hit.transform.CompareTag("canPickUp"))
                    {
                        Debug.Log("Object has canPickUp tag");
                        NetworkObject netObj = hit.transform.GetComponentInParent<NetworkObject>();
                        if (netObj != null)
                        {
                            RequestPickUpServerRpc(netObj.NetworkObjectId);
                        }
                    }
                    else
                    {
                        Debug.Log("Hit object but wrong tag: " + hit.transform.tag);
                    }
                }
                else
                {
                    Debug.Log("Raycast missed");
                }
            }
            else
            {
                if (canDrop == true)
                {
                    StopClipping(); //prevents object from clipping through walls
                    NetworkObject netObj = heldObj.GetComponent<NetworkObject>();
                    DropObjectServerRpc(netObj.NetworkObjectId);
                    heldObj = null;
                    heldObjRb = null;
                }
            }
        }
        if (heldObj != null)
        {
            MoveObject(); //keep object position at holdPos
            RotateObject();
            if (Input.GetKeyDown(KeyCode.Mouse0) && canDrop == true) //Mous0 (leftclick) is used to throw, change this if you want another button to be used)
            {
                StopClipping();
                NetworkObject netObj = heldObj.GetComponent<NetworkObject>();
                ThrowObjectServerRpc(netObj.NetworkObjectId, transform.forward);
                heldObj = null;
                heldObjRb = null;
            }

        }
    }


    void MoveObject()
    {
        //keep object position the same as the holdPosition position
        heldObj.transform.position = holdPos.transform.position;
    }
    void RotateObject()
    {
        if (Input.GetKey(KeyCode.R))//hold R key to rotate, change this to whatever key you want
        {
            canDrop = false; //make sure throwing can't occur during rotating

            //disable player being able to look around
            //mouseLookScript.verticalSensitivity = 0f;
            //mouseLookScript.lateralSensitivity = 0f;

            float XaxisRotation = Input.GetAxis("Mouse X") * rotationSensitivity;
            float YaxisRotation = Input.GetAxis("Mouse Y") * rotationSensitivity;
            //rotate the object depending on mouse X-Y Axis
            heldObj.transform.Rotate(Vector3.down, XaxisRotation);
            heldObj.transform.Rotate(Vector3.right, YaxisRotation);
        }
        else
        {
            //re-enable player being able to look around
            //mouseLookScript.verticalSensitivity = originalvalue;
            //mouseLookScript.lateralSensitivity = originalvalue;
            canDrop = true;
        }
    }

    void StopClipping() //function only called when dropping/throwing
    {
        var clipRange = Vector3.Distance(heldObj.transform.position, transform.position); //distance from holdPos to the camera
        //have to use RaycastAll as object blocks raycast in center screen
        //RaycastAll returns array of all colliders hit within the cliprange
        RaycastHit[] hits;
        hits = Physics.RaycastAll(transform.position, transform.TransformDirection(Vector3.forward), clipRange);
        //if the array length is greater than 1, meaning it has hit more than just the object we are carrying
        if (hits.Length > 1)
        {
            //change object position to camera position 
            heldObj.transform.position = transform.position + new Vector3(0f, -0.5f, 0f); //offset slightly downward to stop object dropping above player 
            //if your player is small, change the -0.5f to a smaller number (in magnitude) ie: -0.1f
        }
    }

    [ServerRpc]
    void RequestPickUpServerRpc(ulong objectId, ServerRpcParams rpcParams = default)
    {
        Debug.Log("ServerRpc: RequestPickUp received for object " + objectId);
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(objectId, out NetworkObject netObj))
        {
            Rigidbody rb = netObj.GetComponent<Rigidbody>();

            rb.isKinematic = true;

            // Give ownership to the player picking it up
            netObj.ChangeOwnership(rpcParams.Receive.SenderClientId);

            AttachObjectClientRpc(objectId, rpcParams.Receive.SenderClientId);
        }
    }

    [ClientRpc]
    void AttachObjectClientRpc(ulong objectId, ulong playerId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(objectId, out NetworkObject netObj))
        {
            // Only the player holding it should assign it locally
            if (NetworkManager.Singleton.LocalClientId == playerId)
            {
                heldObj = netObj.gameObject;
                heldObjRb = heldObj.GetComponent<Rigidbody>();

                heldObj.layer = LayerNumber;

                Physics.IgnoreCollision(
                    heldObj.GetComponent<Collider>(),
                    player.GetComponent<Collider>(),
                    true
                );
            }
        }
    }

    [ServerRpc]
    void DropObjectServerRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(objectId, out NetworkObject netObj))
        {
            Rigidbody rb = netObj.GetComponent<Rigidbody>();

            rb.isKinematic = false;

            // Return ownership to server
            netObj.RemoveOwnership();

            ResetObjectClientRpc(objectId);
        }
    }

    [ServerRpc]
    void ThrowObjectServerRpc(ulong objectId, Vector3 forward)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(objectId, out NetworkObject netObj))
        {
            Rigidbody rb = netObj.GetComponent<Rigidbody>();

            rb.isKinematic = false;
            rb.AddForce(forward * throwForce);

            netObj.RemoveOwnership();

            ResetObjectClientRpc(objectId);
        }
    }

    [ClientRpc]
    void ResetObjectClientRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects
            .TryGetValue(objectId, out NetworkObject netObj))
        {
            Physics.IgnoreCollision(
                netObj.GetComponent<Collider>(),
                player.GetComponent<Collider>(),
                false
            );

            netObj.gameObject.layer = 0;
        }
    }
}
