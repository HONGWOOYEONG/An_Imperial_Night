using System.Collections;
using UnityEngine;

public class StateExecutor : MonoBehaviour
{
    private Coroutine _stateCoroutine;

    public void CallStateCoroutine(float flames)
    {
        if (_stateCoroutine != null)
        {
            Interrupt();
        }

        float time = flames / 30;
        _stateCoroutine = StartCoroutine(StartStateCoroutine(time));
    }

    IEnumerator StartStateCoroutine(float time)
    {
        yield return new WaitForSeconds(time);
        _stateCoroutine = null;
    }

    public void Interrupt()
    {
        if (_stateCoroutine != null)
        {
            StopCoroutine(_stateCoroutine); 
            _stateCoroutine = null;
        }
    }
}