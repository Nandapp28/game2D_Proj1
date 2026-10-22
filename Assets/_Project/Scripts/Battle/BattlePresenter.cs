// P5 — Presenter (pola MVP): menghubungkan BattleSystem (logika) dengan BattleView (tampilan).
// Hidup selama event BattleSystem masih mereferensikannya, lalu ikut hilang bersama scene.
using System.Collections.Generic;

public class BattlePresenter
{
    private readonly BattleSystem battle;
    private readonly BattleView view;

    public BattlePresenter(BattleSystem battle, BattleView view)
    {
        this.battle = battle;
        this.view = view;
        battle.Started += OnStarted;
        battle.MessageShown += view.ShowMessage;
        battle.ChoiceRequested += OnChoiceRequested;
        battle.Attacked += m => PanelOf(m).PlayLunge();
        battle.Hit += OnHit;
        battle.Healed += Refresh;
        battle.Fainted += m => PanelOf(m).PlayFaint();
        battle.LevelUp += shown => view.PlayerPanel.Show(battle.Player, false, shown);   // nama lama sampai evolusi selesai
        battle.Evolving += OnEvolving;
        battle.Evolved += OnEvolved;
        view.ActionClicked += OnActionClicked;
        view.HideMenu();
    }

    private void OnStarted()
    {
        view.PlayerPanel.Show(battle.Player);
        view.EnemyPanel.Show(battle.Enemy, battle.IsBoss);
        view.PlayIntro();
    }

    private void OnChoiceRequested()
    {
        var labels = new List<string>();
        var usable = new List<bool>();
        foreach (BattleAction action in battle.PlayerActions)
        {
            labels.Add(action.MenuLabel(battle));
            usable.Add(action.CanUse(battle));
        }
        MonsterData me = battle.Player.Data;
        view.ShowMenu(labels, usable, $"Apa yang akan {me.DisplayName} lakukan?", $"Jurus elemen: {me.SkillName}");
    }

    private void OnActionClicked(int index)
    {
        BattleAction action = battle.PlayerActions[index];
        if (!action.CanUse(battle)) return;
        view.HideMenu();
        battle.Choose(action);
    }

    private void OnHit(MonsterInstance target, int damage)
    {
        MonsterPanel panel = PanelOf(target);
        panel.Show(target, target == battle.Enemy && battle.IsBoss);
        panel.PlayHurt();
    }

    private void OnEvolving(MonsterData from, MonsterData to)
    {
        view.PlayerPanel.PlayEvolve(from, to, 2.4f);
        view.PlayEvolveRings(2.4f);
    }

    private void OnEvolved(MonsterInstance monster)
    {
        view.PlayFlash();
        view.PlayerPanel.Show(monster);
    }

    private void Refresh(MonsterInstance monster) => PanelOf(monster).Show(monster, monster == battle.Enemy && battle.IsBoss);

    private MonsterPanel PanelOf(MonsterInstance m) => m == battle.Player ? view.PlayerPanel : view.EnemyPanel;
}
