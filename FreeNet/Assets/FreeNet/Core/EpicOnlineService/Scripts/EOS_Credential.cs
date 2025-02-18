using UnityEngine;

[CreateAssetMenu(fileName = "Credential", menuName = "Scriptable Objects/Credential")]
public class EOS_Credential : ScriptableObject
{
    public enum CredentialType
    {
        Dev,
        Stage,
        Live
    }

    [SerializeField]
    private CredentialType type;
    [SerializeField]
    private string productId = "";

    [SerializeField]
    private string applicationId = "";

    [SerializeField]
    private string sandboxId = "";

    [SerializeField]
    private string deploymentId = "";

    [SerializeField]
    private string clientCredentialsId = "";

    [SerializeField]
    private string clientCredentialsSecret = "";

    [SerializeField]
    private string gameName = "";
    [SerializeField]
    private string encryptionKey = "1111111111111111111111111111111111111111111111111111111";

    //// Public getter methods for each private field
    public CredentialType Type => CredentialType.Dev;
    public string ProductId => productId;
    public string ApplicationId => applicationId;
    public string SandboxId => sandboxId;
    public string DeploymentId => deploymentId;
    public string ClientCredentialsId => clientCredentialsId;
    public string ClientCredentialsSecret => clientCredentialsSecret;
    public string GameName => gameName;
    public string EncryptionKey => encryptionKey;
}
