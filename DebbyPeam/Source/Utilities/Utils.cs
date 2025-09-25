using PEAKLib.Items;
using UnityEngine;
namespace DebbyPeam.Utils
{
    public class DebbyPeamUtils
    {
        readonly BepInEx.Logging.ManualLogSource log = DebbyPeam.instance.log;
        public Character ClosestCharacter(Vector3 pos)
        {
            Character character = null;
            float distance = float.MaxValue;
            for (int i = 0; i < Character.AllCharacters.Count; i++)
            {
                float newDistance = Vector3.Distance(pos, Character.AllCharacters[i].Center);
                if (newDistance < distance)
                {
                    distance = newDistance;
                    character = Character.AllCharacters[i];
                }
            }
            return character;
        }
        public void FixShaders(GameObject gameObject)
        {
            Renderer[] renderers = gameObject.GetComponentsInChildren<Renderer>();
            for (int i = 0; i < renderers.Length; i++)
            {
                for (int j = 0; j < renderers[i].materials.Length; j++)
                {
                    if (ItemRegistrar.PeakShaders.TryGetValue(renderers[i].materials[j].shader.name, out Shader peakShader))
                    {
                        renderers[i].materials[j].shader = peakShader;
                    }
                    else
                    {
                        peakShader = Shader.Find(renderers[i].materials[j].shader.name);
                        if (peakShader != null)
                        {
                            renderers[i].materials[j].shader = peakShader;
                        }
                    }
                    log.LogDebug($"Attempting to fix the shaders for material \"{j}\" on object \"{renderers[i].gameObject.name}\"");
                }
            }
            ParticleSystem[] particleSystems = gameObject.GetComponentsInChildren<ParticleSystem>();
            for (int i = 0; i < particleSystems.Length; i++)
            {
                ParticleSystemRenderer particleRenderer = particleSystems[i].GetComponent<ParticleSystemRenderer>();
                for (int j = 0; j < particleRenderer.materials.Length; j++)
                {
                    if (ItemRegistrar.PeakShaders.TryGetValue(particleRenderer.materials[j].shader.name, out Shader peakShader))
                    {
                        particleRenderer.materials[j].shader = peakShader;
                    }
                    else
                    {
                        peakShader = Shader.Find(particleRenderer.materials[j].shader.name);
                        if (peakShader != null)
                        {
                            particleRenderer.materials[j].shader = peakShader;
                        }
                    }
                    log.LogDebug($"Attempting to fix the shaders for material \"{j}\" on object \"{particleRenderer.gameObject.name}\"");
                }
            }
        }
    }
}