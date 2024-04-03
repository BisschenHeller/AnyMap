using UnityEngine;
using UnityEngine.UI;

namespace AnyMap
{
    public class RemainingDistanceScript : MonoBehaviour
    {
        private Text textField;

        private Map map;

        void Start()
        {
            textField = GetComponent<Text>();
            map = transform.parent.parent.GetComponent<Map>();
        }

        void Update()
        {
            textField.text = Mathf.RoundToInt(map.GetRemainingDistanceToQuestMarker()) + "m";
        }
    }
}