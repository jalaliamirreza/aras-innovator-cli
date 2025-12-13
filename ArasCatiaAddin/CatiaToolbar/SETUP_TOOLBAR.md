# Aras Integration Toolbar Setup for CATIA V5

## Step 1: Copy Macro File

Copy `ArasIntegration.catvbs` to your CATIA macros folder:
```
C:\Users\<YourUsername>\AppData\Roaming\DassaultSystemes\CATEnv\Macros\
```

Or any folder you prefer.

## Step 2: Add Macro Library

1. Open CATIA V5
2. Go to **Tools > Macro > Macros...**
3. Click **"Macro libraries..."** button
4. In the **"Library type"** dropdown, select **"Directories"**
5. Click **"Add existing library..."**
6. Navigate to the folder containing `ArasIntegration.catvbs`
7. Click **OK** to close

## Step 3: Create the Toolbar

1. Go to **Tools > Customize...**
2. Click the **"Toolbars"** tab
3. Click **"New..."** button
4. Enter name: **"Aras Integration"**
5. Click **OK**

## Step 4: Add Buttons to Toolbar

1. In the Customize dialog, click the **"Commands"** tab
2. In the **"Category"** list, select **"Macros"**
3. You should see the macros listed:
   - `ArasLogin`
   - `ArasCheckIn`
   - `ArasCheckOut`
   - `ArasGetLatest`
   - `ArasSearch`
   - `ArasBOMSync`
   - `ArasSettings`

4. **Drag each macro** to the "Aras Integration" toolbar

## Step 5: Customize Button Icons (Optional)

1. Right-click on a button in the toolbar
2. Select **"Properties..."**
3. Change the **"Icon"** by clicking "Other..."
4. Select a custom icon (BMP file, 16x16 or 32x32 pixels)
5. Change the **"Title"** if desired

## Step 6: Position the Toolbar

1. Drag the toolbar to your preferred location
2. Go to **Tools > Customize... > Options** tab
3. Check **"Lock toolbar positions"** to prevent accidental moving

## Recommended Button Layout

| Button | Tooltip | Description |
|--------|---------|-------------|
| Login | Login to Aras | Connect to Aras server |
| Check In | Check In | Upload current document |
| Check Out | Check Out | Download with lock |
| Get Latest | Get Latest | Download without lock |
| Search | Search | Search Aras documents |
| BOM Sync | BOM Sync | Sync assembly BOM |
| Settings | Settings | Configure add-in |

## Troubleshooting

### Macros not showing in Commands list
- Make sure the macro library was added correctly
- Check that the .catvbs file has no syntax errors
- Restart CATIA

### Toolbar disappeared
- Go to **Tools > Customize... > Toolbars** tab
- Find "Aras Integration" and check the checkbox
- Click **"Restore position"** if needed

### Settings not saved
- Make sure you have write access to CATSettings folder
- Run CATIA with normal (not elevated) permissions

## Export Settings for Deployment

To deploy this toolbar to other users:

1. Set up the toolbar on one machine
2. Copy the CATSettings files from:
   ```
   C:\Users\<Username>\AppData\Roaming\DassaultSystemes\CATSettings\
   ```
3. Relevant files:
   - `ToolBar*.CATSettings` - Toolbar configurations
   - `DlgBox*.CATSettings` - Dialog positions

Or use CATBatGenXMLSet to export/import settings.
