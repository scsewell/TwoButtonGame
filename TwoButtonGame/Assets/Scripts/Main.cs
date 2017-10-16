using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Framework;
using Framework.Interpolation;

public class Main : ComponentSingleton<Main>
{
    [SerializeField]
    private Camera m_clearCameraPrefab;
    [SerializeField]
    private CameraManager m_cameraPrefab;
    
    [SerializeField] [Range(0.1f, 5)]
    private float m_fadeInTime = 1.0f;

    private List<Player> m_players = new List<Player>();
    private List<CameraManager> m_cameras = new List<CameraManager>();

    private RacePath m_racePath;
    public RacePath RacePath { get { return m_racePath; } }

    private float m_raceStartTime;
    private int m_countdownSecond;

    public float CountdownTime
    {
        get { return m_raceStartTime - Time.time; }
    }

    public float FadeFactor
    {
        get { return 1 - Mathf.Clamp01((Time.time - (m_raceStartTime - m_levelConfig.CountdownDuration)) / m_fadeInTime); }
    }

    private LevelConfig m_levelConfig;
    private bool m_racing = false;

    protected override void Awake()
    {
        base.Awake();
        DontDestroyOnLoad(gameObject);
    }
    
    private void Update()
    {
        InterpolationController.Instance.VisualUpdate();
        InputManager.Instance.Update();

        if (m_racing)
        {
            int countdownSecond = Mathf.CeilToInt(CountdownTime);
            if (countdownSecond != m_countdownSecond && 0 <= countdownSecond && countdownSecond <= 3)
            {
                AudioManager.Instance.PlaySound(countdownSecond == 0 ? m_levelConfig.GoSound : m_levelConfig.CountdownSound);
            }
            m_countdownSecond = countdownSecond;
        }
    }

    private void LateUpdate()
    {
        InputManager.Instance.LateUpdate();
    }

    private void FixedUpdate()
    {
        InterpolationController.Instance.EarlyFixedUpdate();

        if (m_racing)
        {
            m_players.ForEach(p => p.MainUpdate());
            m_cameras.ForEach(c => c.MainUpdate());
        }
    }

    public void LoadMenu()
    {
        m_racing = false;
        SceneManager.LoadScene(0);
    }

    public AsyncOperation LoadRace(LevelConfig levelConfig, List<PlayerConfig> playerConfigs, List<PlayerInput> inputs)
    {
        m_racing = true;
        m_levelConfig = levelConfig;

        AsyncOperation loading = SceneManager.LoadSceneAsync(levelConfig.SceneName);
        loading.allowSceneActivation = false;

        StartCoroutine(StartLevel(loading, playerConfigs, inputs));
        return loading;
    }
    
    private IEnumerator StartLevel(AsyncOperation loading, List<PlayerConfig> playerConfigs, List<PlayerInput> inputs)
    {
        yield return new WaitWhile(() => !loading.allowSceneActivation);
        yield return null;

        Instantiate(m_clearCameraPrefab);
        m_racePath = FindObjectOfType<RacePath>();

        int playerCount = playerConfigs.Count;
        List<Vector3> positions = new List<Vector3>();
        
        Vector3 spacing = 0.5f * m_levelConfig.StartSpacing * Vector3.right;
        for (int playerNum = 0; playerNum < playerCount; playerNum++)
        {
            positions.Add((playerNum - ((playerCount - 1) - playerNum)) * spacing);
        }

        for (int playerNum = 0; playerNum < playerCount; playerNum++)
        {
            int index = Random.Range(0, positions.Count);
            Vector3 pos = positions[index];
            positions.RemoveAt(index);

            Player player = GameObject.Instantiate(playerConfigs[playerNum].PlayerPrefab, pos, Quaternion.identity);
            player.Init(playerNum, inputs[playerNum], playerConfigs[playerNum]);
            m_players.Add(player);

            CameraManager camera = GameObject.Instantiate(m_cameraPrefab);
            camera.Init(player, playerNum, playerCount);
            m_cameras.Add(camera);
        }

        m_raceStartTime = Time.time + m_levelConfig.CountdownDuration;
    }

    public float GetTimeSinceStart(float time)
    {
        return Mathf.Max(time - m_raceStartTime, 0);
    }

    public int GetPlayerRank(Player player)
    {
        return m_players.Count(p => p != player && (
            (p.IsFinished && player.IsFinished && p.FinishTime < player.FinishTime) ||
            !player.IsFinished && (
            (p.WaypointsCompleted > player.WaypointsCompleted) ||
            (p.WaypointsCompleted == player.WaypointsCompleted && Vector3.Distance(p.CurrentWaypoint.Position, p.transform.position) < Vector3.Distance(player.CurrentWaypoint.Position, player.transform.position))
            ))) + 1;
    }
}
