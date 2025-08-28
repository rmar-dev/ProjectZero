using UnityEngine;

namespace ProjectZero.SimpleGrid
{
    /// <summary>
    /// ScriptableObject that defines pathfinding behavior for different unit types
    /// Allows for customized movement capabilities and preferences per unit role
    /// </summary>
    [CreateAssetMenu(fileName = "PathfindingProfile", menuName = "ProjectZero/Grid/Pathfinding Profile", order = 2)]
    public class PathfindingProfile : ScriptableObject
    {
        [Header("Movement Capabilities")]
        [SerializeField] private float _maxClimbHeight = 2.0f;
        [SerializeField] private bool _allowDiagonalMovement = true;
        [SerializeField] private bool _allowVerticalMovement = true;
        [SerializeField] private float _jumpDistance = 1.5f;
        [SerializeField] private TerrainType _allowedTerrainTypes = TerrainType.All;

        [Header("Movement Preferences")]
        [SerializeField] private bool _preferCover = false;
        [SerializeField] private bool _avoidHazards = true;
        [SerializeField] private bool _preferShorterPaths = true;
        [SerializeField] private float _coverSeekingWeight = 1.5f;
        [SerializeField] private float _hazardAvoidanceWeight = 3.0f;

        [Header("Terrain Cost Multipliers")]
        [SerializeField] private float _normalTerrainMultiplier = 1.0f;
        [SerializeField] private float _roughTerrainMultiplier = 1.5f;
        [SerializeField] private float _hazardousTerrainMultiplier = 5.0f;
        [SerializeField] private float _climbableTerrainMultiplier = 2.0f;
        [SerializeField] private float _waterTerrainMultiplier = 2.5f;
        [SerializeField] private float _indoorTerrainMultiplier = 1.0f;
        [SerializeField] private float _outdoorTerrainMultiplier = 1.0f;

        [Header("Unit Role Settings")]
        [SerializeField] private UnitRole _unitRole = UnitRole.Generic;
        [SerializeField] private string _profileDescription = "Default pathfinding profile";

        // Properties for easy access
        public float MaxClimbHeight => _maxClimbHeight;
        public bool AllowDiagonalMovement => _allowDiagonalMovement;
        public bool AllowVerticalMovement => _allowVerticalMovement;
        public float JumpDistance => _jumpDistance;
        public TerrainType AllowedTerrainTypes => _allowedTerrainTypes;
        
        public bool PreferCover => _preferCover;
        public bool AvoidHazards => _avoidHazards;
        public bool PreferShorterPaths => _preferShorterPaths;
        public float CoverSeekingWeight => _coverSeekingWeight;
        public float HazardAvoidanceWeight => _hazardAvoidanceWeight;
        
        public UnitRole UnitRole => _unitRole;
        public string ProfileDescription => _profileDescription;

        /// <summary>
        /// Get movement cost multiplier for a specific terrain type
        /// </summary>
        public float GetMovementMultiplier(TerrainType terrainType)
        {
            switch (terrainType)
            {
                case TerrainType.Normal:
                    return _normalTerrainMultiplier;
                case TerrainType.Rough:
                    return _roughTerrainMultiplier;
                case TerrainType.Hazardous:
                    return _avoidHazards ? _hazardousTerrainMultiplier * _hazardAvoidanceWeight : _hazardousTerrainMultiplier;
                case TerrainType.Climbable:
                    return _climbableTerrainMultiplier;
                case TerrainType.Water:
                    return _waterTerrainMultiplier;
                case TerrainType.Indoor:
                    return _indoorTerrainMultiplier;
                case TerrainType.Outdoor:
                    return _outdoorTerrainMultiplier;
                default:
                    return 1.0f;
            }
        }

        /// <summary>
        /// Get heuristic modifier based on node properties and preferences
        /// </summary>
        public float GetHeuristicModifier(GridNode node, GridNode targetNode)
        {
            float modifier = 1.0f;
            
            // Prefer cover if enabled
            if (_preferCover && node.ProvidesCover)
            {
                modifier *= (1.0f / _coverSeekingWeight); // Lower heuristic for cover nodes
            }
            
            // Avoid hazardous terrain in heuristic
            if (_avoidHazards && node.HasTerrainType(TerrainType.Hazardous))
            {
                modifier *= _hazardAvoidanceWeight; // Higher heuristic for hazardous nodes
            }
            
            return modifier;
        }

        /// <summary>
        /// Check if this profile can traverse the given terrain type
        /// </summary>
        public bool CanTraverse(TerrainType terrainType)
        {
            return (_allowedTerrainTypes & terrainType) != 0;
        }

        /// <summary>
        /// Check if this profile can climb to the given height
        /// </summary>
        public bool CanClimb(float height)
        {
            return _allowVerticalMovement && height <= _maxClimbHeight;
        }

        /// <summary>
        /// Check if this profile can jump the given distance
        /// </summary>
        public bool CanJump(float distance)
        {
            return distance <= _jumpDistance;
        }

        /// <summary>
        /// Get path smoothing preferences
        /// </summary>
        public PathSmoothingSettings GetSmoothingSettings()
        {
            return new PathSmoothingSettings
            {
                EnableSmoothing = _preferShorterPaths,
                MaxSmoothingIterations = 3,
                LineOfSightChecks = true,
                CornerCutting = _allowDiagonalMovement
            };
        }

        /// <summary>
        /// Create a default pathfinding profile
        /// </summary>
        public static PathfindingProfile CreateDefault()
        {
            var profile = ScriptableObject.CreateInstance<PathfindingProfile>();
            profile._maxClimbHeight = 2.0f;
            profile._allowDiagonalMovement = true;
            profile._allowVerticalMovement = true;
            profile._jumpDistance = 1.5f;
            profile._allowedTerrainTypes = TerrainType.All;
            
            profile._preferCover = false;
            profile._avoidHazards = true;
            profile._preferShorterPaths = true;
            profile._coverSeekingWeight = 1.5f;
            profile._hazardAvoidanceWeight = 3.0f;
            
            profile._normalTerrainMultiplier = 1.0f;
            profile._roughTerrainMultiplier = 1.5f;
            profile._hazardousTerrainMultiplier = 5.0f;
            profile._climbableTerrainMultiplier = 2.0f;
            profile._waterTerrainMultiplier = 2.5f;
            profile._indoorTerrainMultiplier = 1.0f;
            profile._outdoorTerrainMultiplier = 1.0f;
            
            profile._unitRole = UnitRole.Generic;
            profile._profileDescription = "Default pathfinding profile";
            
            return profile;
        }

        /// <summary>
        /// Create specialized profiles for different unit roles
        /// </summary>
        public static PathfindingProfile CreateForRole(UnitRole role)
        {
            var profile = CreateDefault();
            profile._unitRole = role;
            
            switch (role)
            {
                case UnitRole.Scout:
                    profile._maxClimbHeight = 3.0f;
                    profile._allowDiagonalMovement = true;
                    profile._allowVerticalMovement = true;
                    profile._jumpDistance = 2.0f;
                    profile._preferShorterPaths = true;
                    profile._roughTerrainMultiplier = 1.2f;
                    profile._profileDescription = "Scout - High mobility, prefers direct routes";
                    break;
                    
                case UnitRole.Heavy:
                    profile._maxClimbHeight = 1.0f;
                    profile._allowDiagonalMovement = true;
                    profile._allowVerticalMovement = false;
                    profile._jumpDistance = 0.5f;
                    profile._preferCover = true;
                    profile._coverSeekingWeight = 2.0f;
                    profile._roughTerrainMultiplier = 2.0f;
                    profile._climbableTerrainMultiplier = 4.0f;
                    profile._profileDescription = "Heavy - Limited mobility, prefers cover";
                    break;
                    
                case UnitRole.Sniper:
                    profile._maxClimbHeight = 2.5f;
                    profile._allowDiagonalMovement = true;
                    profile._allowVerticalMovement = true;
                    profile._jumpDistance = 1.0f;
                    profile._preferCover = true;
                    profile._coverSeekingWeight = 1.2f;
                    profile._indoorTerrainMultiplier = 1.5f; // Snipers prefer outdoor positions
                    profile._profileDescription = "Sniper - Prefers elevated positions with cover";
                    break;
                    
                case UnitRole.Medic:
                    profile._maxClimbHeight = 2.0f;
                    profile._allowDiagonalMovement = true;
                    profile._allowVerticalMovement = true;
                    profile._jumpDistance = 1.5f;
                    profile._avoidHazards = true;
                    profile._hazardAvoidanceWeight = 4.0f;
                    profile._hazardousTerrainMultiplier = 10.0f;
                    profile._profileDescription = "Medic - Avoids dangerous terrain";
                    break;
                    
                case UnitRole.Engineer:
                    profile._maxClimbHeight = 2.0f;
                    profile._allowDiagonalMovement = true;
                    profile._allowVerticalMovement = true;
                    profile._jumpDistance = 1.0f;
                    profile._climbableTerrainMultiplier = 1.5f; // Engineers are good with technical terrain
                    profile._indoorTerrainMultiplier = 0.8f; // Prefers indoor/technical environments
                    profile._profileDescription = "Engineer - Comfortable with technical terrain";
                    break;
            }
            
            return profile;
        }

        /// <summary>
        /// Validate profile settings
        /// </summary>
        private void OnValidate()
        {
            _maxClimbHeight = Mathf.Max(0f, _maxClimbHeight);
            _jumpDistance = Mathf.Max(0f, _jumpDistance);
            _coverSeekingWeight = Mathf.Max(0.1f, _coverSeekingWeight);
            _hazardAvoidanceWeight = Mathf.Max(1.0f, _hazardAvoidanceWeight);
            
            _normalTerrainMultiplier = Mathf.Max(0.1f, _normalTerrainMultiplier);
            _roughTerrainMultiplier = Mathf.Max(0.1f, _roughTerrainMultiplier);
            _hazardousTerrainMultiplier = Mathf.Max(1.0f, _hazardousTerrainMultiplier);
            _climbableTerrainMultiplier = Mathf.Max(0.1f, _climbableTerrainMultiplier);
            _waterTerrainMultiplier = Mathf.Max(0.1f, _waterTerrainMultiplier);
            _indoorTerrainMultiplier = Mathf.Max(0.1f, _indoorTerrainMultiplier);
            _outdoorTerrainMultiplier = Mathf.Max(0.1f, _outdoorTerrainMultiplier);
        }
    }

    /// <summary>
    /// Unit roles for specialized pathfinding behavior
    /// </summary>
    public enum UnitRole
    {
        Generic,    // Default balanced movement
        Scout,      // Fast, agile, prefers direct routes
        Heavy,      // Slow, limited mobility, prefers cover
        Sniper,     // Prefers elevated positions and cover
        Medic,      // Avoids dangerous areas
        Engineer    // Comfortable with technical terrain
    }

    /// <summary>
    /// Settings for path smoothing algorithms
    /// </summary>
    public struct PathSmoothingSettings
    {
        public bool EnableSmoothing;
        public int MaxSmoothingIterations;
        public bool LineOfSightChecks;
        public bool CornerCutting;
    }
}
