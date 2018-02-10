using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.Menus
{
    public class PlayerResultPanel : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField] private Text m_rank;
        [SerializeField] private Text m_name;
        [SerializeField] private Text m_finishTime;
        [SerializeField] private Text m_bestLap;
        
        public void SetResults(RaceResult result)
        {
            SetResults(result, result != null ? result.Rank : 0);
        }

        public void SetResults(RaceResult result, int rank)
        {
            gameObject.SetActive(result != null);

            /*
            m_rank.enabled = active;
            m_name.enabled = active;

            if (m_finishTime != null)
            {
                m_finishTime.enabled = active;
            }
            if (m_bestLap != null)
            {
                m_bestLap.enabled = active;
            }
            */

            if (gameObject.activeInHierarchy)
            {
                m_rank.text = rank.ToString();
                
                UIUtils.FitText(m_name, result.Profile.Name);

                if (m_finishTime != null)
                {
                    m_finishTime.text = UIUtils.FormatRaceTime(result.Finished ? result.FinishTime : -1);
                }
                if (m_bestLap != null)
                {
                    m_bestLap.text = UIUtils.FormatRaceTime(result.LapTimes.Count > 0 ? result.LapTimes.Min() : -1);
                }
            }
        }
    }
}
