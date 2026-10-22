// P4 — FSM sederhana: hanya satu state aktif dalam satu waktu.
public class StateMachine
{
    public IState Current { get; private set; }

    public void ChangeTo(IState next)
    {
        if (next == Current) return;
        Current?.Exit();
        Current = next;
        Current.Enter();
    }

    public void Tick(float deltaTime) => Current?.Tick(deltaTime);
}
