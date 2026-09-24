/// 작성자 : 유희일
/// FSM의 기본이 되는 인터페이스

using UnityEngine;

public interface IState
{
    // 이름은 생성자에서 정해지고 바뀌지 않는다. 세터를 열면 FSM 밖에서 상태 이름을 갈아끼울 수 있게 된다.
    public string Name { get; }
    public void Enter();
    public void Tick();

    // rb에 쓰는 일은 전부 여기서 한다. Tick(Update)에서 rb를 건드리면 한 물리 스텝에
    // 두 번 밀어넣거나 아예 건너뛰어서 벽 판정이 프레임마다 달라진다.
    public void FixedTick();
    public void Exit();

}
