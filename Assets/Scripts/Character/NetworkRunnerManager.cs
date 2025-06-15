using Fusion;
using Fusion.Sockets;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class NetworkRunnerManager : MonoBehaviour, INetworkRunnerCallbacks
{
    public NetworkObject playerPrefab;
    private NetworkRunner _runner;

    private void OnGUI()
    {
        if (_runner == null || _runner.IsRunning == false)
        {
            if (GUI.Button(new Rect(10, 10, 250, 40), "Host Olarak Başlat")) StartGame(GameMode.Host);
            if (GUI.Button(new Rect(10, 60, 250, 40), "Client Olarak Katıl")) StartGame(GameMode.Client);
        }
    }

    private async void StartGame(GameMode mode)
    {
        _runner = gameObject.AddComponent<NetworkRunner>();
        _runner.ProvideInput = true;
        
        await _runner.StartGame(new StartGameArgs()
        {
            GameMode = mode,
            SessionName = "TestOdasi",
            Scene = NetworkSceneInfo.FromBuildIndex(1),
            SceneManager = gameObject.AddComponent<NetworkSceneManagerDefault>()
        });

        _runner.AddCallbacks(this); 
    }
    
    public void OnInput(NetworkRunner runner, NetworkInput input)
    {
        var inputData = new NetworkInputData();

        if (Keyboard.current != null)
        {
            Vector2 moveInput = Vector2.zero;
            if (Keyboard.current.wKey.isPressed) moveInput.y = 1;
            if (Keyboard.current.sKey.isPressed) moveInput.y = -1;
            if (Keyboard.current.aKey.isPressed) moveInput.x = -1;
            if (Keyboard.current.dKey.isPressed) moveInput.x = 1;
            inputData.move = moveInput;

            inputData.runPressed = Keyboard.current.leftShiftKey.isPressed;

            var buttons = default(NetworkButtons);
            buttons.Set(PlayerButtons.Jump, Keyboard.current.spaceKey.isPressed);
            buttons.Set(PlayerButtons.Punch, Keyboard.current.qKey.isPressed);
            buttons.Set(PlayerButtons.Rope, Keyboard.current.eKey.isPressed);
            inputData.buttons = buttons;
        }

        if (Mouse.current != null) inputData.pullHeld = Mouse.current.leftButton.isPressed;

        input.Set(inputData);
    }
    
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
    {
        if (runner.IsServer)
        {
            // DÜZELTME: 'Random' belirsizliğini gideriyoruz.
            Vector3 spawnPos = new Vector3(UnityEngine.Random.Range(-4f, 4f), 1f, UnityEngine.Random.Range(-4f, 4f));
            runner.Spawn(playerPrefab, spawnPos, Quaternion.identity, player);
        }
    }
    
    #region Unused Callbacks
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnDisconnectedFromServer(NetworkRunner runner, NetDisconnectReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ReliableKey key, ArraySegment<byte> data) { }
    public void OnReliableDataProgress(NetworkRunner runner, PlayerRef player, ReliableKey key, float progress) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnObjectEnterAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    public void OnObjectExitAOI(NetworkRunner runner, NetworkObject obj, PlayerRef player) { }
    #endregion
}