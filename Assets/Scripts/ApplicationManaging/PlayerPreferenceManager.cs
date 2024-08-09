using UnityEngine;

namespace Deenote.ApplicationManaging
{
    public sealed class PlayerPreferenceManager : MonoBehaviour
    {
        public int GetInt(string key, int defaultValue)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
            
        }
    }
}