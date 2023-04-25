using UnityEngine;

public class EnemyProd : ProductionStructure
{
    // Start is called before the first frame update
    new void Start()
    {
        base.Start();
        originialTime = 15;
        Vector3 offset = new Vector3(6, 0, -6);
        rallyPoint = spawnPoint.transform.position + offset;
    }

    // Update is called once per frame
    void Update()
    {
        if (GameController.Main.CorruptionController.SpawnUnits)
        {
            timeLeft -= Time.deltaTime;
            if (timeLeft <= 0)
            {
                timeLeft = originialTime;
                SpawnUnits(base.unitToSpawn);
                unitsQueued--;
            }
        }

    }
}
