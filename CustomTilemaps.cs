using MelonLoader;

using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.IO;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem;
using System;
using UnityEngine.Networking;

namespace CustomTilemaps
{
    public static class BuildInfo
    {
        public const string Name = "Custom Tilemaps"; // Name of the Mod.  (MUST BE SET)
        public const string Description = "Enable Saving and Loading of Custom Tilemaps in Starbuster"; // Description for the Mod.  (Set as null if none)
        public const string Author = "Catssandra Ann"; // Author of the Mod.  (MUST BE SET)
        public const string Company = null; // Company that made the Mod.  (Set as null if none)
        public const string Version = "1.0.0"; // Version of the Mod.  (MUST BE SET)
        public const string DownloadLink = null; // Download Link for the Mod.  (Set as null if none)
    }

    public class CustomTilemaps : MelonMod
    {
        public static MelonPreferences_Category starbusterCustomTilemaps;
        public static MelonPreferences_Entry<bool> autoLoadTileset;
        public static MelonPreferences_Entry<string> inputShowCustomTilemapMenu;
        public static MelonPreferences_Entry<string> inputCTMQuickSave;
        public static MelonPreferences_Entry<string> inputCTMQuickLoad;

        public static string currentSceneName = "";
        public static string previousSceneName = "";
        public static bool carnivalIsLoaded = false;
        bool carnivalApplied = false;

        InputHandler key;

        public static Texture2D carnivalTexture = null;
        Sprite carnivalSprite = null;
        //Dictionary<string, Sprite> carnivalSprites = new Dictionary<string, Sprite>();
        Dictionary<string, Tile> carnivalTiles = new Dictionary<string, Tile>();

        Tilemap replaceMySprite = null;

        public override void OnApplicationStart() // Runs after Game Initialization.
        {
            MelonLogger.Msg("OnApplicationStart");
            MelonPreferences.Load();

            GameObject goLoadImage = new UnityEngine.GameObject("goLoadImage");
            GameObject.DontDestroyOnLoad(goLoadImage);
            goLoadImage.AddComponent<CustomTilemapsLoadImages>();

            starbusterCustomTilemaps = MelonPreferences.CreateCategory("starbusterCustomTilemaps");
            autoLoadTileset = (MelonPreferences_Entry<bool>) starbusterCustomTilemaps.CreateEntry<bool>("autoLoadTileset", true);
            inputShowCustomTilemapMenu = (MelonPreferences_Entry<string>)starbusterCustomTilemaps.CreateEntry<string>("inputShowCustomTilemapMenu"
                , "<Gamepad>/view");
            inputCTMQuickSave = (MelonPreferences_Entry<string>)starbusterCustomTilemaps.CreateEntry<string>("inputCTMQuickSave"
                , "<Gamepad>/buttonEast");
            inputCTMQuickLoad = (MelonPreferences_Entry<string>)starbusterCustomTilemaps.CreateEntry<string>("inputCTMQuickLoad"
                , "<Gamepad>/buttonNorth");

            UnityWebRequestTexture.GetTexture("file://" + @"D:\Games\SAGE 2021\Starbuster Demo 2021 V1.07\union.png");
        }

        public override void OnSceneWasLoaded(int buildindex, string sceneName) // Runs when a Scene has Loaded and is passed the Scene's Build Index and Name.
        {
            MelonLogger.Msg("OnSceneWasLoaded: " + buildindex.ToString() + " | " + sceneName);
            MelonPreferences.Save();
            if (LevelManager.currentLevel != null) 
            {
                key = LevelManager.currentLevel.GetComponent<InputHandler>();
            }
        }

        public override void OnApplicationQuit() // Runs when the Game is told to Close.
        {
            MelonLogger.Msg("OnApplicationQuit");
            MelonPreferences.Save();
        }

        public override void OnPreferencesSaved() // Runs when Melon Preferences get saved.
        {
            MelonLogger.Msg("OnPreferencesSaved");
        }

        public override void OnPreferencesLoaded() // Runs when Melon Preferences get loaded.
        {
            MelonLogger.Msg("OnPreferencesLoaded");
        }

        //-------------------------------------------------

        public override void OnUpdate() // Runs once per frame.
        {
            previousSceneName = currentSceneName;
            currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            if (previousSceneName != currentSceneName)
            {
                MelonLogger.Msg("Scene changed to: " + currentSceneName + "; Attempting to dump tilemap data or replace sprites.");
                try
                {
                    if (autoLoadTileset.Value)
                    {
                        //ScrapeAndSaveCurrentTilemapData();
                        //FindAndReplaceUnionTilemap();
                    }
                }
                catch (System.Exception e)
                {
                    MelonLogger.Msg("There was an error when attempting to use FindTilemap somewhere:\r\n");
                    MelonLogger.Msg(e.ToString());
                }


                carnivalApplied = false;
            }
            if (carnivalIsLoaded && !carnivalApplied)
            {
                // Use the heirarchy object path finder to grab the correct tileset and assign the new sprite.
                carnivalApplied = true;
            }

            HandleCustomTilemapInputs();
        }

        public void HandleCustomTilemapInputs()
        {
            if (GetKeyPressed(inputShowCustomTilemapMenu.Value)) 
            {
                if (GetKeyDown(inputCTMQuickSave.Value))
                {
                    MelonLogger.Msg("Show Custom Tilemap Menu: Quick Save");

                    MelonLogger.Msg("Attempting to save tilemap data...");
                    ScrapeAndSaveCurrentTilemapData();
                }
                if (GetKeyDown(inputCTMQuickLoad.Value))
                {
                    MelonLogger.Msg("Show Custom Tilemap Menu: Quick Load");

                    MelonLogger.Msg("Attempting to load tilemap data...");
                    FindAndReplaceUnionTilemap();
                }
            }
        }

        private void FindAndReplaceUnionTilemap()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string tilemapPath = "";
            Tilemap[] tilemaps = GameObject.FindObjectsOfType<Tilemap>();
            if (tilemaps.Length >= 1)
            {
                Tilemap tm = null;
                string tileInfoString = "";
                Sprite currentTileSprite = null;
                int indexZ = 0;
                int indexX = 0;
                int indexY = 0;
                Vector3Int currentCellPosition = Vector3Int.zero;

                HashSet<string> tileNames = new HashSet<string>();

                MelonLogger.Msg("About to start the GIANT MESS OF NESTED LOOPS TO REPLACE TILEMAPS FOR: " + currentSceneName);
                for (int i = 0; i < tilemaps.Length; i++)
                {
                    tm = tilemaps[i];
                    tilemapPath += "\"tilemap:{\"" + i.ToString() + ": " + GetGameObjectPath(tm.transform) + "}";
                    tilemapPath += "\r\n";

                    if (currentSceneName.ToUpper().Contains("UNION"))
                    {
                        MelonLogger.Msg("This tilemap appears to be related to Union. Do the replacement. Except not now, because you didn't add that yet.");
                    }
                    for (indexZ = tm.cellBounds.zMin; indexZ < tm.cellBounds.zMax; indexZ++)
                    {
                        for (indexX = tm.cellBounds.xMin; indexX < tm.cellBounds.xMax; indexX++)
                        {
                            for (indexY = tm.cellBounds.yMin; indexY < tm.cellBounds.yMax; indexY++)
                            {
                                currentCellPosition = new Vector3Int(indexX, indexY, indexZ);
                                TileBase currentTile = tm.GetTile(currentCellPosition);
                                if (currentTile != null)
                                {
                                    currentTileSprite = tm.GetSprite(currentCellPosition);
                                    if (currentTileSprite != null)
                                    {
                                        if (GetGameObjectPath(tm.transform).Equals("Grid/Fore") /*|| GetGameObjectPath(tm.transform).Equals("Grid/Mid")*/)
                                        {
                                            MelonLogger.Msg("Attempting to overwrite Union texture data.");
                                            //currentTileSprite.texture.LoadImage(carnivalTexture.GetRawTextureData());
                                            if (!carnivalTiles.ContainsKey(currentTile.name))
                                            {
                                                SwapTileWithNewCustomTile(tm, currentCellPosition, currentTileSprite, carnivalTexture, currentTile.name);
                                            }
                                            else
                                            {
                                                SwapTileWithLoadedCustomTile(tm, currentCellPosition, carnivalTiles[currentTile.name]);
                                            }
                                        }
                                    }

                                    tileInfoString = currentCellPosition.ToString() + "; " + currentTile.name;
                                    tilemapPath += tileInfoString + "\r\n";

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                tilemapPath = "Couldn't find any tilemaps to replace...";
            }
            MelonLogger.Msg(tilemapPath);
            //SaveStringToFile(tilemapPath, sceneName + "-tilemaps.txt");
        }

        public void SwapTileWithNewCustomTile(Tilemap tilemap, Vector3Int tilePos, Sprite oldSprite, Texture2D newTexture, string newTileName)
        {
            Tile tile = ScriptableObject.CreateInstance<Tile>();
            MelonLogger.Msg("Old Sprite Rectangle: " + oldSprite.textureRect.ToString() + ", RectOffset:" + oldSprite.textureRectOffset.ToString()
                + ", Pivot:" + oldSprite.pivot.ToString());
            //Sprite newSprite = Sprite.Create(newTexture, oldSprite.textureRect, /*oldSprite.pivot*/ new Vector2(0.5f, 0.5f), oldSprite.pixelsPerUnit);
            Vector2 newPivot = (oldSprite.pivot / new Vector2(oldSprite.textureRect.width, oldSprite.textureRect.height));
            Sprite newSprite = Sprite.Create(newTexture, oldSprite.textureRect, newPivot, oldSprite.pixelsPerUnit);
            tile.sprite = newSprite;
            //Matrix4x4 scaleMatrix = Matrix4x4.Scale(new Vector3(10, 10, 1));
            //tile.transform.
            tilemap.SetTile(tilePos, tile);

            carnivalTiles.Add(newTileName, tile);
        }

        public void SwapTileWithLoadedCustomTile(Tilemap tilemap, Vector3Int tilePos, Tile newTile)
        {
            tilemap.SetTile(tilePos, newTile);
        }

        private void Beh()
        {
            // convert texture to sprite if required otherwise load sprite directly from resources folder
            Texture2D myTexture = Resources.Load<Texture2D>("Images/SampleImage");

        }

        /*
    public override void OnInspectorGUI()
    {
        LevelScript myTarget = (LevelScript)target;

        myTarget.experience = EditorGUILayout.IntField("Experience", myTarget.experience);
        EditorGUILayout.LabelField("Level", myTarget.Level.ToString());
    }*/

        static void InsertObject()
        {
            MelonLogger.Msg("Inserting a GameObject");
            // Create a custom game object
            GameObject go = new GameObject("Custom Game Object");
        }

        public void ScrapeAndSaveCurrentTilemapData()
        {
            string sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            string tilemapPath = "";
            Tilemap[] tilemaps = GameObject.FindObjectsOfType<Tilemap>();
            if (tilemaps.Length >= 1)
            {
                Tilemap tm = null;
                string tileInfoString = "";
                Sprite currentTileSprite = null;
                int indexZ = 0;
                int indexX = 0;
                int indexY = 0;
                Vector3Int currentCellPosition = Vector3Int.zero;

                HashSet<string> tileNames = new HashSet<string>();

                MelonLogger.Msg("About to start the GIANT MESS OF NESTED LOOPS FOR: " + currentSceneName);
                for (int i = 0; i < tilemaps.Length; i++)
                {
                    tm = tilemaps[i];
                    tilemapPath += i.ToString() + ": " + GetGameObjectPath(tm.transform);
                    tilemapPath += "\r\n";

                    for (indexZ = 0; indexZ < tm.size.z; indexZ++)
                    {
                        for (indexX = 0; indexX < tm.size.x; indexX++)
                        {
                            for (indexY = 0; indexY < tm.size.y; indexY++)
                            {
                                currentCellPosition = new Vector3Int(indexX, indexY, indexZ);
                                TileBase currentTile = tm.GetTile(currentCellPosition);
                                if (currentTile != null)
                                {
                                    if (!tileNames.Contains(currentTile.name))
                                    {
                                        currentTileSprite = tm.GetSprite(currentCellPosition);
                                        if (currentTileSprite != null)
                                        {
                                            try
                                            {
                                                if (string.IsNullOrEmpty(currentTile.name))
                                                {
                                                    currentTile.name = "Unknown-" + indexX.ToString() + "-" + indexY.ToString() + "-" + indexZ.ToString();
                                                }
                                                //SaveSpriteTextureToFile(currentTileSprite, currentTile.name + ".png");
                                            }
                                            catch (System.UnauthorizedAccessException e)
                                            {
                                                MelonLogger.Msg(e.ToString());
                                            }
                                        }
                                        else
                                        {
                                            MelonLogger.Msg("Attempted to save tile sprite \"" + currentTile.name + " \"But it appears to be null.");
                                        }
                                        tileNames.Add(currentTile.name);
                                    }
                                    tileInfoString = currentCellPosition.ToString() + "; " + currentTile.name;
                                    tilemapPath += tileInfoString + "\r\n";

                                }
                            }
                        }
                    }
                }
            }
            else
            {
                tilemapPath = "Could not find any tilemaps.";
            }
            MelonLogger.Msg(tilemapPath);
            SaveStringToFile(tilemapPath, sceneName + "-tilemaps.txt");
            SaveCarnivalTilesToFile(carnivalTiles, sceneName + "-tiles.txt");
        }

        private static void SaveCarnivalTilesToFile(Dictionary<string, Tile> theCarnivalTiles, string fileName)
        {
            if (theCarnivalTiles == null) { return; }
            try
            {
                string text = "";
                MelonLogger.Msg("About to do a Foreach");
                List<Tile> tiles = new List<Tile>(theCarnivalTiles.Values);
                foreach (Tile t in tiles)
                {
                    if (t == null) { break; }
                    text += "name:{" + t.name + "} ";
                    text += "textureRect:{" + t.sprite.textureRect + "} ";
                    text += "pivot:{" + t.sprite.pivot + "} ";
                    text += "pixelsPerUnit:{" + t.sprite.pixelsPerUnit + "} ";
                    text += "\r\n";
                }
                MelonLogger.Msg("Did the foreach, saving...");
                SaveStringToFile(text, fileName);
            }
            catch (System.NullReferenceException e)
            {
                MelonLogger.Msg("Oh boy, we got a null in the Tiles saver. " + e.ToString());
            }
        }

        private static string GetGameObjectPath(Transform transform)
        {
            string path = transform.name;
            while (transform.parent != null)
            {
                transform = transform.parent;
                path = transform.name + "/" + path;
            }
            return path;
        }

        private static void SaveTextureToFile(Texture2D texture, string fileName)
        {
            string fullPath = Application.dataPath + "/" + fileName;
            MelonLogger.Msg("Attempting to write image: " + fullPath);

            try
            {
                var bytes = texture.EncodeToPNG();

                FileStream file = System.IO.File.Open(fullPath, FileMode.Create);
                BinaryWriter binary = new BinaryWriter(file);
                binary.Write(bytes);
                file.Close();
                MelonLogger.Msg("Succeeded.");
            }
            catch (System.Exception e)
            {
                MelonLogger.Msg("Failed.");
                MelonLogger.Msg(e.ToString());
            }
        }

        private static void SaveSpriteTextureToFile(Sprite sprite, string fileName)
        {
            Texture2D texture = sprite.texture;
            SaveTextureToFile(texture, fileName);
        }

        private static void SaveStringToFile(string text, string fileName)
        {
            string fullPath = Application.dataPath + "/" + fileName;

            MelonLogger.Msg("Attempting to write text file: " + fullPath);
            using (StreamWriter outputFile = new StreamWriter(fullPath))
            {
                outputFile.WriteLine(text);
            }
            MelonLogger.Msg("Succeeded.");
        }

        //String inputLETileCopy = "<Gamepad>/buttonNorth";
        //String inputLETilePaste = "<Gamepad>/buttonEast";
        //String inputLETileLayer = "<Gamepad>/leftShoulder";

        /*
         debugDisplay = "";
                    debugDisplay += "(inputLETileLayer) KeyPressed " + GetKeyPressed(inputLETileLayer).ToString();
                    debugDisplay += "\r\n(inputLETileLayer) KeyDown " + GetKeyDown(inputLETileLayer).ToString();

                    debugDisplay += "CJump KeyPressed " + inputHandler.GetKeyPressed("CJump").ToString();
                    debugDisplay += "\r\nCJump KeyDown " + inputHandler.GetKeyDown("CJump").ToString();

        if (
                        (inputHandler.GetKeyPressed("CDown")
                        && inputHandler.GetKeyPressed("CJump") //KEY PRESSED AND KEY DOWN ARE DIFFERENT THAN I'M USED TO HERE.
                        && inputHandler.GetKeyPressed("CPrimary")
                        && inputHandler.GetKeyPressed("CSecondary")
                        && inputHandler.GetKeyPressed("CSpecial")
                        ) // Cassie's Combo Input
                        ||
                        (inputHandler.GetKeyPressed("Down")
                        && inputHandler.GetKeyPressed("Jump") //KEY PRESSED AND KEY DOWN ARE DIFFERENT THAN I'M USED TO HERE.
                        && inputHandler.GetKeyPressed("Primary")
                        && inputHandler.GetKeyPressed("Secondary")
                        && inputHandler.GetKeyPressed("Special")
                        ) // Alpha's Combo Input

                        ||
                        (
                            inputHandler.GetKeyPressed("Menu")
                            && (inputHandler.GetKeyPressed("CSpecial") || inputHandler.GetKeyPressed("Special"))
                        ) // Alternate Pause + Special warp.
                        ) //  Down + A + E + RB + RT
         */
        public bool GetKeyPressed(string s)
        {
            try
            {
                String sTrim = s.Split('/')[1];
                if (key != null)
                {
                    if (s.Contains("<Mouse>"))
                    {
                        return ((ButtonControl)Mouse.current[sTrim]).isPressed;
                    }
                    if (s.Contains("<Keyboard>"))
                    {
                        return ((KeyControl)Keyboard.current[sTrim]).isPressed;
                    }
                    if (s.Contains("<Gamepad>"))
                    {
                        return ((ButtonControl)Gamepad.current[sTrim]).isPressed;
                    }
                }
            }
            catch (Exception e)
            {
                // I should probably do a log here.
                MelonLogger.Msg(e.Message);
            }
            return false;
        }

        public bool GetKeyDown(string s)
        {
            try
            {
                String sTrim = s.Split('/')[1];
                if (key != null)
                {
                    if (s.Contains("<Mouse>"))
                    {
                        return ((ButtonControl)Mouse.current[sTrim]).wasPressedThisFrame;
                    }
                    if (s.Contains("<Keyboard>"))
                    {
                        return ((KeyControl)Keyboard.current[sTrim]).wasPressedThisFrame;
                    }
                    if (s.Contains("<Gamepad>"))
                    {
                        return ((ButtonControl)Gamepad.current[sTrim]).wasPressedThisFrame;
                    }
                }
            }
            catch (Exception e)
            {
                // I should probably do a log here.
                MelonLogger.Msg(e.Message);
            }
            return false;
        }
    }
}