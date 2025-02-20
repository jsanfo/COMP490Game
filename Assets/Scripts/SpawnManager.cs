using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public GameObject capybara;
    public GameObject capybaraPlacer;

    public GameObject entrancePath;
    private Vector3 entranceForward;
    private float elapsedSpawnTime = 0;
    private float spawnLength = 5.0f;

    GameObject stats;
    private GameplayState gameplayStateScript;
    private CapybaraHandler capybaraHandlerScript;

    enum States { center, right, left };
    States states;

    // Start is called before the first frame update
    IEnumerator Start()
    {
        stats = GameObject.Find("Stats");
        gameplayStateScript = stats.GetComponent<GameplayState>();

        capybaraHandlerScript = gameObject.GetComponent<CapybaraHandler>();

        entrancePath = GameObject.Find("EntrancePath");
        var entranceScript = entrancePath.GetComponent<Path>();
        entranceForward = entranceScript.spacedPoints[0] - entranceScript.spacedPoints[1];
        yield return new WaitForSeconds(0.01f);
    }

    // Update is called once per frame
    void Update()
    {
        elapsedSpawnTime += Time.deltaTime;

        if (elapsedSpawnTime > spawnLength)
        {
            if (capybaraHandlerScript.CapybaraCount() == 0)
            {
                StartCoroutine(SpawnCapybara());
                spawnLength = Random.Range(6, 13);
            }
            else
            {
                float weight = GetSpawnWeight() * 11;
                float random = Random.Range(0, 100);
                if (random < weight)
                {
                    StartCoroutine(SpawnCapybara());
                    spawnLength = Random.Range(6, 13);
                }
                else
                {
                    spawnLength = Random.Range(2, 5);
                }
            }

            elapsedSpawnTime = 0;
        }
    }

    float GetSpawnWeight()
    {
        if (gameplayStateScript.GetCapacity() < 1)
        {
            return 0f;
        }

        // Weigh happiness
        float averageHappiness = 50;
        if (capybaraHandlerScript != null)
        {
            averageHappiness = capybaraHandlerScript.AverageHappiness();
        }

        float happinessCapacity = gameplayStateScript.GetCapacity();

        if (capybaraHandlerScript.CapybaraCount() >= happinessCapacity)
        {
            happinessCapacity = Mathf.RoundToInt(Mathf.Pow(happinessCapacity, 0.95f));
        }
        else
        {
            if (averageHappiness > 50)
            {
                happinessCapacity = Mathf.RoundToInt(Mathf.Pow((happinessCapacity + 0.5f), 1.025f));
            }
            else
            {
                happinessCapacity = Mathf.RoundToInt(Mathf.Pow((happinessCapacity - 0.5f), 0.975f));
            }
        }

        var currentCount = capybaraHandlerScript.CapybaraCount();
        if (currentCount == 0)
        {
            currentCount = 1;
        }

        return happinessCapacity / currentCount;
    }

    IEnumerator SpawnCapybara()
    {
        GameObject capyPlacer = GameObject.Instantiate(capybaraPlacer);
        capyPlacer.transform.position = new Vector3(1, 0, 1);
        capyPlacer.transform.Rotate(Vector3.up, 45);
        var placerScript = capyPlacer.GetComponent<CapybaraPlacer>();

        float pathDistance = 0;
        states = States.center;

        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if (placerScript.Collisions > 0)
            {
                if (states == States.center)
                {
                    states = States.right;
                    capyPlacer.transform.Translate(Vector3.right * 0.13f);
                    pathDistance += 0.10f;
                }
                else if (states == States.right)
                {
                    if (pathDistance < 0.30)
                    {
                        capyPlacer.transform.Translate(Vector3.right * 0.13f);
                        pathDistance += 0.10f;
                    }
                    else
                    {
                        states = States.left;
                        capyPlacer.transform.position = new Vector3(1, 0, 1);
                        capyPlacer.transform.Translate(Vector3.left * 0.13f);
                        pathDistance = -0.10f;
                    }
                }
                else if (states == States.left)
                {
                    if (pathDistance > -0.30)
                    {
                        capyPlacer.transform.Translate(Vector3.left * 0.13f);
                        pathDistance -= 0.10f;
                    }
                    else
                    {
                        states = States.center;
                        capyPlacer.transform.position = new Vector3(1, 0, 1);
                        pathDistance = 0;
                    }
                }
            }
            else
            {
                var newCapy = GameObject.Instantiate(capybara);
                var capyInfo = newCapy.GetComponent<CapybaraInfo>();
                var capyAI = newCapy.GetComponent<CapyAI>();
                capyInfo.capyName = CapyNames.GetRandomName();
                capyInfo.hunger = Random.Range(50, 75);
                capyInfo.comfort = Random.Range(50, 75);
                capyInfo.fun = Random.Range(50, 75);
                newCapy.transform.position = capyPlacer.transform.position;
                newCapy.transform.Rotate(Vector3.up, 45);

                GameObject.Destroy(capyPlacer);

                if (capybaraHandlerScript != null)
                {
                    capybaraHandlerScript.AddCapybara(newCapy);
                }
                break;
            }
        }
    }
}
