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
        
        private RaceResult m_result;

        private void Awake()
        {
        }

        public void SetResults(RaceResult result)
        {
            m_result = result;

            gameObject.SetActive(m_result != null);

            if (m_result != null)
            {
                m_rank.text = result.Rank.ToString();
                
                UIUtils.FitText(m_name, result.Profile.Name);

                m_finishTime.text = UIUtils.FormatRaceTime(result.Finished ? result.FinishTime : -1);
                m_bestLap.text = UIUtils.FormatRaceTime(result.LapTimes.Count > 0 ? result.LapTimes.Min() : -1);
            }
        }
    }
}
