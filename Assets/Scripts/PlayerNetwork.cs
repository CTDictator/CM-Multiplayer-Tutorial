using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using Random = UnityEngine.Random;

public class PlayerNetwork : NetworkBehaviour
{
    [SerializeField] private Transform spawnedObjectPrefab;
    private Transform spawnedObjectTransform;
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData
        {
            _int = 56,
            _bool = true,
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    public struct MyCustomData : INetworkSerializable
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;
        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }
    
    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + "; " + newValue._int + "; " + newValue._bool + "; " + newValue.message);
        };
    }
    
    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.T))
        {
            spawnedObjectTransform = Instantiate(spawnedObjectPrefab);
            spawnedObjectTransform.GetComponent<NetworkObject>().Spawn(true);
            //TestClientRpc(new ClientRpcParams
            //    { Send = new ClientRpcSendParams { TargetClientIds = new List<ulong> { 1 } } });
            /*
            randomNumber.Value = new MyCustomData
            {
                _int = Random.Range(0, 100),
                _bool = false,
                message = "THE HORROR!",
            };
            */
        }

        if (Input.GetKeyDown(KeyCode.Y))
        {
            spawnedObjectTransform.GetComponent<NetworkObject>().Despawn(true);
            Destroy(spawnedObjectTransform.gameObject);
        }
        
        Vector3 moveDirection = new Vector3();
        if (Input.GetKey(KeyCode.W)) moveDirection.z = 1f;
        if (Input.GetKey(KeyCode.A)) moveDirection.x = -1f;
        if (Input.GetKey(KeyCode.S)) moveDirection.z = -1f;
        if (Input.GetKey(KeyCode.D)) moveDirection.x = 1f;
        float moveSpeed = 3f;
        transform.position += moveDirection * (Time.deltaTime * moveSpeed);
    }

    [ServerRpc]
    private void TestServerRpc(ServerRpcParams serverRpcParams)
    {
        Debug.Log("TestServerRpc " + OwnerClientId + "; " + serverRpcParams.Receive.SenderClientId);
    }

    [ClientRpc]
    private void TestClientRpc(ClientRpcParams clientRpcParams)
    {
        Debug.Log("testClientRpc ");
    }
}
