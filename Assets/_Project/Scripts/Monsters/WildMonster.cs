// P4 — monster liar di dunia: FSM Wander → Chase → Return, lalu memicu battle saat menyentuh pemain.
// P6: menerima StoryFlags — boss yang sudah kalah menandai boss_defeated dan tidak muncul lagi setelah Load.
// Versi demo: tanda "!" merah saat mengejar, tidak mengejar selama dialog / 2,2 detik setelah battle,
// dan boss mengaum dulu lewat dialog sebelum battle dimulai.
using System.Collections;
using UnityEngine;

public class WildMonster : MonoBehaviour
{
    [SerializeField] private MonsterData species;
    [SerializeField] private Vector2Int levelRange = new Vector2Int(2, 4);
    [SerializeField] private bool isBoss;
    [SerializeField] private bool stationary;
    [SerializeField] private float wanderSpeed = 1.125f;       // 18 px/detik
    [SerializeField] private float chaseSpeed = 2.5f;          // 40 px/detik
    [SerializeField] private float sightRadius = 3.25f;        // 52 px
    [SerializeField] private float giveUpRadius = 6.25f;       // 100 px
    [SerializeField] private float homeRadius = 3f;
    [SerializeField] private float respawnSeconds = 20f;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private SpriteRenderer body;
    [SerializeField] private Collider2D hitbox;
    [SerializeField] private GameObject alert;                 // versi demo: tanda "!"
    [SerializeField] private string roar = "GRRRAAAH!! Raijugar menghalangi jalan, percikan listrik memenuhi gua!";

    private BattleSession session;
    private SceneFlow scenes;
    private PlayerParty party;
    private StoryFlags flags;                                   // P6
    private InputGate gate;                                     // versi demo: terkunci = dialog/menu
    private DialogueRunner dialogue;                            // versi demo: auman boss
    private StateMachine fsm;
    private Vector2 desiredVelocity;
    private bool inBattle;
    private float cooldown;

    public Transform Target { get; private set; }
    public Vector2 Home { get; private set; }
    public float WanderSpeed => wanderSpeed;
    public float ChaseSpeed => chaseSpeed;
    public float HomeRadius => homeRadius;
    public bool IsBoss => isBoss;
    public WanderState Wander { get; private set; }
    public ChaseState Chase { get; private set; }
    public ReturnState Return { get; private set; }

    public void Construct(BattleSession battleSession, SceneFlow sceneFlow, PlayerParty playerParty,
                          StoryFlags storyFlags, InputGate inputGate, DialogueRunner dialogueRunner, Transform player)
    {
        session = battleSession;
        scenes = sceneFlow;
        party = playerParty;
        flags = storyFlags;
        gate = inputGate;
        dialogue = dialogueRunner;
        Target = player;
        if (isBoss && flags.Has(StoryFlags.BossDefeated)) gameObject.SetActive(false);   // P6
    }

    void Awake()
    {
        Home = transform.position;
        fsm = new StateMachine();
        Wander = new WanderState(this);
        Chase = new ChaseState(this);
        Return = new ReturnState(this);
    }

    void Start() => fsm.ChangeTo(Wander);

    void Update()
    {
        if (cooldown > 0f) cooldown -= Time.deltaTime;
        if (stationary || inBattle || Target == null) desiredVelocity = Vector2.zero;
        else fsm.Tick(Time.deltaTime);
        if (alert != null) alert.SetActive(fsm.Current == Chase && !inBattle);
        if (desiredVelocity.x != 0f) body.flipX = desiredVelocity.x < 0f;
    }

    void FixedUpdate() => rb.linearVelocity = desiredVelocity;

    public void Move(Vector2 velocity) => desiredVelocity = velocity;
    public void ChangeState(IState next) => fsm.ChangeTo(next);

    public float DistanceToTarget => Vector2.Distance(transform.position, Target.position);
    private bool Calm => cooldown > 0f || session.IsSafe || gate.IsLocked;
    public bool CanSeeTarget => party != null && party.Partner != null && !Calm && DistanceToTarget < sightRadius;
    public bool LostTarget => Calm || DistanceToTarget > giveUpRadius;

    void OnCollisionEnter2D(Collision2D collision) => TryEncounter(collision);
    void OnCollisionStay2D(Collision2D collision) => TryEncounter(collision);    // pemain diam menempel boss

    private void TryEncounter(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player")) StartEncounter();
    }

    private void StartEncounter()
    {
        if (inBattle || Calm || scenes.IsBusy || party == null || party.Partner == null) return;
        inBattle = true;
        int level = Random.Range(levelRange.x, levelRange.y + 1);
        session.Begin(new MonsterInstance(species, level), isBoss, OnBattleFinished);
        if (isBoss) StartCoroutine(BossIntro());
        else scenes.StartBattle();
    }

    private IEnumerator BossIntro()
    {
        dialogue.Run(Roar());
        yield return null;
        while (dialogue.IsRunning) yield return null;
        scenes.StartBattle();
    }

    private IEnumerator Roar()
    {
        yield return dialogue.Say("", roar);
    }

    // Dipanggil SceneFlow setelah Scn_World aktif kembali.
    private void OnBattleFinished(BattleResult result)
    {
        inBattle = false;
        if (result == BattleResult.Win)
        {
            if (isBoss) { flags.Set(StoryFlags.BossDefeated); gameObject.SetActive(false); return; }   // P6
            StartCoroutine(Respawn());
        }
        else
        {
            cooldown = 2.5f;                     // beri pemain waktu menjauh
            fsm.ChangeTo(Return);
        }
    }

    private IEnumerator Respawn()
    {
        SetVisible(false);
        yield return new WaitForSeconds(respawnSeconds);
        transform.position = Home;
        fsm.ChangeTo(Wander);
        SetVisible(true);
    }

    private void SetVisible(bool visible)
    {
        body.enabled = visible;
        hitbox.enabled = visible;
        desiredVelocity = Vector2.zero;
        if (alert != null) alert.SetActive(false);
        enabled = visible;
    }
}
