using System.Collections;
using UnityEngine;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Sensor;
using RosMessageTypes.Std;

public class CameraImagePublisher : MonoBehaviour
{
    [Header("ROS Settings")]
    public string topicName = "/camera/image_raw/compressed";
    public float publishFrequency = 30f; // 30 FPS
    
    [Header("Camera Settings")]
    [Tooltip("카메라 이미지 너비 (640 권장)")]
    public int imageWidth = 640;
    [Tooltip("카메라 이미지 높이 (480 권장)")]
    public int imageHeight = 480;
    public Camera targetCamera;
    
    [Header("Image Settings")]
    public bool flipImageVertically = false;
    public int jpegQuality = 80;
    
    private ROSConnection ros;
    private RenderTexture renderTexture;
    private Texture2D texture2D;
    private Coroutine publishCoroutine;
    
    void Start()
    {
        // 해상도를 640x480으로 강제 설정
        imageWidth = 640;
        imageHeight = 480;
        
        // ROS 연결 초기화
        ros = ROSConnection.GetOrCreateInstance();
        ros.RegisterPublisher<CompressedImageMsg>(topicName);
        
        // 카메라 설정
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindObjectOfType<Camera>();
            }
        }
        
        if (targetCamera == null)
        {
            Debug.LogError("CameraImagePublisher: No camera found! Please assign a camera.");
            enabled = false;
            return;
        }
        
        // RenderTexture 설정
        SetupRenderTexture();
        
        // 텍스처 초기화
        texture2D = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
        
        Debug.Log($"CameraImagePublisher initialized - Camera: {targetCamera.name}, Resolution: {imageWidth}x{imageHeight}, Topic: {topicName}");
        
        // 발행 시작
        StartPublishing();
    }
    
    void SetupRenderTexture()
    {
        // 기존 RenderTexture 정리
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
        
        // 새로운 RenderTexture 생성
        renderTexture = new RenderTexture(imageWidth, imageHeight, 24);
        renderTexture.antiAliasing = 1;
        renderTexture.filterMode = FilterMode.Bilinear;
        renderTexture.format = RenderTextureFormat.ARGB32;
        
        // 카메라에 RenderTexture 할당
        targetCamera.targetTexture = renderTexture;
    }
    
    public void StartPublishing()
    {
        if (publishCoroutine != null)
        {
            StopCoroutine(publishCoroutine);
        }
        publishCoroutine = StartCoroutine(PublishImageRoutine());
    }
    
    public void StopPublishing()
    {
        if (publishCoroutine != null)
        {
            StopCoroutine(publishCoroutine);
            publishCoroutine = null;
        }
    }
    
    private IEnumerator PublishImageRoutine()
    {
        while (true)
        {
            PublishImage();
            yield return new WaitForSeconds(1f / publishFrequency);
        }
    }
    
    private void PublishImage()
    {
        if (targetCamera == null || renderTexture == null) return;
        
        // 카메라에서 이미지 캡처
        targetCamera.Render();
        
        // RenderTexture에서 픽셀 데이터 읽기
        RenderTexture.active = renderTexture;
        texture2D.ReadPixels(new Rect(0, 0, imageWidth, imageHeight), 0, 0);
        
        // 이미지 뒤집기 (Unity는 Y축이 위쪽이지만 ROS는 아래쪽이 기준)
        if (flipImageVertically)
        {
            Color[] pixels = texture2D.GetPixels();
            Color[] flippedPixels = new Color[pixels.Length];
            
            for (int y = 0; y < imageHeight; y++)
            {
                for (int x = 0; x < imageWidth; x++)
                {
                    flippedPixels[y * imageWidth + x] = pixels[(imageHeight - 1 - y) * imageWidth + x];
                }
            }
            
            texture2D.SetPixels(flippedPixels);
        }
        
        texture2D.Apply();
        RenderTexture.active = null;
        
        // 이미지를 JPEG로 압축
        byte[] imageData = texture2D.EncodeToJPG(jpegQuality);
        
        // ROS CompressedImage 메시지 생성
        var compressedImageMsg = new CompressedImageMsg
        {
            header = new HeaderMsg
            {
                stamp = new RosMessageTypes.BuiltinInterfaces.TimeMsg
                {
                    sec = (int)Time.time,
                    nanosec = (uint)((Time.time - (int)Time.time) * 1e9)
                },
                frame_id = "camera_link"
            },
            format = "jpeg", // JPEG 압축 형식
            data = imageData
        };
        
        // ROS로 발행
        ros.Publish(topicName, compressedImageMsg);
    }
    
    void OnDestroy()
    {
        StopPublishing();
        
        // 리소스 정리
        if (renderTexture != null)
        {
            renderTexture.Release();
            DestroyImmediate(renderTexture);
        }
        
        if (texture2D != null)
        {
            DestroyImmediate(texture2D);
        }
        
        // 카메라 targetTexture 복원
        if (targetCamera != null)
        {
            targetCamera.targetTexture = null;
        }
    }
    
    void OnValidate()
    {
        // 에디터에서 해상도 변경 시 RenderTexture 재생성
        if (Application.isPlaying && renderTexture != null)
        {
            if (renderTexture.width != imageWidth || renderTexture.height != imageHeight)
            {
                SetupRenderTexture();
            }
        }
    }
    
    // 공개 메서드들
    public void SetPublishFrequency(float frequency)
    {
        publishFrequency = Mathf.Clamp(frequency, 1f, 60f);
        if (Application.isPlaying)
        {
            StartPublishing(); // 코루틴 재시작
        }
    }
    
    public void SetImageResolution(int width, int height)
    {
        imageWidth = Mathf.Clamp(width, 64, 1920);
        imageHeight = Mathf.Clamp(height, 64, 1080);
        
        if (Application.isPlaying)
        {
            SetupRenderTexture();
            if (texture2D != null)
            {
                DestroyImmediate(texture2D);
                texture2D = new Texture2D(imageWidth, imageHeight, TextureFormat.RGB24, false);
            }
        }
    }
    
    public bool IsPublishing()
    {
        return publishCoroutine != null;
    }
    
    public float GetCurrentFPS()
    {
        return publishFrequency;
    }
}
