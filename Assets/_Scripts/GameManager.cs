using System;
using System.Collections.Generic;
using System.IO;
using _Scripts.Player;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace _Scripts
{
    
    /*
     * The plan:
     * Create a new file, not a script.
     * Give this file cleanup and setup functions
     * call the cleanup function from the GameManager's Awake() function
     * Make seperate functions for completing the level and for dying in the level
     * call these functions when appropriate
     * Create a JSON file that will be the saved data
     * Make sure that there is always 1 and only one of these at any given time
     * 
     */
    public class GameManager : MonoBehaviour
    {
        public List<GameObject> activeSins;
        public List<GameObject> potentialSins;
        public int remainingSin;
        public int winThreshold;//what is the maximum amount of sin that can remain and you still win
        public int sinEscapedWith; // The amount of held sin the player has when they go to the Escape Screen

        public int DeathPenalty; //the penalty in sin for dying

        public bool isDead;
        //public Scene credits;

        //prefabs
        public GameObject smallSinPrefab;
        public GameObject mediumSinPrefab;
        public GameObject largeSinPrefab;
        public GameObject grandSinPrefab;
        public GameObject potentialSinPrefab;

        public GameObject quicktimeEventPanel;
        public GameObject quicktimeEventProgressPanel; // QTE progress meter panel to be set active / inactive
        public GameObject quicktimeEventProgressMeter; // The actual meter that gets adjusted
        public GameObject quicktimeEventTimeLeftMeter; // The meter to display how much time is remaining before death in the patroller QTE
        
        public GameObject pauseMenuDefaultButton;
        public GameObject deathScreenDefaultButton;
        
        public GameObject deathPanel;
            
        public event Action sinChanged; 
            
        #region Singleton

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType(typeof(GameManager)) as GameManager;

                return _instance;
            }
            set { _instance = value; }
        }

        private static GameManager _instance;

        #endregion
        
        public void Awake()
        {
            if (File.Exists(Application.persistentDataPath + "/save.txt"))
            {
                PurgeSin();
            }
            
            activeSins = new List<GameObject>();
            SaveManager.Instance.Setup();
            //activeSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Sin"));
            Debug.Log("Number of sins: " +activeSins.Count);
            calculateRemainingSin();
            potentialSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
            Debug.Log("The total amount of sin in the game is " + remainingSin);
            
        }

        private void Start()
        {
            EventSystem.current.SetSelectedGameObject(null);
            EventSystem.current.SetSelectedGameObject(pauseMenuDefaultButton);
        }

        private int calculateRemainingSin()
        {
            remainingSin = 0;
            foreach (GameObject sin in activeSins)
            {
                remainingSin += sin.GetComponent<Sin>().weight;
            }

            return remainingSin;
        }
        public void CollectSin(GameObject sin)
        {

            //deal with stats in GameManager

            int sinWeight = sin.GetComponent<Sin>().weight;
            remainingSin -= sinWeight;

            //deal with stats in Player
            PlayerVariables.Instance.CollectSin(sinWeight);
            
           
            SinToPotentialSin(sin);
            
            // GameObject newPotentialSin = Instantiate(potentialSinPrefab, sin.transform.position, Quaternion.identity);
            // potentialSins.Add(newPotentialSin);
            // Debug.Log("Number of potential sins: " + potentialSins.Count);
            // InstantiatePotentialSin(sin.transform.position);

            //TODO have an event that is called to let all enemies know that a sin was collected
            //deal with win condition, later we will remove this and put it in a different script for finishing a level
            Debug.Log("Remaining sin " + remainingSin);
            if (remainingSin <= winThreshold)
            {
                Debug.Log("It is now possible to win");
            }


            //Everything is done, remove the sin object
            //Destroy(sin);

            if (sinChanged != null )
            {
                sinChanged.Invoke();
            }
           
        }

        public void releaseSin(int weight)
        {
            //making sure that if you have filled every possible potential sin location, we can return safely
            // if (potentialSins.Count == 0)
            // {
            //     Debug.Log("It is impossible to hold more sins");
            //     return;
            // }
            potentialSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
            
            PotentialSinToSin(weight);
            //choosing the location
            // int location = Random.Range(0, potentialSins.Count);
            //
            // //remove the old potential sin
            // GameObject potentialSin = potentialSins[location];
            // //GameObject newSin = GameObject.Instantiate(sinPrefab, potentialSin.transform.position, Quaternion.identity);
            // InstantiateSin(weight,potentialSin.transform.position);
            // potentialSins.RemoveAt(location);
            // Destroy(potentialSin);
            
            //create a new sin
            // newSin.GetComponent<Sin>().weight = weight;
            // activeSins.Add(newSin);
            //
            // remainingSin += weight;
            //
            Debug.Log("Number of potential sins: " + potentialSins.Count);
            Debug.Log("Remaining sin " + remainingSin);
        }
        
        public void EscapeLevel()
        {
            /*
             * The plan:
             * 1. reset player sin held to 0 (release the sin they collected)
             * 2. Run check to see if you have won the game. If they have:
             *      a: delete their saved data
             *      b: set there saved data to be the default JSON
             *      c: transition to credits
             * 3. Call cleanup, possibly pass in a cutscene to transition to
             * 
             */
                
            //release all of the sin you hold
            if (PlayerVariables.Instance.sinHeld == 0) return;
            
            SinEscapedWith.Instance.sinHeldOnEscape = PlayerVariables.Instance.sinHeld;
            SinEscapedWith.Instance.sinLeftInLevelOnEscape = remainingSin;
            
            PlayerVariables.Instance.sinHeld = 0;
            
            if (checkForGameComplete(PlayerVariables.Instance.sinAccrued))
            {
                LevelLoader.Instance.LoadLevel(LevelLoader.Instance.credits);
                // SceneManager.LoadScene("winScreenPlaytest2");
                SaveManager.Instance.deleteSaveFile();
                return;
            }
               
            activeSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Sin"));
            potentialSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
            SaveManager.Instance.Cleanup();
            LevelLoader.Instance.LoadLevel(LevelLoader.Instance.escapeScreen); // TODO: Change to Escape Scene
        }

        
        public void Die()
        {
            /*
             * The plan:
             * 1. Redistribute all sin that has been accumulated in the level
             *      a: this should be sin held + a sin penalty + sin comitted
             *      b: Break this down into several reasonably sized portions
             *      c: instantiate each of these portions into a new sin from the potential sins
             * 2. Reset the player's sin held and sin commited to 0
             * 3. Call cleanup, possibly pass in a cutscene to transition to
             */
            
            int sinToDistribute = PlayerVariables.Instance.sinHeld + PlayerVariables.Instance.sinAccrued + DeathPenalty;
            RedistributeSin(sinToDistribute);
            
            PlayerVariables.Instance.sinHeld = 0;
            PlayerVariables.Instance.sinAccrued = 0;
            
            SaveManager.Instance.Cleanup();

            isDead = true;
            
            // Set focus to the default button
            if (EventSystem.current is not null)
            {
                EventSystem.current.SetSelectedGameObject(null);
                EventSystem.current.SetSelectedGameObject(deathScreenDefaultButton);
            }
            
            deathPanel.SetActive(true);
        }

        
        //used when the player dies. Takes all of their sin, and breaks it down into several sins, and then instantiates them
        private void RedistributeSin(int sinToDistribute)
        {

            List<int> segments = new List<int>();

            //break down sinToDistribute into segments
            while (sinToDistribute > 20)
            {
                int segment = Random.Range(10, 50);
                
                //if we have a situation with small enough sinToDistribute left, we will just throw all of it into one last sin
                if (segment > sinToDistribute || sinToDistribute - segment < 10)
                {
                    segments.Add(sinToDistribute);
                    sinToDistribute = 0;
                    break;
                }
                segments.Add(segment);
                sinToDistribute -= segment;
            }

            Debug.Log("Distributing these segments: " + segments);
            foreach (int segment in segments)
            {
                Debug.Log("Distributing segment of weight " + segment);
                PotentialSinToSin(segment);
            }
            
            
            return;
        }

        //called at the start of a scene to prevent sins from being saved multiple times
        public void PurgeSin()
        {
            List<GameObject> sinsToPurge =  new List<GameObject>(GameObject.FindGameObjectsWithTag("Sin"));
            foreach (GameObject sin in sinsToPurge)
            {
                Destroy(sin);
            }
            
            List<GameObject> potentialSinsToPurge = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
            for (int i = 0; i < potentialSinsToPurge.Count; i++)
            {
                Destroy(potentialSinsToPurge[i]);
            }
            activeSins.Clear();
            potentialSins = new List<GameObject>();
        }
        public bool checkForGameComplete(int modifier)
        {
            calculateRemainingSin();
            int trueSinCount = remainingSin + modifier;
            if (trueSinCount <= winThreshold)
            {
                
                Debug.Log("You win!");
                //SceneManager.LoadScene("winScreenPlaytest1");
                return true;
               
            }

            return false;
        }


        //turns an existing active sin into a potential sin
        private void SinToPotentialSin(GameObject sin)
        {
            Debug.Log("Trying to remove active sin");
            Debug.Log("Number of sins beforehand: " + activeSins.Count);
            //Debug.Log(activeSins);
            for (int i = 0; i < activeSins.Count; i++)
            {
                Debug.Log("Entered loop");
                GameObject activeSin = activeSins[i];
                //Debug.Log("Active sin at " + activeSin.transform.position);
                //.Log("sin at " + sin.transform.position);
                if (Mathf.Abs(activeSin.transform.position.x - sin.transform.position.x) < 0.1f && Mathf.Abs(activeSin.transform.position.y - sin.transform.position.y) < 0.1f) 
                {
                    Debug.Log("Trying to remove sin");
                    activeSins.RemoveAt(i);
                    // return;
                    break;
                }
            }
            Debug.Log("Number of sins afterwards: " + activeSins.Count);
            //Debug.Log(activeSins);
            //Destroy(sin);
            InstantiatePotentialSin(sin.transform.position);
            sin.GetComponent<Sin>().DestroySin();
        }

        //randomly picks a potential sin, and turns it into a sin
        private void PotentialSinToSin(int weight)
        {
            if (potentialSins.Count == 0)
            {
                Debug.Log("It is impossible to hold more sins");
                return;
            }
            
            potentialSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("PotentialSin"));
            activeSins = new List<GameObject>(GameObject.FindGameObjectsWithTag("Sin"));
            
            Debug.Log("When trying to instantiate a potential sin, there are " + potentialSins.Count + " potential sins");
            int location = Random.Range(0, potentialSins.Count-1);
            GameObject potentialSin = potentialSins[location];
            Vector3 sinLocation = potentialSin.transform.position;
            InstantiateSin(weight ,sinLocation);
            GameObject.Destroy(potentialSins[location]);
            potentialSins.RemoveAt(location);

            if (potentialSin is null)
            {
                Debug.Log("Potential sin no longer exists");
                return;
            }
            
            Destroy(potentialSin);
        }

        public void InstantiateSin(int weight, Vector3 pos)
        {
            
            //todo add logic to select the correct sin prefab based on the sin's weight
            GameObject sinToInstantiate;
            switch (weight)
            {
                case < 25: 
                    sinToInstantiate = smallSinPrefab;
                    break;
                case < 40:
                    sinToInstantiate = mediumSinPrefab;
                    break;
                case < 60:
                    sinToInstantiate = largeSinPrefab;
                    break;
                default:
                    sinToInstantiate = grandSinPrefab;
                    break;
            }
            GameObject newSin = Instantiate(sinToInstantiate, pos, Quaternion.identity);
            newSin.GetComponent<Sin>().weight = weight;
            newSin.GetComponent<Sin>().location = pos;
            activeSins.Add(newSin);

            remainingSin += weight;
        }

        public void InstantiatePotentialSin(Vector3 position)
        {
            GameObject newPotentialSin = Instantiate(potentialSinPrefab, position, Quaternion.identity);
            potentialSins.Add(newPotentialSin);
            Debug.Log("Number of potential sins: " + potentialSins.Count);
        }

        public void AddSinsInSceneToActiveSins()
        {
            activeSins.AddRange(GameObject.FindGameObjectsWithTag("Sin"));
            potentialSins.AddRange(GameObject.FindGameObjectsWithTag("PotentialSin"));
        }
    }
}