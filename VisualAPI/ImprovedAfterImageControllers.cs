using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Alexandria.VisualAPI
{
    public class ImprovedAfterImage : BraveBehaviour
    {

        public ImprovedAfterImage()
        {
            shaders = new List<Shader>
            {
                ShaderCache.Acquire("Brave/Internal/RainbowChestShader"),
                ShaderCache.Acquire("Brave/Internal/GlitterPassAdditive"),
                ShaderCache.Acquire("Brave/Internal/HologramShader"),
                ShaderCache.Acquire("Brave/Internal/HighPriestAfterImage")
            };
            //shaders.Add(ShaderCache.Acquire("Brave/ItemSpecific/MetalSkinShader"));
            this.IsRandomShader = false;
            this.spawnShadows = true;
            this.shadowTimeDelay = 0.1f;
            this.shadowLifetime = 0.6f;
            this.minTranslation = 0.2f;
            this.maxEmission = 800f;
            this.minEmission = 100f;
            this.targetHeight = -2f;
            this.dashColor = new Color(1f, 0f, 1f, 1f);
            this.m_activeShadows = new LinkedList<Shadow>();
            this.m_inactiveShadows = new LinkedList<Shadow>();
            this.OverrideImageShader = null;
        }

        public void Start()
        {
            if (this.OptionalImageShader != null)
            {
                this.OverrideImageShader = this.OptionalImageShader;
            }
            if (base.transform.parent != null && base.transform.parent.GetComponent<Projectile>() != null)
            {
                base.transform.parent.GetComponent<Projectile>().OnDestruction += this.ProjectileDestruction;
            }
            this.m_lastSpawnPosition = base.transform.position;
        }

        private void ProjectileDestruction(Projectile source)
        {
            if (this.m_activeShadows.Count > 0)
            {
                GameManager.Instance.StartCoroutine(this.HandleDeathShadowCleanup());
            }
        }

        public void LateUpdate()
        {
            if (this.spawnShadows && !this.m_previousFrameSpawnShadows)
            {
                this.m_spawnTimer = this.shadowTimeDelay;
            }
            this.m_previousFrameSpawnShadows = this.spawnShadows;
            LinkedListNode<ImprovedAfterImage.Shadow> next;
            for (LinkedListNode<ImprovedAfterImage.Shadow> linkedListNode = this.m_activeShadows.First; linkedListNode != null; linkedListNode = next)
            {
                next = linkedListNode.Next;
                linkedListNode.Value.timer -= BraveTime.DeltaTime;
                if (linkedListNode.Value.timer <= 0f)
                {
                    this.m_activeShadows.Remove(linkedListNode);
                    this.m_inactiveShadows.AddLast(linkedListNode);
                    if (linkedListNode.Value.sprite)
                    {
                        linkedListNode.Value.sprite.renderer.enabled = false;
                    }
                }
                else if (linkedListNode.Value.sprite)
                {
                    float num = linkedListNode.Value.timer / this.shadowLifetime;
                    Material sharedMaterial = linkedListNode.Value.sprite.renderer.sharedMaterial;
                    sharedMaterial.SetFloat("_EmissivePower", Mathf.Lerp(this.maxEmission, this.minEmission, num));
                    sharedMaterial.SetFloat("_Opacity", num);
                }
            }
            if (this.spawnShadows)
            {
                if (this.m_spawnTimer > 0f)
                {
                    this.m_spawnTimer -= BraveTime.DeltaTime;
                }
                if (this.m_spawnTimer <= 0f && Vector2.Distance(this.m_lastSpawnPosition, base.transform.position) > this.minTranslation)
                {
                    this.SpawnNewShadow();
                    this.m_spawnTimer += this.shadowTimeDelay;
                    this.m_lastSpawnPosition = base.transform.position;
                }
            }
        }

        private IEnumerator HandleDeathShadowCleanup()
        {
            while (this.m_activeShadows.Count > 0)
            {
                LinkedListNode<ImprovedAfterImage.Shadow> next;
                for (LinkedListNode<ImprovedAfterImage.Shadow> node = this.m_activeShadows.First; node != null; node = next)
                {
                    next = node.Next;
                    node.Value.timer -= BraveTime.DeltaTime;
                    if (node.Value.timer <= 0f)
                    {
                        this.m_activeShadows.Remove(node);
                        this.m_inactiveShadows.AddLast(node);
                        if (node.Value.sprite)
                        {
                            node.Value.sprite.renderer.enabled = false;
                        }
                    }
                    else if (node.Value.sprite)
                    {
                        float num = node.Value.timer / this.shadowLifetime;
                        Material sharedMaterial = node.Value.sprite.renderer.sharedMaterial;
                        sharedMaterial.SetFloat("_EmissivePower", Mathf.Lerp(this.maxEmission, this.minEmission, num));
                        sharedMaterial.SetFloat("_Opacity", num);
                    }
                }
                yield return null;
            }
            yield break;
        }

        public override void OnDestroy()
        {
            GameManager.Instance.StartCoroutine(this.HandleDeathShadowCleanup());
            base.OnDestroy();
        }


        private void SpawnNewShadow()
        {
            if (this.m_inactiveShadows == null)
            {
                return;
            }

            if (this.m_inactiveShadows.Count == 0)
            {
                this.CreateInactiveShadow();
            }

            LinkedListNode<ImprovedAfterImage.Shadow> first = this.m_inactiveShadows.First;
            tk2dSprite sprite = first.Value.sprite;
            this.m_inactiveShadows.RemoveFirst();
            if (!sprite || !sprite.renderer)
            {
                return;
            }


            first.Value.timer = this.shadowLifetime;
            sprite.SetSprite(base.sprite.Collection, base.sprite.spriteId);
            sprite.transform.position = base.sprite.transform.position;
            sprite.transform.rotation = base.sprite.transform.rotation;
            sprite.scale = base.sprite.scale;
            sprite.usesOverrideMaterial = true;
            sprite.IsPerpendicular = true;


            if (sprite.renderer && IsRandomShader)
            {
                sprite.renderer.enabled = true;
                sprite.renderer.material.shader = shaders[(int)UnityEngine.Random.Range(0, shaders.Count)];

                if (sprite.renderer.material.shader == shaders[3])
                {
                    sprite.renderer.sharedMaterial.SetFloat("_EmissivePower", this.minEmission);
                    sprite.renderer.sharedMaterial.SetFloat("_Opacity", 1f);
                    sprite.renderer.sharedMaterial.SetColor("_DashColor", Color.HSVToRGB(UnityEngine.Random.value, 1.0f, 1.0f));
                }
                if (sprite.renderer.material.shader == shaders[0])
                {
                    sprite.renderer.sharedMaterial.SetFloat("_AllColorsToggle", 1f);
                }
            }
            else if (sprite.renderer)
            {

                sprite.renderer.enabled = true;
                sprite.renderer.material.shader = (this.OverrideImageShader ?? ShaderCache.Acquire("Brave/Internal/HighPriestAfterImage"));
                sprite.renderer.sharedMaterial.SetFloat("_EmissivePower", this.minEmission);
                sprite.renderer.sharedMaterial.SetFloat("_Opacity", 1f);
                sprite.renderer.sharedMaterial.SetColor("_DashColor", this.dashColor);
                sprite.renderer.sharedMaterial.SetFloat("_AllColorsToggle", 0f);
            }

            sprite.HeightOffGround = this.targetHeight;
            sprite.UpdateZDepth();
            this.m_activeShadows.AddLast(first);

        }

        public bool IsRandomShader;

        private void CreateInactiveShadow()
        {
            GameObject gameObject = new GameObject("after image");
            if (this.UseTargetLayer)
            {
                gameObject.layer = LayerMask.NameToLayer(this.TargetLayer);
            }
            tk2dSprite sprite = gameObject.AddComponent<tk2dSprite>();
            gameObject.transform.parent = SpawnManager.Instance.VFX;
            this.m_inactiveShadows.AddLast(new ImprovedAfterImage.Shadow
            {
                timer = this.shadowLifetime,
                sprite = sprite
            });
        }


        public bool spawnShadows;

        public float shadowTimeDelay;

        public float shadowLifetime;

        public float minTranslation;

        public float maxEmission;

        public float minEmission;

        public float targetHeight;

        public Color dashColor;

        public Shader OptionalImageShader;

        public bool UseTargetLayer;

        public string TargetLayer;

        [NonSerialized]
        public Shader OverrideImageShader;

        private readonly LinkedList<ImprovedAfterImage.Shadow> m_activeShadows;

        private readonly LinkedList<ImprovedAfterImage.Shadow> m_inactiveShadows;

        private readonly List<Shader> shaders;

        private float m_spawnTimer;

        private Vector2 m_lastSpawnPosition;

        private bool m_previousFrameSpawnShadows;

        private class Shadow
        {
            public float timer;
            public tk2dSprite sprite;
        }
    }

}

namespace Planetside
{
    public class ImprovedAfterImageForTiled : BraveBehaviour
    {

        public ImprovedAfterImageForTiled()
        {
            shaders = new List<Shader>
            {
                ShaderCache.Acquire("Brave/Internal/RainbowChestShader"),
                ShaderCache.Acquire("Brave/Internal/GlitterPassAdditive"),
                ShaderCache.Acquire("Brave/Internal/HologramShader"),
                ShaderCache.Acquire("Brave/Internal/HighPriestAfterImage")
            };
            //shaders.Add(ShaderCache.Acquire("Brave/ItemSpecific/MetalSkinShader"));
            this.IsRandomShader = false;
            this.spawnShadows = true;
            this.shadowTimeDelay = 0.1f;
            this.shadowLifetime = 0.6f;
            this.minTranslation = 0.2f;
            this.maxEmission = 800f;
            this.minEmission = 100f;
            this.targetHeight = -2f;
            this.dashColor = new Color(1f, 0f, 1f, 1f);
            this.m_activeShadows = new LinkedList<Shadow>();
            this.m_inactiveShadows = new LinkedList<Shadow>();
        }

        public void Start()
        {
            if (this.OptionalImageShader != null)
            {
                this.OverrideImageShader = this.OptionalImageShader;
            }
            if (base.transform.parent != null && base.transform.parent.GetComponent<Projectile>() != null)
            {
                base.transform.parent.GetComponent<Projectile>().OnDestruction += this.ProjectileDestruction;
            }
            this.lastSpawnAngle = base.transform.eulerAngles.z;
        }

        private void ProjectileDestruction(Projectile source)
        {
            if (this.m_activeShadows.Count > 0)
            {
                GameManager.Instance.StartCoroutine(this.HandleDeathShadowCleanup());
            }
        }

        public void LateUpdate()
        {

            if (this.spawnShadows && !this.m_previousFrameSpawnShadows)
            {
                this.m_spawnTimer = this.shadowTimeDelay;
            }
            this.m_previousFrameSpawnShadows = this.spawnShadows;
            LinkedListNode<ImprovedAfterImageForTiled.Shadow> next;
            for (LinkedListNode<ImprovedAfterImageForTiled.Shadow> linkedListNode = this.m_activeShadows.First; linkedListNode != null; linkedListNode = next)
            {
                next = linkedListNode.Next;
                linkedListNode.Value.timer -= BraveTime.DeltaTime;
                if (linkedListNode.Value.timer <= 0f)
                {
                    this.m_activeShadows.Remove(linkedListNode);
                    this.m_inactiveShadows.AddLast(linkedListNode);
                    if (linkedListNode.Value.sprite)
                    {
                        linkedListNode.Value.sprite.renderer.enabled = false;
                    }
                }
                else if (linkedListNode.Value.sprite)
                {
                    float num = linkedListNode.Value.timer / this.shadowLifetime;
                    Material sharedMaterial = linkedListNode.Value.sprite.renderer.sharedMaterial;
                    sharedMaterial.SetFloat("_EmissivePower", Mathf.Lerp(this.maxEmission, this.minEmission, num));
                    sharedMaterial.SetFloat("_Opacity", num);
                }
            }
            if (this.spawnShadows)// && CanTrigger)
            {
                if (base.GetComponent<tk2dTiledSprite>() == null) { ETGModConsole.Log("fucc"); }
                if (this.m_spawnTimer > 0f)
                {
                    this.m_spawnTimer -= BraveTime.DeltaTime;
                }
                if (this.m_spawnTimer <= 0f)
                {
                    this.SpawnNewShadow();
                    this.m_spawnTimer += this.shadowTimeDelay;
                    this.lastSpawnAngle = base.transform.eulerAngles.z;
                }
            }
        }

        private IEnumerator HandleDeathShadowCleanup()
        {
            while (this.m_activeShadows.Count > 0)
            {
                LinkedListNode<ImprovedAfterImageForTiled.Shadow> next;
                for (LinkedListNode<ImprovedAfterImageForTiled.Shadow> node = this.m_activeShadows.First; node != null; node = next)
                {
                    next = node.Next;
                    node.Value.timer -= BraveTime.DeltaTime;
                    if (node.Value.timer <= 0f)
                    {
                        this.m_activeShadows.Remove(node);
                        this.m_inactiveShadows.AddLast(node);
                        if (node.Value.sprite)
                        {
                            node.Value.sprite.renderer.enabled = false;
                        }
                    }
                    else if (node.Value.sprite)
                    {
                        float num = node.Value.timer / this.shadowLifetime;
                        Material sharedMaterial = node.Value.sprite.renderer.sharedMaterial;
                        sharedMaterial.SetFloat("_EmissivePower", Mathf.Lerp(this.maxEmission, this.minEmission, num));
                        sharedMaterial.SetFloat("_Opacity", num);
                    }
                }
                yield return null;
            }
            yield break;
        }

        public override void OnDestroy()
        {
            GameManager.Instance.StartCoroutine(this.HandleDeathShadowCleanup());
            base.OnDestroy();
        }


        private void SpawnNewShadow()
        {

            if (base.GetComponentInChildren<tk2dTiledSprite>() == null) { ETGModConsole.Log("tk2dTiledSprite is NULL"); return; }
            if (base.GetComponent<tk2dTiledSprite>() == null) { ETGModConsole.Log("tk2dTiledSprite is NULL"); return; }
            if (this.m_inactiveShadows.Count == 0)
            {
                this.CreateInactiveShadow();
            }


            LinkedListNode<ImprovedAfterImageForTiled.Shadow> first = this.m_inactiveShadows.First;
            tk2dTiledSprite sprite = first.Value.sprite;
            this.m_inactiveShadows.RemoveFirst();
            if (!sprite || !sprite.renderer)
            {
                return;
            }

            first.Value.timer = this.shadowLifetime;


            sprite.SetSprite(base.GetComponent<tk2dTiledSprite>().sprite.Collection, base.GetComponent<tk2dTiledSprite>().sprite.spriteId);

            sprite.transform.position = base.GetComponent<tk2dTiledSprite>().sprite.transform.position;

            sprite.transform.rotation = base.GetComponent<tk2dTiledSprite>().sprite.transform.rotation;

            if (base.transform.parent != null)
            {
                if (base.transform.parent.GetComponentInChildren<BasicBeamController>() != null)
                {

                    float angle = base.transform.parent.GetComponentInChildren<BasicBeamController>().Direction.ToAngle();

                    sprite.transform.rotation = Quaternion.Euler(0, 0, angle);

                }
                if (base.transform.parent.GetComponentInChildren<BeamController>() != null)
                {
                    float angle = base.transform.parent.GetComponentInChildren<BeamController>().Direction.ToAngle();
                    sprite.transform.rotation = Quaternion.Euler(0, 0, angle);
                }
            }


            sprite.scale = base.GetComponent<tk2dTiledSprite>().sprite.scale;
            sprite.dimensions = base.GetComponent<tk2dTiledSprite>().dimensions;
            sprite.usesOverrideMaterial = true;
            sprite.IsPerpendicular = true;
            sprite.renderer.enabled = true;
            if (overrideHeight != -1)
            {
                sprite.renderer.gameObject.layer = overrideHeight;

            }

            if (sprite.renderer && IsRandomShader)
            {
                sprite.renderer.enabled = true;
                sprite.renderer.material.shader = shaders[(int)UnityEngine.Random.Range(0, shaders.Count)];

                if (sprite.renderer.material.shader == shaders[3])
                {
                    sprite.renderer.sharedMaterial.SetFloat("_EmissivePower", this.minEmission);
                    sprite.renderer.sharedMaterial.SetFloat("_Opacity", 1f);
                    sprite.renderer.sharedMaterial.SetColor("_DashColor", Color.HSVToRGB(UnityEngine.Random.value, 1.0f, 1.0f));
                }
                if (sprite.renderer.material.shader == shaders[0])
                {
                    sprite.renderer.sharedMaterial.SetFloat("_AllColorsToggle", 1f);
                }
            }
            else if (sprite.renderer)
            {
                sprite.renderer.enabled = true;
                sprite.renderer.material.shader = (this.OverrideImageShader ?? ShaderCache.Acquire("Brave/Internal/HighPriestAfterImage"));
                sprite.renderer.sharedMaterial.SetFloat("_EmissivePower", this.minEmission);
                sprite.renderer.sharedMaterial.SetFloat("_Opacity", 1f);
                sprite.renderer.sharedMaterial.SetColor("_DashColor", this.dashColor);
                sprite.renderer.sharedMaterial.SetFloat("_AllColorsToggle", 1f);
            }

            sprite.HeightOffGround = this.targetHeight;
            sprite.UpdateZDepth();
            this.m_activeShadows.AddLast(first);
        }

        public bool IsRandomShader;

        private void CreateInactiveShadow()
        {
            GameObject gameObject = new GameObject("after image");
            if (this.UseTargetLayer)
            {
                gameObject.layer = LayerMask.NameToLayer(this.TargetLayer);
            }
            //gameObject.AddComponent<tk2dBaseSprite>();
            tk2dTiledSprite sprite = gameObject.AddComponent<tk2dTiledSprite>();
            gameObject.transform.parent = SpawnManager.Instance.VFX;
            this.m_inactiveShadows.AddLast(new ImprovedAfterImageForTiled.Shadow
            {
                timer = this.shadowLifetime,
                sprite = sprite
            });
        }

        public int overrideHeight = -1;

        public bool spawnShadows;

        public float shadowTimeDelay;

        public float shadowLifetime;

        public float minTranslation;

        public float maxEmission;

        public float minEmission;

        public float targetHeight;

        public Color dashColor;

        public Shader OptionalImageShader;

        public bool UseTargetLayer;

        public string TargetLayer;

        [NonSerialized]
        public Shader OverrideImageShader;

        private readonly LinkedList<ImprovedAfterImageForTiled.Shadow> m_activeShadows;

        private readonly LinkedList<ImprovedAfterImageForTiled.Shadow> m_inactiveShadows;

        private readonly List<Shader> shaders;

        private float m_spawnTimer;

        private float lastSpawnAngle;

        private bool m_previousFrameSpawnShadows;

        private class Shadow
        {
            public float timer;
            public tk2dTiledSprite sprite;
        }
    }
}
