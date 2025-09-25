using System;
using System.Collections.Generic;
using UnityEngine;
namespace DebbyPeam.Utils
{
    [Serializable]
    public class RepeatAnim
    {
        public RepeatAnim(AnimationHandler animHandler, string animName, float min = 1f, float max = 2f, float randMin = 0f, float randMax = 0f)
        {
            if (!animHandler.nameHashTypeDict.TryGetValue(animName, out (int, AnimatorControllerParameterType) pair))
            {
                return;
            }
            anim = animHandler;
            animation = animName;
            type = pair.Item2;
            minimumTime = min;
            maximumTime = max;
            randomMin = randMin;
            randomMax = randMax;
            timer = UnityEngine.Random.Range(minimumTime, maximumTime);
        }
        public string animation;
        public AnimatorControllerParameterType type;
        public AnimationHandler anim;
        public float timer;
        public float minimumTime;
        public float maximumTime;
        public float randomMin;
        public float randomMax;
        public bool Run(out object value)
        {
            value = null;
            timer -= Time.deltaTime;
            if (timer <= 0f)
            {
                timer = UnityEngine.Random.Range(minimumTime, maximumTime);
                try
                {
                    switch (type)
                    {
                        case AnimatorControllerParameterType.Float:
                            {
                                value = randomMax;
                                if (randomMin != randomMax)
                                {
                                    value = UnityEngine.Random.Range(randomMin, randomMax);
                                }
                                break;
                            }
                        case AnimatorControllerParameterType.Int:
                            {
                                value = Mathf.FloorToInt(randomMax);
                                if (randomMin != randomMax)
                                {
                                    value = UnityEngine.Random.Range(Mathf.FloorToInt(randomMin), Mathf.FloorToInt(randomMax));
                                }
                                break;
                            }
                        case AnimatorControllerParameterType.Bool:
                            {
                                if (randomMin == randomMax)
                                {
                                    value = !(bool)anim.GetValue(animation);
                                }
                                else
                                {
                                    value = UnityEngine.Random.Range(0, 2) == 0;
                                }
                                break;
                            }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            return false;
        }
    }
    [Serializable]
    public class ItemRepeatAnim : RepeatAnim
    {
        public ItemRepeatAnim(AnimationHandler animHandler, string animation, IEnumerable<ItemState> states = null, float min = 1f, float max = 2f, float randMin = 0f, float randMax = 0f) : base(animHandler, animation, min, max, randMin, randMax)
        {
            triggerableStates = states;
            triggerableStates ??= new ItemState[] { ItemState.Ground, ItemState.Held, ItemState.InBackpack };
        }
        public IEnumerable<ItemState> triggerableStates;
    }
    public class AnimationHandler
    {
        public AnimationHandler(Animator newAnim)
        {
            animator = newAnim;
            AnimatorControllerParameter[] parameters = animator.parameters;
            Dictionary<string, (int, AnimatorControllerParameterType)> newDict = new Dictionary<string, (int, AnimatorControllerParameterType)>();
            for (int i = 0; i < parameters.Length; i++)
            {
                AnimatorControllerParameter param = parameters[i];
                newDict.Add(param.name, (param.nameHash, param.type));
            }
            nameHashTypeDict = new Dictionary<string, (int, AnimatorControllerParameterType)>(newDict);
        }
        public Animator animator;
        public Dictionary<string, (int, AnimatorControllerParameterType)> nameHashTypeDict;
        public int GetHash(string paramName)
        {
            if (nameHashTypeDict.TryGetValue(paramName, out (int, AnimatorControllerParameterType) pair))
            {
                return pair.Item1;
            }
            return -1;
        }
        public bool SetValue(string paramName, object value = null)
        {
            if (!nameHashTypeDict.ContainsKey(paramName))
            {
                return false;
            }
            (int, AnimatorControllerParameterType) pair = nameHashTypeDict[paramName];
            int hash = pair.Item1;
            switch (pair.Item2)
            {
                case AnimatorControllerParameterType.Float:
                    {
                        try
                        {
                            animator.SetFloat(hash, (float)value);
                        }
                        catch
                        {
                            Debug.Log($"{animator.name} animator tried to set float parameter but something went wrong!");
                            return false;
                        }
                        break;
                    }
                case AnimatorControllerParameterType.Int:
                    {
                        try
                        {
                            animator.SetInteger(hash, (int)value);
                        }
                        catch
                        {
                            Debug.Log($"{animator.name} animator tried to set integer parameter but something went wrong!");
                            return false;
                        }
                        break;
                    }
                case AnimatorControllerParameterType.Bool:
                    {
                        try
                        {
                            animator.SetBool(hash, (bool)value);
                        }
                        catch
                        {
                            Debug.Log($"{animator.name} animator tried to set boolean parameter but something went wrong!");
                            return false;
                        }
                        break;
                    }
                case AnimatorControllerParameterType.Trigger:
                    {
                        try
                        {
                            animator.SetTrigger(hash);
                        }
                        catch
                        {
                            Debug.Log($"{animator.name} animator tried to set trigger parameter but something went wrong!");
                            return false;
                        }
                        break;
                    }
                default:
                    {
                        return false;
                    }
            }
            return true;
        }
        public object GetValue(string paramName)
        {
            if (!nameHashTypeDict.ContainsKey(paramName))
            {
                return null;
            }
            object value;
            (int, AnimatorControllerParameterType) pair = nameHashTypeDict[paramName];
            int hash = pair.Item1;
            switch (pair.Item2)
            {
                case AnimatorControllerParameterType.Float:
                    {
                        try
                        {
                            value = animator.GetFloat(hash);
                            break;
                        }
                        catch
                        {
                            Debug.Log($"{animator.name} animator tried to get float parameter but something went wrong!");
                            return null;
                        }
                    }
                case AnimatorControllerParameterType.Int:
                    {
                        try
                        {
                            value = animator.GetInteger(hash);
                            break;
                        }
                        catch
                        {
                            Debug.Log($"{animator.name} animator tried to get int parameter but something went wrong!");
                            return null;
                        }
                    }
                case AnimatorControllerParameterType.Bool:
                    {
                        try
                        {
                            value = animator.GetBool(hash);
                            break;
                        }
                        catch
                        {
                            Debug.Log($"{animator.name} animator tried to get bool parameter but something went wrong!");
                            return null;
                        }
                    }
                default:
                    {
                        return null;
                    }
            }
            return value;
        }
        public float GetNormalizedTime(int layerIndex)
        {
            return animator.GetCurrentAnimatorStateInfo(layerIndex).normalizedTime;
        }
    }
}