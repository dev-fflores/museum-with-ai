using System.Collections;
using System.Collections.Generic;
using Extras;
using UnityEngine;

namespace Managers
{
    public class GameManager : Singleton<GameManager>
    {
        [SerializeField] private List<AgentController> agentsList = new List<AgentController>();
        
        private void Start()
        {
            agentsList.AddRange(FindObjectsOfType<AgentController>());
            SetPriorityToAgents(agentsList);
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
