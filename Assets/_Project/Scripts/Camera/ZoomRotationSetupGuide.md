# Zoom Rotation Setup Guide

This guide will help you fix the zoom rotation issue by choosing the best approach for your Cinemachine setup.

## 🔍 Step 1: Diagnose the Issue

1. **Add the CinemachineRotationAnalyzer** to any GameObject in your scene
2. **Assign your Virtual Camera** to the "Virtual Camera" field
3. **Run the analysis** by playing the scene or using the context menu "Analyze Cinemachine Configuration"
4. **Check the Console** for analysis results

The analyzer will tell you if any Cinemachine components are overriding your manual rotation changes.

## 🛠️ Step 2: Choose Your Solution

### Option A: Fix Existing Approach (if no conflicts found)

If the analyzer shows no problematic components:

1. **Use the enhanced CameraZoomControl** (already has debug logging)
2. **Check the Console** when zooming to see detailed rotation logs
3. **Verify settings** are properly loaded and `enableZoomRotation` is true

### Option B: Use Cinemachine-Integrated Approach (recommended)

If the analyzer found conflicting components, or if Option A doesn't work:

1. **Add CinemachineZoomRotation** component to your TacticalCameraController GameObject
2. **Remove or disable** the rotation part of CameraZoomControl 
3. **Let CinemachineZoomRotation handle** zoom rotation through Cinemachine's PanTilt system

## 🎯 Step 3: Setup Instructions

### For Option B (Cinemachine-Integrated):

1. **Add Components:**
   ```
   TacticalCameraController GameObject:
   - TacticalCameraController
   - CameraZoomControl (keep for zoom, disable rotation)  
   - CinemachineZoomRotation (new - handles rotation)
   ```

2. **Virtual Camera Setup:**
   ```
   Virtual Camera GameObject:
   - CinemachineCamera
   - CinemachinePositionComposer (for distance)
   - CinemachinePanTilt (for rotation - added automatically)
   ```

3. **Disable Conflicting Components** (if any):
   - CinemachineComposer
   - CinemachineOrbitalFollow  
   - Any other rotation-controlling components

## 🧪 Step 4: Test

1. **Play the scene**
2. **Use mouse wheel** to zoom in/out
3. **Watch Console** for debug logs showing rotation changes
4. **Verify rotation behavior:**
   - Zoom out → Camera tilts up (more strategic/top-down view)
   - Zoom in → Camera tilts down (more angled/tactical view)

## ⚙️ Step 5: Fine-tune Settings

Adjust these values in your CameraSettings asset:

- `enableZoomRotation`: true/false
- `minPitchAngle`: 45° (close-up tactical angle)
- `maxPitchAngle`: 80° (strategic top-down angle)  
- `pitchSmoothTime`: 0.3s (rotation speed)

## 🐛 Troubleshooting

### Still No Rotation?

1. **Check Console logs** - are rotation values being calculated?
2. **Verify CameraSettings** - is `enableZoomRotation` true?
3. **Run CinemachineRotationAnalyzer** again to check for conflicts
4. **Try Option B** if using Option A

### Rotation Too Fast/Slow?

- Adjust `pitchSmoothTime` in CameraSettings (lower = faster)

### Wrong Rotation Direction?

- Check if angles are inverted in your settings
- Verify `minPitchAngle` < `maxPitchAngle`

## 📝 Notes

- **CinemachineZoomRotation** is the more reliable approach as it works WITH Cinemachine
- **CameraZoomControl** can still handle the zoom distance changes
- **Both approaches** use the same CameraSettings for consistency
- **Debug logging** helps identify exactly where the issue occurs
