using System.Collections.Generic;
using Fusion;
using Fusion.Sockets;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkObject playerPrefab;

    private NetworkRunner _runner;

    private bool jumpPressedCache = false;
    private bool punchPressedCache = false;
    private bool ropePressedCache = false;
    private bool pullHeldCache = false;

    private async void Start()
    {
        _runner = GetComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        _runner.AddCallbacks(this);
        GameMode mode = Application.isEditor ? GameMode.Host : GameMode.Client;

        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "OyunOdasi",
            SceneManager = GetComponent<NetworkSceneManagerDefault>()
        });
    }

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
{
    Debug.Log($"[JOINED] {player} joined. IsServer: {runner.IsServer} | IsClient: {runner.IsClient}");

    // SADECE SERVER SPAWN EDER → herkes birbirini görsün
    if (!runner.IsServer) return;

    Vector3 spawnPos = new Vector3(
        Random.Range(-2f, 2f),
        0.2f,
        Random.Range(-2f, 2f)
    );

    if (playerPrefab != null)
    {
        NetworkObject spawnedPlayer = runner.Spawn(
            playerPrefab,
            spawnPos,
            Quaternion.identity,
            inputAuthority: player
        );

        if (spawnedPlayer == null)
        {
            Debug.LogError("❌ PlayerRoot spawn başarısız! Prefab referansını ve NetworkPrefab tablosunu kontrol et.");
        }
        else
        {
            Debug.Log($"✅ PlayerRoot başarıyla spawn edildi → PlayerRef: {player}");
        }
    }
    else
    {
        Debug.LogWarning("⚠️ Player prefab atanmadı!");
    }
}


    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        NetworkInputData inputData = new NetworkInputData();

        if (Keyboard.current != null)
        {
            Vector2 moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) moveInput.y += 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y -= 1;
            if (Keyboard.current.aKey.isPressed) moveInput.x -= 1;
            if (Keyboard.current.dKey.isPressed) moveInput.x += 1;
            inputData.move = moveInput;

            if (!jumpPressedCache && Keyboard.current.spaceKey.isPressed)
            {
                inputData.jumpRequested = true;
                jumpPressedCache = true;
            }

            if (!punchPressedCache && Keyboard.current.qKey.isPressed)
            {
                inputData.punchPressed = true;
                punchPressedCache = true;
            }

            if (!ropePressedCache && Keyboard.current.eKey.isPressed &&
                (Mouse.current == null || !Mouse.current.leftButton.isPressed))
            {
                inputData.ropePressed = true;
                ropePressedCache = true;
            }
        }

        if (Mouse.current != null)
        {
            bool pressedNow = Mouse.current.leftButton.isPressed;

            if (pressedNow != pullHeldCache)
            {
                pullHeldCache = pressedNow;
            }

            inputData.pullHeld = pullHeldCache;
        }

        input.Set(inputData);
    }

    private void LateUpdate()
    {
        jumpPressedCache = false;
        punchPressedCache = false;
        ropePressedCache = false;
    }

    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, System.ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
}
