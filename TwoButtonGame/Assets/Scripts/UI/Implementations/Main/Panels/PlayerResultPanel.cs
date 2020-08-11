using System.Linq;

using BoostBlasters.Profiles;

using UnityEngine;
using UnityEngine.UI;

namespace BoostBlasters.UI.MainMenus
{
    public class PlayerResultPanel : MonoBehaviour
    {
        [Header("UI Elements")]

        [SerializeField] private Text m_rank = null;
        [SerializeField] private Text m_name = null;
        [SerializeField] private Text m_finishTime = null;
        [SerializeField] private Text m_bestLap = null;

        public void SetResults(Profile profile, RaceResult result)
        {
            SetResults(profile, result, result?.Rank ?? 0);
        }

        public void SetResults(Profile profile, RaceResult result, int rank)
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

                //UIUtils.FitText(m_name, profile.Name);

                if (m_finishTime != null)
                {
                    m_finishTime.text = UIUtils.FormatRaceTime(result.Finished ? result.FinishTime : -1f);
                }
                if (m_bestLap != null)
                {
                    m_bestLap.text = UIUtils.FormatRaceTime(result.LapTimes.Count > 0 ? result.LapTimes.Min() : -1f);
                }
            }
        }
    }
}
