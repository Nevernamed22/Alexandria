using Alexandria.ItemAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace Alexandria.ItemAPI
{
    public static class TrailAPI
    {
        /// <summary>
        /// Adds a tiled trail to the Projectile
        /// </summary>
        /// <param name="timeTillAnimStart">If set, once this time is reached, the whole trail will animate and dissipate uniformly, interrupting the progressive cascade.</param>
        /// <param name="target">The projectile its being added to.</param>
        /// <param name="spritePath">The sprite path for the first frame of your trail's animation. Used for collider generation.</param>
        /// <param name="colliderDimensions">The collider dimensions of your trail.</param>
        /// <param name="colliderOffsets">The offset of your trail from the bottom left corner of your projectile.</param>
        /// <param name="animPaths">The full list of sprite paths for your trails animation.</param>
        /// <param name="animFPS">The frames per second of the main trail animation. The longer the animation lasts, the longer the trail will linger.</param>
        /// <param name="startAnimPaths">The list of sprites for the first segment of the trail, aka the part right next to the gun barrel. Used for 'flaring' trail effects. Can be set to the same list as the regular animation.</param>
        /// <param name="startAnimFPS">The frames per second of the 'start' animation.</param>
        /// <param name="cascadeTimer">How quickly the animation will progress between each segment of the trail once it begins to play. Aka: How fast will the trail dissipate.</param>
        /// <param name="softMaxLength">If the trail length is longer than this value, it will begin to dissipate.</param>
        /// <param name="destroyOnEmpty">Will it be destroyed if it isnt visible anymore? No idea.</param>
        /// <param name="emissive">If set to true, the trail will glow.</param>
        public static void AddTrailToProjectile(this Projectile target, string spritePath, Vector2 colliderDimensions, Vector2 colliderOffsets, List<string> animPaths = null, int animFPS = -1, List<string> startAnimPaths = null, int startAnimFPS = -1, float timeTillAnimStart = -1, float cascadeTimer = -1, float softMaxLength = -1, bool emissive = false, bool destroyOnEmpty = true)
        {
            try
            {
                GameObject newTrailObject = new GameObject();
                FakePrefab.InstantiateAndFakeprefab(newTrailObject);
                newTrailObject.transform.parent = target.transform;

                float convertedColliderX = colliderDimensions.x / 16f;
                float convertedColliderY = colliderDimensions.y / 16f;
                float convertedOffsetX = colliderOffsets.x / 16f;
                float convertedOffsetY = colliderOffsets.y / 16f;

                int spriteID = SpriteBuilder.AddSpriteToCollection(spritePath, ETGMod.Databases.Items.ProjectileCollection, Assembly.GetCallingAssembly());
                tk2dTiledSprite tiledSprite = newTrailObject.GetOrAddComponent<tk2dTiledSprite>();

                tiledSprite.SetSprite(ETGMod.Databases.Items.ProjectileCollection, spriteID);
                tk2dSpriteDefinition def = tiledSprite.GetCurrentSpriteDef();
                def.colliderVertices = new Vector3[]{
                    new Vector3(convertedOffsetX, convertedOffsetY, 0f),
                    new Vector3(convertedColliderX, convertedColliderY, 0f)
                };

                def.ConstructOffsetsFromAnchor(tk2dBaseSprite.Anchor.MiddleLeft);

                tk2dSpriteAnimator animator = newTrailObject.GetOrAddComponent<tk2dSpriteAnimator>();
                tk2dSpriteAnimation animation = newTrailObject.GetOrAddComponent<tk2dSpriteAnimation>();
                animation.clips = new tk2dSpriteAnimationClip[0];
                animator.Library = animation;

                TrailController trail = newTrailObject.AddComponent<TrailController>();

                //---------------- Sets up the animation for the main part of the trail
                if (animPaths != null)
                {
                    BeamAPI.SetupBeamPart(animation, animPaths, "trail_mid", animFPS, Assembly.GetCallingAssembly(), null, null, def.colliderVertices);
                    trail.animation = "trail_mid";
                    trail.usesAnimation = true;
                }
                else
                {
                    trail.usesAnimation = false;
                }

                if (startAnimPaths != null)
                {
                    BeamAPI.SetupBeamPart(animation, startAnimPaths, "trail_start", startAnimFPS, Assembly.GetCallingAssembly(), null, null, def.colliderVertices);
                    trail.startAnimation = "trail_start";
                    trail.usesStartAnimation = true;
                }
                else
                {
                    trail.usesStartAnimation = false;
                }

                //Trail Variables
                if (softMaxLength > 0) { trail.usesSoftMaxLength = true; trail.softMaxLength = softMaxLength; }
                if (cascadeTimer > 0) { trail.usesCascadeTimer = true; trail.cascadeTimer = cascadeTimer; }
                if (timeTillAnimStart > 0) { trail.usesGlobalTimer = true; trail.globalTimer = timeTillAnimStart; }
                if (emissive) { target.gameObject.GetOrAddComponent<EmmisiveTrail>(); }
                trail.destroyOnEmpty = destroyOnEmpty;
            }
            catch (Exception e)
            {
                ETGModConsole.Log(e.ToString());
            }
        }
        public class EmmisiveTrail : MonoBehaviour
        {
            public EmmisiveTrail()
            {
                this.EmissivePower = 75;
                this.EmissiveColorPower = 1.55f;
                debugLogging = false;
            }
            public void Start()
            {
                Shader glowshader = ShaderCache.Acquire("Brave/LitTk2dCustomFalloffTiltedCutoutEmissive");

                foreach (Transform transform in base.transform)
                {

                    tk2dBaseSprite sproot = transform.GetComponent<tk2dBaseSprite>();
                    if (sproot != null)
                    {
                        if (debugLogging) Debug.Log($"Checks were passed for transform; {transform.name}");
                        sproot.usesOverrideMaterial = true;
                        sproot.renderer.material.shader = glowshader;
                        sproot.renderer.material.EnableKeyword("BRIGHTNESS_CLAMP_ON");
                        sproot.renderer.material.SetFloat("_EmissivePower", EmissivePower);
                        sproot.renderer.material.SetFloat("_EmissiveColorPower", EmissiveColorPower);
                    }
                    else
                    {
                        if (debugLogging) Debug.Log("Sprite was null");
                    }
                }
            }
            private List<string> TransformList = new List<string>()
        {
            "trailObject",
        };
            public float EmissivePower;
            public float EmissiveColorPower;
            public bool debugLogging;
        }
    }
}
