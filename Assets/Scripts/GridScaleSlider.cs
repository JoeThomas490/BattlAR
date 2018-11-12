using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GridScaleSlider : MonoBehaviour
{

    public Transform m_gridParent;
    public Slider m_scaleSlider;

    void Awake()
    {
        m_scaleSlider.value = m_gridParent.localScale.x;
        string txt = m_scaleSlider.value.ToString().Substring(0, 4);
    }

    public void ScaleGrid(float newScale)
    {
        m_gridParent.localScale = new Vector3(newScale, newScale, newScale);
        string txt = newScale.ToString();
        txt = txt.Substring(0, 4);
    }

}
