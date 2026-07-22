using UnityEngine;

public interface IEnemyState
{
    public void Enter(PuppeteerController controller);
    public void Update(PuppeteerController controller);
    public void Exit(PuppeteerController controller);
}
