using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class DebugTextManager : MonoBehaviour
{
    [SerializeField] private DebugScriptableObject _debugSO;
    private RectTransform m_RectTransform;
    // Start is called before the first frame update
    void Start()
    {
        m_RectTransform= GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        m_RectTransform.GetComponent<TextMeshProUGUI>().text = _debugSO.generalDebugMessage;
    }
}
