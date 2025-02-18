using System.Collections;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using Unity.Netcode;
using Unity.Netcode.TestHelpers.Runtime;
using UnityEngine;
using UnityEngine.TestTools;
using Debug = UnityEngine.Debug;

namespace TestProject.RuntimeTests
{
    public class MultiClientConnectionApproval
    {
        private string m_ConnectionToken;
        private uint m_SuccessfulConnections;
        private uint m_FailedConnections;
        private uint m_PrefabOverrideGlobalObjectIdHash;

        private GameObject m_PlayerPrefab;
        private GameObject m_PlayerPrefabOverride;
        private bool m_DelayedApproval;
        private List<NetworkManager.ConnectionApprovalResponse> m_ResponseToSet = new List<NetworkManager.ConnectionApprovalResponse>();

        /// <summary>
        /// Tests connection approval and connection approval failure
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ConnectionApproval([Values(true, false)] bool delayedApproval)
        {
            m_ConnectionToken = "ThisIsTheRightPassword";
            m_DelayedApproval = delayedApproval;
            return ConnectionApprovalHandler(3, 1);
        }

        /// <summary>
        /// Tests player prefab overriding, connection approval, and connection approval failure
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ConnectionApprovalPrefabOverride()
        {
            m_ConnectionToken = "PrefabOverrideCorrectPassword";
            return ConnectionApprovalHandler(3, 1, true);
        }


        /// <summary>
        /// Allows for several connection approval related configurations
        /// </summary>
        /// <param name="numClients">total number of clients (excluding the host)</param>
        /// <param name="failureTestCount">how many clients are expected to fail</param>
        /// <param name="prefabOverride">if we are also testing player prefab overrides</param>
        /// <returns></returns>
        private IEnumerator ConnectionApprovalHandler(int numClients, int failureTestCount = 1, bool prefabOverride = false)
        {
            var startFrameCount = Time.frameCount;
            var startTime = Time.realtimeSinceStartup;

            m_SuccessfulConnections = 0;
            m_FailedConnections = 0;
            Assert.IsTrue(numClients >= failureTestCount);

            // Create Host and (numClients) clients
            Assert.True(NetcodeIntegrationTestHelpers.Create(numClients, out NetworkManager server, out NetworkManager[] clients));

            // Create a default player GameObject to use
            m_PlayerPrefab = new GameObject("Player");
            var networkObject = m_PlayerPrefab.AddComponent<NetworkObject>();

            // Prefabs should always be owned by the server
            // This assures that if a client is shutdown it will not destroy the prefab
            networkObject.NetworkManagerOwner = server;

            // Make it a prefab
            NetcodeIntegrationTestHelpers.MakeNetworkObjectTestPrefab(networkObject);

            // Create the player prefab override if set
            if (prefabOverride)
            {
                // Create a default player GameObject to use
                m_PlayerPrefabOverride = new GameObject("PlayerPrefabOverride");
                var networkObjectOverride = m_PlayerPrefabOverride.AddComponent<NetworkObject>();
                // Prefabs should always be owned by the server
                // This assures that if a client is shutdown it will not destroy the prefab
                networkObjectOverride.NetworkManagerOwner = server;
                NetcodeIntegrationTestHelpers.MakeNetworkObjectTestPrefab(networkObjectOverride);
                m_PrefabOverrideGlobalObjectIdHash = networkObjectOverride.GlobalObjectIdHash;

                server.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = m_PlayerPrefabOverride });
                foreach (var client in clients)
                {
                    client.NetworkConfig.Prefabs.Add(new NetworkPrefab { Prefab = m_PlayerPrefabOverride });
                }
            }
            else
            {
                m_PrefabOverrideGlobalObjectIdHash = 0;
            }

            // [Host-Side] Set the player prefab
            server.NetworkConfig.PlayerPrefab = m_PlayerPrefab;
            server.NetworkConfig.ConnectionApproval = true;
            server.ConnectionApprovalCallback = ConnectionApprovalCallback;
            server.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(m_ConnectionToken);

            // [Client-Side] Get all of the RpcQueueManualTests instances relative to each client
            var clientsAdjustedList = new List<NetworkManager>();
            var clientsToClean = new List<NetworkManager>();
            var markedForFailure = 0;

            foreach (var client in clients)
            {
                client.NetworkConfig.PlayerPrefab = m_PlayerPrefab;
                client.NetworkConfig.ConnectionApproval = true;
                if (markedForFailure < failureTestCount)
                {
                    client.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes("ThisIsTheWrongPassword");
                    markedForFailure++;
                    clientsToClean.Add(client);
                }
                else
                {
                    client.NetworkConfig.ConnectionData = Encoding.ASCII.GetBytes(m_ConnectionToken);
                    clientsAdjustedList.Add(client);
                }
            }

            // Start the instances
            if (!NetcodeIntegrationTestHelpers.Start(true, server, clients))
            {
                Assert.Fail("Failed to start instances");
            }

            if (m_DelayedApproval)
            {
                // This is necessary so that clients gets the time to attempt connecting and fill the pending approval responses
                var nextFrameNumber = Time.frameCount + 10;
                yield return new WaitUntil(() => Time.frameCount >= nextFrameNumber);

                foreach (var response in m_ResponseToSet)
                {
                    // perform delayed approval
                    // The response class has already been filled, when created in ConnectionApprovalCallback()
                    yield return new WaitForSeconds(0.2f);
                    response.Pending = false;
                }
                m_ResponseToSet.Clear();
            }

            // [Client-Side] Wait for a connection to the server
            yield return NetcodeIntegrationTestHelpers.WaitForClientsConnected(clientsAdjustedList.ToArray(), null, 512);

            // [Host-Side] Check to make sure all clients are connected
            yield return NetcodeIntegrationTestHelpers.WaitForClientsConnectedToServer(server, clientsAdjustedList.Count + 1, null, 512);

            // Validate the number of failed connections is the same as expected
            Assert.IsTrue(m_FailedConnections == failureTestCount);

            // Validate the number of successful connections is the total number of expected clients minus the failed client count
            Assert.IsTrue(m_SuccessfulConnections == (numClients + 1) - failureTestCount);

            // If we are doing player prefab overrides, then check all of the players to make sure they spawned the appropriate NetworkObject
            if (prefabOverride)
            {
                foreach (var networkClient in server.ConnectedClientsList)
                {
                    Assert.IsNotNull(networkClient.PlayerObject);
                    Assert.AreEqual(networkClient.PlayerObject.GlobalObjectIdHash, m_PrefabOverrideGlobalObjectIdHash);
                }
            }

            foreach (var c in clientsToClean)
            {
                Assert.AreEqual(c.DisconnectReason, "Some valid reason");
            }

            foreach (var client in clients)
            {
                // If a client failed, then it will already be shutdown
                if (client.IsListening)
                {
                    client.Shutdown();
                }
            }

            server.ConnectionApprovalCallback = null;
            server.Shutdown();

            Debug.Log($"Total frames updated = {Time.frameCount - startFrameCount} within {Time.realtimeSinceStartup - startTime} seconds.");
        }

        /// <summary>
        /// Delegate handler for the connection approval callback
        /// </summary>
        /// <param name="connectionData">the NetworkConfig.ConnectionData sent from the client being approved</param>
        /// <param name="clientId">the client id being approved</param>
        /// <param name="callback">the callback invoked to handle approval</param>
        private void ConnectionApprovalCallback(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
        {
            string approvalToken = Encoding.ASCII.GetString(request.Payload);
            var isApproved = approvalToken == m_ConnectionToken;

            if (m_DelayedApproval)
            {
                response.Pending = true;
                m_ResponseToSet.Add(response);
            }

            if (isApproved)
            {
                m_SuccessfulConnections++;
            }
            else
            {
                m_FailedConnections++;
            }

            if (m_PrefabOverrideGlobalObjectIdHash == 0)
            {
                response.CreatePlayerObject = true;
                response.Approved = isApproved;
                response.Position = null;
                response.Rotation = null;
                response.PlayerPrefabHash = null;
            }
            else
            {
                response.CreatePlayerObject = true;
                response.Approved = isApproved;
                response.Position = null;
                response.Rotation = null;
                response.PlayerPrefabHash = m_PrefabOverrideGlobalObjectIdHash;
            }
            if (!response.Approved)
            {
                response.Reason = "Some valid reason";
            }
            else
            {
                response.Reason = string.Empty;
            }
        }



        private int m_ServerClientConnectedInvocations;

        private int m_ServerClientConnectedInvocationsThroughConnectionEvent;

        private int m_ClientConnectedInvocations;

        private int m_ClientConnectedInvocationsThroughConnectionEvent;

        private int m_PeerConnectedInvocations;

        /// <summary>
        /// Tests that the OnClientConnectedCallback is invoked when scene management is enabled and disabled
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ClientConnectedCallbackTest([Values(true, false)] bool enableSceneManagement, [Values(true, false)] bool isHost)
        {
            m_ServerClientConnectedInvocations = 0;
            m_ServerClientConnectedInvocationsThroughConnectionEvent = 0;
            m_ClientConnectedInvocations = 0;
            m_ClientConnectedInvocationsThroughConnectionEvent = 0;
            m_PeerConnectedInvocations = 0;

            // Create Host and (numClients) clients
            Assert.True(NetcodeIntegrationTestHelpers.Create(3, out NetworkManager server, out NetworkManager[] clients));

            server.NetworkConfig.EnableSceneManagement = enableSceneManagement;
            server.OnClientConnectedCallback += Server_OnClientConnectedCallback;
            server.OnConnectionEvent += Server_OnConnectionEventCallback;

            foreach (var client in clients)
            {
                client.NetworkConfig.EnableSceneManagement = enableSceneManagement;
                client.OnClientConnectedCallback += Client_OnClientConnectedCallback;
                client.OnConnectionEvent += Client_OnConnectionEventCallback;
            }

            // Start the instances
            if (!NetcodeIntegrationTestHelpers.Start(isHost, server, clients))
            {
                Assert.Fail("Failed to start instances");
            }

            // [Client-Side] Wait for a connection to the server
            yield return NetcodeIntegrationTestHelpers.WaitForClientsConnected(clients, null, 512);

            // [Host-Side] Check to make sure all clients are connected
            yield return NetcodeIntegrationTestHelpers.WaitForClientsConnectedToServer(server, isHost ? (clients.Length + 1) : clients.Length, null, 512);


            Assert.AreEqual(3, m_ClientConnectedInvocations);
            Assert.AreEqual(3, m_ClientConnectedInvocationsThroughConnectionEvent);
            var timeoutHelper = new TimeoutHelper(2);

            var expectedClientCount = (isHost ? 4 : 3);
            yield return NetcodeIntegrationTest.WaitForConditionOrTimeOut(() => m_ServerClientConnectedInvocations == expectedClientCount, timeoutHelper);
            Assert.False(timeoutHelper.TimedOut, $"Timed out waiting for server client connections to reach a count of {expectedClientCount} but only has {m_ServerClientConnectedInvocations}!");
            Assert.AreEqual(expectedClientCount, m_ServerClientConnectedInvocations);
            Assert.AreEqual(expectedClientCount, m_ServerClientConnectedInvocationsThroughConnectionEvent);

            // Host will get peer connection events, server will not
            var expectedPeerConnections = isHost ? (4 * (4 - 1)) : (3 * (3 - 1));
            Assert.AreEqual(expectedPeerConnections, m_PeerConnectedInvocations);
        }

        private void Client_OnClientConnectedCallback(ulong clientId)
        {
            m_ClientConnectedInvocations++;
        }


        private void Client_OnConnectionEventCallback(NetworkManager networkManager, ConnectionEventData data)
        {
            switch (data.EventType)
            {
                case ConnectionEvent.ClientConnected:
                    m_ClientConnectedInvocationsThroughConnectionEvent++;
                    Assert.AreEqual(networkManager.LocalClientId, data.ClientId);
                    Assert.AreEqual(networkManager.ConnectedClientsIds.Count - 1, data.PeerClientIds.Length);
                    var set = new HashSet<ulong>(networkManager.ConnectedClientsIds);
                    for (var i = 0; i < data.PeerClientIds.Length; ++i)
                    {
                        Assert.IsTrue(set.Contains(data.PeerClientIds[i]));
                        // detect duplicates
                        set.Remove(data.PeerClientIds[i]);
                    }
                    Assert.IsTrue(set.Contains(data.ClientId));
                    m_PeerConnectedInvocations += data.PeerClientIds.Length;
                    break;
                case ConnectionEvent.PeerConnected:
                    m_PeerConnectedInvocations++;
                    break;
            }
        }

        private void Server_OnClientConnectedCallback(ulong clientId)
        {
            m_ServerClientConnectedInvocations++;
        }
        private void Server_OnConnectionEventCallback(NetworkManager networkManager, ConnectionEventData data)
        {
            switch (data.EventType)
            {
                case ConnectionEvent.ClientConnected:
                    m_ServerClientConnectedInvocationsThroughConnectionEvent++;
                    Assert.IsFalse(data.PeerClientIds.IsCreated);
                    break;
                case ConnectionEvent.PeerConnected:
                    m_PeerConnectedInvocations++;
                    Assert.IsFalse(data.PeerClientIds.IsCreated);
                    break;
            }
        }


        private int m_ClientDisconnectedInvocations;

        /// <summary>
        /// Tests that clients are disconnected when their ConnectionApproval setting is mismatched with the host-server
        /// and  when scene management is enabled and disabled
        /// </summary>
        /// <returns></returns>
        [UnityTest]
        public IEnumerator ConnectionApprovalMismatchTest([Values(true, false)] bool enableSceneManagement, [Values(true, false)] bool connectionApproval)
        {
            m_ClientDisconnectedInvocations = 0;

            // Create Host and (numClients) clients
            Assert.True(NetcodeIntegrationTestHelpers.Create(3, out NetworkManager server, out NetworkManager[] clients));

            server.NetworkConfig.EnableSceneManagement = enableSceneManagement;
            server.NetworkConfig.ConnectionApproval = connectionApproval;
            foreach (var client in clients)
            {
                client.NetworkConfig.EnableSceneManagement = enableSceneManagement;
                client.NetworkConfig.ConnectionApproval = !connectionApproval;
                client.OnClientDisconnectCallback += Client_OnClientDisconnectedCallback; //Server notifies client (not vice versa)
            }
            // Start the instances
            if (!NetcodeIntegrationTestHelpers.Start(true, server, clients))
            {
                Assert.Fail("Failed to start instances");
            }

            var timeoutHelper = new TimeoutHelper();
            yield return NetcodeIntegrationTest.WaitForConditionOrTimeOut(() => m_ClientDisconnectedInvocations == 3);
            Assert.False(timeoutHelper.TimedOut, "Timed out waiting for clients to be disconnected!");
            Assert.AreEqual(3, m_ClientDisconnectedInvocations);
        }

        private void Client_OnClientDisconnectedCallback(ulong clientId)
        {
            m_ClientDisconnectedInvocations++;
        }


        [TearDown]
        public void TearDown()
        {
            if (m_PlayerPrefab != null)
            {
                Object.Destroy(m_PlayerPrefab);
                m_PlayerPrefab = null;
            }

            if (m_PlayerPrefabOverride != null)
            {
                Object.Destroy(m_PlayerPrefabOverride);
                m_PlayerPrefabOverride = null;
            }

            // Shutdown and clean up both of our NetworkManager instances
            NetcodeIntegrationTestHelpers.Destroy();
        }
    }
}
