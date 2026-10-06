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
    private int wave;

    private void Start()
    {
        mainCamera = Camera.main;
        StartCoroutine(SpawnWaves());
    }

    private IEnumerator SpawnWaves()
    {
        while (true)
        {
            wave++;
            waveText.text = $"Wave {wave}";
            yield return new WaitForSeconds(timeBetweenWaves);

            int zombieCount = firstWaveSize + wave - 1;
            float speedMultiplier = 1f + (wave - 1) * speedIncreasePerWave;
            for (int i = 0; i < zombieCount; i++)
            {
                ZombieAI zombie = Instantiate(zombiePrefab, GetSpawnPosition(), Quaternion.identity);
                zombie.SetSpeedMultiplier(speedMultiplier);
                yield return new WaitForSeconds(spawnDelay);
            }

            while (FindFirstObjectByType<ZombieAI>() != null)
            {
                yield return null;
            }
        }
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