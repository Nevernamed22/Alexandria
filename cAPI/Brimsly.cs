using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Gungeon;
using Dungeonator;
using System.Reflection;
using Alexandria.ItemAPI;
using System.Collections;
using System.Globalization;
using HarmonyLib;

namespace Alexandria.cAPI
{
    public class Brimsly : BraveBehaviour, IPlayerInteractable
    {
        private static bool TalkedThisSession = false;
        private static GameObject BrimslyPlaceable = null;
        private static Vector3 BrimslyBreachPosition = new Vector3(44.2f, 42.5f, 43.0f);

        public Transform talkpoint;

        public static void Init()
        {
            tk2dSpriteCollectionData collection = SpriteBuilder.ConstructCollection(new GameObject(), "NPC_Brimsly_Collection");;

            List<int> idleIdsList = new();
            for (int i = 1; i <= 8; ++i)
                idleIdsList.Add(SpriteBuilder.AddSpriteToCollection($"Alexandria/cAPI/brimsly_sprites/brimsly_idle_00{i}.png", collection));

            List<int> talkIdsList = new();
            for (int i = 1; i <= 5; ++i)
                talkIdsList.Add(SpriteBuilder.AddSpriteToCollection($"Alexandria/cAPI/brimsly_sprites/brimsly_talk_00{i}.png", collection));

            BrimslyPlaceable = new GameObject("BrimslyPlaceable", typeof(Brimsly));
            FakePrefab.MakeFakePrefab(BrimslyPlaceable);

            tk2dSprite sprite = BrimslyPlaceable.AddComponent<tk2dSprite>();
            sprite.SetSprite(collection, idleIdsList[0]);
            sprite.transform.SetParent(BrimslyPlaceable.transform);
            sprite.transform.localPosition = new Vector3(-1f, 2f / 26);
            sprite.SetUpSpeculativeRigidbody(new IntVector2(0, 0), new IntVector2(27, 12));
            // sprite.Collection = collection;

            tk2dSpriteAnimator spriteAnimator = BrimslyPlaceable.AddComponent<tk2dSpriteAnimator>();
            spriteAnimator.playAutomatically = true;
            SpriteBuilder.AddAnimation(spriteAnimator, collection, idleIdsList, "idle", tk2dSpriteAnimationClip.WrapMode.Loop, 8);
            SpriteBuilder.AddAnimation(spriteAnimator, collection, talkIdsList, "talk", tk2dSpriteAnimationClip.WrapMode.Loop, 12);

            Transform talktransform = new GameObject("talkpoint").transform;
            talktransform.SetParent(sprite.transform);
            talktransform.localPosition = new Vector3(13f / 16f, 27f / 16f);
        }

        private void Start()
        {
            talkpoint = base.transform.Find("talkpoint");
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
            GameManager.Instance.Dungeon.data.GetAbsoluteRoomFromPosition(base.transform.position.IntXY(VectorConversions.Round)).RegisterInteractable(this);
        }

        public IEnumerator Conversation(string dialogue, PlayerController speaker)
        {
            base.spriteAnimator.PlayForDuration($"talk", 0.7f, $"idle", false);
            TextBoxManager.ShowTextBox(talkpoint.position, talkpoint, 0.7f, dialogue, "bower", false, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, false, false);
            yield break;
        }

        private static List<string> Articles = new List<string>(){ "a", "an", "the" };
        private static string GetHatNameWithArticles(string name)
        {
            string lowerName = name.ToLower();
            for (int i = 0; i < Articles.Count; ++i)
                if (lowerName.StartsWith(Articles[i]))
                    return name;
            return $"this {name}";
        }

        public IEnumerator LongConversation(List<string> dialogue, PlayerController speaker, Hat hat = null)
        {
            for (int conversationIndex = 0; conversationIndex < dialogue.Count; conversationIndex++)
            {
                base.spriteAnimator.PlayForDuration($"talk", 0.7f, $"idle", false);
                TextBoxManager.ClearTextBox(talkpoint);
                string curLine = dialogue[conversationIndex];
                if (hat && !string.IsNullOrEmpty(hat.hatName) && curLine.Contains("[HAT NAME]"))
                {
                    curLine = curLine.Replace("[HAT NAME]", GetHatNameWithArticles(hat.hatName));
                }
                TextBoxManager.ShowTextBox(talkpoint.position, talkpoint, -1f, curLine, "bower", instant: false, showContinueText: true);

                float timer = 0;
                while (!BraveInput.GetInstanceForPlayer(speaker.PlayerIDX).ActiveActions.GetActionFromType(GungeonActions.GungeonActionType.Interact).WasPressed || timer < 0.4f)
                {
                    timer += BraveTime.DeltaTime;
                    yield return null;
                }
            }
            TextBoxManager.ClearTextBox(talkpoint);
            base.spriteAnimator.Play($"idle");
            yield break;
        }

        public float GetDistanceToPoint(Vector2 point)
        {
            return Vector2.Distance(point, base.specRigidbody.UnitCenter) / 1.5f;
        }

        public void OnEnteredRange(PlayerController interactor)
        {
            if (base.spriteAnimator.CurrentClip.name == "talk")
            {
                base.spriteAnimator.Play($"idle");
                TextBoxManager.ClearTextBox(talkpoint);
            }
            SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);
        }

        public void OnExitRange(PlayerController interactor)
        {
            base.spriteAnimator.PlayForDuration($"talk", 1f, $"idle", false);
            TextBoxManager.ShowTextBox(talkpoint.position, talkpoint, 1f, BraveUtility.RandomElement(ExitConvoStrings), "mainframe", false, TextBoxManager.BoxSlideOrientation.NO_ADJUSTMENT, false, false);
            SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
        }

        public void Interact(PlayerController interactor)
        {
            if (!TextBoxManager.HasTextBox(talkpoint))
                base.StartCoroutine(HandleInteract(interactor));
        }

        private void BeginLongConversation(PlayerController interactor)
        {
            interactor.SetInputOverride("npcConversation");
            Pixelator.Instance.LerpToLetterbox(0.35f, 0.25f);
            CameraController mainCameraController = GameManager.Instance.MainCameraController;
            mainCameraController.SetManualControl(true, true);
            mainCameraController.OverridePosition = base.transform.position;

            SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.black);
        }

        private void EndLongConversation(PlayerController interactor)
        {
            SpriteOutlineManager.RemoveOutlineFromSprite(base.sprite, true);
            SpriteOutlineManager.AddOutlineToSprite(base.sprite, Color.white);

            interactor.ClearInputOverride("npcConversation");
            Pixelator.Instance.LerpToLetterbox(1, 0.25f);
            GameManager.Instance.MainCameraController.SetManualControl(false, true);
        }

        // can theoretically fail if not enough hats are unlocked
        private static Hat GetRandomUnlockedHat()
        {
            const int MAX_TRIES = 20;

            if (Hatabase.HatRoomHats.Count == 0)
                return null;

            Hat randomHat = null;
            for (int i = 0; i < MAX_TRIES; ++i)
            {
                randomHat = Hatabase.HatRoomHats[UnityEngine.Random.Range(0, Hatabase.HatRoomHats.Count)];
                if (randomHat && randomHat.HasBeenUnlocked)
                    break;
            }
            return (randomHat && randomHat.HasBeenUnlocked) ? randomHat : null;
        }

        public IEnumerator HandleInteract(PlayerController interactor)
        {
            Hat randomHat = GetRandomUnlockedHat();

            BeginLongConversation(interactor);
            if (!TalkedThisSession || !randomHat)
            {
                TalkedThisSession = true;
                yield return LongConversation(FirstConvoStrings, interactor);
                EndLongConversation(interactor);
                yield break;
            }

            if (interactor.characterIdentity == PlayableCharacters.Gunslinger)
                yield return LongConversation(SlingerConvoStrings, interactor, randomHat);
            else if (interactor.characterIdentity == PlayableCharacters.Soldier)
                yield return LongConversation(MarineConvoStrings, interactor, randomHat);
            else
                yield return LongConversation(NormalConvoStrings, interactor, randomHat);
            EndLongConversation(interactor);

            interactor.GetComponent<HatController>().SetHat(randomHat);
            LootEngine.DoDefaultItemPoof(interactor.sprite.WorldBottomCenter + new Vector2(0f, 1f));
            yield return Conversation("HAT!", interactor);
            yield break;
        }

        public string GetAnimationState(PlayerController interactor, out bool shouldBeFlipped)
        {
            shouldBeFlipped = false;
            return string.Empty;
        }

        public float GetOverrideMaxDistance()
        {
            return 1.5f;
        }

        public static List<string> FirstConvoStrings = new List<string>()
        {
            "So many... {wj}hats{w}... it's hard to pick just one to {wj}wear{w}...",
            "I can help you... adorn that {wj}head{w} properly, heh, heh...",
        };

        public static List<string> NormalConvoStrings = new List<string>()
        {
            "...{wj}Hat!{w}...",
            "Don't forget to {wj}wear a hat!{w}",
            "I think [HAT NAME] will look {wj}GREAT{w} on you.",
        };

        public static List<string> SlingerConvoStrings = new List<string>()
        {
            "Why not try a hat with your hat!",
            "One hat is good, but TWO is better!",
            "I think [HAT NAME] will look {wj}GREAT{w} on you.",
        };

        public static List<string> MarineConvoStrings = new List<string>()
        {
            "Hats with Helmets are in style!",
            "I think we can squeeze [HAT NAME] over that helmet!",
        };

        public static List<string> ExitConvoStrings = new List<string>()
        {
            "{wj}...adorn all hatless...{w}",
            "{wj}...souls for the brim reaper...{w}",
            "{wj}...adorn the heads...{w}",
            "{wj}... hatless...soulless...{w}",
            "{wj}...for master...{w}",
        };

        private static bool TryToPlaceBrimsly;

        [HarmonyPatch(typeof(Foyer), nameof(Foyer.Start))]
        private static class OnFoyerStartPatch
        {
            private static void Postfix(Foyer __instance)
            {
                TryToPlaceBrimsly = (GetRandomUnlockedHat() != null);
            }
        }

        [HarmonyPatch(typeof(Foyer), nameof(Foyer.ProcessPlayerEnteredFoyer))]
        private static class ProcessPlayerEnteredFoyerPatch
        {
            static void Postfix(Foyer __instance, PlayerController p)
            {
                if (!TryToPlaceBrimsly)
                    return;
                foreach (Brimsly breachItem in UnityEngine.Object.FindObjectsOfType<Brimsly>())
                    breachItem.gameObject.SetActive(false);
                var placed = UnityEngine.Object.Instantiate<GameObject>(BrimslyPlaceable);
                placed.SetActive(true);
                placed.transform.position = BrimslyBreachPosition;
                TryToPlaceBrimsly = false;
            }
        }
    }
}
