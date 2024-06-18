using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Extras;
using UnityEngine;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private List<AgentController> agentsList = new List<AgentController>();
        [SerializeField] private List<ObservationPoint> observationPoints = new List<ObservationPoint>();
        [SerializeField] private string _oberservationPointTag = "ObservationPoint";
        
        private void Start()
        {
            agentsList.AddRange(FindObjectsOfType<AgentController>());
            observationPoints.AddRange(GameObject.FindGameObjectsWithTag(_oberservationPointTag).Select(p => p.GetComponent<ObservationPoint>()));
            SetIndexObservationPoints();
            
            SetPriorityToAgents(agentsList);
        }
        
        public void SetIndexObservationPoints()
        {
            for (var i = 0; i < observationPoints.Count; i++)
            {
                observationPoints[i].Index = i;
                Debug.Log("Index: " + observationPoints[i].Index + " Position: " + observationPoints[i].position);
            }
        }
        
        public void SetObservationPointAvailability(int pIndex, bool pAvailability)
        {
            observationPoints[pIndex].IsAvailable = pAvailability;
        }
        
        public bool GetObservationPointAvailability(int pIndex)
        {
            return observationPoints[pIndex].IsAvailable;
        }
        
        public Vector3 GetRandomObservationPoint()
        {
            var availableObservationPoints = new List<ObservationPoint>();
            for (var i = 0; i < observationPoints.Count; i++)
            {
                if (observationPoints[i].IsAvailable)
                {
                    availableObservationPoints.Add(observationPoints[i]);
                }
            }
            var randomIndex = Random.Range(0, availableObservationPoints.Count);
            return availableObservationPoints[randomIndex].transform.position;
        }
        
        private void SetPriorityToAgents(List<AgentController> pAgentsList)
        {
            for (var i = 0; i < pAgentsList.Count; i++)
            {
                var priority = (i + 1) * 0.01f;
                pAgentsList[i].SetPriority(priority);
            }
        }
    }
}
