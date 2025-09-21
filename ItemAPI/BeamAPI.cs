using Alexandria.Misc;
using Alexandria.ItemAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using System.Reflection;

namespace Alexandria.ItemAPI
{
    public static class BeamAPI
    {
        //Methods and Extensions related to obtaining information or modifying beam instances at runtime.

        /// <summary>
        /// Returns true if the given position is within the given distance of any of the beam's bones.
        /// </summary>
        /// <param name="beam">The beam whose bones should be checked.</param>
        /// <param name="positionToCheck">The position whose distance is being calculated.</param>
        /// <param name="distance">The radius within which a bone must be to the position in order to return true.</param>
        public static bool PosIsNearAnyBoneOnBeam(this BasicBeamController beam, Vector2 positionToCheck, float distance)
        {
            foreach (BasicBeamController.BeamBone bone in beam.m_bones)
                if (Vector2.Distance(positionToCheck, beam.GetBonePosition(bone)) < distance)
                    return true;
            return false;
        }

        /// <summary>
        /// Returns the total number of bones which makes up the given beam.
        /// </summary>
        /// <param name="beam">The beam whose bones should be counted.</param>
        public static int GetBoneCount(this BasicBeamController beam)
        {
            return beam.UsesBones ? beam.m_bones.Count() : 1;
        }

        /// <summary>
        /// Returns the angle of the final bone in the beam.
        /// </summary>
        /// <param name="beam">The beam to be checked.</param>
        public static float GetFinalBoneDirection(this BasicBeamController beam)
        {
            return beam.UsesBones ? beam.m_bones.Last.Value.RotationAngle : beam.Direction.ToAngle();
        }

        /// <summary>
        /// Returns the bone at the given index on the beam.        
        /// </summary>
        /// <param name="beam">The beam to be checked.</param>
        /// <param name="boneIndex">The index whose bone should be returned..</param>
        public static BasicBeamController.BeamBone GetIndexedBone(this BasicBeamController beam, int boneIndex)
        {
            if (beam.m_bones == null)
                return null;
            if (beam.m_bones.ElementAt(boneIndex) is BasicBeamController.BeamBone bone)
                return bone;
            Debug.LogError("Attempted to fetch a beam bone at an invalid index");
            return null;
        }

        /// <summary>
        /// Returns the position of the bone at the given index on the beam. Can be used to get positions at various distances along the beam's length.
        /// IE: Using GetBoneCount to get the count of all bones, halving it, rounding up, and then getting the position of the bone at that index will get the position halfway along the beam.
        /// </summary>
        /// <param name="beam">The beam to be checked.</param>
        /// <param name="boneIndex">The index whose bone position should be returned..</param>
        public static Vector2 GetIndexedBonePosition(this BasicBeamController beam, int boneIndex)
        {
            if (beam.m_bones.ElementAt(boneIndex) is BasicBeamController.BeamBone bone)
                return beam.GetBonePosition(bone);
            Debug.LogError("Attempted to fetch the position of a beam bone at an invalid index");
            return Vector2.zero;
        }

        /// <summary>
        /// Returns the position of the specified bone on the given beam.
        /// </summary>
        /// <param name="beam">The beam to be checked.</param>
        /// <param name="bone">The bone whose position should be returned.</param>
        public static Vector2 GetBonePosition(this BasicBeamController beam, BasicBeamController.BeamBone bone)
        {
            if (!beam.UsesBones)
                return beam.Origin + BraveMathCollege.DegreesToVector(beam.Direction.ToAngle(), bone.PosX);
            if (beam.ProjectileAndBeamMotionModule != null)
                return bone.Position + beam.ProjectileAndBeamMotionModule.GetBoneOffset(bone, beam, beam.projectile.Inverted);
            return bone.Position;
        }

        //Methods and Extensions related to setting up beam prefabs 

        /// <summary>
        /// Constructs and returns a prefab for a beam projectile, based off the given regular projectile.
        /// NOTE THAT ALL BEAM SPRITES MUST BE PERFECTLY SQUARE, AND ALL ANIMATIONS SHOULD HAVE THE SAME DIMENSIONS.
        /// </summary>
        /// <param name="projectile">The regular projectile that the beam is based off. Gets the regular projectile's stats and effects by default, where applicable.</param>
        /// <param name="spritePath">A path to an embedded sprite representing the 'default' state of the beam. Should ideally be the first frame of the midsection animation.</param>
        /// <param name="colliderDimensions">The X and Y dimensions of the beam's midsection hitbox per-section.</param>
        /// <param name="colliderOffsets">The X and Y offsets of the beam's midsection hitbox per-section. Offsets are relative to the bottom right.</param>
        /// <param name="beamAnimationPaths">A list of sprite paths for the beam's midsection animation. Can be any length.</param>
        /// <param name="beamFPS">The frames per second  of the beam midsection.</param>
        /// <param name="impactVFXAnimationPaths">A list of sprite paths for the impact VFX. Can be any length. Note, impact is NOT the same as Beam End. Leave null for no impact VFX.</param>
        /// <param name="beamImpactFPS">The frames per second of the beam's impact VFX.</param>
        /// <param name="impactVFXColliderDimensions">The X and Y dimensions of the beam's impact VFX collider. Note that the impact vfx cannot actually hit anything, so collider dimensions are largely arbitrary.</param>
        /// <param name="impactVFXColliderOffsets">The X and Y offsets of the beam's impact VFX collider. As with the dimensions, this is largely arbitrary.</param>
        /// <param name="endVFXAnimationPaths">A list of sprite paths for the beam's ending animation, which plays as the beam's final segment. Can be any length. If null, will default to a copy of the midsection.</param>
        /// <param name="beamEndFPS">The frames per second  of the beam ending section.</param>
        /// <param name="endVFXColliderDimensions">The X and Y dimensions of the beam's ending section hitbox.</param>
        /// <param name="endVFXColliderOffsets">The X and Y offsets of the beam's ending section hitbox. Offsets are relative to the bottom right.</param>
        /// <param name="muzzleVFXAnimationPaths">A list of sprite paths for the beam's starting animation, which plays as the beam's first segment. Can be any length. If null, will default to a copy of the midsection.</param>
        /// <param name="beamMuzzleFPS">The frames per second  of the beam's first segment.</param>
        /// <param name="muzzleVFXColliderDimensions">The X and Y dimensions of the beam's first section hitbox.</param>
        /// <param name="muzzleVFXColliderOffsets">The X and Y offsets of the beam's first section hitbox. Offsets are relative to the bottom right.</param>
        /// <param name="glowAmount">The intensity with which the beam should glow.</param>
        /// <param name="emissivecolouramt">The intensity of the beam's emissive colour power.</param>
        public static BasicBeamController GenerateBeamPrefab(this Projectile projectile, string spritePath, Vector2 colliderDimensions, Vector2 colliderOffsets, List<string> beamAnimationPaths = null, int beamFPS = -1, List<string> impactVFXAnimationPaths = null, int beamImpactFPS = -1, Vector2? impactVFXColliderDimensions = null, Vector2? impactVFXColliderOffsets = null, List<string> endVFXAnimationPaths = null, int beamEndFPS = -1, Vector2? endVFXColliderDimensions = null, Vector2? endVFXColliderOffsets = null, List<string> muzzleVFXAnimationPaths = null, int beamMuzzleFPS = -1, Vector2? muzzleVFXColliderDimensions = null, Vector2? muzzleVFXColliderOffsets = null, float glowAmount = 0, float emissivecolouramt = 0)
        {
            try
            {
                Assembly assembly = Assembly.GetCallingAssembly();

                if (projectile.specRigidbody)
                    projectile.specRigidbody.CollideWithOthers = false;

                int spriteID = SpriteBuilder.AddSpriteToCollection(spritePath, ETGMod.Databases.Items.ProjectileCollection, Assembly.GetCallingAssembly());
                tk2dTiledSprite tiledSprite = projectile.gameObject.GetOrAddComponent<tk2dTiledSprite>();

                tiledSprite.SetSprite(ETGMod.Databases.Items.ProjectileCollection, spriteID);
                tk2dSpriteDefinition def = tiledSprite.GetCurrentSpriteDef();
                def.colliderVertices = new Vector3[]{ 0.0625f * colliderOffsets, 0.0625f * colliderDimensions };

                def.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleLeft); //NOTE: this seems right, but double check later

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
                    tk2dSpriteAnimationClip clip = new tk2dSpriteAnimationClip() {
                        name = "beam_idle", frames = new tk2dSpriteAnimationFrame[beamAnimationPaths.Count], fps = beamFPS };
                    tk2dSpriteCollectionData collection = ETGMod.Databases.Items.ProjectileCollection;
                    for (int i = 0; i < beamAnimationPaths.Count; ++i)
                    {
                        int frameSpriteId = SpriteBuilder.AddSpriteToCollection(beamAnimationPaths[i], collection, assembly);
                        tk2dSpriteDefinition frameDef = collection.spriteDefinitions[frameSpriteId];
                        frameDef.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleLeft);
                        frameDef.colliderVertices = def.colliderVertices;
                        clip.frames[i] = new tk2dSpriteAnimationFrame { spriteId = frameSpriteId, spriteCollection = collection };
                    }
                    Shared.Append(ref animation.clips, clip);
                    beamController.beamAnimation = "beam_idle";
                }

                //------------- Sets up the animation for the part of the beam that touches the wall
                if (endVFXAnimationPaths != null && endVFXColliderDimensions != null && endVFXColliderOffsets != null)
                    SetupBeamPart(animation, endVFXAnimationPaths, "beam_end", beamEndFPS, assembly, (Vector2)endVFXColliderDimensions, (Vector2)endVFXColliderOffsets);
                else
                    SetupBeamPart(animation, beamAnimationPaths, "beam_end", beamFPS, assembly, null, null, def.colliderVertices);
                beamController.beamEndAnimation = "beam_end";

                //---------------Sets up the animaton for the VFX that plays over top of the end of the beam where it hits stuff
                if (impactVFXAnimationPaths != null && impactVFXColliderDimensions != null && impactVFXColliderOffsets != null)
                {
                    SetupBeamPart(animation, impactVFXAnimationPaths, "beam_impact", beamImpactFPS, assembly, (Vector2)impactVFXColliderDimensions, (Vector2)impactVFXColliderOffsets);
                    beamController.impactAnimation = "beam_impact";
                }

                //--------------Sets up the animation for the very start of the beam
                if (muzzleVFXAnimationPaths != null && muzzleVFXColliderDimensions != null && muzzleVFXColliderOffsets != null)
                    SetupBeamPart(animation, muzzleVFXAnimationPaths, "beam_start", beamMuzzleFPS, assembly, (Vector2)muzzleVFXColliderDimensions, (Vector2)muzzleVFXColliderOffsets);
                else
                    SetupBeamPart(animation, beamAnimationPaths, "beam_start", beamFPS, assembly, null, null, def.colliderVertices);
                beamController.beamStartAnimation = "beam_start";

                if (glowAmount > 0)
                {
                    EmmisiveBeams emission = projectile.gameObject.GetOrAddComponent<EmmisiveBeams>();
                    emission.EmissivePower = glowAmount;
                    if (emissivecolouramt != 0) emission.EmissiveColorPower = emissivecolouramt;
                }
                return beamController;
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
                return null;
            }
        }

        /// <summary>
        /// Used during beam prefab generation to initialise various segments of the beam. ONLY USE IF YOU KNOW WHAT YOU ARE DOING.
        /// </summary>
        /// <param name="beamAnimation">The animation to be added to.</param>
        /// <param name="animSpritePaths">The sprite paths of the segment's animations.</param>
        /// <param name="animationName">The name of the animation.</param>
        /// <param name="fps">The frames per second of the segment's animation.</param>
        /// <param name="assembly">The calling assembly.</param>
        /// <param name="colliderDimensions">The dimensions of the segment's pixel collider.</param>
        /// <param name="colliderOffsets">The offsets of the segment's pixel collider. Offsets are calculated from the bottom left.</param>
        /// <param name="overrideVertices">A set of override colliders, if applicable.</param>
        public static void SetupBeamPart(tk2dSpriteAnimation beamAnimation, List<string> animSpritePaths, string animationName, int fps, Assembly assembly, Vector2? colliderDimensions = null, Vector2? colliderOffsets = null, Vector3[] overrideVertices = null)
        {
            Shared.SetupBeamPart(beamAnimation, animSpritePaths, animationName, fps, assembly, colliderDimensions, colliderOffsets, overrideVertices, anchor: tk2dBaseSprite.Anchor.MiddleCenter);
        }

        //Methods and extensions related to spawning beam prefabs at runtime

        /// <summary>
        /// Fires and maintains a beam from the specified object or position for the specified time.
        /// </summary>
        /// <param name="projectileToSpawn">The Beam Prefab to be created.</param>
        /// <param name="owner">The owner of the new beam.</param>
        /// <param name="otherShooter">If set, rather than being fired from a set position, the beam will be fired from the center of the set gameobject's rigid body, and will update it's position for it's duration..</param>
        /// <param name="fixedPosition">The position the beam should be spawned at. No effect if otherShooter is set.</param>
        /// <param name="targetAngle">The initial angle of the beam.</param>
        /// <param name="duration">How many seconds the beam should fire for.</param>
        /// <param name="skipChargeTime">If true, the beam will not need to 'charge' before firing, like the Disintegrator.</param>
        /// <param name="followDirOnProjectile">If true and otherShooter is a projectile, the angle of the beam will be dynamically updated to the projectile's current direction.</param>
        /// <param name="angleOffsetFromProjectileAngle">If followDirOnProjectile is true, this sets an amount of fixed offset from the projectile's direction in the beam's dynamic angle.</param>
        public static BeamController FreeFireBeamFromAnywhere(Projectile projectileToSpawn, PlayerController owner, GameObject otherShooter, Vector2 fixedPosition, float targetAngle, float duration, bool skipChargeTime = false, bool followDirOnProjectile = false, float angleOffsetFromProjectileAngle = 0)
        {
            Vector2 sourcePos = Vector2.zero;
            SpeculativeRigidbody rigidBod = null;
            if (otherShooter == null)
                sourcePos = fixedPosition;
            else
            {
                if (otherShooter.GetComponent<SpeculativeRigidbody>() is SpeculativeRigidbody body1)
                    rigidBod = body1;
                else if (otherShooter.GetComponentInChildren<SpeculativeRigidbody>() is SpeculativeRigidbody body2)
                    rigidBod = body2;
                if (rigidBod)
                    sourcePos = rigidBod.UnitCenter;
            }
            if (sourcePos != Vector2.zero)
            {
                GameObject gameObject = SpawnManager.SpawnProjectile(projectileToSpawn.gameObject, sourcePos, Quaternion.identity, true);
                gameObject.GetComponent<Projectile>().Owner = owner;
                BeamController beam = gameObject.GetComponent<BeamController>();
                if (skipChargeTime)
                {
                    beam.chargeDelay = 0f;
                    beam.usesChargeDelay = false;
                }
                beam.Owner = owner;
                beam.HitsPlayers = false;
                beam.HitsEnemies = true;
                Vector3 vector = BraveMathCollege.DegreesToVector(targetAngle, 1f);
                if (otherShooter != null && otherShooter.GetComponent<Projectile>() && followDirOnProjectile)
                    beam.Direction = (otherShooter.GetComponent<Projectile>().Direction.ToAngle() + angleOffsetFromProjectileAngle).DegreeToVector2();
                else
                    beam.Direction = vector;
                beam.Origin = sourcePos;
                GameManager.Instance.Dungeon.StartCoroutine(BeamAPI.HandleFreeFiringBeam(beam, rigidBod, fixedPosition, targetAngle, duration, followDirOnProjectile, angleOffsetFromProjectileAngle));
                return beam;
            }
            else
            {
                ETGModConsole.Log("ERROR IN BEAM FREEFIRE CODE. SOURCEPOS WAS NULL, EITHER DUE TO INVALID FIXEDPOS OR SOURCE GAMEOBJECT.");
                return null;
            }
        }

        private static IEnumerator HandleFreeFiringBeam(BeamController beam, SpeculativeRigidbody otherShooter, Vector2 fixedPosition, float targetAngle, float duration, bool followProjDir, float projFollowOffset)
        {
            bool parented = otherShooter != null;
            Projectile projToFollow = (followProjDir && otherShooter) ? otherShooter.GetComponent<Projectile>() : null;
            Vector2 sourcePos = otherShooter ? otherShooter.UnitCenter : fixedPosition;
            yield return null;
            for (float elapsed = 0f; elapsed < duration; elapsed += BraveTime.DeltaTime)
            {
                if (!beam || (parented && !otherShooter))
                    break;
                if (otherShooter)
                {
                    sourcePos = otherShooter.UnitCenter;
                    beam.Origin = sourcePos;
                }
                if (projToFollow)
                    beam.Direction = (projToFollow.Direction.ToAngle() + projFollowOffset).DegreeToVector2();
                beam.LateUpdatePosition(sourcePos);
                yield return null;
            }
            if (beam)
                beam.CeaseAttack();
            yield break;
        }
    }

    internal class EmmisiveBeams : MonoBehaviour
    {
        public void Start()
        {
            glowshader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutEmissive");

            foreach (Transform transform in base.transform)
            {
                if (!TransformList.Contains(transform.name))
                    continue;
                if (transform.GetComponent<tk2dSprite>() is not tk2dSprite sproot)
                    continue;

                sproot.usesOverrideMaterial = true;
                Material mat = sproot.renderer.material;
                mat.shader = glowshader;
                mat.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                mat.SetFloat("_EmissivePower", EmissivePower);
                mat.SetFloat("_EmissiveColorPower", EmissiveColorPower);
            }
            BasicBeamController cont = base.GetComponent<BasicBeamController>();
            Material mat2 = cont.sprite.renderer.material;
            mat2.shader = glowshader;
            mat2.EnableKeyword("BRIGHTNESS_CLAMP_ON");
            mat2.SetFloat("_EmissivePower", EmissivePower);
            mat2.SetFloat("_EmissiveColorPower", EmissiveColorPower);
        }

        public void Update()
        {
            if (base.transform.Find("beam pierce impact vfx") is not Transform t)
                return;
            if (t.GetComponent<tk2dSprite>() is not tk2dSprite sproot)
                return;

            Material mat = sproot.renderer.material;
            mat.shader = glowshader;
            mat.EnableKeyword("BRIGHTNESS_CLAMP_ON");
            mat.SetFloat("_EmissivePower", EmissivePower);
            mat.SetFloat("_EmissiveColorPower", EmissiveColorPower);
        }

        private static List<string> TransformList = new List<string>()
        {
            "Sprite",
            "beam impact vfx 2",
            "beam impact vfx",
        };

        private Shader glowshader;
        public float EmissivePower = 100;
        public float EmissiveColorPower = 1.55f;
    }
}

 
