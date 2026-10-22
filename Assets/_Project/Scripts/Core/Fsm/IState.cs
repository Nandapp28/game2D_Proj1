// P4 — kontrak sebuah state pada Finite State Machine.
public interface IState
{
    void Enter();
    void Tick(float deltaTime);
    void Exit();
}
