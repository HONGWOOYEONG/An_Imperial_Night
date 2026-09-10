using UnityEngine;

public interface IPuppeteerState
{
    public void Enter(E_PuppeteerController controller);
    public void Update(E_PuppeteerController controller);
    public void Exit(E_PuppeteerController controller);
}
