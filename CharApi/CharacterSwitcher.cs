using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Reflection;

using UnityEngine;
using ChangeCoopMode = HutongGames.PlayMaker.Actions.ChangeCoopMode;
using InputDevice = InControl.InputDevice;

namespace Alexandria.CharacterAPI
{
    //Handles character switching just like the character command

    class CharacterSwitcher
    {
        private static string prefabPath;       

        public static void SwitchSecondaryCharacter(string[] args)
        {
            if (args == null || args.Length < 1) return;
            if (!GameManager.Instance.SecondaryPlayer)
            {
                ToolsCharApi.PrintError("You need to enter co-op mode before using the character2 command");
                return;
            }

            prefabPath = "Player" + args[0];
            var prefab = (GameObject)BraveResources.Load(prefabPath, ".prefab");
            if (prefab == null)
            {
                if (ToolsCharApi.EnableDebugLogging == true)
                {
                    ToolsCharApi.Print("Failed getting prefab for " + args[0]);
                }
                return;
            }
            GameManager.Instance.StartCoroutine(HandleCharacterChange());
            Hooks.ResetInfiniteGuns();
        }

        private static IEnumerator HandleCharacterChange()
        {
            //Pixelator.Instance.FadeToBlack(0.5f, false);
            InputDevice lastActiveDevice = GameManager.Instance.LastUsedInputDeviceForConversation;

            Vector3 position = GameManager.Instance.SecondaryPlayer.transform.position;
            //Destroy Player 2
            if (GameManager.Instance.CurrentGameType == GameManager.GameType.COOP_2_PLAYER)
            {
                GameManager.Instance.SecondaryPlayer.SetInputOverride("getting deleted");
                GameManager.Instance.ClearSecondaryPlayer();

                if (GameManager.Instance.PrimaryPlayer)
                    GameManager.Instance.PrimaryPlayer.ReinitializeMovementRestrictors();
                yield return null;
            }

            //Build new Player 2
            GameManager.Instance.CurrentGameType = GameManager.GameType.COOP_2_PLAYER;
            if (GameManager.Instance.PrimaryPlayer)
            {
                GameManager.Instance.PrimaryPlayer.ReinitializeMovementRestrictors();
            }
            PlayerController newPlayer = GeneratePlayer(position);
            yield return null;
                
            GameUIRoot.Instance.ConvertCoreUIToCoopMode();
            PhysicsEngine.Instance.RegisterOverlappingGhostCollisionExceptions(newPlayer.specRigidbody, null, false);
            GameManager.Instance.MainCameraController.ClearPlayerCache();
            BraveInput.ReassignAllControllers(lastActiveDevice);
            if (Foyer.Instance)
            {
                Foyer.Instance.ProcessPlayerEnteredFoyer(newPlayer);
                Foyer.Instance.OnCoopModeChanged?.Invoke();
            }

            GameManager.Instance.SecondaryPlayer.PlayerIDX = 1;
            GameManager.Instance.SecondaryPlayer.characterIdentity = PlayableCharacters.CoopCultist;

            //Reset
            GameManager.Instance.RefreshAllPlayers();
            if (ToolsCharApi.EnableDebugLogging == true)
            {
                ToolsCharApi.Print("Character swapped", "FFFFFF", true);
            }
            yield break;
        }

        private static PlayerController GeneratePlayer(Vector3 position)
        {
            if (GameManager.Instance.SecondaryPlayer != null)
            {
                return GameManager.Instance.SecondaryPlayer;
            }
            GameManager.Instance.ClearSecondaryPlayer();
            GameManager.LastUsedCoopPlayerPrefab = (GameObject)BraveResources.Load(prefabPath);
            PlayerController playerController = null;
            if (playerController == null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(GameManager.LastUsedCoopPlayerPrefab, position, Quaternion.identity);
                gameObject.SetActive(true);
                playerController = gameObject.GetComponent<PlayerController>();
            }

            GameManager.Instance.SecondaryPlayer = playerController;
            playerController.PlayerIDX = 1;
            return playerController;
        }
    }
}
