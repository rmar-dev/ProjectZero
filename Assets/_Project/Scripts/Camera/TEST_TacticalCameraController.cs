using UnityEngine;
using UnityEngine.InputSystem;
using Cinemachine;
using System.Collections.Generic;

namespace ProjectZero.Camera
{
    /// <summary>
    /// Main tactical camera controller that coordinates all camera components.
    /// Uses compositional architecture with single-responsibility components.
    /// </summary>
    public class TacticalCameraController : MonoBehaviour
    {
        [Header("Camera References")]
        [SerializeField] private Transform cameraTarget;
        [SerializeField] private CinemachineVirtualCamera virtualCamera;
        
        [Header("Input Actions")]
        [SerializeField] private InputActionReference moveActionRef;
        [SerializeField] private InputActionReference zoomActionRef;
        
        [Header("Camera Components")]
        [SerializeField] private CameraMovement movementComponent = new CameraMovement();
        [SerializeField] private CameraZoom zoomComponent = new CameraZoom();
        [SerializeField] private CameraBounds boundsComponent = new CameraBounds();
        [SerializeField] private CameraInputHandler inputHandler = new CameraInputHandler();
        
        [Header("Debug Settings")]
        [SerializeField] private bool showDebugInfo = false;
        [SerializeField] private bool enableGizmos = true;
        
        // Component management
        private List<ICameraComponent> cameraComponents;
        private bool isInitialized = false;
        
        private void Awake()
        {
            ValidateReferences();
            SetupComponents();
        }
        
        private void Start()
        {
            InitializeCameraSystem();
        }
        
        private void Update()
        {
            if (!isInitialized) return;
            
            UpdateAllComponents();
        }
        
        private void OnDestroy()
        {
            CleanupCameraSystem();
        }
        
        /// <summary>
        /// Validate required references and setup defaults
        /// </summary>
        private void ValidateReferences()
        {
            // Auto-find camera target if not assigned
            if (cameraTarget == null)
            {
                cameraTarget = transform;
            }
            
            // Auto-find virtual camera if not assigned
            if (virtualCamera == null)
            {
                virtualCamera = FindObjectOfType<CinemachineVirtualCamera>();
            }
            
            // Ensure we have Framing Transposer for zoom
            if (virtualCamera != null)
            {
                var transposer = virtualCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
                if (transposer == null)
                {
                    Debug.LogWarning("TacticalCameraController: Virtual Camera needs a Framing Transposer for zoom functionality.");
                }
            }
        }
        
        /// <summary>
        /// Set up component list and connections
        /// </summary>
        private void SetupComponents()
        {
            cameraComponents = new List<ICameraComponent>
            {
                movementComponent,
                zoomComponent,
                boundsComponent,
                inputHandler
            };
            
            // Connect input handler to movement and zoom components
            inputHandler.ConnectComponents(movementComponent, zoomComponent);
        }
        
        /// <summary>
        /// Initialize the entire camera system
        /// </summary>
        private void InitializeCameraSystem()
        {
            // Initialize all components
            foreach (var component in cameraComponents)
            {
                component.Initialize(cameraTarget);
            }
            
            // Set up input actions
            if (moveActionRef != null && zoomActionRef != null)
            {
                inputHandler.SetupInputActions(moveActionRef.action, zoomActionRef.action);
            }
            else
            {
                Debug.LogWarning("TacticalCameraController: Input Action References are not assigned.");
            }
            
            isInitialized = true;
            
            Debug.Log("TacticalCameraController: Camera system initialized successfully.");
        }
        
        /// <summary>
        /// Update all active camera components
        /// </summary>
        private void UpdateAllComponents()
        {
            foreach (var component in cameraComponents)
            {
                if (component.IsActive)
                {
                    component.UpdateComponent();
                }
            }
        }
        
        /// <summary>
        /// Clean up camera system resources
        /// </summary>
        private void CleanupCameraSystem()
        {
            if (cameraComponents != null)
            {
                foreach (var component in cameraComponents)
                {
                    component.Cleanup();
                }
            }
            
            isInitialized = false;
        }
        
        /// <summary>
        /// Configure movement settings at runtime
        /// </summary>
        public void ConfigureMovement(float speed, float smoothTime = 0.1f, bool enableSmoothing = true)
        {
            movementComponent.ConfigureMovement(speed, smoothTime, enableSmoothing);
        }
        
        /// <summary>
        /// Configure zoom settings at runtime
        /// </summary>
        public void ConfigureZoom(float speed, float min, float max, float smoothTime = 0.2f)
        {
            zoomComponent.ConfigureZoom(speed, min, max, smoothTime);
        }
        
        /// <summary>
        /// Configure camera boundaries at runtime
        /// </summary>
        public void ConfigureBounds(Vector2 size, Vector2 center = default)
        {
            boundsComponent.ConfigureBounds(size, center);
        }
        
        /// <summary>
        /// Enable or disable specific camera components
        /// </summary>
        public void SetComponentActive(System.Type componentType, bool active)
        {
            foreach (var component in cameraComponents)
            {
                if (component.GetType() == componentType)
                {
                    component.IsActive = active;
                    break;
                }
            }
        }
        
        /// <summary>
        /// Get reference to specific camera component
        /// </summary>
        public T GetComponent<T>() where T : class, ICameraComponent
        {
            foreach (var component in cameraComponents)
            {
                if (component is T)
                {
                    return component as T;
                }
            }
            return null;
        }
        
        /// <summary>
        /// Set camera position directly (useful for teleporting)
        /// </summary>
        public void SetCameraPosition(Vector3 position)
        {
            if (cameraTarget != null)
            {
                cameraTarget.position = position;
            }
        }
        
        /// <summary>
        /// Set zoom distance directly
        /// </summary>
        public void SetZoomDistance(float distance)
        {
            zoomComponent.SetZoomDistance(distance);
        }
        
        /// <summary>
        /// Enable or disable input processing
        /// </summary>
        public void SetInputEnabled(bool enabled)
        {
            inputHandler.SetInputEnabled(enabled);
        }
        
        /// <summary>
        /// Get current camera status for debugging
        /// </summary>
        public CameraStatus GetCameraStatus()
        {
            return new CameraStatus
            {
                position = cameraTarget != null ? cameraTarget.position : Vector3.zero,
                zoomDistance = zoomComponent.GetCurrentZoom(),
                zoomProgress = zoomComponent.GetZoomProgress(),
                movementSpeed = movementComponent.GetCurrentSpeed(),
                inputs = inputHandler.GetCurrentInputs(),
                hasActiveInput = inputHandler.HasActiveInput(),
                boundsInfo = boundsComponent.GetBoundaryInfo()
            };
        }
        
        private void OnDrawGizmos()
        {
            if (enableGizmos && boundsComponent != null)
            {
                boundsComponent.DrawBoundaryGizmos();
            }
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo || !isInitialized) return;
            
            var status = GetCameraStatus();
            var style = new GUIStyle(GUI.skin.box)
            {
                alignment = TextAnchor.UpperLeft,
                fontSize = 12
            };
            
            string debugText = $"Tactical Camera Debug:\n" +
                             $"Position: {status.position:F2}\n" +
                             $"Zoom: {status.zoomDistance:F1} ({status.zoomProgress:P0})\n" +
                             $"Movement Speed: {status.movementSpeed:F1}\n" +
                             $"Input: Move={status.inputs.movement:F2}, Zoom={status.inputs.zoom:F2}\n" +
                             $"Has Input: {status.hasActiveInput}\n" +
                             $"Bounds: {status.boundsInfo.size}";
            
            GUI.Box(new Rect(10, 10, 300, 150), debugText, style);
        }
    }
    
    /// <summary>
    /// Camera status information for debugging and external systems
    /// </summary>
    public struct CameraStatus
    {
        public Vector3 position;
        public float zoomDistance;
        public float zoomProgress;
        public float movementSpeed;
        public (Vector2 movement, float zoom) inputs;
        public bool hasActiveInput;
        public (Vector2 min, Vector2 max, Vector2 size) boundsInfo;
    }
}
