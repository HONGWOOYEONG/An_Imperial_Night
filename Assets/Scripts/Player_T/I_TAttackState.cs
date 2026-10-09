using UnityEngine;

public interface I_TAttackState 
{
    public void Enter(T_Attack t_Attack);
    public void Update(T_Attack t_Attack);
    public void Exit(T_Attack t_Attack);
}
