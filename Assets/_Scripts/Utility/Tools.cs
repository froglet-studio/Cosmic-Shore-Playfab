using System;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace StarWriter.Utility.Tools
{
    public struct Tools
    {
        public IEnumerator LerpingCoroutine(System.Action<float> replacementMethod, System.Func<float> getCurrent, float newValue, float duration, int steps)
        {
            float elapsedTime = 0;
            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                replacementMethod(Mathf.Lerp(getCurrent(), newValue, elapsedTime / duration));
                yield return new WaitForSeconds(duration / (float)steps);
            }
        }

        public IEnumerator LateStart(float seconds, string functionName)
        {
            yield return new WaitForSeconds(seconds);

            Type thisType = this.GetType();
            MethodInfo method = thisType.GetMethod(functionName);
            if (method != null)
            {
                Action function = (Action)Delegate.CreateDelegate(typeof(Action), this, method);
                function();
            }
            else
            {
                Debug.LogError("Could not find function: " + functionName);
            }
        }

    }
}

