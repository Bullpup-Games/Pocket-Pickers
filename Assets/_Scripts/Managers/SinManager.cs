using System;
using System.Collections.Generic;
using _Scripts;
using UnityEngine;
using Random = UnityEngine.Random;

/*!SECTION
    IMPORTANT: The implementation of this class is temporary. 
    The existing sin logic has been migrated and slightly altered for easier management during the ServiceLocator refactor stage.
    This class will be entirely rewritten during the full sin refactor.
*/
public class SinManager : MonoBehaviour, ISinManager
{
    [SerializeField] private GameObject _smallSinPrefab;
    [SerializeField] private GameObject _mediumSinPrefab;
    [SerializeField] private GameObject _largeSinPrefab;
    [SerializeField] private GameObject _grandSinPrefab;
    [SerializeField] private GameObject _potentialSinPrefab;

    private List<GameObject> _activeSins = new List<GameObject>();
    private List<GameObject> _potentialSins = new List<GameObject>();
    private int _remainingSin;

    public int RemainingSin => _remainingSin;
    public int WinThreshold { get; set; }
    public List<GameObject> ActiveSins => _activeSins;
    public List<GameObject> PotentialSins => _potentialSins;

    public event Action SinChanged;

    public void Initialize()
    {
        RefreshSinLists();
        CalculateRemainingSin();
        Debug.Log($"SinManager initialized with {_activeSins.Count} active sins, total weight: {_remainingSin}");
    }

    public void RefreshSinLists()
    {
        _activeSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Sin"));
        _potentialSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
    }

    public void CollectSin(GameObject sin)
    {
        int sinWeight = sin.GetComponent<Sin>().weight;
        _remainingSin -= sinWeight;

        SinToPotentialSin(sin);

        Debug.Log($"Sin collected. Remaining sin: {_remainingSin}");

        if (_remainingSin <= WinThreshold)
        {
            Debug.Log("Win condition met - possible to win!");
        }

        SinChanged?.Invoke();
    }

    public void SpendSin(UInt16 weight)
    {
        // Spend sin from player's held sin (for purchasing, etc)
        // This would interact with player variables
    }

    public void DepositSin(UInt16 weight)
    {
        // Deposit sin (player releases it back into the world)
        ReleaseSin(weight);
    }

    public void WithdrawSin(UInt16 weight)
    {
        // Withdraw sin from world into player inventory
        // This happens during collection
    }

    public void ReleaseSin(UInt16 weight)
    {
        RefreshSinLists();

        if (_potentialSins.Count == 0)
        {
            Debug.LogWarning("No potential sin locations available!");
            return;
        }

        PotentialSinToSin(weight);
        Debug.Log($"Sin released. Remaining potential sins: {_potentialSins.Count}");
    }

    public void RedistributeSin(int sinToDistribute)
    {
        List<int> segments = new List<int>();

        while (sinToDistribute > 20)
        {
            int segment = Random.Range(10, 50);

            if (segment > sinToDistribute || sinToDistribute - segment < 10)
            {
                segments.Add(sinToDistribute);
                sinToDistribute = 0;
                break;
            }

            segments.Add(segment);
            sinToDistribute -= segment;
        }

        Debug.Log($"Redistributing {segments.Count} sin segments");

        foreach (int segment in segments)
        {
            Debug.Log($"Distributing segment of weight {segment}");
            PotentialSinToSin(segment);
        }
    }

    public void PurgeSin()
    {
        List<GameObject> sinsToPurge = new List<GameObject>(GameObject.FindGameObjectsWithTag("Sin"));
        foreach (GameObject sin in sinsToPurge)
        {
            Destroy(sin);
        }

        List<GameObject> potentialSinsToPurge = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
        foreach (GameObject potentialSin in potentialSinsToPurge)
        {
            Destroy(potentialSin);
        }

        _activeSins.Clear();
        _potentialSins.Clear();
    }

    public bool CheckWinCondition(int modifier)
    {
        CalculateRemainingSin();
        int trueSinCount = _remainingSin + modifier;

        if (trueSinCount <= WinThreshold)
        {
            Debug.Log("Win condition achieved!");
            return true;
        }

        return false;
    }

    public void InstantiateSin(int weight, Vector3 position)
    {
        GameObject sinPrefab = GetSinPrefabByWeight(weight);
        GameObject newSin = Instantiate(sinPrefab, position, Quaternion.identity);
        newSin.GetComponent<Sin>().weight = weight;
        newSin.GetComponent<Sin>().location = position;

        _activeSins.Add(newSin);
        _remainingSin += weight;
    }

    public void InstantiatePotentialSin(Vector3 position)
    {
        GameObject newPotentialSin = Instantiate(_potentialSinPrefab, position, Quaternion.identity);
        _potentialSins.Add(newPotentialSin);
        Debug.Log($"Potential sin created. Total potential sins: {_potentialSins.Count}");
    }

    public void AddSinsInSceneToActiveSins()
    {
        _activeSins.AddRange(GameObject.FindGameObjectsWithTag("Sin"));
        _potentialSins.AddRange(GameObject.FindGameObjectsWithTag("PotentialSin"));
    }

    private int CalculateRemainingSin()
    {
        _remainingSin = 0;
        foreach (GameObject sin in _activeSins)
        {
            if (sin != null)
            {
                _remainingSin += sin.GetComponent<Sin>().weight;
            }
        }
        return _remainingSin;
    }

    private void SinToPotentialSin(GameObject sin)
    {
        Debug.Log($"Converting active sin to potential sin. Active sins before: {_activeSins.Count}");

        for (int i = 0; i < _activeSins.Count; i++)
        {
            GameObject activeSin = _activeSins[i];

            if (Mathf.Abs(activeSin.transform.position.x - sin.transform.position.x) < 0.1f &&
                Mathf.Abs(activeSin.transform.position.y - sin.transform.position.y) < 0.1f)
            {
                _activeSins.RemoveAt(i);
                break;
            }
        }

        Debug.Log($"Active sins after: {_activeSins.Count}");
        InstantiatePotentialSin(sin.transform.position);
        sin.GetComponent<Sin>().DestroySin();
    }

    private void PotentialSinToSin(int weight)
    {
        if (_potentialSins.Count == 0)
        {
            Debug.LogWarning("Cannot convert potential sin - no locations available!");
            return;
        }

        RefreshSinLists();

        int location = Random.Range(0, _potentialSins.Count);
        GameObject potentialSin = _potentialSins[location];
        Vector3 sinLocation = potentialSin.transform.position;

        InstantiateSin(weight, sinLocation);

        _potentialSins.RemoveAt(location);
        Destroy(potentialSin);
    }

    private GameObject GetSinPrefabByWeight(int weight)
    {
        return weight switch
        {
            < 25 => _smallSinPrefab,
            < 40 => _mediumSinPrefab,
            < 60 => _largeSinPrefab,
            _ => _grandSinPrefab
        };
    }
}
