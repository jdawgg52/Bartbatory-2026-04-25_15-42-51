using System.Collections;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    
    public static MapGeneration Instance { get; private set; }
    [Header("References")]
    public MapGeneration BoardManager;
    public PlayerController PlayerController;
    public EnemySpawner enemySpawner;
    public UpgradeUIController upgrades;
    [Header("UI")]
    public UIDocument gameUIDocument;
    public float fadeInDuration = 1.5f;
    private VisualElement fadeScreen;
    [Header("Spawn Settings")]
    private float spawnInterval = 3f;   
    public float spawnIntervalOrginal = 3f;
    public float offscreenMargin = 2f;
    private int Scaling = 1;
    public int ScalingOrginal = 10;
    public float postBossSpawnInterval = 2f;
    public int PostBossScaling = 5;
    public bool done = false;
    public int level = 1;
    private Camera mainCam;

    [SerializeField] AudioSource BossMusic;
    void Awake()
    {
        mainCam = Camera.main;
    }
public void OnBossDefeated()
    {
        ClearEnemies();
        ClearXp();
        ClearProjectiles();
        done = true;
        Scaling = ScalingOrginal;
        spawnInterval = spawnIntervalOrginal;
        if(level == 2)
        {
            PlayerController.EndScreen();
        }
        fadeScreen = gameUIDocument.rootVisualElement.Q<VisualElement>("FadeScreen");

        if (fadeScreen != null)
        {
            fadeScreen.style.display = DisplayStyle.Flex;
            fadeScreen.style.opacity = 1f;
            StartCoroutine(FadeInFromBlack());
        }
        PlayerController.Spawn(BoardManager, new Vector2Int(80, 5));
        done = false;
        StartCoroutine(SpawnEnemiesLoop(level++));

    }
    void Start()
    {
        if (gameUIDocument != null)
        {
            fadeScreen = gameUIDocument.rootVisualElement.Q<VisualElement>("FadeScreen");

            if (fadeScreen != null)
            {
                fadeScreen.style.display = DisplayStyle.Flex;
                fadeScreen.style.opacity = 1f;
                StartCoroutine(FadeInFromBlack());
            }
        }
    spawnInterval = spawnIntervalOrginal;
    Scaling = ScalingOrginal;
        BoardManager.Init();
        PlayerController.Spawn(BoardManager, new Vector2Int(80, 5));
        StartCoroutine(SpawnEnemiesLoop(1));
    }

    

    private IEnumerator SpawnEnemiesLoop(int level)
{
    int count = 0;

    while (!done)
    {
        yield return new WaitForSeconds(spawnInterval);

        if (PlayerController == null || PlayerController.transform == null)
            continue;

        if (count == Scaling && spawnInterval >= 0)
        {
            spawnInterval -= 0.1f;
            count = 0;
            Scaling += Scaling + (int)(Scaling * .5);
        }

        if (spawnInterval < 0)
        {
            spawnInterval = 0;
        }

        Vector3 spawnPos = BoardManager != null ? BoardManager.GetOffscreenSpawnPosition(mainCam, offscreenMargin) : GetOffscreenSpawnPosition();

            if (spawnInterval <= .1f)

            {
                ClearEnemies();
                BossMusic.Play();
                enemySpawner.SpawnBossBulk(BoardManager, PlayerController.transform, spawnPos, PlayerController, level);
                spawnInterval = postBossSpawnInterval;
                Scaling = PostBossScaling;
                count = 0;
            }
        if (spawnInterval <= 0.3f)
{
    int randomEnemy = Random.Range(0, 3);

    if (randomEnemy == 0)
    {
        enemySpawner.SpawnZomb(BoardManager, PlayerController.transform, spawnPos, PlayerController, level);
    }
    else if (randomEnemy == 1)
    {
        enemySpawner.SpawnCheetah(BoardManager, PlayerController.transform, spawnPos, PlayerController, level);
    }
    else
    {
        enemySpawner.SpawnBulk(BoardManager, PlayerController.transform, spawnPos, PlayerController, level);
    }

}
else if (spawnInterval <= 0.4f)
{
    if (Random.value < 0.5f)
    {
        enemySpawner.SpawnZomb(BoardManager, PlayerController.transform, spawnPos, PlayerController, level);
    }
    else
    {
        enemySpawner.SpawnCheetah(BoardManager, PlayerController.transform, spawnPos, PlayerController, level);
    }
}
else
{
    enemySpawner.SpawnZomb(BoardManager, PlayerController.transform, spawnPos, PlayerController, level);
}

        count++;
    }
    
}

    private Vector3 GetOffscreenSpawnPosition()
    {        Vector3 bottomLeft = mainCam.ViewportToWorldPoint(new Vector3(0f, 0f, mainCam.nearClipPlane));
        Vector3 topRight = mainCam.ViewportToWorldPoint(new Vector3(1f, 1f, mainCam.nearClipPlane));

        float minX = bottomLeft.x;
        float maxX = topRight.x;
        float minY = bottomLeft.y;
        float maxY = topRight.y;
        int side = Random.Range(0, 4); 

        float x = 0f;
        float y = 0f;

        switch (side)
        {
            case 0: // left
                x = minX - offscreenMargin;
                y = Random.Range(minY, maxY);
                break;
            case 1: // right
                x = maxX + offscreenMargin;
                y = Random.Range(minY, maxY);
                break;
            case 2: // bottom
                x = Random.Range(minX, maxX);
                y = minY - offscreenMargin;
                break;
            case 3: // top
                x = Random.Range(minX, maxX);
                y = maxY + offscreenMargin;
                break;
        }
        return new Vector3(x, y, 0f);
    }

    private void ClearEnemies()
    {
        EnemyScript[] enemies = FindObjectsOfType<EnemyScript>();
        foreach (EnemyScript enemy in enemies)
        {
            Destroy(enemy.gameObject);
        }
    }
    private void ClearXp()
    
    {
        XPOrb[] xps = FindObjectsOfType<XPOrb>();
        foreach (XPOrb xp in xps)
        {
            Destroy(xp.gameObject);
        }
    }
    private void ClearProjectiles()
    {
        Baseball[] projectiles = FindObjectsOfType<Baseball>();
        foreach (Baseball proj in projectiles)
        {
            Destroy(proj.gameObject);
        }
        BossBall[] bossProjectiles = FindObjectsOfType<BossBall>();
        foreach (BossBall proj in bossProjectiles)        {
            Destroy(proj.gameObject);
        }
    }
    private IEnumerator FadeInFromBlack()
{
    float timer = 0f;

    while (timer < fadeInDuration)
    {
        timer += Time.deltaTime;
        float t = timer / fadeInDuration;

        fadeScreen.style.opacity = Mathf.Lerp(1f, 0f, t);

        yield return null;
    }

    fadeScreen.style.opacity = 0f;
    fadeScreen.style.display = DisplayStyle.None;
}
}
