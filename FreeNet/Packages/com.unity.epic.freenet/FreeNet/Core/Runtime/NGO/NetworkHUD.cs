using UnityEngine;
using Unity.Netcode;

public class NetworkHUD : MonoBehaviour
{
    private bool showHud = true; // HUD 표시 여부
    private void OnGUI()
    {
        if (showHud)
        {
            // 기본 GUI 스타일 설정
            GUIStyle style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 16,
                normal = new GUIStyleState { textColor = Color.white }
            };

            GUILayout.BeginArea(new Rect(10, 10, 300, 250)); // 높이를 더 크게 설정
            GUILayout.BeginVertical();

            // 네트워크 상태 표시
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

            // 호스트, 서버, 클라이언트 시작 버튼
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

            // 연결 종료 버튼
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

    // 호스트 시작
    private void StartHost()
    {
        NetworkManager.Singleton.StartHost();
    }

    // 서버 시작
    private void StartServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    // 클라이언트 시작
    private void StartClient()
    {
        NetworkManager.Singleton.StartClient();
    }

    // 연결 종료
    private void Disconnect()
    {
        if (NetworkManager.Singleton.IsServer)
        {
            NetworkManager.Singleton.Shutdown(); // 서버 종료
        }
        else if (NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.Shutdown(); // 클라이언트 종료
        }
    }

    // HUD 표시/숨기기 토글
    public void ToggleHud()
    {
        showHud = !showHud;
    }
}
