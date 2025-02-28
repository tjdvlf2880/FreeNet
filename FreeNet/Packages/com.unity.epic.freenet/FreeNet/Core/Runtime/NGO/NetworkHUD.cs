using UnityEngine;
using Unity.Netcode;

public class NetworkHUD : MonoBehaviour
{
    private bool showHud = true; // HUD ǥ�� ����
    private void OnGUI()
    {
        if (showHud)
        {
            // �⺻ GUI ��Ÿ�� ����
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = new GUIStyleState { textColor = Color.white }
            };

            GUILayout.BeginArea(new Rect(10, 10, 300, 250)); // ���̸� �� ũ�� ����
            GUILayout.BeginVertical();

            // ��Ʈ��ũ ���� ǥ��
            GUILayout.Label("Network Status: " + (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer ? "Connected" : "Disconnected"), style);

            if (NetworkManager.Singleton.IsServer)
            {
                GUILayout.Label("Server Active", style);
            }
            else if (NetworkManager.Singleton.IsClient)
            {
                GUILayout.Label("Client Connected", style);
            }
            else
            {
                GUILayout.Label("No connection", style);
            }

            // ȣ��Ʈ, ����, Ŭ���̾�Ʈ ���� ��ư
            if (!NetworkManager.Singleton.IsClient && !NetworkManager.Singleton.IsServer)
            {
                GUILayout.Space(10);

                if (GUILayout.Button("Start Host"))
                {
                    StartHost();
                }

                if (GUILayout.Button("Start Server"))
                {
                    StartServer();
                }

                if (GUILayout.Button("Start Client"))
                {
                    StartClient();
                }
            }

            // ���� ���� ��ư
            if (NetworkManager.Singleton.IsClient || NetworkManager.Singleton.IsServer)
            {
                GUILayout.Space(10);
                if (GUILayout.Button("Disconnect"))
                {
                    Disconnect();
                }
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }
    }

    // ȣ��Ʈ ����
    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    // ���� ����
    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    // Ŭ���̾�Ʈ ����
    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    // ���� ����
    private void Disconnect()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown(); // ���� ����
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown(); // Ŭ���̾�Ʈ ����
        }
    }

    // HUD ǥ��/����� ���
    public void ToggleHud()
    {
        showHud = !showHud;
    }
}
