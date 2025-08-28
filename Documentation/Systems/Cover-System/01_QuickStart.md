# Quick Start Guide - 5 Minutes to Working Cover

## 🎯 Goal
Get a working cover point in your level in 5 minutes or less.

## ⚡ Prerequisites
- Unreal Editor is open
- ProjectHive project is loaded
- You can see the Content Browser

## 🚀 Step 1: Verify System (1 minute)

### Check Components Are Available
1. Create a new Blueprint: **Content Browser → Create → Blueprint Class → Actor**
2. Name it `BP_QuickTest`
3. Click **Add Component** → Search for **"Cover Detection"**
4. ✅ **Success**: You see "Cover Detection Component" in the list
5. ❌ **Problem**: Component missing → See [Troubleshooting](Examples/Troubleshooting.md)

## 🏗️ Step 2: Create Basic Cover Point (2 minutes)

### Add Components
1. **Add Cover Detection Component**
   - Click "Cover Detection Component" to add it

2. **Add Visual Mesh**
   - Add Component → Static Mesh Component
   - Set Static Mesh to `/Engine/BasicShapes/Cube` 
   - Set Scale to `(2, 0.5, 2)` (wall-like shape)

3. **Add Position Marker**
   - Add Component → Scene Component
   - Rename to "Unit Position Marker"
   - Set Location to `(0, 100, 0)` (in front of the wall)

### Configure Cover Properties
1. **Select Cover Detection Component**
2. **Set these values:**
   - Cover Type: `Heavy`
   - Cover Material: `Stone`
   - Max Health: `100`
   - Max Occupants: `2`
   - Can Be Destroyed: `✓ Checked`

3. **Configure Visual Properties (Optional)**
   **Select the CoverPoint Actor** for additional customization:
   - **Cover Visuals:**
     - Use Modular Visuals: `✓ Checked` (recommended)
     - Cover Mesh Scale: `(1.0, 1.0, 1.0)` (adjust as needed)
     - Ground Marker Scale: `(0.8, 0.8, 0.02)` (flat ground indicator)
   - **Visual Effects:**
     - Emissive Color Multiplier: `0.5` (glow intensity)
     - Hover Scale Multiplier: `1.2` (grow on hover)
     - Available Color: `Green`
     - Occupied Color: `Red`

4. **Compile and Save**

## 🎮 Step 3: Test in Level (2 minutes)

### Place in World
1. **Create test level** or use existing level
2. **Drag your BP_QuickTest** into the level
3. **Position it** somewhere accessible
4. **Press Play**

### Quick Test
1. **Open Console** (` ~ ` key)
2. **Type:** `log LogTemp Verbose`
3. **Look for messages** like:
   - Component initialization
   - Health values
   - Cover state updates

✅ **Success**: You see cover-related log messages  
❌ **Problem**: No messages → Check [Testing Guide](04_Testing.md)

## 🔧 Step 4: Add Basic Testing (Optional - 2 minutes)

### Add Test Function
1. **Open your Blueprint**
2. **Go to Event Graph**
3. **Add Event BeginPlay**
4. **Add these nodes:**

```blueprint
Event BeginPlay
→ Get Cover Detection Component
→ Get Cover Debug Info Native
→ Print String (to screen)
```

### Test Damage Function
1. **Create Custom Event** named "Test Damage"
2. **Add nodes:**

```blueprint
Test Damage
→ Get Cover Detection Component  
→ Apply Cover Damage Native (Damage: 25, Weapon: "Test", Direction: (1,0,0))
→ Get Cover Debug Info Native
→ Print String
```

3. **Bind to keyboard:** Input Action "1" → Test Damage

## ✅ Success Criteria

After 5 minutes, you should have:
- ✅ Cover Detection Component in your Blueprint
- ✅ Visual representation of cover point
- ✅ Component initializes without errors
- ✅ Debug info displays current state
- ✅ Basic damage testing works

## 🚨 Common Quick Issues

### Component Doesn't Appear
```console
// Try in Editor console:
bp.refresh.all
```

### No Debug Output
```console
// Enable logging:
log LogTemp Verbose
log LogCover Verbose
```

### Blueprint Compile Errors
- Check all component references are valid
- Verify function calls match available functions
- Try restarting Unreal Editor

## 🎉 You Did It!

**Congratulations!** You now have a working cover point. 

### Next Steps:
- 📖 **Learn More**: Read [Detailed Guide](02_DetailedGuide.md)
- 🎨 **Create Better Visuals**: Check [Blueprint Guide](03_BlueprintGuide.md)  
- 🔧 **Add More Features**: Explore [Advanced Guide](05_Advanced.md)
- 🐛 **Having Issues**: See [Troubleshooting](Examples/Troubleshooting.md)

### Quick Validation Checklist:
- [ ] Component appears in Blueprint
- [ ] Level loads without errors
- [ ] Press Play works
- [ ] Console commands work
- [ ] Debug info shows cover state

**Time Spent: ⏱️ ~5 minutes**  
**Status: 🎯 Ready for next steps!**
