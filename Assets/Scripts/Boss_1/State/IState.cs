/// 작성자 : 유희일
/// FSM의 기본이 되는 인터페이스

using UnityEngine;

public interface IState
{
    public void Enter();
    public void Tick();
    public void Exit();

}
