using System.Collections;
using UnityEngine;

public class StateExecutor : MonoBehaviour
{
    private Coroutine stateCoroutine;

    public bool IsCoroutineRunning => stateCoroutine != null;

    public void CallStateCoroutine(float time)
    {
        Interrupt();

        stateCoroutine = StartCoroutine(StartStateCoroutine(time));
    }

    private IEnumerator StartStateCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        stateCoroutine = null;
    }

    public void Interrupt()
    {
        if (stateCoroutine != null)
        {
            StopCoroutine(stateCoroutine);
            stateCoroutine = null;
        }
    }
}