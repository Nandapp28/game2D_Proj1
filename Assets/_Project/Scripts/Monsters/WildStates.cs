// P4 — tiga state perilaku monster liar. Setiap state hanya tahu kapan harus pindah.
using UnityEngine;

public class WanderState : IState
{
    private readonly WildMonster monster;
    private float timer;
    private Vector2 direction;

    public WanderState(WildMonster monster) => this.monster = monster;

    public void Enter() => timer = 0f;

    public void Tick(float deltaTime)
    {
        if (monster.CanSeeTarget) { monster.ChangeState(monster.Chase); return; }

        timer -= deltaTime;
        if (timer <= 0f)
        {
            timer = Random.Range(0.8f, 2f);
            direction = Random.value < 0.6f ? Random.insideUnitCircle.normalized : Vector2.zero;
        }
        Vector2 toHome = monster.Home - (Vector2)monster.transform.position;
        if (toHome.magnitude > monster.HomeRadius) direction = toHome.normalized;
        monster.Move(direction * monster.WanderSpeed);
    }

    public void Exit() => monster.Move(Vector2.zero);
}

public class ChaseState : IState
{
    private readonly WildMonster monster;

    public ChaseState(WildMonster monster) => this.monster = monster;

    public void Enter() { }

    public void Tick(float deltaTime)
    {
        if (monster.LostTarget) { monster.ChangeState(monster.Return); return; }
        Vector2 toTarget = (Vector2)(monster.Target.position - monster.transform.position);
        monster.Move(toTarget.normalized * monster.ChaseSpeed);
    }

    public void Exit() => monster.Move(Vector2.zero);
}

public class ReturnState : IState
{
    private readonly WildMonster monster;

    public ReturnState(WildMonster monster) => this.monster = monster;

    public void Enter() { }

    public void Tick(float deltaTime)
    {
        if (monster.CanSeeTarget) { monster.ChangeState(monster.Chase); return; }
        Vector2 toHome = monster.Home - (Vector2)monster.transform.position;
        if (toHome.magnitude < 0.2f) { monster.ChangeState(monster.Wander); return; }
        monster.Move(toHome.normalized * monster.WanderSpeed);
    }

    public void Exit() => monster.Move(Vector2.zero);
}
