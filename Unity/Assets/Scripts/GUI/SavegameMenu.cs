﻿// Copyright (c) 2015 Eamon Woortman
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.

using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SavegameMenu : BaseMenu {
    public delegate void LoadButtonPressed(string filename);
    public delegate void SaveButtonPressed(string filename, int savegameId);

    public SaveButtonPressed OnSaveButtonPressed;
    public LoadButtonPressed OnLoadButtonPressed;
    public delegate void VoidDelegate();
    public VoidDelegate OnHasSaved;
    [HideInInspector]
    public string SavegameType;

    public string SaveName {
        get {
            return saveName;
        }
    }

    private const string NoSavegamesFoundText = "No savegames found";
    private const string LoadingGamesText = "Loading games...";
    private List<Savegame> saveGames;
    private int selectedNameIndex = -1, oldSelectedNameIndex = -1;
    private string saveName = "";
    private string[] saveGameNames = { LoadingGamesText };
    private bool isLoading = true;
    
    public SavegameMenu() {
        windowRect = new Rect(10, 10, 200, 235);
    }

    public void LoadSavegames() {
        backendManager.LoadGames(SavegameType);
    }

    private void Start() {
        backendManager.OnSaveGameSucces += OnSaveGameSuccess;
        backendManager.OnSaveGameFailed += OnSaveGameFailed;
        backendManager.OnGamesLoaded += OnGamesLoaded;
    }

    private void OnSaveGameSuccess() {
        LoadSavegames();
    }

    private void OnSaveGameFailed(string error) {
        isLoading = false;
        saveGameNames = new string[] { NoSavegamesFoundText };
    }

    private void OnGamesLoaded(List<Savegame> games) {
        isLoading = false;
        saveGames = games;

        if (games.Count != 0) {
            saveGameNames = saveGames.Select(game => game.Name).ToArray();
        } else {
            saveGameNames = new string[] { NoSavegamesFoundText };
        }
    }

    private int GetSaveId() {
        foreach (Savegame savegame in saveGames) {
            if (savegame.Name == saveName) {
                return savegame.Id;
            }
        }
        return -1;
    }

    private void DoSave() {
        saveGameNames = new string[] { LoadingGamesText };
        isLoading = true;
        int saveId = GetSaveId();

        if (OnSaveButtonPressed != null) {
            OnSaveButtonPressed(SaveName, saveId);
        }
    }

    private void OverwriteConfirmed() {
        DoSave();
    }

    private void ShowWindow(int id) {
        GUILayout.BeginVertical();
        GUILayout.Label("Save games");
        bool savegamesFound = (saveGameNames[0] != NoSavegamesFoundText);
        GUI.enabled = savegamesFound && !isLoading;
        selectedNameIndex = GUILayout.SelectionGrid(selectedNameIndex, saveGameNames, 1);
        if (selectedNameIndex != oldSelectedNameIndex) {
            saveName = saveGameNames[selectedNameIndex];
            oldSelectedNameIndex = selectedNameIndex;
        }
        GUI.enabled = true;
        GUILayout.FlexibleSpace();
        saveName = GUILayout.TextField(saveName);
        GUILayout.BeginHorizontal();

        GUI.enabled = (SaveName != "");
        if (GUILayout.Button("Save")) {
            int saveId = GetSaveId();
            if (saveId != -1) {
                ConfirmPopup popup = ConfirmPopup.Create("Overwriting savegame", "You are about to overwrite the savegame '"+saveName+"', are you sure?");
                popup.OnConfirmed += OverwriteConfirmed;
            } else if (saveGameNames.Length == 5) {
                ConfirmPopup.Create("Savegame limit reached", "You have reached the limit(5) of savegames. Please overwrite an existing savegame.", true);
            } else {
                DoSave();
            }
        }

        GUI.enabled = savegamesFound && selectedNameIndex != -1;
        if (GUILayout.Button("Load")) {
            if (OnLoadButtonPressed != null) {
                OnLoadButtonPressed(saveGames[selectedNameIndex].File);
            }
        }
        GUI.enabled = true;

        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void OnGUI() {
        GUI.skin = Skin;
        windowRect = GUILayout.Window(0, windowRect, ShowWindow, "Load/save menu");
    }

    public void RefreshSaveGames() {

    }
}
