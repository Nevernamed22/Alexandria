using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.VisualAPI
{
    public class VFXBuilder
    {
        public static tk2dSpriteCollectionData itemCollection = PickupObjectDatabase.GetById(155).sprite.Collection;
        public static VFXComplex CreateBlankVFXComplex()
        {
            return new VFXComplex() { effects = new VFXObject[] { } };
        }
        public static VFXPool CreateBlankVFXPool()
        {
            return new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[] { new VFXComplex() { effects = new VFXObject[] { new VFXObject() { } } } } };
        }
        public static VFXPool CreateBlankVFXPool(GameObject effect)
        {
            return new VFXPool { type = VFXPoolType.None, effects = new VFXComplex[] { new VFXComplex() { effects = new VFXObject[] { new VFXObject() { effect = effect } } } } };
        }
        public static VFXPool CreateVFXPool(string name, List<string> spritePaths, int fps, IntVector2 Dimensions, tk2dBaseSprite.Anchor anchor, bool usesZHeight, float zHeightOffset, bool persist = false, VFXAlignment alignment = VFXAlignment.NormalAligned, float emissivePower = -1, Color? emissiveColour = null, Assembly ass = null)
        {
            GameObject Obj = new GameObject(name);
            VFXPool pool = new VFXPool();
            pool.type = VFXPoolType.All;
            VFXComplex complex = new VFXComplex();
            VFXObject vfObj = new VFXObject();
            Obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(Obj);
            UnityEngine.Object.DontDestroyOnLoad(Obj);

            tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(Obj, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(spritePaths[0], VFXSpriteCollection, ass != null ? ass : Assembly.GetCallingAssembly());

            tk2dSprite sprite = Obj.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(VFXSpriteCollection, spriteID);
            tk2dSpriteDefinition defaultDef = sprite.GetCurrentSpriteDef();
            defaultDef.colliderVertices = new Vector3[]{
                      new Vector3(0f, 0f, 0f),
                      new Vector3((Dimensions.x / 16), (Dimensions.y / 16), 0f)
                  };

            tk2dSpriteAnimator animator = Obj.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = Obj.GetOrAddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = "start", frames = new tk2dSpriteAnimationFrame[0], fps = fps };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Count; i++)
            {
                tk2dSpriteCollectionData collection = VFXSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection, Assembly.GetCallingAssembly());
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(anchor);
                frameDef.colliderVertices = defaultDef.colliderVertices;
                if (emissivePower > 0) frameDef.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.material.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.material.SetColor("_EmissiveColor", (Color)emissiveColour);
                if (emissivePower > 0) frameDef.materialInst.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.materialInst.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.materialInst.SetColor("_EmissiveColor", (Color)emissiveColour);
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            if (emissivePower > 0) sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
            if (emissivePower > 0) sprite.renderer.material.SetFloat("_EmissiveColorPower", emissivePower);
            if (emissiveColour != null) sprite.renderer.material.SetColor("_EmissiveColor", (Color)emissiveColour);
            clip.frames = frames.ToArray();
            clip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            if (!persist)
            {
                SpriteAnimatorKiller kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
                kill.fadeTime = -1f;
                kill.animator = animator;
                kill.delayDestructionTime = -1f;
            }
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("start");
            vfObj.attached = true;
            vfObj.persistsOnDeath = persist;
            vfObj.usesZHeight = usesZHeight;
            vfObj.zHeight = zHeightOffset;
            vfObj.alignment = alignment;
            vfObj.destructible = false;
            vfObj.effect = Obj;
            complex.effects = new VFXObject[] { vfObj };
            pool.effects = new VFXComplex[] { complex };
            return pool;
        }
        public static VFXComplex CreateVFXComplex(string name, List<string> spritePaths, int fps, IntVector2 Dimensions, tk2dBaseSprite.Anchor anchor, bool usesZHeight, float zHeightOffset, bool persist = false, float emissivePower = -1, Color? emissiveColour = null)
        {
            GameObject Obj = new GameObject(name);
            VFXPool pool = new VFXPool();
            pool.type = VFXPoolType.All;
            VFXComplex complex = new VFXComplex();
            VFXObject vfObj = new VFXObject();
            Obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(Obj);
            UnityEngine.Object.DontDestroyOnLoad(Obj);

            tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(Obj, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(spritePaths[0], VFXSpriteCollection, Assembly.GetCallingAssembly());

            tk2dSprite sprite = Obj.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(VFXSpriteCollection, spriteID);
            tk2dSpriteDefinition defaultDef = sprite.GetCurrentSpriteDef();
            defaultDef.colliderVertices = new Vector3[]{
                      new Vector3(0f, 0f, 0f),
                      new Vector3((Dimensions.x / 16), (Dimensions.y / 16), 0f)
                  };

            tk2dSpriteAnimator animator = Obj.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = Obj.GetOrAddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = "start", frames = new tk2dSpriteAnimationFrame[0], fps = fps };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Count; i++)
            {
                tk2dSpriteCollectionData collection = VFXSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection, Assembly.GetCallingAssembly());
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(anchor);
                frameDef.colliderVertices = defaultDef.colliderVertices;
                if (emissivePower > 0) frameDef.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.material.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.material.SetColor("_EmissiveColor", (Color)emissiveColour);
                if (emissivePower > 0) frameDef.materialInst.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.materialInst.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.materialInst.SetColor("_EmissiveColor", (Color)emissiveColour);
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            if (emissivePower > 0) sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
            if (emissivePower > 0) sprite.renderer.material.SetFloat("_EmissiveColorPower", emissivePower);
            if (emissiveColour != null) sprite.renderer.material.SetColor("_EmissiveColor", (Color)emissiveColour);
            clip.frames = frames.ToArray();
            clip.wrapMode = tk2dSpriteAnimationClip.WrapMode.Once;
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            SpriteAnimatorKiller kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
            kill.fadeTime = -1f;
            kill.animator = animator;
            kill.delayDestructionTime = -1f;
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("start");
            vfObj.attached = true;
            vfObj.persistsOnDeath = persist;
            vfObj.usesZHeight = usesZHeight;
            vfObj.zHeight = zHeightOffset;
            vfObj.alignment = VFXAlignment.NormalAligned;
            vfObj.destructible = false;
            vfObj.effect = Obj;
            complex.effects = new VFXObject[] { vfObj };
            pool.effects = new VFXComplex[] { complex };
            return complex;
        }
        public static GameObject CreateVFX(string name, List<string> spritePaths, int fps, IntVector2 Dimensions, tk2dBaseSprite.Anchor anchor, bool usesZHeight, float zHeightOffset, float emissivePower = -1, Color? emissiveColour = null, tk2dSpriteAnimationClip.WrapMode wrap = tk2dSpriteAnimationClip.WrapMode.Once, bool persist = false)
        {
            GameObject Obj = new GameObject(name);
            VFXObject vfObj = new VFXObject();
            Obj.SetActive(false);
            FakePrefab.MarkAsFakePrefab(Obj);
            UnityEngine.Object.DontDestroyOnLoad(Obj);

            tk2dSpriteCollectionData VFXSpriteCollection = SpriteBuilder.ConstructCollection(Obj, (name + "_Collection"));
            int spriteID = SpriteBuilder.AddSpriteToCollection(spritePaths[0], VFXSpriteCollection, Assembly.GetCallingAssembly());

            tk2dSprite sprite = Obj.GetOrAddComponent<tk2dSprite>();
            sprite.SetSprite(VFXSpriteCollection, spriteID);
            tk2dSpriteDefinition defaultDef = sprite.GetCurrentSpriteDef();
            defaultDef.colliderVertices = new Vector3[]{
                      new Vector3(0f, 0f, 0f),
                      new Vector3((Dimensions.x / 16), (Dimensions.y / 16), 0f)
                  };

            tk2dSpriteAnimator animator = Obj.GetOrAddComponent<tk2dSpriteAnimator>();
            tk2dSpriteAnimation animation = Obj.GetOrAddComponent<tk2dSpriteAnimation>();
            animation.clips = new tk2dSpriteAnimationClip[0];
            animator.Library = animation;
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = "start", frames = new tk2dSpriteAnimationFrame[0], fps = fps };
            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            for (int i = 0; i < spritePaths.Count; i++)
            {
                tk2dSpriteCollectionData collection = VFXSpriteCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(spritePaths[i], collection, Assembly.GetCallingAssembly());
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(anchor);
                frameDef.colliderVertices = defaultDef.colliderVertices;
                if (emissivePower > 0) frameDef.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.material.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.material.SetColor("_EmissiveColor", (Color)emissiveColour);
                if (emissivePower > 0) frameDef.materialInst.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
                if (emissivePower > 0) frameDef.materialInst.SetFloat("_EmissiveColorPower", emissivePower);
                if (emissiveColour != null) frameDef.materialInst.SetColor("_EmissiveColor", (Color)emissiveColour);
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            if (emissivePower > 0) sprite.renderer.material.shader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTintableTiltedCutoutEmissive");
            if (emissivePower > 0) sprite.renderer.material.SetFloat("_EmissiveColorPower", emissivePower);
            if (emissiveColour != null) sprite.renderer.material.SetColor("_EmissiveColor", (Color)emissiveColour);
            clip.frames = frames.ToArray();
            clip.wrapMode = wrap;
            animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
            if (!persist)
            {
                SpriteAnimatorKiller kill = animator.gameObject.AddComponent<SpriteAnimatorKiller>();
                kill.fadeTime = -1f;
                kill.animator = animator;
                kill.delayDestructionTime = -1f;
            }
            animator.playAutomatically = true;
            animator.DefaultClipId = animator.GetClipIdByName("start");
            vfObj.attached = true;
            vfObj.persistsOnDeath = false;
            vfObj.usesZHeight = usesZHeight;
            vfObj.zHeight = zHeightOffset;
            vfObj.alignment = VFXAlignment.NormalAligned;
            vfObj.destructible = false;
            vfObj.effect = Obj;
            return Obj;
        }
        public static GameObject CreateCustomShellCasing(string spriteName, int pixelWidth, int pixelHeight)
        {
            GameObject casing = UnityEngine.Object.Instantiate((PickupObjectDatabase.GetById(202) as Gun).shellCasing);
            casing.gameObject.SetActive(false);
            FakePrefab.MarkAsFakePrefab(casing.gameObject);
            UnityEngine.Object.DontDestroyOnLoad(casing);
            casing.GetComponent<tk2dBaseSprite>().spriteId = itemCollection.inst.GetSpriteIdByName(spriteName);
            VFXBuilder.SetupDefinitionForShellSprite(spriteName, casing.GetComponent<tk2dBaseSprite>().spriteId, pixelWidth, pixelHeight);
            return casing;
        }
        public static tk2dSpriteDefinition SetupDefinitionForShellSprite(string name, int id, int pixelWidth, int pixelHeight, tk2dSpriteDefinition overrideToCopyFrom = null)
        {
            float thing = 14;
            float trueWidth = (float)pixelWidth / thing;
            float trueHeight = (float)pixelHeight / thing;
            tk2dSpriteDefinition def = overrideToCopyFrom ?? itemCollection.inst.spriteDefinitions[(PickupObjectDatabase.GetById(202) as Gun).shellCasing.GetComponent<tk2dBaseSprite>().spriteId].CopyDefinitionFrom();
            def.boundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.boundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.untrimmedBoundsDataCenter = new Vector3(trueWidth / 2f, trueHeight / 2f, 0f);
            def.untrimmedBoundsDataExtents = new Vector3(trueWidth, trueHeight, 0f);
            def.position0 = new Vector3(0f, 0f, 0f);
            def.position1 = new Vector3(0f + trueWidth, 0f, 0f);
            def.position2 = new Vector3(0f, 0f + trueHeight, 0f);
            def.position3 = new Vector3(0f + trueWidth, 0f + trueHeight, 0f);
            def.name = name;
            itemCollection.spriteDefinitions[id] = def;
            return def;
        }
    }
}
