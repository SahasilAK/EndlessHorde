using System.Collections;
using UnityEngine;

public class WaveSpawner : MonoBehaviour
{
    [SerializeField] private ZombieAI zombiePrefab;
    [SerializeField] private int firstWaveSize = 5;
    [SerializeField] private float timeBetweenWaves = 3f;
    [SerializeField] private float spawnDelay = 0.35f;
    [SerializeField] private float speedIncreasePerWave = 0.1f;
    [SerializeField] private float spawnPadding = 1f;
    [SerializeField] private TMPro.TMP_Text waveText;

    private Camera mainCamera;
    private GameManager gameManager;
    private int wave;

    private void Start()
    {
        mainCamera = Camera.main;
        gameManager = FindAnyObjectByType<GameManager>();
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        while (!gameManager.IsGameOver)
        {
            wave++;
            bool bossWave = wave % 5 == 0;
            waveText.text = bossWave ? $"Wave {wave} - BOSS" : $"Wave {wave}";
            yield return new WaitForSeconds(timeBetweenWaves);

            int zombieCount = firstWaveSize + wave - 1;
            float speedMultiplier = 1f + (wave - 1) * speedIncreasePerWave;
            for (int i = 0; i < zombieCount; i++)
            {
                SpawnZombie(speedMultiplier);
                yield return new WaitForSeconds(spawnDelay);
            }

            if (bossWave)
            {
                ZombieAI boss = Instantiate(zombiePrefab, GetSpawnPosition(), Quaternion.identity);
                boss.ConfigureBoss(wave);
                boss.SetSpeedMultiplier(speedMultiplier);
            }

            while (FindAnyObjectByType<ZombieAI>() != null && !gameManager.IsGameOver)
            {
                yield return null;
            }

            if (gameManager.IsGameOver)
            {
                yield break;
            }

            Health playerHealth = gameManager.Player.GetComponent<Health>();
            playerHealth.Heal(playerHealth.Max);
            Time.timeScale = 0f;
            bool skillChosen = false;
            gameManager.ShowSkillChoices(PlayerController.RollUpgradeChoices(3), upgrade =>
            {
                gameManager.Player.GetComponent<PlayerController>().ApplyUpgrade(upgrade);
                skillChosen = true;
            });
            yield return new WaitUntil(() => skillChosen || gameManager.IsGameOver);
        }
    }

    private void SpawnZombie(float speedMultiplier)
    {
        ZombieAI zombie = Instantiate(zombiePrefab, GetSpawnPosition(), Quaternion.identity);
        zombie.ConfigureVariant(Random.Range(0, 100) < 15
            ? ZombieAI.Variant.FastWeak
            : Random.Range(0, 100) < 18
                ? ZombieAI.Variant.SlowTough
                : ZombieAI.Variant.Normal);
        zombie.SetSpeedMultiplier(speedMultiplier);
    }

    private Vector2 GetSpawnPosition()
    {
        float height = mainCamera.orthographicSize;
        float width = height * mainCamera.aspect;
        Vector2 center = mainCamera.transform.position;
        switch (Random.Range(0, 4))
        {
            case 0: return center + new Vector2(Random.Range(-width, width), height + spawnPadding);
            case 1: return center + new Vector2(Random.Range(-width, width), -height - spawnPadding);
            case 2: return center + new Vector2(width + spawnPadding, Random.Range(-height, height));
            default: return center + new Vector2(-width - spawnPadding, Random.Range(-height, height));
        }
    }
}