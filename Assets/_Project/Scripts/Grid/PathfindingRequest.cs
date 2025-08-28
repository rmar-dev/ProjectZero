using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// Represents a pathfinding request with priority and callback support
    /// Enables async pathfinding operations with queue management
    /// </summary>
    public class PathfindingRequest
    {
        public string RequestId { get; private set; }
        public Vector3 StartWorldPosition { get; private set; }
        public Vector3 TargetWorldPosition { get; private set; }
        public PathfindingProfile Profile { get; private set; }
        public PathfindingPriority Priority { get; private set; }
        public float RequestTime { get; private set; }
        public float TimeoutDuration { get; private set; }
        public PathfindingRequestStatus Status { get; internal set; }
        
        // Results
        public List<GridNode> ResultPath { get; set; }
        public AStarPathfinder.PathfindingStats Stats { get; set; }
        public string ErrorMessage { get; set; }
        
        // Callbacks
        public Action<PathfindingRequest> OnCompleted { get; set; }
        public Action<PathfindingRequest> OnFailed { get; set; }
        public Action<PathfindingRequest> OnTimeout { get; set; }
        
        // Optional requestor reference for debugging
        public UnityEngine.Object Requestor { get; set; }

        public PathfindingRequest(
            Vector3 startWorldPos, 
            Vector3 targetWorldPos, 
            PathfindingProfile profile = null,
            PathfindingPriority priority = PathfindingPriority.Normal,
            float timeoutDuration = 5.0f,
            UnityEngine.Object requestor = null)
        {
            RequestId = System.Guid.NewGuid().ToString();
            StartWorldPosition = startWorldPos;
            TargetWorldPosition = targetWorldPos;
            Profile = profile;
            Priority = priority;
            RequestTime = Time.time;
            TimeoutDuration = timeoutDuration;
            Status = PathfindingRequestStatus.Pending;
            Requestor = requestor;
            
            ResultPath = new List<GridNode>();
        }

        /// <summary>
        /// Check if this request has timed out
        /// </summary>
        public bool HasTimedOut => (Time.time - RequestTime) > TimeoutDuration;

        /// <summary>
        /// Get the age of this request in seconds
        /// </summary>
        public float Age => Time.time - RequestTime;

        /// <summary>
        /// Mark request as completed with results
        /// </summary>
        public void Complete(List<GridNode> path, AStarPathfinder.PathfindingStats stats)
        {
            Status = PathfindingRequestStatus.Completed;
            ResultPath = path ?? new List<GridNode>();
            Stats = stats;
            
            OnCompleted?.Invoke(this);
        }

        /// <summary>
        /// Mark request as failed with error message
        /// </summary>
        public void Fail(string errorMessage)
        {
            Status = PathfindingRequestStatus.Failed;
            ErrorMessage = errorMessage;
            
            OnFailed?.Invoke(this);
        }

        /// <summary>
        /// Mark request as timed out
        /// </summary>
        public void Timeout()
        {
            Status = PathfindingRequestStatus.TimedOut;
            ErrorMessage = "Pathfinding request timed out";
            
            OnTimeout?.Invoke(this);
        }

        /// <summary>
        /// Cancel the request
        /// </summary>
        public void Cancel()
        {
            Status = PathfindingRequestStatus.Cancelled;
        }

        public override string ToString()
        {
            string requestorName = Requestor != null ? Requestor.name : "Unknown";
            return $"PathfindingRequest({RequestId[..8]}) - {requestorName}: {Status} - Age: {Age:F2}s";
        }
    }

    /// <summary>
    /// Manager for pathfinding requests with queue and priority support
    /// </summary>
    public class PathfindingRequestManager : MonoBehaviour
    {
        [Header("Request Management")]
        [SerializeField] private int maxConcurrentRequests = 4;
        [SerializeField] private float processingTimeSlice = 0.005f; // 5ms per frame
        [SerializeField] private bool logRequestStatistics = false;

        private GridSystem3D gridSystem;
        private GridConfiguration config;
        private AStarPathfinder pathfinder;
        
        private Queue<PathfindingRequest> pendingRequests;
        private List<PathfindingRequest> processingRequests;
        private Dictionary<string, PathfindingRequest> allRequests;
        
        private bool isProcessing = false;
        private Coroutine processingCoroutine;

        // Statistics
        public int PendingRequestCount => pendingRequests.Count;
        public int ProcessingRequestCount => processingRequests.Count;
        public int TotalRequestCount => allRequests.Count;

        // Events
        public Action<PathfindingRequest> OnRequestCompleted;
        public Action<PathfindingRequest> OnRequestFailed;
        public Action<int> OnQueueSizeChanged;

        private void Awake()
        {
            pendingRequests = new Queue<PathfindingRequest>();
            processingRequests = new List<PathfindingRequest>();
            allRequests = new Dictionary<string, PathfindingRequest>();
        }

        /// <summary>
        /// Initialize the request manager with grid system and configuration
        /// </summary>
        public void Initialize(GridSystem3D gridSystem, GridConfiguration config)
        {
            this.gridSystem = gridSystem;
            this.config = config;
            
            if (config != null)
            {
                maxConcurrentRequests = config.MaxConcurrentPathfindingRequests;
                processingTimeSlice = config.PathfindingTimeSlice;
                logRequestStatistics = config.LogPathfindingStatistics;
            }
            
            pathfinder = new AStarPathfinder(gridSystem, config);
            
            StartProcessing();
        }

        /// <summary>
        /// Submit a pathfinding request
        /// </summary>
        public string SubmitRequest(PathfindingRequest request)
        {
            if (request == null) return null;

            // Add to tracking collections
            allRequests[request.RequestId] = request;
            
            // Add to priority queue
            EnqueueByPriority(request);
            
            OnQueueSizeChanged?.Invoke(pendingRequests.Count);
            
            if (logRequestStatistics)
            {
                Debug.Log($"Pathfinding request submitted: {request}");
            }
            
            return request.RequestId;
        }

        /// <summary>
        /// Submit a simple pathfinding request
        /// </summary>
        public string SubmitRequest(
            Vector3 start, 
            Vector3 target, 
            Action<List<GridNode>> onCompleted = null,
            PathfindingProfile profile = null,
            PathfindingPriority priority = PathfindingPriority.Normal,
            UnityEngine.Object requestor = null)
        {
            var request = new PathfindingRequest(start, target, profile, priority, 5.0f, requestor);
            
            if (onCompleted != null)
            {
                request.OnCompleted = (req) => onCompleted(req.ResultPath);
            }
            
            return SubmitRequest(request);
        }

        /// <summary>
        /// Cancel a pathfinding request
        /// </summary>
        public bool CancelRequest(string requestId)
        {
            if (allRequests.TryGetValue(requestId, out PathfindingRequest request))
            {
                request.Cancel();
                
                // Remove from processing if currently processing
                processingRequests.Remove(request);
                
                // Remove from tracking
                allRequests.Remove(requestId);
                
                return true;
            }
            
            return false;
        }

        /// <summary>
        /// Get request status
        /// </summary>
        public PathfindingRequestStatus GetRequestStatus(string requestId)
        {
            if (allRequests.TryGetValue(requestId, out PathfindingRequest request))
            {
                return request.Status;
            }
            
            return PathfindingRequestStatus.NotFound;
        }

        /// <summary>
        /// Get request results if completed
        /// </summary>
        public List<GridNode> GetRequestResults(string requestId)
        {
            if (allRequests.TryGetValue(requestId, out PathfindingRequest request))
            {
                return request.ResultPath;
            }
            
            return new List<GridNode>();
        }

        /// <summary>
        /// Start processing requests
        /// </summary>
        private void StartProcessing()
        {
            if (!isProcessing)
            {
                isProcessing = true;
                processingCoroutine = StartCoroutine(ProcessRequestsCoroutine());
            }
        }

        /// <summary>
        /// Stop processing requests
        /// </summary>
        private void StopProcessing()
        {
            if (isProcessing)
            {
                isProcessing = false;
                if (processingCoroutine != null)
                {
                    StopCoroutine(processingCoroutine);
                    processingCoroutine = null;
                }
            }
        }

        /// <summary>
        /// Main processing coroutine
        /// </summary>
        private IEnumerator ProcessRequestsCoroutine()
        {
            while (isProcessing)
            {
                float frameStartTime = Time.realtimeSinceStartup;
                
                // Clean up timed out and completed requests
                CleanupRequests();
                
                // Start new requests if we have capacity
                while (processingRequests.Count < maxConcurrentRequests && pendingRequests.Count > 0)
                {
                    PathfindingRequest request = pendingRequests.Dequeue();
                    if (request.Status == PathfindingRequestStatus.Pending)
                    {
                        StartProcessingRequest(request);
                    }
                    OnQueueSizeChanged?.Invoke(pendingRequests.Count);
                }
                
                // Continue processing existing requests
                for (int i = processingRequests.Count - 1; i >= 0; i--)
                {
                    if (Time.realtimeSinceStartup - frameStartTime > processingTimeSlice)
                        break;
                        
                    ProcessRequestStep(processingRequests[i]);
                }
                
                yield return null;
            }
        }

        /// <summary>
        /// Enqueue request based on priority
        /// </summary>
        private void EnqueueByPriority(PathfindingRequest request)
        {
            // Simple priority implementation - could be enhanced with a proper priority queue
            if (request.Priority == PathfindingPriority.Urgent)
            {
                // Add urgent requests to a temporary list and reorder
                var tempList = new List<PathfindingRequest>();
                tempList.Add(request);
                
                while (pendingRequests.Count > 0)
                {
                    tempList.Add(pendingRequests.Dequeue());
                }
                
                foreach (var req in tempList)
                {
                    pendingRequests.Enqueue(req);
                }
            }
            else
            {
                pendingRequests.Enqueue(request);
            }
        }

        /// <summary>
        /// Start processing a specific request
        /// </summary>
        private void StartProcessingRequest(PathfindingRequest request)
        {
            request.Status = PathfindingRequestStatus.Processing;
            processingRequests.Add(request);
            
            if (logRequestStatistics)
            {
                Debug.Log($"Started processing: {request}");
            }
        }

        /// <summary>
        /// Process a single step of a request
        /// </summary>
        private void ProcessRequestStep(PathfindingRequest request)
        {
            try
            {
                // Find path using A* pathfinder
                List<GridNode> path = pathfinder.FindPath(
                    request.StartWorldPosition,
                    request.TargetWorldPosition,
                    request.Profile
                );
                
                // Complete the request
                request.Complete(path, pathfinder.LastStats);
                CompleteRequest(request);
            }
            catch (System.Exception ex)
            {
                request.Fail($"Pathfinding error: {ex.Message}");
                CompleteRequest(request);
            }
        }

        /// <summary>
        /// Complete a request and remove from processing
        /// </summary>
        private void CompleteRequest(PathfindingRequest request)
        {
            processingRequests.Remove(request);
            
            if (request.Status == PathfindingRequestStatus.Completed)
            {
                OnRequestCompleted?.Invoke(request);
            }
            else
            {
                OnRequestFailed?.Invoke(request);
            }
            
            if (logRequestStatistics)
            {
                Debug.Log($"Request completed: {request} - Path length: {request.ResultPath.Count}");
            }
        }

        /// <summary>
        /// Clean up timed out and completed requests
        /// </summary>
        private void CleanupRequests()
        {
            var requestsToRemove = new List<string>();
            
            foreach (var kvp in allRequests)
            {
                var request = kvp.Value;
                
                // Check for timeout
                if (request.HasTimedOut && request.Status == PathfindingRequestStatus.Processing)
                {
                    request.Timeout();
                    processingRequests.Remove(request);
                    OnRequestFailed?.Invoke(request);
                }
                
                // Mark completed requests for removal after a delay
                if (request.Status != PathfindingRequestStatus.Pending && 
                    request.Status != PathfindingRequestStatus.Processing && 
                    request.Age > 30f) // Keep results for 30 seconds
                {
                    requestsToRemove.Add(kvp.Key);
                }
            }
            
            // Remove old requests
            foreach (string requestId in requestsToRemove)
            {
                allRequests.Remove(requestId);
            }
        }

        /// <summary>
        /// Get current statistics
        /// </summary>
        public PathfindingManagerStatistics GetStatistics()
        {
            return new PathfindingManagerStatistics
            {
                PendingRequests = pendingRequests.Count,
                ProcessingRequests = processingRequests.Count,
                TotalRequests = allRequests.Count,
                MaxConcurrentRequests = maxConcurrentRequests,
                ProcessingTimeSlice = processingTimeSlice
            };
        }

        private void OnDestroy()
        {
            StopProcessing();
        }

        private void OnDisable()
        {
            StopProcessing();
        }

        private void OnEnable()
        {
            if (gridSystem != null && !isProcessing)
            {
                StartProcessing();
            }
        }
    }

    /// <summary>
    /// Pathfinding request priority levels
    /// </summary>
    public enum PathfindingPriority
    {
        Low = 0,
        Normal = 1,
        High = 2,
        Urgent = 3
    }

    /// <summary>
    /// Pathfinding request status
    /// </summary>
    public enum PathfindingRequestStatus
    {
        Pending,
        Processing,
        Completed,
        Failed,
        TimedOut,
        Cancelled,
        NotFound
    }

    /// <summary>
    /// Statistics for pathfinding request manager
    /// </summary>
    public struct PathfindingManagerStatistics
    {
        public int PendingRequests;
        public int ProcessingRequests;
        public int TotalRequests;
        public int MaxConcurrentRequests;
        public float ProcessingTimeSlice;
    }
}
