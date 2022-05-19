using Alexandria.Misc;
using ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.ItemAPI
{
    static class BeamAPI
    {
        public static bool PosIsNearAnyBoneOnBeam(this BasicBeamController beam, Vector2 positionToCheck, float distance)
        {
            LinkedList<BasicBeamController.BeamBone> bones;
            bones = ReflectionUtility.ReflectGetField<LinkedList<BasicBeamController.BeamBone>>(typeof(BasicBeamController), "m_bones", beam);
            foreach (BasicBeamController.BeamBone bone in bones)
            {
                Vector2 bonepos = beam.GetBonePosition(bone);
                if (Vector2.Distance(positionToCheck, bonepos) < distance) return true;
            }           
            return false;
        }
        public static int GetBoneCount(this BasicBeamController beam)
        {
            if (!beam.UsesBones)
            {
                return 1;
            }
            else
            {
                LinkedList<BasicBeamController.BeamBone> bones;
                bones = ReflectionUtility.ReflectGetField<LinkedList<BasicBeamController.BeamBone>>(typeof(BasicBeamController), "m_bones", beam);
                return bones.Count();
            }
        }
        public static float GetFinalBoneDirection(this BasicBeamController beam)
        {
            if (!beam.UsesBones)
            {
                return beam.Direction.ToAngle();
            }
            else
            {
                LinkedList<BasicBeamController.BeamBone> bones;
                bones = ReflectionUtility.ReflectGetField<LinkedList<BasicBeamController.BeamBone>>(typeof(BasicBeamController), "m_bones", beam);
                LinkedListNode<BasicBeamController.BeamBone> linkedListNode = bones.Last;
                return linkedListNode.Value.RotationAngle;
            }
        }
        public static BasicBeamController.BeamBone GetIndexedBone(this BasicBeamController beam, int boneIndex)
        {
            LinkedList<BasicBeamController.BeamBone> bones;
            bones = ReflectionUtility.ReflectGetField<LinkedList<BasicBeamController.BeamBone>>(typeof(BasicBeamController), "m_bones", beam);
            if (bones == null) return null;
            if (bones.ElementAt(boneIndex) == null) { Debug.LogError("Attempted to fetch a beam bone at an invalid index"); return null; }
            return bones.ElementAt(boneIndex);
        }
        public static Vector2 GetIndexedBonePosition(this BasicBeamController beam, int boneIndex)
        {
            LinkedList<BasicBeamController.BeamBone> bones;
            bones = ReflectionUtility.ReflectGetField<LinkedList<BasicBeamController.BeamBone>>(typeof(BasicBeamController), "m_bones", beam);

            if (bones.ElementAt(boneIndex) == null) { Debug.LogError("Attempted to fetch the position of a beam bone at an invalid index"); return Vector2.zero; }
            if (!beam.UsesBones)
            {
                return beam.Origin + BraveMathCollege.DegreesToVector(beam.Direction.ToAngle(), bones.ElementAt(boneIndex).PosX);
            }
            if (beam.ProjectileAndBeamMotionModule != null)
            {
                return bones.ElementAt(boneIndex).Position + beam.ProjectileAndBeamMotionModule.GetBoneOffset(bones.ElementAt(boneIndex), beam, beam.projectile.Inverted);
            }
            return bones.ElementAt(boneIndex).Position;
        }
        public static Vector2 GetBonePosition(this BasicBeamController beam, BasicBeamController.BeamBone bone)
        {
            if (!beam.UsesBones)
            {
                return beam.Origin + BraveMathCollege.DegreesToVector(beam.Direction.ToAngle(), bone.PosX);
            }
            if (beam.ProjectileAndBeamMotionModule != null)
            {
                return bone.Position + beam.ProjectileAndBeamMotionModule.GetBoneOffset(bone, beam, beam.projectile.Inverted);
            }
            return bone.Position;
        }
        public static BasicBeamController GenerateBeamPrefab(this Projectile projectile, string spritePath, Vector2 colliderDimensions, Vector2 colliderOffsets, List<string> beamAnimationPaths = null, int beamFPS = -1, List<string> impactVFXAnimationPaths = null, int beamImpactFPS = -1, Vector2? impactVFXColliderDimensions = null, Vector2? impactVFXColliderOffsets = null, List<string> endVFXAnimationPaths = null, int beamEndFPS = -1, Vector2? endVFXColliderDimensions = null, Vector2? endVFXColliderOffsets = null, List<string> muzzleVFXAnimationPaths = null, int beamMuzzleFPS = -1, Vector2? muzzleVFXColliderDimensions = null, Vector2? muzzleVFXColliderOffsets = null, float glowAmount = 0, float emissivecolouramt = 0)
        {
            try
            {
                projectile.specRigidbody.CollideWithOthers = false;
                float convertedColliderX = colliderDimensions.x / 16f;
                float convertedColliderY = colliderDimensions.y / 16f;
                float convertedOffsetX = colliderOffsets.x / 16f;
                float convertedOffsetY = colliderOffsets.y / 16f;

                int spriteID = SpriteBuilder.AddSpriteToCollection(spritePath, ETGMod.Databases.Items.ProjectileCollection);
                tk2dTiledSprite tiledSprite = projectile.gameObject.GetOrAddComponent<tk2dTiledSprite>();



                tiledSprite.SetSprite(ETGMod.Databases.Items.ProjectileCollection, spriteID);
                tk2dSpriteDefinition def = tiledSprite.GetCurrentSpriteDef();
                def.colliderVertices = new Vector3[]{
                    new Vector3(convertedOffsetX, convertedOffsetY, 0f),
                    new Vector3(convertedColliderX, convertedColliderY, 0f)
                };

                def.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleLeft);

                //tiledSprite.anchor = tk2dBaseSprite.Anchor.MiddleCenter;
                tk2dSpriteAnimator animator = projectile.gameObject.GetOrAddComponent<tk2dSpriteAnimator>();
                tk2dSpriteAnimation animation = projectile.gameObject.GetOrAddComponent<tk2dSpriteAnimation>();
                animation.clips = new tk2dSpriteAnimationClip[0];
                animator.Library = animation;
                UnityEngine.Object.Destroy(projectile.GetComponentInChildren<tk2dSprite>());
                BasicBeamController beamController = projectile.gameObject.GetOrAddComponent<BasicBeamController>();
                projectile.sprite = tiledSprite;
                //---------------- Sets up the animation for the main part of the beam
                if (beamAnimationPaths != null)
                {
                    tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = "beam_idle", frames = new tk2dSpriteAnimationFrame[0], fps = beamFPS };
                    List<string> spritePaths = beamAnimationPaths;

                    List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
                    foreach (string path in spritePaths)
                    {
                        tk2dSpriteCollectionData collection = ETGMod.Databases.Items.ProjectileCollection;
                        int frameSpriteId = SpriteBuilder.AddSpriteToCollection(path, collection);
                        tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                        frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleLeft);
                        frameDef.colliderVertices = def.colliderVertices;
                        frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
                    }
                    clip.frames = frames.ToArray();
                    animation.clips = animation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
                    beamController.beamAnimation = "beam_idle";
                }

                //------------- Sets up the animation for the part of the beam that touches the wall
                if (endVFXAnimationPaths != null && endVFXColliderDimensions != null && endVFXColliderOffsets != null)
                {
                    SetupBeamPart(animation, endVFXAnimationPaths, "beam_end", beamEndFPS, (Vector2)endVFXColliderDimensions, (Vector2)endVFXColliderOffsets);
                    beamController.beamEndAnimation = "beam_end";
                }
                else
                {
                    SetupBeamPart(animation, beamAnimationPaths, "beam_end", beamFPS, null, null, def.colliderVertices);
                    beamController.beamEndAnimation = "beam_end";
                }

                //---------------Sets up the animaton for the VFX that plays over top of the end of the beam where it hits stuff
                if (impactVFXAnimationPaths != null && impactVFXColliderDimensions != null && impactVFXColliderOffsets != null)
                {
                    SetupBeamPart(animation, impactVFXAnimationPaths, "beam_impact", beamImpactFPS, (Vector2)impactVFXColliderDimensions, (Vector2)impactVFXColliderOffsets);
                    beamController.impactAnimation = "beam_impact";
                }

                //--------------Sets up the animation for the very start of the beam
                if (muzzleVFXAnimationPaths != null && muzzleVFXColliderDimensions != null && muzzleVFXColliderOffsets != null)
                {
                    SetupBeamPart(animation, muzzleVFXAnimationPaths, "beam_start", beamMuzzleFPS, (Vector2)muzzleVFXColliderDimensions, (Vector2)muzzleVFXColliderOffsets);
                    beamController.beamStartAnimation = "beam_start";
                }
                else
                {
                    SetupBeamPart(animation, beamAnimationPaths, "beam_start", beamFPS, null, null, def.colliderVertices);
                    beamController.beamStartAnimation = "beam_start";
                }

                if (glowAmount > 0)
                {
                    EmmisiveBeams emission = projectile.gameObject.GetOrAddComponent<EmmisiveBeams>();
                    emission.EmissivePower = glowAmount;
                    if (emissivecolouramt != 0) emission.EmissiveColorPower = emissivecolouramt;
                    //emission

                }
                return beamController;
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
                return null;
            }
        }
        public static void SetupBeamPart(tk2dSpriteAnimation beamAnimation, List<string> animSpritePaths, string animationName, int fps, Vector2? colliderDimensions = null, Vector2? colliderOffsets = null, Vector3[] overrideVertices = null)
        {
            tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() { name = animationName, frames = new tk2dSpriteAnimationFrame[0], fps = fps };
            List<string> spritePaths = animSpritePaths;

            List<tk2dSpriteAnimationFrame> frames = new List<tk2dSpriteAnimationFrame>();
            foreach (string path in spritePaths)
            {
                tk2dSpriteCollectionData collection = ETGMod.Databases.Items.ProjectileCollection;
                int frameSpriteId = SpriteBuilder.AddSpriteToCollection(path, collection);
                tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleCenter);
                if (overrideVertices != null)
                {
                    frameDef.colliderVertices = overrideVertices;
                }
                else
                {
                    if (colliderDimensions == null || colliderOffsets == null)
                    {
                        ETGModConsole.Log("<size=100><color=#ff0000ff>BEAM ERROR: colliderDimensions or colliderOffsets was null with no override vertices!</color></size>", false);
                    }
                    else
                    {
                        Vector2 actualDimensions = (Vector2)colliderDimensions;
                        Vector2 actualOffsets = (Vector2)colliderDimensions;
                        frameDef.colliderVertices = new Vector3[]{
                            new Vector3(actualOffsets.x / 16, actualOffsets.y / 16, 0f),
                            new Vector3(actualDimensions.x / 16, actualDimensions.y / 16, 0f)
                        };
                    }
                }
                frames.Add(new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection });
            }
            clip.frames = frames.ToArray();
            beamAnimation.clips = beamAnimation.clips.Concat(new tk2dSpriteAnimationClip[] { clip }).ToArray();
        }
        public static BeamController FreeFireBeamFromAnywhere(Projectile projectileToSpawn, PlayerController owner, GameObject otherShooter, Vector2 fixedPosition, bool usesFixedPosition, float targetAngle, float duration, bool skipChargeTime = false, bool followDirOnProjectile = false, float angleOffsetFromProjectileAngle = 0)
        {
            Vector2 sourcePos = Vector2.zero;
            SpeculativeRigidbody rigidBod = null;
            if (usesFixedPosition) sourcePos = fixedPosition;
            else
            {
                if (otherShooter.GetComponent<SpeculativeRigidbody>()) rigidBod = otherShooter.GetComponent<SpeculativeRigidbody>();
                else if (otherShooter.GetComponentInChildren<SpeculativeRigidbody>()) rigidBod = otherShooter.GetComponentInChildren<SpeculativeRigidbody>();

                if (rigidBod) sourcePos = rigidBod.UnitCenter;
            }
            if (sourcePos != Vector2.zero)
            {

                GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, sourcePos, Quaternion.identity, true);
                Projectile component = gameObject.GetComponent<Projectile>();
                component.Owner = owner;
                BeamController component2 = gameObject.GetComponent<BeamController>();
                if (skipChargeTime)
                {
                    component2.chargeDelay = 0f;
                    component2.usesChargeDelay = false;
                }
                component2.Owner = owner;
                component2.HitsPlayers = false;
                component2.HitsEnemies = true;
                Vector3 vector = BraveMathCollege.DegreesToVector(targetAngle, 1f);
                if (otherShooter != null && !usesFixedPosition && otherShooter.GetComponent<Projectile>() && followDirOnProjectile) component2.Direction = (otherShooter.GetComponent<Projectile>().Direction.ToAngle() + angleOffsetFromProjectileAngle).DegreeToVector2();
                else component2.Direction = vector;
                component2.Origin = sourcePos;
                GameManager.Instance.Dungeon.StartCoroutine(BeamAPI.HandleFreeFiringBeam(component2, rigidBod, fixedPosition, usesFixedPosition, targetAngle, duration, followDirOnProjectile, angleOffsetFromProjectileAngle));
                return component2;
            }
            else
            {
                ETGModConsole.Log("ERROR IN BEAM FREEFIRE CODE. SOURCEPOS WAS NULL, EITHER DUE TO INVALID FIXEDPOS OR SOURCE GAMEOBJECT.");
                return null;
            }
        }
        private static IEnumerator HandleFreeFiringBeam(BeamController beam, SpeculativeRigidbody otherShooter, Vector2 fixedPosition, bool usesFixedPosition, float targetAngle, float duration, bool followProjDir, float projFollowOffset)
        {
            float elapsed = 0f;
            yield return null;
            while (elapsed < duration)
            {
                Vector2 sourcePos;
                if (otherShooter == null) { break; }
                if (usesFixedPosition) sourcePos = fixedPosition;
                else sourcePos = otherShooter.UnitCenter;

                elapsed += BraveTime.DeltaTime;
                if (sourcePos != null)
                {
                    if (otherShooter != null && !usesFixedPosition && otherShooter.GetComponent<Projectile>() && followProjDir)
                    {
                        beam.Direction = (otherShooter.GetComponent<Projectile>().Direction.ToAngle() + projFollowOffset).DegreeToVector2();
                    }
                    beam.Origin = sourcePos;
                    beam.LateUpdatePosition(sourcePos);


                }
                else { ETGModConsole.Log("SOURCEPOS WAS NULL IN BEAM FIRING HANDLER"); }
                yield return null;
            }
            beam.CeaseAttack();
            yield break;
        }
    }
    internal class EmmisiveBeams : MonoBehaviour
    {
        public EmmisiveBeams()
        {
            this.EmissivePower = 100;
            this.EmissiveColorPower = 1.55f;
        }
        public void Start()
        {
            Shader glowshader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutEmissive");

            foreach (Transform transform in base.transform)
            {
                if (TransformList.Contains(transform.name))
                {
                    tk2dSprite sproot = transform.GetComponent<tk2dSprite>();
                    if (sproot != null)
                    {
                        sproot.usesOverrideMaterial = true;
                        sproot.renderer.material.shader = glowshader;
                        sproot.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                        sproot.renderer.material.SetFloat("_EmissivePower", EmissivePower);
                        sproot.renderer.material.SetFloat("_EmissiveColorPower", EmissiveColorPower);
                    }
                }
            }
            this.beamcont = base.GetComponent<BasicBeamController>();
            BasicBeamController beam = this.beamcont;
            beam.sprite.usesOverrideMaterial = true;
            BasicBeamController component = beam.gameObject.GetComponent<BasicBeamController>();
            bool flag = component != null;
            bool flag2 = flag;
            if (flag2)
            {
                component.sprite.renderer.material.shader = glowshader;
                component.sprite.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                component.sprite.renderer.material.SetFloat("_EmissivePower", EmissivePower);
                component.sprite.renderer.material.SetFloat("_EmissiveColorPower", EmissiveColorPower);
            }
        }


        private List<string> TransformList = new List<string>()
        {
            "Sprite",
            "beam impact vfx 2",
            "beam impact vfx",
        };


        public void Update()
        {

            Shader glowshader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutEmissive");
            Transform trna = base.transform.Find("beam pierce impact vfx");
            if (trna != null)
            {
                tk2dSprite sproot = trna.GetComponent<tk2dSprite>();
                if (sproot != null)
                {
                    sproot.renderer.material.shader = glowshader;
                    sproot.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                    sproot.renderer.material.SetFloat("_EmissivePower", EmissivePower);
                    sproot.renderer.material.SetFloat("_EmissiveColorPower", EmissiveColorPower);
                }
            }

        }

        private BasicBeamController beamcont;
        public float EmissivePower;
        public float EmissiveColorPower;
    }
}

