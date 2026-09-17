/// 작성자 : 유희일
/// FSM의 기본이 되는 인터페이스

using UnityEngine;

public interface IState
{
    // 이름은 생성자에서 정해지고 바뀌지 않는다. 세터를 열면 FSM 밖에서 상태 이름을 갈아끼울 수 있게 된다.
    public string Name { get; }
    public void Enter();
    public void Tick();
    public void Exit();

}
