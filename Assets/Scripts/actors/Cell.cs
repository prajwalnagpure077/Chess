using System;
using UnityEngine;

public class Cell : MonoBehaviour
{
    [SerializeField] new Renderer renderer;
    [SerializeField] GameObject moveHighlight, attackHighlight;
    [SerializeField] TMPro.TextMeshPro numberTmp;

    internal int index;
    float lastTouchTime = 0;

    internal Troop troop;
    internal void init(Color cellColor, bool showNumbers, int index)
    {
        renderer.material.color = cellColor;
        moveLit(false);
        attackLit(false);
        this.index = index;

        if (showNumbers)
        {
            numberTmp.text = index.ToString();
        }

    }

    internal void moveLit(bool t)
    {
        moveHighlight.SetActive(t);
    }

    internal void attackLit(bool t)
    {
        attackHighlight.SetActive(t);
    }

    private void OnMouseDown()
    {
        lastTouchTime = Time.time;
    }
    private void OnMouseUp()
    {
        if (Time.time - lastTouchTime < 0.2f)
        {
            gridGenerator.currentSelectedTroop?.Move(index, true);
        }
    }
}