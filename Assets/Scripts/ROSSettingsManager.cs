using UnityEngine;

public class ROSSettingsManager : MonoBehaviour
{
    private static ROSSettingsManager instance;
    
    [Header("ROS Connection Settings")]
    public string rosIPAddress = "127.0.0.1";
    public int rosPort = 10000;
    public bool settingsConfigured = false;
    
    public static ROSSettingsManager Instance
    {
        get
        {
            if (instance == null)
            {
                // 새로운 GameObject 생성하고 ROSSettingsManager 추가
                GameObject go = new GameObject("ROSSettingsManager");
                instance = go.AddComponent<ROSSettingsManager>();
                DontDestroyOnLoad(go);
            }
            return instance;
        }
    }
    
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    
    public void SetROSSettings(string ip, int port)
    {
        rosIPAddress = ip;
        rosPort = port;
        settingsConfigured = true;
        
        // PlayerPrefs에도 저장 (영구 저장)
        PlayerPrefs.SetString("ROS_IP", ip);
        PlayerPrefs.SetInt("ROS_Port", port);
        PlayerPrefs.SetString("ROS_Configured", "true");
        PlayerPrefs.Save();
        
        Debug.Log($"ROS settings saved: {rosIPAddress}:{rosPort}");
    }
    
    public void LoadROSSettings()
    {
        if (PlayerPrefs.HasKey("ROS_IP"))
        {
            rosIPAddress = PlayerPrefs.GetString("ROS_IP", "127.0.0.1");
            rosPort = PlayerPrefs.GetInt("ROS_Port", 10000);
            settingsConfigured = PlayerPrefs.GetString("ROS_Configured", "false") == "true";
        }
    }
    
    public void ClearSettings()
    {
        settingsConfigured = false;
        PlayerPrefs.SetString("ROS_Configured", "false");
        PlayerPrefs.Save();
    }
}