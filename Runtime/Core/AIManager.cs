using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 MIT License

Copyright(c) 2022 Kitbashery

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.



Need support or additional features? Please visit https://kitbashery.com/
*/

namespace Kitbashery.AI
{
    /// <summary>
    /// Consolidates <see cref="AIAgent"/> update loops improving performance.
    /// </summary>
    [HelpURL("https://kitbashery.com/docs/modular-ai/ai-manager.html")]
    [DefaultExecutionOrder(-21)]
    [DisallowMultipleComponent]
    [AddComponentMenu("Kitbashery/AI/AI Manager")]
    public class AIManager : MonoBehaviour
    {
        public static AIManager Instance;

        public List<AIAgent> agents = new List<AIAgent>();

        [field: SerializeField, Tooltip("If true pauses all AI updates.")]
        public bool pauseAI { get; set; } = false;
        [Min(1), Tooltip("The amount of framerate frames between AI updates.")]
        public int updateRate = 1;
        private int frames = 0;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if(pauseAI == false && agents.Count > 0)
            {
                frames++;
                if(frames == updateRate)
                {
                    foreach (AIAgent agent in agents)
                    {
                        agent.UpdateAI();
                    }
                    frames = 0;
                }
            }
        }

        public void Register(AIAgent agent)
        {
            if(agents.Contains(agent) == false)
            {
                agents.Add(agent);
            }
        }

        public void Unregister(AIAgent agent)
        {
            if(agents.Contains(agent) == true)
            {
                agents.Remove(agent);
            }
        }
    }
}
